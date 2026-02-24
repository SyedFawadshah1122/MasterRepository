using CrudMaster.Classes;
using CrudMaster.Models;
using Microsoft.AspNetCore.Mvc;

namespace CrudMaster.Controllers
{
    public class SupplierController : Controller
    {
        private readonly MainRepository _repo;

        public SupplierController(MainRepository repo)
        {
            _repo = repo;
        }

        // ============================================================
        // PAGE LOAD
        // ============================================================
        public IActionResult Suppliers()
        {
            return View();
        }

        // ============================================================
        // SAVE SUPPLIER
        // ============================================================
        [HttpPost]
        public IActionResult SaveSupplier(SuppliersVm model)
        {
            model.CmpyId = "CMPY-001";
            model.BranchId = "BR-001";

            model.CreatedBy = "Admin01";
            model.CreatedOn = DateTime.Now;

            model.ModifiedBy = "Admin01";
            model.ModifiedOn = DateTime.Now;

            _repo.SaveSupplier(model);
            return Ok();
        }

        // ============================================================
        // GET ALL SUPPLIERS
        // ============================================================
        [HttpGet]
        public IActionResult GetSuppliers()
        {
            var list = _repo.GetSuppliers();
            return Json(list);
        }

        // ============================================================
        // GET SUPPLIER BY ID
        // ============================================================
        [HttpGet]
        public IActionResult GetSupplierById(string id)
        {
            var data = _repo.GetSupplierById(id);
            return Json(data);
        }

        // ============================================================
        // UPDATE SUPPLIER
        // ============================================================
        [HttpPost]
        public IActionResult UpdateSupplier(SuppliersVm model)
        {
            model.ModifiedBy = "Admin01";
            model.ModifiedOn = DateTime.Now;

            _repo.UpdateSupplier(model);
            return Ok();
        }

        // ============================================================
        // DELETE SUPPLIER
        // ============================================================
        [HttpPost]
        public IActionResult DeleteSupplier(string id)
        {
            _repo.DeleteSupplier(id);
            return Ok();
        }
    }
}