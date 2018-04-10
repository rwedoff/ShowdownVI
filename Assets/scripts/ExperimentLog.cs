/// <summary>
/// Class that defines Log information that will be saved after each experiment
/// </summary>
public class ExperimentLogFile
{
    public string Tag { get; set; }
    public string Time { get; set; }
    public string Message { get; set; }

    public override string ToString()
    {
        return Time + "," + Tag + "," + Message;
    }
}

/// <summary>
/// Static class for logging experiment data throughout the program
/// </summary>
public static class ExperimentLog
{
    public static void Log(string message, string tag = "info", string time = null)
    {
        ExpManager.LogFileList.Add(new ExperimentLogFile()
        {
            Message = message,
            Tag = tag,
            Time = ExpManager.globalClockString
        });
    }
}

