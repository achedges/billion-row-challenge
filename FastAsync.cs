using System.Text;

namespace BillionRowChallenge;

public static class FastAsync
{
    public static async Task Calculate(string filepath, string mode, int numSegments)
    {
        FileInfo fileInfo = new(filepath);
        Logger.Log($"File size (bytes): {fileInfo.Length:N0}");

        FileStream fileStream = new(filepath, FileMode.Open);

        long segmentSize = fileInfo.Length / numSegments;

        List<Tuple<long, long>> segments = [];
        long offset = 0;
        while (offset < fileInfo.Length)
        {
            long endOffset = offset + segmentSize;
            if (endOffset > fileInfo.Length)
            {
                endOffset = fileInfo.Length;
            }
            else
            {
                fileStream.Seek(endOffset, SeekOrigin.Begin);

                int rb = fileStream.ReadByte();
                while (rb >= 0)
                {
                    if (rb == -1)
                    {
                        endOffset = fileInfo.Length - 1;
                        break;
                    }

                    if ((char)rb == '\n')
                    {
                        break;
                    }

                    endOffset++;
                    rb = fileStream.ReadByte();
                }
            }

            segments.Add(new(offset, endOffset));
            offset = endOffset + 1;
        }

        fileStream.Close();

        List<Task<Dictionary<string, StationMeasurement>>> measurementTasks = [];
        foreach (Tuple<long, long> segment in segments)
        {
            Logger.Log($"Queueing segment [ {segment.Item1,11} -> {segment.Item2,11} ]");
            measurementTasks.Add(Task.Run(() => CalculateChunk(filepath, segment.Item1, segment.Item2)));
        }

        Logger.Log($"Awaiting {measurementTasks.Count} tasks");
        await Task.WhenAll(measurementTasks);

        Logger.Log("Merging results");
        Dictionary<string, StationMeasurement> mergedResults = [];
        foreach (Dictionary<string, StationMeasurement> taskResults in measurementTasks.Select(t => t.Result))
        {
            foreach (string station in taskResults.Keys)
            {
                StationMeasurement sm = taskResults[station];
                if (!mergedResults.TryGetValue(station, out StationMeasurement? mergedMeasurement))
                {
                    mergedMeasurement = new(sm.Min, sm.Max, sm.Sum, sm.Count);
                    mergedResults.Add(station, mergedMeasurement);
                }
                else
                {
                    if (sm.Min < mergedMeasurement.Min)
                    {
                        mergedMeasurement.Min = sm.Min;
                    }
                    if (sm.Max > mergedMeasurement.Max)
                    {
                        mergedMeasurement.Max = sm.Max;
                    }
                    mergedMeasurement.Sum += sm.Sum;
                    mergedMeasurement.Count += sm.Count;
                }
            }
        }

        FileStream output = File.Open($"{Helpers.GetResultsFileName(filepath, mode)}", FileMode.Create);
        using StreamWriter writer = new(output);

        foreach (string station in mergedResults.Keys)
        {
            writer.WriteLine($"{station};{mergedResults[station].Min};{mergedResults[station].Avg:F1};{mergedResults[station].Max}");
        }

        Logger.Log("Done");
    }

    public static Dictionary<string, StationMeasurement> CalculateChunk(string filepath, long startOffset, long endOffset)
    {
        Dictionary<string, StationMeasurement> measurements = [];

        long pos = startOffset;
        FileStream fileStream = new(filepath, FileMode.Open);
        fileStream.Seek(pos, SeekOrigin.Begin);

        byte[] buffer = new byte[128];
        int bufferlen = 0;

        int _byte = fileStream.ReadByte();
        while (_byte >= 0 && pos <= endOffset)
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
            pos++;
        }

        fileStream.Close();

        return measurements;
    }
}