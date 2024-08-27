namespace BillionRowChallenge;

public static class Reference
{
    public static void Calculate(string filepath, string mode)
    {
        FileInfo fileInfo = new(filepath);
        Logger.Log($"File size (bytes): {fileInfo.Length:N0}");

        Dictionary<string, List<decimal>> measurements = [];

        Logger.Log("Reading file...");
        string[] lines = File.ReadAllLines(filepath);

        Logger.Log("Parsing station measurements...");
        foreach (string line in lines)
        {
            string[] fields = line.Split(';');
            if (!measurements.TryGetValue(fields[0], out List<decimal>? value)) 
            {
                value = ([]);
                measurements.Add(fields[0], value);
            }

            value.Add(decimal.Parse(fields[1]));
        }

        Logger.Log("Calculating statistics...");
        FileStream output = File.Open($"{Helpers.GetResultsFileName(filepath, mode)}", FileMode.Create);
        using StreamWriter writer = new(output);

        foreach (string station in measurements.Keys)
        {
            List<decimal> m = measurements[station];
            writer.WriteLine($"{station};{m.Min()};{m.Average():F1};{m.Max()}");
        }

        Logger.Log("Done");
    }
}