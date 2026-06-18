namespace CuoiKy.ViewModels;

public class AdminRevenueViewModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string FilterType { get; set; } = "day";
    public int SelectedYear { get; set; }

    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int PaidOrders { get; set; }

    public List<RevenueByDay> RevenueByDays { get; set; } = new();
    public List<RevenueGroupedItem> RevenueGroupedItems { get; set; } = new();
    public List<TopProductRevenue> TopProducts { get; set; } = new();
}

public class RevenueGroupedItem
{
    public string Label { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
}

public class RevenueByDay
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
}

public class TopProductRevenue
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Revenue { get; set; }
}

