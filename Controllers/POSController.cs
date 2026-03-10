using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;

namespace YourNamespace.Controllers
{
    public class POSController : Controller
    {
        private readonly IConfiguration _config;

        public POSController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult PosIndex()
        {
            return View();
        }
        // Get Customers
        [HttpGet]
        public IActionResult GetCustomers()
        {
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var customers = con.Query("SELECT CustomerId, CustomerName FROM Customers WHERE IsActive=1").ToList();
            return Json(customers);
        }

        // Get Products
        [HttpGet]
        public IActionResult GetProducts()
        {
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var products = con.Query("SELECT ProductId, ProductName, SellingPrice FROM Products WHERE IsActive=1").ToList();
            return Json(products);
        }

        
        // Existing GetProducts, GetCustomers actions assumed here

        [HttpGet]
        public IActionResult GetProductByBarcode(string barcode)
        {
            if (string.IsNullOrEmpty(barcode))
                return Json(null);

            using (var db = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                string query = @"SELECT TOP 1 ProductId, ProductName, SellingPrice, Barcode
                         FROM Products
                         WHERE Barcode = @Barcode";

                var product = db.QueryFirstOrDefault(query, new { Barcode = barcode });

                return Json(product);
            }
        }
        
        
        // Save Sale
        [HttpPost]
        public IActionResult AddSale([FromBody] SaleVM vm)
        {
            if (vm == null || vm.details == null || vm.details.Count == 0) return BadRequest("No products added");

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var dt = new DataTable();
            dt.Columns.Add("ProductId", typeof(string));
            dt.Columns.Add("Qty", typeof(decimal));
            dt.Columns.Add("Price", typeof(decimal));
            dt.Columns.Add("amount", typeof(decimal));

            foreach (var item in vm.details)
                dt.Rows.Add(item.productId, item.qty, item.price);

            var p = new DynamicParameters();
            p.Add("@CustomerId", vm.customerId);
            p.Add("@CmpyId", "CMP001"); // optional, set your default
            p.Add("@BranchId", "BR001"); // optional
            p.Add("@CreatedBy", "Admin"); // optional
            p.Add("@Products", dt.AsTableValuedParameter("dbo.SalesProductType"));

            con.Execute("sp_SaveSale", p, commandType: CommandType.StoredProcedure);

            return Ok("Sale completed successfully");
        }

        // Get all Sales for DataTable
        [HttpGet]
        public IActionResult GetSales()
        {
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var sales = con.Query(@"
        SELECT s.SaleId, p.ProductName AS productName, sd.Qty AS qty, sd.Price AS price,
               (sd.Qty*sd.Price) AS amount,
               (SELECT SUM(Qty*Price) FROM SalesDetail WHERE SaleId=s.SaleId) AS totalAmount
        FROM SalesHeader s
        INNER JOIN SalesDetail sd ON s.SaleId=sd.SaleId
        INNER JOIN Products p ON sd.ProductId=p.ProductId
    ").ToList();

            return Json(sales);
        }

        // Get single Sale by ID for edit/print
        [HttpGet]
        public IActionResult GetSaleById(string saleId)
        {
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

            var header = con.QueryFirstOrDefault(@"
                SELECT SaleId, CustomerId, SaleDate
                FROM SalesHeader WHERE SaleId=@SaleId", new { SaleId = saleId });

            var customer = con.QueryFirstOrDefault("SELECT CustomerName FROM Customers WHERE CustomerId=@CustomerId", new { CustomerId = header.CustomerId });

            var details = con.Query(@"
                SELECT sd.ProductId, p.ProductName, sd.Qty, sd.Price, (sd.Qty*sd.Price) AS Amount
                FROM SalesDetail sd
                INNER JOIN Products p ON sd.ProductId=p.ProductId
                WHERE sd.SaleId=@SaleId", new { SaleId = saleId }).ToList();

            var totalAmount = details.Sum(d => (decimal)d.Amount);

            return Json(new
            {
                saleId = header.SaleId,
                customerName = customer.CustomerName,
                saleDate = header.SaleDate,
                details,
                totalAmount
            });
        }

        // Sales Report Page
        public IActionResult SalesReport() => View();
    }

    // VM classes
    public class SaleVM
    {
        public string customerId { get; set; }
        public string saleDate { get; set; }
        public List<SaleDetailVM> details { get; set; }
    }

    public class SaleDetailVM
    {
        public string productId { get; set; }
        public decimal qty { get; set; }
        public decimal price { get; set; }
        public decimal amount { get; set; }
    }
}