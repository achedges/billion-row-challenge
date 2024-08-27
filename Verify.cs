namespace BillionRowChallenge;

public static class Verifier
{
    public static void Verify(string filepath, string checkMode, string referenceMode)
    {
        string referenceFilePath = Helpers.GetResultsFileName(filepath, referenceMode);
        string resultsFilePath = Helpers.GetResultsFileName(filepath, checkMode);

        HashSet<string> reference = new(File.ReadAllLines(referenceFilePath));
        HashSet<string> results = new(File.ReadAllLines(resultsFilePath));

        if (reference.SetEquals(results))
        {
            Logger.Log("Reference match");
        }
        else
        {
            Logger.Log("Reference MISMATCH");
            foreach (string r in reference.Except(results).Union(results.Except(reference)))
            {
                Logger.Log($"- {r}");
            }
        }
    }
}