using System;

namespace congestion.calculator;

public class TaxHour
{
    public TimeSpan From { get; set; }
    public TimeSpan To { get; set; }
    public int Amount { get; set; }
}
