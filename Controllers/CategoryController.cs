using CRUD_Operation.Classes;
using CrudMaster.Classes;
using EmployeeRegistraion.Models;
using Microsoft.AspNetCore.Mvc;


namespace Master.Controllers
{
    public class CategoryController : Controller
    {
        private readonly MainRepository _repo;

        public CategoryController(MainRepository repo)
        {
            _repo = repo;
        }


        public IActionResult Categories()
        {
            return View();
        }

        public IActionResult SubCategories()
        {
            return View();
        }


        // ---------------- SAVE CATEGORY ------------------
        [HttpPost]
        public IActionResult SaveCategory(CategoryVm model)
        {
            _repo.Savecategory(model);
            return Ok();
        }

        // ---------------- SAVE SUBCATEGORY ------------------
        [HttpPost]
        public IActionResult SaveSubCategory(SubCategoryVm model)
        {
            if (string.IsNullOrEmpty(model.SubCategoryId))
                model.SubCategoryId = Guid.NewGuid().ToString("N").Substring(0, 8);

            _repo.SaveSubCategory(model);
            return Ok();
        }

        // ---------------- GET ALL CATEGORIES ------------------
        [HttpGet]
        public IActionResult GetCategories()
        {
            var list = _repo.GetCategories();
            return Json(list);
        }

        // ---------------- GET CATEGORY BY ID ------------------
        [HttpGet]
        public IActionResult GetCategoryById(string id)
        {
            var data = _repo.GetCategoryById(id);
            return Json(data);
        }

        // ---------------- GET SUBCATEGORY BY ID ------------------
        [HttpGet]
        public IActionResult GetSubCategoryById(string id)
        {
            var data = _repo.GetSubCategoryById(id);
            return Json(data);
        }

        // ---------------- GET ALL SUBCATEGORIES ------------------
        [HttpGet]
        public IActionResult GetSubCategories()
        {
            var list = _repo.GetSubCategories();
            return Json(list);
        }

        // ---------------- UPDATE CATEGORY ------------------
        [HttpPost]
        public IActionResult UpdateCategory(CategoryVm model)
        {
            _repo.UpdateCategory(model);
            return Ok();
        }

        // ---------------- UPDATE SUBCATEGORY ------------------
        [HttpPost]
        public IActionResult UpdateSubCategory(SubCategoryVm model)
        {
            _repo.UpdateSubCategory(model);
            return Ok();
        }

        // ---------------- DELETE CATEGORY ------------------
        [HttpPost]
        public IActionResult DeleteCategory(string id)
        {
            _repo.DeleteCategory(id);
            return Ok();
        }

        // ---------------- DELETE SUBCATEGORY ------------------
        [HttpPost]
        public IActionResult DeleteSubCategory(string id)
        {
            _repo.DeleteSubCategory(id);
            return Ok();
        }
    }
}