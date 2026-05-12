using MobileStore.Models;

namespace MobileStore.ViewModels.Admin
{
    public class DashboardViewModel
    {
        public int     TotalOrders     { get; set; }
        public int     PendingOrders   { get; set; }
        public decimal TotalRevenue    { get; set; }
        public decimal MonthRevenue    { get; set; }
        public int     TotalPhones     { get; set; }
        public int     LowStockCount   { get; set; }
        public int     TotalUsers      { get; set; }

        public IEnumerable<Order>            RecentOrders  { get; set; } = new List<Order>();
        public IEnumerable<TopPhoneItem>     TopPhones     { get; set; } = new List<TopPhoneItem>();
        public IEnumerable<MonthlySalesItem> MonthlySales  { get; set; } = new List<MonthlySalesItem>();
    }

    public class TopPhoneItem
    {
        public Phone  Phone        { get; set; } = null!;
        public int    TotalSold    { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class MonthlySalesItem
    {
        public string  Month   { get; set; } = "";
        public decimal Revenue { get; set; }
        public int     Orders  { get; set; }
    }
}
