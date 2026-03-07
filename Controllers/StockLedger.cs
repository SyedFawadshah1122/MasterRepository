
namespace YourNamespace.Controllers
{
    internal class StockLedger
    {
        public object PurchaseId { get; set; }
        public object ProductId { get; set; }
        public object QtyOut { get; set; }
        public string CmpyId { get; set; }
        public string BranchId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
    }
}