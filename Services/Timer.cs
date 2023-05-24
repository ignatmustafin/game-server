using System.Timers;

namespace GameServer.Services;

public class TimerData
{
    public DateTime StartTime { get; set; }
    public System.Timers.Timer Timer { get; set; }
}
public class Timer
{
    private static Dictionary<Guid, TimerData> _timers = new Dictionary<Guid, TimerData>();
    public static Dictionary<Guid, TimerData> GetTimers
    {
        get { return _timers; }
    }
    
    public static void StartTimer(Guid timerId, double interval, ElapsedEventHandler elapsed)
    {
        if (!_timers.ContainsKey(timerId))
        {
            var timerData = new TimerData
            {
                StartTime = DateTime.Now,
                Timer = new System.Timers.Timer(interval)
            };
            _timers.Add(timerId, timerData);
            timerData.Timer.Elapsed += elapsed;
            timerData.Timer.Start();
        }
    }

    public static void StopTimer(Guid timerId)
    {
        if (_timers.ContainsKey(timerId))
        {
            _timers[timerId].Timer.Stop();
            _timers.Remove(timerId);
        }
    }
}