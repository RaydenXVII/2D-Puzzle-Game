using System.Globalization;

// Menyimpan data permainan yang harus bertahan saat scene dimuat ulang atau berpindah stage
public static class GameSession
{
    public static float ElapsedTime;
    public static int Deaths;
    public static bool Running;
    public static bool Finished;

    public static void Reset()
    {
        ElapsedTime = 0f;
        Deaths = 0;
        Running = false;
        Finished = false;
    }

    public static void StartIfNeeded()
    {
        if (!Finished)
            Running = true;
    }

    public static void Finish()
    {
        Running = false;
        Finished = true;
    }

    public static string FormatTime(float t)
    {
        int minutes = (int)(t / 60f);
        float seconds = t - minutes * 60f;
        return minutes.ToString("00") + ":" + seconds.ToString("00.0", CultureInfo.InvariantCulture);
    }
}