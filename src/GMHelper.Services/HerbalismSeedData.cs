using GMHelper.Core.Enums;

namespace GMHelper.Services;

/// <summary>
/// Mitgelieferte Sammelgebiete der Schwertküste samt Fundtabellen. Die Gebiete und ihre SG
/// folgen dem Gelände (ergiebiges Kulturland SG 10, gemischtes Gelände SG 15, karges oder
/// gefährliches Gelände SG 20); die Zutaten selbst sind bewusst Eigenentwicklung, da die
/// Regelwerke keine Zutatenlisten definieren — sie sind in der App frei änderbar.
/// </summary>
public static class HerbalismSeedData
{
    public record SeedIngredient(
        string Name,
        IngredientKind Kind,
        IngredientRarity Rarity,
        string Effect,
        int? ValueInGoldPieces);

    public record SeedRegion(
        string Name,
        string Terrain,
        int DifficultyClass,
        string Description,
        IReadOnlyList<SeedIngredient> Ingredients);

    public static IReadOnlyList<SeedRegion> Regions { get; } =
    [
        new SeedRegion(
            "Dessarin-Tal",
            "Fluss- und Ackerland",
            10,
            "Fruchtbares Tal um Rotlärchen und die Goldfelder. Das ergiebigste und ungefährlichste Sammelgebiet der Schwertküste.",
            [
                new SeedIngredient("Feldkamille", IngredientKind.PotionIngredient, IngredientRarity.Common,
                    "Allgegenwärtig an Feldrainen. Beruhigt den Magen und ist Trägerstoff für fast jeden einfachen Trank.", 2),
                new SeedIngredient("Rotlärchenrinde", IngredientKind.PotionIngredient, IngredientRarity.Common,
                    "Von den namensgebenden Lärchen geschält. Fiebersenkend, Grundstoff für Heiltränke geringer Stärke.", 5),
                new SeedIngredient("Goldfeld-Ähre", IngredientKind.SpellComponent, IngredientRarity.Common,
                    "Aus den Tempelfeldern von Chauntea. Materialkomponente für Zauber, die Pflanzen wachsen lassen oder Nahrung erschaffen.", 4),
                new SeedIngredient("Sichelklee", IngredientKind.Both, IngredientRarity.Uncommon,
                    "Vierblättrig gewachsener Klee vom Rand der Steinkreise. Trank des Glücks in geringer Dosis, sonst Fokus für Wahrsagerei.", 30),
                new SeedIngredient("Baldrianwurzel", IngredientKind.PotionIngredient, IngredientRarity.Uncommon,
                    "Tief unter feuchten Böschungen. Grundstoff für Schlaftränke; zu hoch dosiert ein Gift.", 40),
                new SeedIngredient("Dessarin-Flussperlkraut", IngredientKind.SpellComponent, IngredientRarity.Rare,
                    "Wächst nur in den klarsten Flussarmen und bildet perlmuttfarbene Knoten. Ersatzfokus für Zauber, die eine Perle verlangen.", 110),
            ]),

        new SeedRegion(
            "Neverwinterwald",
            "Wald",
            15,
            "Von den Thermalquellen des Berges Hotenow durchzogen. Teile des Waldes bleiben auch im Winter warm und blühen ganzjährig.",
            [
                new SeedIngredient("Silberflechte", IngredientKind.Both, IngredientRarity.Common,
                    "Auf der Nordseite alter Eichen. Klärt verdorbenes Wasser und dient als Komponente für Reinigungszauber.", 8),
                new SeedIngredient("Glutkappe", IngredientKind.PotionIngredient, IngredientRarity.Common,
                    "Roter Pilz der warmen Aschefelder. Grundstoff für Tränke der Feuerresistenz.", 15),
                new SeedIngredient("Hotenow-Schwefelblüte", IngredientKind.PotionIngredient, IngredientRarity.Uncommon,
                    "Nur unmittelbar an den Thermalquellen. Verstärkt die Wirkung eines Trankes spürbar, macht ihn aber bitter.", 50),
                new SeedIngredient("Nachtwurzel", IngredientKind.SpellComponent, IngredientRarity.Uncommon,
                    "Schwarze Knolle, die bei Tageslicht zerfällt und im Dunkeln geerntet werden muss. Komponente für Illusionszauber.", 45),
                new SeedIngredient("Mondtau", IngredientKind.Both, IngredientRarity.Rare,
                    "Sammelt sich nur in klaren Vollmondnächten in den Blattkelchen. Grundstoff hochwertiger Heiltränke und Fokus für Verzauberungen.", 120),
                new SeedIngredient("Herz der Aschenesche", IngredientKind.SpellComponent, IngredientRarity.VeryRare,
                    "Kernholz einer Esche, die den Ausbruch des Hotenow überstanden hat. Fokus für die Beschwörung von Feuerelementaren.", 400),
            ]),

        new SeedRegion(
            "Ardeep-Wald",
            "Elfenwald",
            15,
            "Kleiner, von elfischer Magie durchwirkter Wald südlich von Tiefwasser. Wer respektlos erntet, findet beim nächsten Mal nichts mehr.",
            [
                new SeedIngredient("Elfenhaargras", IngredientKind.Both, IngredientRarity.Common,
                    "Feine, silbrige Halme auf den Lichtungen. Bindemittel für Tränke und Faden für Zauberweben.", 6),
                new SeedIngredient("Sternmoos", IngredientKind.SpellComponent, IngredientRarity.Common,
                    "Leuchtet nachts schwach. Komponente für Zauber, die Licht erzeugen oder Dunkelheit vertreiben.", 10),
                new SeedIngredient("Ardeep-Veilchen", IngredientKind.PotionIngredient, IngredientRarity.Uncommon,
                    "Blüht über den Gräbern der elfischen Ahnen. Grundstoff für Tränke, die Erschöpfung aufheben.", 55),
                new SeedIngredient("Sängerlaub", IngredientKind.SpellComponent, IngredientRarity.Uncommon,
                    "Blätter, die im Wind einen Ton halten. Komponente für Bezauberungen und Zauber, die Stimmen tragen.", 60),
                new SeedIngredient("Traumkirsche", IngredientKind.PotionIngredient, IngredientRarity.Rare,
                    "Trägt nur alle sieben Jahre Frucht. Grundstoff für Tränke des Hellsehens und des traumlosen Schlafs.", 140),
                new SeedIngredient("Blattgold-Eichel", IngredientKind.SpellComponent, IngredientRarity.VeryRare,
                    "Von der letzten Hochwald-Eiche Ardeeps. Fokus für Zauber, die Bäume erwecken oder Wälder verschieben.", 450),
            ]),

        new SeedRegion(
            "Küste des Schwertmeeres",
            "Küste und Gezeitenzone",
            15,
            "Klippen, Tangwälder und Gezeitentümpel zwischen Tiefwasser und Baldurs Tor. Nur bei Ebbe zugänglich.",
            [
                new SeedIngredient("Gezeitentang", IngredientKind.PotionIngredient, IngredientRarity.Common,
                    "In jedem Gezeitentümpel. Salzhaltiger Grundstoff, der Tränke haltbar macht.", 3),
                new SeedIngredient("Möwenkraut", IngredientKind.PotionIngredient, IngredientRarity.Common,
                    "Zäh und salzresistent auf den Klippen. Wirkt gegen Seekrankheit und leichte Vergiftungen.", 7),
                new SeedIngredient("Salzkorallensplitter", IngredientKind.SpellComponent, IngredientRarity.Common,
                    "Von abgestorbenen Korallenbänken angespült. Komponente für Zauber, die Wasser formen oder Kälte bannen.", 12),
                new SeedIngredient("Purpurschnecke", IngredientKind.Both, IngredientRarity.Uncommon,
                    "Liefert einen tiefvioletten Farbstoff. Fixiert Trankwirkungen und ergibt Tinte zum Abschreiben von Zaubern.", 65),
                new SeedIngredient("Sirenengras", IngredientKind.SpellComponent, IngredientRarity.Uncommon,
                    "Wächst dort, wo Meervolk singt. Komponente für Zauber, die Gedanken oder Willen beeinflussen.", 70),
                new SeedIngredient("Perle der Tiefe", IngredientKind.Both, IngredientRarity.Rare,
                    "Aus Austernbänken unterhalb der Niedrigwasserlinie. Grundstoff für Wasseratmen-Tränke und als Fokus mindestens 100 GM wert.", 130),
                new SeedIngredient("Kraken-Tintenblase", IngredientKind.SpellComponent, IngredientRarity.VeryRare,
                    "Nur aus dem Kadaver eines Tiefseekraken zu bergen. Fokus für Zauber der Dunkelheit und der Beschwörung aus der Tiefe.", 500),
            ]),

        new SeedRegion(
            "Sumpf der Toten Männer",
            "Salzmarsch",
            15,
            "Salziges Marschland entlang der Küstenstraße, durchsetzt von versunkenen Schiffen und ihren untoten Besatzungen.",
            [
                new SeedIngredient("Salzbinse", IngredientKind.PotionIngredient, IngredientRarity.Common,
                    "Faserige Halme der Marsch. Konserviert andere Zutaten wochenlang.", 4),
                new SeedIngredient("Faulwasserlinse", IngredientKind.SpellComponent, IngredientRarity.Common,
                    "Schwimmender grüner Teppich. Komponente für Zauber, die Nebel oder Wasser formen.", 8),
                new SeedIngredient("Ertrunkenenkraut", IngredientKind.PotionIngredient, IngredientRarity.Uncommon,
                    "Wurzelt in den Rippen versunkener Schiffe. Grundstoff für Tränke des Wasseratmens.", 55),
                new SeedIngredient("Modermorchel", IngredientKind.Both, IngredientRarity.Uncommon,
                    "Wächst auf Treibgut. Roh giftig, gekocht wirksam gegen Krankheiten und als Komponente gegen Flüche.", 40),
                new SeedIngredient("Leichenlilie", IngredientKind.SpellComponent, IngredientRarity.Rare,
                    "Weiße Blüte über einem unbestatteten Toten. Komponente für Zauber, die mit Toten sprechen.", 135),
                new SeedIngredient("Herz des Hexenmoors", IngredientKind.PotionIngredient, IngredientRarity.VeryRare,
                    "Schwarze Knolle im unzugänglichen Zentrum des Sumpfes. Grundstoff für Tränke, die dem Tod trotzen.", 480),
            ]),

        new SeedRegion(
            "Hochmoor",
            "Moor und Hochland",
            20,
            "Windgepeitschtes Hochland über den Ruinen des gefallenen Miyeritar. Karg, gefährlich, aber reich an zähen Heilkräutern.",
            [
                new SeedIngredient("Moorbart", IngredientKind.PotionIngredient, IngredientRarity.Common,
                    "Zähes Moos an Torfkanten. Blutstillend, Grundstoff einfacher Heiltränke.", 9),
                new SeedIngredient("Torfwollgras", IngredientKind.Both, IngredientRarity.Common,
                    "Weiße Wollköpfe über dem Moor. Filtert Gifte aus Flüssigkeiten und dient als Zunder für Ritualfeuer.", 6),
                new SeedIngredient("Bleicher Enzian", IngredientKind.PotionIngredient, IngredientRarity.Uncommon,
                    "Blüht ausschließlich auf den Gräbern gefallener Miyeritari. Grundstoff für Tränke gegen Gift.", 50),
                new SeedIngredient("Schattenkraut", IngredientKind.SpellComponent, IngredientRarity.Uncommon,
                    "Über den Ruinen des Schattenreichs gewachsen, wirft keinen eigenen Schatten. Komponente für Nekromantie.", 70),
                new SeedIngredient("Wanderdistelsame", IngredientKind.SpellComponent, IngredientRarity.Rare,
                    "Vom Moorwind über Meilen getragen. Fokus für Zauber der Bewegung, des Sprungs und des Fliegens.", 150),
                new SeedIngredient("Geisterblüte", IngredientKind.Both, IngredientRarity.VeryRare,
                    "Blüht nur dort, wo ein Untoter endgültig vernichtet wurde. Bannt Furcht und dient als Fokus für Schutzkreise.", 520),
            ]),

        new SeedRegion(
            "Kryptengarten-Wald",
            "Urwald über Zwergenruinen",
            20,
            "Uralter, kaum begangener Wald über den Ruinen von Delzoun-Außenposten. Ein weißer Drache beansprucht ihn als Revier.",
            [
                new SeedIngredient("Zwergenbartflechte", IngredientKind.Both, IngredientRarity.Common,
                    "Hängt von Ästen über den Ruinenzugängen. Bitteres Stärkungsmittel und Komponente für Zauber gegen Erschöpfung.", 10),
                new SeedIngredient("Steinkappenpilz", IngredientKind.PotionIngredient, IngredientRarity.Common,
                    "Wächst auf behauenem Zwergenstein. Grundstoff für Tränke, die die Haut verhärten.", 14),
                new SeedIngredient("Drachenzungenfarn", IngredientKind.PotionIngredient, IngredientRarity.Uncommon,
                    "Nur im Umkreis des Drachenlagers, gerötet vom Frostatem. Grundstoff für Tränke der Kälteresistenz.", 65),
                new SeedIngredient("Rankenherz", IngredientKind.SpellComponent, IngredientRarity.Uncommon,
                    "Knoten im Zentrum einer Würgeranke. Komponente für Zauber, die Wesen festhalten oder fesseln.", 60),
                new SeedIngredient("Eisrindenharz", IngredientKind.Both, IngredientRarity.Rare,
                    "Gefrorenes Harz an vom Drachenatem gestreiften Stämmen. Trank der Kälteresistenz und Fokus für Frostzauber.", 145),
                new SeedIngredient("Wurzel des Ersten Baums", IngredientKind.SpellComponent, IngredientRarity.VeryRare,
                    "Aus der tiefsten überfluteten Zwergenkammer. Fokus für Zauber, die Verwundete zurück ins Leben holen.", 500),
            ]),

        new SeedRegion(
            "Schwertberge",
            "Gebirge",
            20,
            "Zerklüftete Gipfel nördlich von Tiefwasser, Revier von Greifen und Wyvern. Sammeln erfordert oft Klettern.",
            [
                new SeedIngredient("Felsspaltenmoos", IngredientKind.PotionIngredient, IngredientRarity.Common,
                    "Einziges Grün auf den Geröllhalden. Wasserspeichernd, Grundstoff für Tränke gegen Durst und Hitze.", 7),
                new SeedIngredient("Gipfelthymian", IngredientKind.Both, IngredientRarity.Common,
                    "Aromatisch und zäh, wächst über der Baumgrenze. Konserviert Tränke und würzt Räucherwerk für Rituale.", 11),
                new SeedIngredient("Adleraugenblüte", IngredientKind.PotionIngredient, IngredientRarity.Uncommon,
                    "Wächst an Greifenhorsten. Grundstoff für Tränke, die die Sicht auf große Entfernung schärfen.", 60),
                new SeedIngredient("Sturmquarzmoos", IngredientKind.SpellComponent, IngredientRarity.Uncommon,
                    "Siedelt auf blitzgetroffenem Fels und knistert bei Berührung. Komponente für Blitz- und Donnerzauber.", 75),
                new SeedIngredient("Wyvernfarn", IngredientKind.PotionIngredient, IngredientRarity.Rare,
                    "Gedeiht nur im Nistmaterial von Wyvern. Einziger verlässlicher Grundstoff für ein Gegengift gegen Wyvern-Gift.", 160),
                new SeedIngredient("Wolkenlotos", IngredientKind.Both, IngredientRarity.VeryRare,
                    "Blüht auf den höchsten Graten, wo Wolken hängenbleiben. Grundstoff für Tränke des Fliegens und Fokus für Luftzauber.", 550),
            ]),
    ];
}
