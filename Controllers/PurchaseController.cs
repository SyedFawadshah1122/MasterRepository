using CrudMaster.Models;
using Master.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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

            var d = vm.Details.First();

            using SqlConnection con =
                new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            using SqlCommand cmd = new SqlCommand("sp_PurchaseEntry", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@PurchaseDate", SqlDbType.Date).Value = vm.PurchaseDate;
            cmd.Parameters.Add("@SupplierId", SqlDbType.NVarChar, 50).Value = vm.SupplierId;
            cmd.Parameters.Add("@ProductId", SqlDbType.NVarChar, 50).Value = d.ProductId;

            cmd.Parameters.Add("@Qty", SqlDbType.Decimal).Value = d.Qty;
            cmd.Parameters.Add("@CostPrice", SqlDbType.Decimal).Value = d.CostPrice;
            cmd.Parameters.Add("@Amount", SqlDbType.Decimal).Value = d.Amount;
            cmd.Parameters.Add("@TotalAmount", SqlDbType.Decimal).Value = d.Amount;

            cmd.Parameters.Add("@Barcode", SqlDbType.NVarChar, 100).Value = d.Barcode ?? "";
            cmd.Parameters.Add("@Active", SqlDbType.Bit).Value = true;

            // Mock audit fields
            cmd.Parameters.Add("@CmpyId", SqlDbType.NVarChar, 50).Value = "CMP-001";
            cmd.Parameters.Add("@BranchId", SqlDbType.NVarChar, 50).Value = "BR-001";
            cmd.Parameters.Add("@CreatedBy", SqlDbType.NVarChar, 50).Value = "admin";
            cmd.Parameters.Add("@CreatedOn", SqlDbType.DateTime).Value = DateTime.Now;

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
            S.SupplierName,
            PD.ProductId,
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

            if (!dr.HasRows)
            {
                con.Close();
                return NotFound();
            }

            string _purchaseId = "";
            DateTime _purchaseDate = DateTime.Now;
            string _supplierId = "";
            string _supplierName = "";

            List<object> details = new List<object>();

            while (dr.Read())
            {
                _purchaseId = dr["PurchaseId"].ToString();
                _purchaseDate = Convert.ToDateTime(dr["PurchaseDate"]);
                _supplierId = dr["SupplierId"].ToString();
                _supplierName = dr["SupplierName"].ToString();

                details.Add(new
                {
                    productId = dr["ProductId"].ToString(),
                    productName = dr["ProductName"].ToString(),
                    qty = Convert.ToDecimal(dr["Qty"]),
                    costPrice = Convert.ToDecimal(dr["CostPrice"]),
                    amount = Convert.ToDecimal(dr["Amount"]),
                    barcode = dr["Barcode"].ToString()
                });
            }

            con.Close();

            return Ok(new
            {
                purchaseId = _purchaseId,
                purchaseDate = _purchaseDate,
                supplierId = _supplierId,
                supplierName = _supplierName,
                details = details
            });
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


    }
}
