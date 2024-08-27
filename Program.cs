using BillionRowChallenge;

string mode = string.Empty;
bool verify = false;
int numSegments = 4;
string filepath = args[0];

int i = 0;
while (i < args.Length)
{
    if (args[i] == "--reference")
    {
        mode = "reference";
    }
    else if (args[i] == "--async")
    {
        mode = "fast-async";
    }
    else if (args[i] == "--sync")
    {
        mode = "fast-sync";
    }
    else if (args[i] == "--verify")
    {
        verify = true;
    }
    else if (args[i] == "--segments")
    {
        i++;
        numSegments = int.Parse(args[i]);
    }
    i++;
}

Logger.Log($"Reading file {filepath}");

HashSet<string> modes = ["reference", "fast-sync", "fast-async"];
if (!modes.Contains(mode))
{
    Console.WriteLine("Please specify an option (--reference, --sync, or --async)");
    return;
}

switch (mode)
{
    case "fast-sync":
        if (verify)
        {
            Verifier.Verify(filepath, mode, "reference");
        }
        else
        {
            FastSync.Calculate(filepath, mode);
        }
        break;
    case "fast-async":
        if (verify)
        {
            Verifier.Verify(filepath, mode, "fast-sync");
        }
        else
        {
            await FastAsync.Calculate(filepath, mode, numSegments);
        }
        break;
    case "reference": 
        Logger.Log("Running reference implementation");
        Reference.Calculate(filepath, mode);
        break;
    default:
        break;
}