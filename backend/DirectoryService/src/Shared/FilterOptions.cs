namespace Shared;

public sealed class FilterOptions
{
    public DateTime ThresholdDate { get; private set; }
    
    private FilterOptions(DateTime thresholdDate)
    {
        ThresholdDate = thresholdDate;
    }
    
    public static FilterOptions FromDaysAgo(int days)
    {
        return new FilterOptions(DateTime.UtcNow.AddDays(-days));
    }
    
    public static FilterOptions FromMonthsAgo(int months)
    {
        return new FilterOptions(DateTime.UtcNow.AddMonths(-months));
    }
}