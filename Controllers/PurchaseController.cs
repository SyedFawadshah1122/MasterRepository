using CrudMaster.Models;
using Dapper;
using Master.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;

namespace Master.Controllers
{
    public class PurchaseController : Controller
    {
        private readonly IConfiguration _configuration;

        public PurchaseController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

       
        // =========================
        // MAIN VIEW
        // =========================
        public IActionResult Purchases()
        {
            return View();
        }


        // =========================
        // SUPPLIERS
        // =========================
        [HttpGet]
        public IActionResult GetSuppliers()
        {
            var list = new List<object>();

            using SqlConnection con =
                new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            string query = "SELECT SupplierId, SupplierName FROM Suppliers";

            using SqlCommand cmd = new SqlCommand(query, con);
            con.Open();

            using SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new
                {
                    supplierId = dr["SupplierId"].ToString(),
                    supplierName = dr["SupplierName"].ToString()
                });
            }

            return Ok(list);
        }

        // =========================
        // PRODUCTS
        // =========================
        [HttpGet]
        public IActionResult GetProducts()
        {
            var list = new List<object>();

            using SqlConnection con =
                new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            string query = "SELECT ProductId, ProductName FROM Products";

            using SqlCommand cmd = new SqlCommand(query, con);
            con.Open();

            using SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new
                {
                    productId = dr["ProductId"].ToString(),
                    productName = dr["ProductName"].ToString()
                });
            }

            return Ok(list);
        }




        [HttpGet]
        public IActionResult GetProductByBarcode(string barcode)
        {
            using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var sql = "SELECT ProductId, ProductName, CostPrice FROM Products WHERE Barcode=@barcode";

                var product = con.QueryFirstOrDefault(sql, new { barcode });

                return Json(product);
            }
        }




        // =========================
        // ADD PURCHASE
        // =========================
        [HttpPost]
        public IActionResult AddPurchase([FromBody] PurchaseVm vm)
        {
            if (vm == null)
                return BadRequest("Request body missing");

            if (vm.Details == null || vm.Details.Count == 0)
                return BadRequest("Purchase detail missing");

            using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            using SqlCommand cmd = new SqlCommand("sp_PurchaseEntry_Multi", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Header params
            cmd.Parameters.AddWithValue("@PurchaseDate", vm.PurchaseDate);
            cmd.Parameters.AddWithValue("@CmpyId", "CMP-001");
            cmd.Parameters.AddWithValue("@BranchId", "BR-001");
            cmd.Parameters.AddWithValue("@CreatedOn", DateTime.Now);
            cmd.Parameters.AddWithValue("@CreatedBy", "admin");
            cmd.Parameters.AddWithValue("@SupplierId", vm.SupplierId);

            // TVP – Multiple products
            DataTable tvp = new DataTable();
            tvp.Columns.Add("ProductId", typeof(string));
            tvp.Columns.Add("Qty", typeof(decimal));
            tvp.Columns.Add("CostPrice", typeof(decimal));
            tvp.Columns.Add("Amount", typeof(decimal));
            tvp.Columns.Add("Barcode", typeof(string));
            tvp.Columns.Add("Active", typeof(bool));

            foreach (var d in vm.Details)
            {
                tvp.Rows.Add(d.ProductId, d.Qty, d.CostPrice, d.Amount, d.Barcode ?? "", true);
            }

            var param = cmd.Parameters.AddWithValue("@Products", tvp);
            param.SqlDbType = SqlDbType.Structured;
            param.TypeName = "dbo.PurchaseProductType"; // TVP ka exact type

            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();

            return Ok(new { message = "Purchase saved successfully" });
        }

        // =========================
        // LIST PURCHASES (DATATABLE)
        // =========================
        [HttpGet]
        public IActionResult GetPurchases()
        {
            var list = new List<object>();

            using SqlConnection con =
                new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            string query = @"
                SELECT 
                    PH.PurchaseId,
                    P.ProductName,
                    PD.Qty,
                    PD.CostPrice,
                    PD.Amount,
                    (SELECT SUM(Amount) FROM PurchaseDetail WHERE PurchaseId = PH.PurchaseId) AS TotalAmount
                FROM PurchaseHeader PH
                INNER JOIN PurchaseDetail PD ON PH.PurchaseId = PD.PurchaseId
                INNER JOIN Products P ON PD.ProductId = P.ProductId
                ORDER BY PH.PurchaseId DESC";

            using SqlCommand cmd = new SqlCommand(query, con);
            con.Open();

            using SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new
                {
                    purchaseId = dr["PurchaseId"].ToString(),
                    productName = dr["ProductName"].ToString(),
                    qty = Convert.ToDecimal(dr["Qty"]),
                    costPrice = Convert.ToDecimal(dr["CostPrice"]),
                    amount = Convert.ToDecimal(dr["Amount"]),
                    totalAmount = Convert.ToDecimal(dr["TotalAmount"])
                });
            }

            return Json(new { data = list });
        }

        // =========================
        // GET BY ID (EDIT / PRINT)
        // =========================
        [HttpGet]
        public IActionResult GetPurchaseById(string purchaseId)
        {
            using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            string query = @"
        SELECT 
            PH.PurchaseId,
            PH.PurchaseDate,
            PH.SupplierId,
            PD.ProductId,
            S.SupplierName,
            P.ProductName,
            PD.Qty,
            PD.CostPrice,
            PD.Amount,
            PD.Barcode
        FROM PurchaseHeader PH
        INNER JOIN PurchaseDetail PD ON PH.PurchaseId = PD.PurchaseId
        INNER JOIN Suppliers S ON PH.SupplierId = S.SupplierId
        INNER JOIN Products P ON PD.ProductId = P.ProductId
        WHERE PH.PurchaseId = @PurchaseId";

            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@PurchaseId", purchaseId);

            con.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            if (!dr.Read()) return NotFound();

            var result = new
            {
                purchaseId = dr["PurchaseId"].ToString(),
                purchaseDate = Convert.ToDateTime(dr["PurchaseDate"]),
                supplierId = dr["SupplierId"].ToString(),
                productId = dr["ProductId"].ToString(),
                supplierName = dr["SupplierName"].ToString(),
                productName = dr["ProductName"].ToString(),
                qty = Convert.ToDecimal(dr["Qty"]),
                costPrice = Convert.ToDecimal(dr["CostPrice"]),
                amount = Convert.ToDecimal(dr["Amount"]),
                barcode = dr["Barcode"].ToString()
            };

            con.Close();
            return Ok(result);
        }


        // =========================
        // UPDATE PURCHASE
        // =========================
        [HttpPost]
        public IActionResult UpdatePurchase([FromBody] PurchaseVm vm)
        {
            if (vm == null)
                return BadRequest("Request body missing");

            if (string.IsNullOrEmpty(value: vm.PurchaseId))
                return BadRequest("PurchaseId is required");


            if (vm.Details == null || vm.Details.Count == 0)
                return BadRequest("Purchase detail missing");

            var d = vm.Details.First();

            using SqlConnection con =
                new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            using SqlCommand cmd = new SqlCommand("sp_PurchaseUpdate", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@PurchaseId", SqlDbType.NVarChar, 50).Value = vm.PurchaseId;
            cmd.Parameters.Add("@PurchaseDate", SqlDbType.Date).Value = vm.PurchaseDate;
            cmd.Parameters.Add("@SupplierId", SqlDbType.NVarChar, 50).Value = vm.SupplierId;
            cmd.Parameters.Add("@ProductId", SqlDbType.NVarChar, 50).Value = d.ProductId;
            cmd.Parameters.Add("@Qty", SqlDbType.Decimal).Value = d.Qty;
            cmd.Parameters.Add("@CostPrice", SqlDbType.Decimal).Value = d.CostPrice;
            cmd.Parameters.Add("@Amount", SqlDbType.Decimal).Value = d.Amount;
            cmd.Parameters.Add("@Barcode", SqlDbType.NVarChar, 100).Value = d.Barcode ?? "";

            con.Open();
            int rows = cmd.ExecuteNonQuery();
            con.Close();

            if (rows > 0)
                return Ok(new { message = "Purchase updated successfully" });

            return BadRequest("Update failed");
        }

        // Delete---------------------------

        [HttpPost]
        public IActionResult DeletePurchase(string purchaseId)
        {
            if (string.IsNullOrEmpty(purchaseId))
                return BadRequest("PurchaseId is required");

            using SqlConnection con =
                new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            using SqlCommand cmd = new SqlCommand("sp_PurchaseDelete", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@PurchaseId", SqlDbType.NVarChar, 50).Value = purchaseId;

            try
            {
                con.Open();
                cmd.ExecuteNonQuery(); // NOCOUNT ON → rows check nahi karna
                con.Close();

                return Ok(new { message = "Purchase deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // Report page view
        public IActionResult ReportPage()
        {
            return View(); // Views/Purchase/ReportPage.cshtml
        }


        // Report----------------
        [HttpGet]
        public IActionResult GetPurchaseList(DateTime? fromDate, DateTime? toDate, int reportType = 0)
        {
            var list = new List<object>();
            using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            string query = @"
        SELECT PH.PurchaseId, PH.PurchaseDate, S.SupplierName, P.ProductName, PD.Qty, PD.CostPrice, PD.Amount
        FROM PurchaseHeader PH
        INNER JOIN PurchaseDetail PD ON PH.PurchaseId = PD.PurchaseId
        INNER JOIN Suppliers S ON PH.SupplierId = S.SupplierId
        INNER JOIN Products P ON PD.ProductId = P.ProductId
        WHERE 1=1";

            if (fromDate.HasValue)
                query += " AND PH.PurchaseDate >= @FromDate";
            if (toDate.HasValue)
                query += " AND PH.PurchaseDate <= @ToDate";
            if (reportType == 1) // Monthly
                query += " AND MONTH(PH.PurchaseDate) = MONTH(GETDATE()) AND YEAR(PH.PurchaseDate) = YEAR(GETDATE())";
            else if (reportType == 2) // Yearly
                query += " AND YEAR(PH.PurchaseDate) = YEAR(GETDATE())";

            using SqlCommand cmd = new SqlCommand(query, con);
            if (fromDate.HasValue) cmd.Parameters.AddWithValue("@FromDate", fromDate.Value);
            if (toDate.HasValue) cmd.Parameters.AddWithValue("@ToDate", toDate.Value);

            con.Open();
            using SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new
                {
                    purchaseId = dr["PurchaseId"].ToString(),
                    purchaseDate = Convert.ToDateTime(dr["PurchaseDate"]).ToString("yyyy-MM-dd"),
                    supplierName = dr["SupplierName"].ToString(),
                    productName = dr["ProductName"].ToString(),
                    qty = Convert.ToDecimal(dr["Qty"]),
                    costPrice = Convert.ToDecimal(dr["CostPrice"]),
                    amount = Convert.ToDecimal(dr["Amount"])
                });
            }
            con.Close();

            return Json(new { data = list });
        }

        [HttpGet]
        public IActionResult GetPurchaseSummary(DateTime? fromDate, DateTime? toDate, int reportType = 0)
        {
            var list = new List<object>();
            using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            string query = @"
          SELECT 
        S.SupplierName, 
        CAST(PH.PurchaseDate AS DATE) AS PurchaseDate, 
        SUM(PD.Qty) AS TotalQty, 
        SUM(PD.Amount) AS TotalAmount
        FROM PurchaseHeader PH
        INNER JOIN PurchaseDetail PD ON PH.PurchaseId = PD.PurchaseId
        INNER JOIN Suppliers S ON PH.SupplierId = S.SupplierId
    WHERE 1=1";

            if (fromDate.HasValue)
                query += " AND PH.PurchaseDate >= @FromDate";
            if (toDate.HasValue)
                query += " AND PH.PurchaseDate <= @ToDate";

            if (reportType == 1) // Monthly
                query += " AND MONTH(PH.PurchaseDate) = MONTH(GETDATE()) AND YEAR(PH.PurchaseDate) = YEAR(GETDATE())";
            else if (reportType == 2) // Yearly
                query += " AND YEAR(PH.PurchaseDate) = YEAR(GETDATE())";

            query += " GROUP BY S.SupplierName, CAST(PH.PurchaseDate AS DATE) ORDER BY S.SupplierName";

            using SqlCommand cmd = new SqlCommand(query, con);
            if (fromDate.HasValue) cmd.Parameters.AddWithValue("@FromDate", fromDate.Value);
            if (toDate.HasValue) cmd.Parameters.AddWithValue("@ToDate", toDate.Value);

            con.Open();
            using SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new
                {
                    SupplierName = dr["SupplierName"].ToString(),
                    PurchaseDate = Convert.ToDateTime(dr["PurchaseDate"]).ToString("yyyy-MM-dd"),
                    TotalQty = Convert.ToDecimal(dr["TotalQty"]),
                    TotalAmount = Convert.ToDecimal(dr["TotalAmount"])
                });
            }
            con.Close();

            return Json(new { data = list });
        }


    }
}
