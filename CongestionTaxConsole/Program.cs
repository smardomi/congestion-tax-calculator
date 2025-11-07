using congestion.calculator;
using System.Text.Json;

Console.WriteLine("Hello, World!");


var jsonString = File.ReadAllText("city_rules.json");
var jsonDoc = JsonDocument.Parse(jsonString);
var gothenburg = jsonDoc.RootElement.GetProperty("Gothenburg");

var taxHours = new List<TaxHour>();
foreach (var th in gothenburg.GetProperty("tax_hours").EnumerateArray())
{
    taxHours.Add(new TaxHour
    {
        From = TimeSpan.Parse(th.GetProperty("from").GetString()),
        To = TimeSpan.Parse(th.GetProperty("to").GetString()),
        Amount = th.GetProperty("amount").GetInt32()
    });
}

var holidays = new List<DateTime>();
foreach (var h in gothenburg.GetProperty("holidays").EnumerateArray())
{
    holidays.Add(DateTime.Parse(h.GetString()));
}

var rule = new CityRule
{
    TaxHours = taxHours,
    Holidays = holidays,
    MaxDailyFee = gothenburg.GetProperty("max_daily_fee").GetInt32(),
    ExemptVehicles = new List<string>()
};
foreach (var v in gothenburg.GetProperty("exempt_vehicles").EnumerateArray())
{
    rule.ExemptVehicles.Add(v.GetString());
}

var calculator = new CongestionTaxCalculator(rule);
var car = new Car();

DateTime[] dates = new DateTime[]
{
            DateTime.Parse("2013-02-07 06:23:27"),
            DateTime.Parse("2013-02-07 15:27:00"),
            DateTime.Parse("2013-02-08 06:27:00"),
            DateTime.Parse("2013-02-08 06:20:27"),
            DateTime.Parse("2013-02-08 14:35:00"),
            DateTime.Parse("2013-02-08 15:29:00"),
            DateTime.Parse("2013-02-08 15:47:00"),
            DateTime.Parse("2013-02-08 16:01:00"),
            DateTime.Parse("2013-02-08 16:48:00")
};

var datesByDay = new Dictionary<DateTime, List<DateTime>>();
foreach (var dt in dates)
{
    var day = dt.Date;
    if (!datesByDay.ContainsKey(day))
        datesByDay[day] = new List<DateTime>();
    datesByDay[day].Add(dt);
}

foreach (var day in datesByDay.Keys)
{
    int tax = calculator.GetTax(car, datesByDay[day].ToArray());
    Console.WriteLine($"Date: {day.ToShortDateString()}, Tax: {tax} SEK");
}