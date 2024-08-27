# Billion Row Challenge

## Test Files

All test measurement files were generated using the Python script available [here](https://github.com/gunnarmorling/1brc/blob/main/src/main/python/create_measurements.py).  This GitHub repository was cloned and the script was run locally.

## Reference Implementation

This naive implementation performs the following:
- Read entire file into memory as a `List<string>`
- Iterates each line
- Splits line on `;`
- Populates `Dictionary<string, List<decimal>>` with station measurements
- Calculates statistics using `List<T>.Min()`, `List<T>.Average()`, and `List<T>.Max()`
- Writes results

### Performance

The naive implementation was run against a file containing 100M measurements, one order of magnitude away from the target 1B.  The program ran for a total of 40 seconds, as demonstrated by the following console output:

```
user@localhost       BillionRowChallenge % dotnet run data/measurements-100m.txt --reference
2024-08-26 10:29:04Z Reading file data/measurements-100m.txt
2024-08-26 10:29:04Z Running reference implementation
2024-08-26 10:29:04Z File size (bytes): 1,586,325,022
2024-08-26 10:29:04Z Reading file...
2024-08-26 10:29:43Z Calculating statistics...
2024-08-26 10:29:44Z Done
```

Execution for the full 1B record file did not finish within 1 hour, so I stopped it.

Time: ?  
Memory: ~30GB

## Fast Synchronous Implementation

A dramatic speed-up was discovered by realizing that we don't actually need to load the file.  Instead we can open a `FileStream` and read byte-by-byte.  This along with a consolidated method of capturing station statistics during the initial read of the file allowed a runtime of just under 4 minutes against the full file of 1B measurements.

This implementation performs the following:
- Open a `FileStream` and seek to the beginning offset
- Initialize a `byte` buffer of size `128` (based on max station name size of 100 bytes)
- Loop while bytes are read from the `FileStream`
- If a non-newline character is read, add that character to the byte buffer
- If a newline character is read, convert byte buffer to UTF8 string
- Split line on `;`
- If station not yet initialize, set `Dictionary<string, StationMeasurement>` with the parsed value
- Otherwise incorporate current value into `StationMeasurement` statistics
- Write results

### Performance

```
user@localhost       BillionRowChallenge % dotnet run data/measurements-1b.txt --sync            
2024-08-26 20:37:27Z Reading file data/measurements-1b.txt
2024-08-26 20:37:27Z File size (bytes): 15,893,796,027
2024-08-26 20:37:27Z Opening random access file stream
2024-08-26 20:37:27Z Parsing station measurements...
2024-08-26 20:41:24Z Calculating statistics...
2024-08-26 20:41:24Z Done
```

Time: 3m 57s  
Memory: ~32.5 MB

## Fast Asynchronous Implementation

This final approach enables parallelism by slicing the input file into chunks, each of which will be aggregated by a "worker" `Task`.  This is an enhancement to the "Fast Synchronous Implementation" noted above with the following differences:
- The "Fast Synchronous Implementation" logic is executed on some smaller segment of the measurements file
- The results from the worker `Tasks` are aggregated at the end

### Performance

```
user@localhost       BillionRowChallenge % dotnet run data/measurements-1b.txt --async --segments 10
2024-08-27 08:14:37Z Reading file data/measurements-1b.txt
2024-08-27 08:14:37Z File size (bytes): 15,893,796,027
2024-08-27 08:14:37Z Queueing segment [           0 ->  1589379613 ]
2024-08-27 08:14:37Z Queueing segment [  1589379614 ->  3178759223 ]
2024-08-27 08:14:37Z Queueing segment [  3178759224 ->  4768138835 ]
2024-08-27 08:14:37Z Queueing segment [  4768138836 ->  6357518450 ]
2024-08-27 08:14:37Z Queueing segment [  6357518451 ->  7946898062 ]
2024-08-27 08:14:37Z Queueing segment [  7946898063 ->  9536277682 ]
2024-08-27 08:14:37Z Queueing segment [  9536277683 -> 11125657297 ]
2024-08-27 08:14:37Z Queueing segment [ 11125657298 -> 12715036912 ]
2024-08-27 08:14:37Z Queueing segment [ 12715036913 -> 14304416517 ]
2024-08-27 08:14:37Z Queueing segment [ 14304416518 -> 15893796027 ]
2024-08-27 08:14:37Z Awaiting 10 tasks
2024-08-27 08:15:44Z Merging results
2024-08-27 08:15:44Z Done
```

Time: 1m 7s  
Memory: 50.8MB

> We reach diminishing returns past 10 segments.  The maximum segment count tested was 100, but this improved performance by only 3 seconds while increasing memory usage 3x.