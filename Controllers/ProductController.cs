using CRUD_Operation.Classes;
using CrudMaster.Classes;
using CrudMaster.Models;
using EmployeeRegistraion.Models;
using Microsoft.AspNetCore.Mvc;

namespace CRUD_Operation.Controllers
{
    public class ProductController : Controller
    {
        private readonly MainRepository _repo;

        public ProductController(MainRepository repo)
        {
            _repo = repo;
        }

        // GET: Show product page
        public IActionResult Products()
        {
            return View();
        }

        // GET: All products for DataTable
        [HttpGet]
        public IActionResult GetProducts()
        {
            var list = _repo.GetProducts();
            return Json(list);
        }

        // GET: Subcategories dropdown based on category
        [HttpGet]
        public IActionResult GetSubCategoriesByCategory(string categoryId)
        {
            if (string.IsNullOrWhiteSpace(categoryId))
                return Json(new List<object>());

            var list = _repo.GetSubCategoriesByCategoryId(categoryId);
            return Json(list);
        }

        // GET: Product by ID (for Edit)
        [HttpGet]
        public IActionResult GetProductById(string productId)
        {
            var product = _repo.GetProductById(productId);
            if (product == null) return NotFound();
            return Json(product);
        }

        // POST: Save or Update product
        [HttpPost]
        public IActionResult SaveProducts([FromBody] ProductsVm model)  // <--- add [FromBody]
        {
            if (model == null || string.IsNullOrWhiteSpace(model.ProductName))
                return BadRequest(new { success = false, message = "Invalid product data" });

            try
            {
                var existing = _repo.GetProductById(model.ProductId);

                if (existing == null)
                {
                    _repo.SaveProducts(model);
                    return Ok(new { success = true, message = "Product saved successfully" });
                }
                else
                {
                    _repo.UpdateProduct(model);  // ye method aapke repository me hona chahiye
                    return Ok(new { success = true, message = "Product updated successfully" });
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error: " + ex.Message });
            }
        }

        // POST: Delete product
        [HttpPost]
        public IActionResult DeleteProduct([FromBody] string productId)
        {
            try
            {
                var existing = _repo.GetProductById(productId);
                if (existing == null)
                    return NotFound(new { success = false, message = "Product not found" });

                _repo.DeleteProduct(productId);
                return Ok(new { success = true, message = "Product deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}
