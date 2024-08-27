namespace BillionRowChallenge;

public static class Helpers
{
    public static string GetResultsFileName(string filepath, string mode)
    {
        string[] pathComponents = filepath.Split('.');
        string basePath = string.Join('.', pathComponents.Take(pathComponents.Length - 1));
        return mode switch
        {
            "reference" => $"{basePath}-results-ref.txt",
            "fast-sync" => $"{basePath}-results-sync.txt",
            "fast-async" => $"{basePath}-results-async.txt",
            _ => string.Empty
        };
    }
}