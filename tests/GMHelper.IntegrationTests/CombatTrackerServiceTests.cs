using GMHelper.Core.Enums;
using GMHelper.Data;
using GMHelper.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GMHelper.IntegrationTests;

public class CombatTrackerServiceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly ServiceProvider _serviceProvider;
    private readonly CombatTrackerService _sut;
    private readonly PlayerService _playerService;
    private readonly MonsterService _monsterService;
    private readonly StatFieldService _statFieldService;
    private readonly int _campaignId;

    public CombatTrackerServiceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "GMHelperTests", Guid.NewGuid().ToString("N"));
        var appPaths = new AppPaths(_tempRoot);

        var services = new ServiceCollection();
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite($"Data Source={appPaths.DatabaseFilePath}"));
        _serviceProvider = services.BuildServiceProvider();

        var factory = _serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using (var db = factory.CreateDbContext())
        {
            Directory.CreateDirectory(_tempRoot);
            db.Database.Migrate();

            var campaign = new Core.Entities.Campaign { Name = "Testkampagne", CreatedAt = DateTime.UtcNow };
            db.Campaigns.Add(campaign);
            db.SaveChanges();
            _campaignId = campaign.Id;
        }

        _sut = new CombatTrackerService(factory);
        _playerService = new PlayerService(factory);
        _monsterService = new MonsterService(factory, appPaths);
        _statFieldService = new StatFieldService(factory);
    }

    [Fact]
    public async Task PrepareEncounterAsync_PullsOnlyActivePlayers()
    {
        var active = await _playerService.CreatePlayerAsync(_campaignId, "Grog");
        var inactive = await _playerService.CreatePlayerAsync(_campaignId, "Vax");
        await _playerService.SetActiveAsync(inactive.Id, false);

        var encounter = await _sut.PrepareEncounterAsync(_campaignId);

        var participants = await _sut.GetParticipantsAsync(encounter.Id);
        Assert.Single(participants);
        Assert.Equal("Grog", participants[0].DisplayName);
        Assert.Equal(CombatParticipantSourceType.PlayerRef, participants[0].SourceType);
    }

    [Fact]
    public async Task PrepareEncounterAsync_SnapshotsPlayerHpFromStatField()
    {
        var player = await _playerService.CreatePlayerAsync(_campaignId, "Grog");
        await _statFieldService.ReplaceStatFieldsAsync(StatFieldOwnerType.Player, player.Id, [("HP", "45")]);

        var encounter = await _sut.PrepareEncounterAsync(_campaignId);

        var participants = await _sut.GetParticipantsAsync(encounter.Id);
        Assert.Equal(45, participants[0].CurrentTrackedValue);
        Assert.Equal(45, participants[0].MaxTrackedValue);
    }

    [Fact]
    public async Task AddMonsterParticipantAsync_SnapshotsHpAndAutoNumbersDuplicates()
    {
        var monster = await _monsterService.CreateMonsterAsync("Goblin");
        await _statFieldService.ReplaceStatFieldsAsync(StatFieldOwnerType.Monster, monster.Id, [("HP", "7")]);
        var encounter = await _sut.PrepareEncounterAsync(_campaignId);

        var first = await _sut.AddMonsterParticipantAsync(encounter.Id, monster.Id);
        var second = await _sut.AddMonsterParticipantAsync(encounter.Id, monster.Id);

        Assert.Equal("Goblin 1", first.DisplayName);
        Assert.Equal("Goblin 2", second.DisplayName);
        Assert.Equal(7, first.CurrentTrackedValue);
        Assert.Equal(7, first.MaxTrackedValue);
    }

    [Fact]
    public async Task StartEncounterAsync_SortsByInitiativeDescendingAndSetsRoundOne()
    {
        var encounter = await _sut.PrepareEncounterAsync(_campaignId);
        var monster = await _monsterService.CreateMonsterAsync("Goblin");
        var low = await _sut.AddMonsterParticipantAsync(encounter.Id, monster.Id);
        var high = await _sut.AddMonsterParticipantAsync(encounter.Id, monster.Id);
        await _sut.UpdateParticipantAsync(low.Id, low.DisplayName, 5, null, null);
        await _sut.UpdateParticipantAsync(high.Id, high.DisplayName, 18, null, null);

        await _sut.StartEncounterAsync(encounter.Id);

        var started = await _sut.GetActiveEncounterAsync(_campaignId);
        Assert.Equal(1, started!.CurrentRound);
        Assert.Equal(high.Id, started.CurrentTurnParticipantId);

        var ordered = await _sut.GetParticipantsAsync(encounter.Id);
        Assert.Equal([high.Id, low.Id], ordered.Select(p => p.Id));
    }

    [Fact]
    public async Task AdvanceTurnAsync_WrapsAroundAndIncrementsRound()
    {
        var encounter = await _sut.PrepareEncounterAsync(_campaignId);
        var monster = await _monsterService.CreateMonsterAsync("Goblin");
        var a = await _sut.AddMonsterParticipantAsync(encounter.Id, monster.Id);
        var b = await _sut.AddMonsterParticipantAsync(encounter.Id, monster.Id);
        await _sut.UpdateParticipantAsync(a.Id, a.DisplayName, 20, null, null);
        await _sut.UpdateParticipantAsync(b.Id, b.DisplayName, 10, null, null);
        await _sut.StartEncounterAsync(encounter.Id);

        await _sut.AdvanceTurnAsync(encounter.Id);
        var afterFirstAdvance = await _sut.GetActiveEncounterAsync(_campaignId);
        Assert.Equal(1, afterFirstAdvance!.CurrentRound);
        Assert.Equal(b.Id, afterFirstAdvance.CurrentTurnParticipantId);

        await _sut.AdvanceTurnAsync(encounter.Id);
        var afterWrap = await _sut.GetActiveEncounterAsync(_campaignId);
        Assert.Equal(2, afterWrap!.CurrentRound);
        Assert.Equal(a.Id, afterWrap.CurrentTurnParticipantId);
    }

    [Fact]
    public async Task RemoveParticipantAsync_HidesFromGetParticipants()
    {
        var encounter = await _sut.PrepareEncounterAsync(_campaignId);
        var monster = await _monsterService.CreateMonsterAsync("Goblin");
        var participant = await _sut.AddMonsterParticipantAsync(encounter.Id, monster.Id);

        await _sut.RemoveParticipantAsync(participant.Id);

        var participants = await _sut.GetParticipantsAsync(encounter.Id);
        Assert.DoesNotContain(participants, p => p.Id == participant.Id);
    }

    [Fact]
    public async Task EndEncounterAsync_ArchivesEncounter_NoLongerActive()
    {
        var encounter = await _sut.PrepareEncounterAsync(_campaignId);

        await _sut.EndEncounterAsync(encounter.Id);

        var active = await _sut.GetActiveEncounterAsync(_campaignId);
        Assert.Null(active);
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        SqliteConnection.ClearAllPools();

        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }
}
