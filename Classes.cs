namespace BillionRowChallenge;

public class StationMeasurement
{
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public decimal Sum { get; set; }
    public int Count { get; set; } = 1;

    public decimal Avg => Count > 0 ? Sum / Count : 0;

    public StationMeasurement(decimal value)
    {
        Min = value;
        Max = value;
        Sum = value;
        Count = 1;
    }

    public StationMeasurement(decimal min, decimal max, decimal sum, int count)
    {
        Min = min;
        Max = max;
        Sum = sum;
        Count = count;
    }
}