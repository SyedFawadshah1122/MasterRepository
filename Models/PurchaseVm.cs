namespace Master.Models
{
    public class PurchaseVm
    {
        public string PurchaseId { get; set; }

        public DateTime PurchaseDate { get; set; }
        public string SupplierId { get; set; }
        public List<PurchaseDetailVm> Details { get; set; }
    }

    public class Purchase
    {
        public int PurchaseId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Qty { get; set; }
        public decimal CostPrice { get; set; }
        public decimal Amount { get; set; }
    }
    public class PurchaseDetailVm
    {
        public string ProductId { get; set; }
        public decimal Qty { get; set; }
        public decimal CostPrice { get; set; }
        public decimal Amount { get; set; }
        public string Barcode { get; set; }
    }

    public class PurchasePrintVm
    {
        public int PurchaseId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string SupplierName { get; set; }
        public string ProductName { get; set; }
        public decimal Qty { get; set; }
        public decimal CostPrice { get; set; }
        public decimal Amount { get; set; }
        public string Barcode { get; set; }
    }

}