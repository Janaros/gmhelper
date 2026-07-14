namespace GMHelper.Core.Enums;

/// <summary>How to handle an imported monster whose name already exists in the database.</summary>
public enum MonsterImportConflictStrategy
{
    Skip,
    Overwrite,
    CreateDuplicate,
}
