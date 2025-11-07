using congestion.calculator;
using System.Linq;
using System;

public class CongestionTaxCalculator
{
    private readonly CityRule _rule;

    public CongestionTaxCalculator(CityRule rule)
    {
        _rule = rule;
    }

    public int GetTax(Vehicle vehicle, DateTime[] dates)
    {
        if (dates.Length == 0 || IsTollFreeVehicle(vehicle)) return 0;

        var sortedDates = dates.OrderBy(d => d).ToList();
        int totalFee = 0;

        DateTime windowStart = sortedDates[0];
        int windowMaxFee = GetTollFee(sortedDates[0].TimeOfDay);

        foreach (var date in sortedDates.Skip(1))
        {
            if (IsTollFreeDate(date)) continue;

            int fee = GetTollFee(date.TimeOfDay);
            double minutes = (date - windowStart).TotalMinutes;

            if (minutes <= 60)
            {
                windowMaxFee = Math.Max(windowMaxFee, fee);
            }
            else
            {
                totalFee += windowMaxFee;
                windowStart = date;
                windowMaxFee = fee;
            }
        }

        totalFee += windowMaxFee;
        return Math.Min(totalFee, _rule.MaxDailyFee);
    }

    private bool IsTollFreeVehicle(Vehicle vehicle)
    {
        if (vehicle == null) return false;
        return _rule.ExemptVehicles.Contains(vehicle.GetVehicleType());
    }

    private bool IsTollFreeDate(DateTime date)
    {
        if (date.Month == 7) return true;
        if (_rule.Holidays.Any(d => d.Date == date.Date)) return true;

        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }

    private int GetTollFee(TimeSpan time)
    {
        foreach (var th in _rule.TaxHours)
        {
            if (th.From <= th.To)
            {
                if (time >= th.From && time <= th.To) return th.Amount;
            }
            else 
            {
                if (time >= th.From || time <= th.To) return th.Amount;
            }
        }
        return 0;
    }
}
