using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Globalization;
using System.Threading.Tasks;

public class TimerHelper : MonoBehaviour
{
    public Action<DateTime> CronForEveryOneMin;

    public DateTime LastUtcTime { get; private set; }
    public DateTime LastIstTime { get; private set; }
    public static TimerHelper instance;
    private bool isFetching = false;
    public static TimeSpan bufferTime;
    private float checkInterval = .8f; // check twice per second

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        Application.runInBackground = true; // try to allow background running
        InvokeRepeating(nameof(CheckCron), 0f, checkInterval);
        UpdateCurrentUtcTime();
    }

    /// <summary>
    /// Checks if the current second is 0, then triggers cron
    /// </summary>
    private async void CheckCron()
    {
        if (isFetching) return;

        DateTime utcNow = GetCurrentAdjustedUtc();
        LastUtcTime = utcNow;
        LastIstTime = ConvertUtcToIst(utcNow);

        if (utcNow.Second == 4)
        {
            // Run your cron task at 0th second
            RunCronTask();
            await UpdateCurrentUtcTime();
        }
    }

    private void RunCronTask()
    {
        CronForEveryOneMin?.Invoke(GetCurrentAdjustedUtc());
        // TODO: Add your logic here
    }

    /// <summary>
    /// Get online UTC time from WorldTimeAPI
    /// </summary>
    ///
    public static DateTime GetCurrentAdjustedUtc()
    {
        return DateTime.UtcNow + bufferTime;
    }

    private async Task<DateTime> UpdateCurrentUtcTime()
    {
        isFetching = true;
        DateTime utcTime = DateTime.UtcNow; // fallback
        bool isNeedToUpdate = true;
        int updateCount = 0;
        while (isNeedToUpdate)
        {

            using (UnityWebRequest www = UnityWebRequest.Get("https://worldtimeapi.org/api/timezone/Etc/UTC"))
            {
                var operation = www.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string json = www.downloadHandler.text;
                    string key = "\"datetime\":\"";
                    int startIndex = json.IndexOf(key) + key.Length;
                    int endIndex = json.IndexOf("\"", startIndex);
                    string datetimeStr = json.Substring(startIndex, endIndex - startIndex);

                    utcTime = DateTime.Parse(
                        datetimeStr,
                        null,
                        System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal
                    );
                }
            }
            bufferTime = utcTime - DateTime.UtcNow;
            updateCount++;
            isNeedToUpdate = bufferTime > TimeSpan.FromMinutes(10) && updateCount <= 3;
        }

        isFetching = false;
        return utcTime;
    }

    /// <summary>
    /// Convert UTC to IST safely
    /// </summary>
    public static DateTime ConvertUtcToIst(DateTime utcTime)
    {
        try
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
#else
            TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
#endif
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, istZone);
        }
        catch
        {
            return utcTime.AddHours(5).AddMinutes(30);
        }
    }
    public static DateTime UtcStringToDateTimeIst(string utcString)
    {
        if (string.IsNullOrEmpty(utcString))
            throw new ArgumentException("UTC string cannot be null or empty.");

        // Parse UTC string
        DateTime utcTime = DateTime.Parse(utcString, null, DateTimeStyles.RoundtripKind);

        // Ensure Kind is Utc
        if (utcTime.Kind != DateTimeKind.Utc)
            utcTime = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);


        return ConvertUtcToIst(utcTime);
    }
    public static DateTime UtcStringToDateTimeUtc(string utcString)
    {
        if (string.IsNullOrEmpty(utcString))
            throw new ArgumentException("UTC string cannot be null or empty.");

        // Parse UTC string
        DateTime utcTime = DateTime.Parse(utcString, null, DateTimeStyles.RoundtripKind);

        // Ensure Kind is Utc
        if (utcTime.Kind != DateTimeKind.Utc)
            utcTime = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);


        return utcTime;
    }

}
