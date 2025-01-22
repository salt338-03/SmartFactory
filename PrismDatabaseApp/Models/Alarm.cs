using System;

public class Alarm
{
    public int Id { get; set; }
    public string Message { get; set; }
    public string AlarmCode { get; set; }
    public double Value { get; set; }
    public DateTime Timestamp { get; set; } // DateTime 형식으로 변경
}
