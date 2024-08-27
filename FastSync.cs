using System.Text;

namespace BillionRowChallenge;

public static class FastSync
{
    public static void Calculate(string filepath, string mode)
    {
        FileInfo fileInfo = new(filepath);
        Logger.Log($"File size (bytes): {fileInfo.Length:N0}");

        Dictionary<string, StationMeasurement> measurements = [];

        Logger.Log("Opening random access file stream");
        FileStream fileStream = new(filepath, FileMode.Open);
        fileStream.Seek(0, SeekOrigin.Begin);

        Logger.Log("Parsing station measurements...");

        byte[] buffer = new byte[128];
        int bufferlen = 0;

        int _byte = fileStream.ReadByte();
        while (_byte >= 0)
        {
            char c = (char)_byte;
            if (c == '\n')
            {
                string line = Encoding.UTF8.GetString(buffer, 0, bufferlen);
                string[] fields  = line.ToString().Split(';');
                string station = fields[0];
                decimal value = decimal.Parse(fields[1]);
                
                if (!measurements.TryGetValue(fields[0], out StationMeasurement? measurement))
                {
                    measurement = new(value);
                    measurements.Add(station, measurement);
                }
                else
                {
                    if (value < measurement.Min)
                    {
                        measurement.Min = value;
                    }
                    if (value > measurement.Max)
                    {
                        measurement.Max = value;
                    }
                    measurement.Sum += value;
                    measurement.Count += 1;
                }
                
                bufferlen = 0;
            }
            else
            {
                buffer[bufferlen++] = (byte)_byte;
            }

            _byte = fileStream.ReadByte();
        }

        fileStream.Close();

        Logger.Log("Calculating statistics...");
        FileStream output = File.Open($"{Helpers.GetResultsFileName(filepath, mode)}", FileMode.Create);
        using StreamWriter writer = new(output);

        foreach (string station in measurements.Keys)
        {
            writer.WriteLine($"{station};{measurements[station].Min};{measurements[station].Avg:F1};{measurements[station].Max}");
        }

        Logger.Log("Done");
    }
}