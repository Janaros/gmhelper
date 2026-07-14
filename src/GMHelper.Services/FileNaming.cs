namespace GMHelper.Services;

internal static class FileNaming
{
    /// <summary>
    /// Returns a destination path for <paramref name="fileName"/> inside <paramref name="folder"/>,
    /// appending " (1)", " (2)", ... if a file with that name already exists there.
    /// </summary>
    public static string ResolveUniqueDestinationPath(string folder, string fileName)
    {
        var destinationPath = Path.Combine(folder, fileName);
        if (!File.Exists(destinationPath))
        {
            return destinationPath;
        }

        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);

        var counter = 1;
        string candidatePath;
        do
        {
            candidatePath = Path.Combine(folder, $"{nameWithoutExtension} ({counter}){extension}");
            counter++;
        } while (File.Exists(candidatePath));

        return candidatePath;
    }
}
