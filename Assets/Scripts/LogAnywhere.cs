using UnityEngine;

public class LogAnywhere : MonoBehaviour
{
    string filename = "";

    void OnEnable()
    {
        Application.logMessageReceived += Log;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= Log;
    }

    public void Log(string logString, string stackTrace, LogType type)
    {
        if (filename == "")
        {
            // Set the log directory to your Assets folder
            string d = "D:/Unity/Bronze Age Romance/Game Jam Kalev/Game Jam Kalev/Assets/Logs";
            System.IO.Directory.CreateDirectory(d);
            filename = d + "/game_debug_log.txt";
        }

        try
        {
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logEntry = $"[{timestamp}] [{type}] {logString}\n";
            System.IO.File.AppendAllText(filename, logEntry);
        }
        catch
        {
            // Silently fail if we can't write to file
        }
    }
}
