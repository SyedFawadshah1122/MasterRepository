public class ProductsVm
{
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public string CategoryId { get; set; }
    public string SubCategoryId { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal StockQty { get; set; }

    public string Barcode { get; set; }   // ✅ ADD THIS

    public bool IsActive { get; set; }
    public string CmpyId { get; set; }
    public string BranchId { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string ModifiedBy { get; set; }
}