namespace CrudMaster.Models
{
    public class SuppliersVm
    {
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string Mobile { get; set; }
        public string Address { get; set; }
        public bool Active { get; set; }
        public string CmpyId { get; set; }
        public string BranchId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
    }
}
