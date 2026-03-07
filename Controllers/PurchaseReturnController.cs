using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace YourNamespace.Controllers
{
    public class PurchaseReturnController : Controller
    {
        private readonly IConfiguration _config;

        public PurchaseReturnController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        // =========================
        // GET PURCHASE HEADERS BY SUPPLIER
        // =========================
        [HttpGet]
        public IActionResult GetPurchaseHeadersBySupplier(string supplierId)
        {
            if (string.IsNullOrEmpty(supplierId))
                return BadRequest("SupplierId is required");

            var list = new List<object>();

            using (SqlConnection con = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                string query = @"
                SELECT DISTINCT PH.PurchaseId
                FROM PurchaseHeader PH
                WHERE PH.SupplierId = @SupplierId
                ORDER BY PH.PurchaseId DESC";

                using SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@SupplierId", supplierId);

                con.Open();
                using SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(new
                    {
                        purchaseId = dr["PurchaseId"].ToString()
                    });
                }

                con.Close();
            }

            return Ok(list);
        }

        // =========================
        // GET PURCHASE DETAILS BY HEADER ID
        // =========================
        [HttpGet]
        public IActionResult GetPurchaseItemsByHeaderId(string purchaseId)
        {
            if (string.IsNullOrEmpty(purchaseId))
                return BadRequest("PurchaseId is required");

            var list = new List<object>();

            using SqlConnection con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

            string query = @"
            SELECT 
                PD.DetailId,
                PD.PurchaseId,
                PD.ProductId,
                P.ProductName,
                PD.Qty,
                PD.CostPrice,
                PD.Amount,
                PD.Barcode
            FROM PurchaseDetail PD
            INNER JOIN Products P ON PD.ProductId = P.ProductId
            WHERE PD.PurchaseId = @PurchaseId
            ORDER BY PD.DetailId";

            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@PurchaseId", purchaseId);

            con.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new
                {
                    detailId = dr["DetailId"].ToString(),
                    purchaseId = dr["PurchaseId"].ToString(),
                    productId = dr["ProductId"].ToString(),
                    productName = dr["ProductName"].ToString(),
                    qty = Convert.ToDecimal(dr["Qty"]),
                    costPrice = Convert.ToDecimal(dr["CostPrice"]),
                    amount = Convert.ToDecimal(dr["Amount"]),
                    barcode = dr["Barcode"].ToString()
                });
            }

            con.Close();

            return Json(new { data = list });
        }

        // =========================
        // ADD PURCHASE RETURN
        // =========================
        [HttpPost]
        public IActionResult AddBulkPurchaseReturn([FromBody] List<ReturnItemVM> items)
        {
            if (items == null || items.Count == 0)
                return BadRequest("No items to return.");

            string cmpyId = "CMP001";
            string branchId = "BR001";
            string createdBy = User.Identity?.Name ?? "system";

            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                conn.Open();

                foreach (var item in items)
                {
                    var parameters = new DynamicParameters();

                    parameters.Add("@PurchaseId", item.purchaseId);
                    parameters.Add("@ProductId", item.productId);
                    parameters.Add("@ReturnQty", item.returnQty);
                    parameters.Add("@Reason", item.reason);
                    parameters.Add("@CmpyId", cmpyId);
                    parameters.Add("@BranchId", branchId);
                    parameters.Add("@CreatedBy", createdBy);

                    conn.Execute(
                        "sp_AddPurchaseReturn_StockLedger",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );
                }
            }

            return Ok(new { message = "Items returned successfully!" });
        }

        // PAGE LOAD
        public IActionResult GetStockLedgerData()
        {
            using (var db = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                var query = @"SELECT 
                        LedgerId,
                        PurchaseId,
                        ProductId,
                        QtyIn,
                        QtyOut,
                        
                        CmpyId,
                        BranchId,
                        CreatedOn,
                        CreatedBy
                      FROM StockLedger";

                var data = db.Query(query).ToList();

                return Json(new { data });
            }
        }
        public class StockLedger
        {
            public int LedgerId { get; set; }
            public String PurchaseId { get; set; }
            public string ProductId { get; set; }
            public decimal QtyIn { get; set; }
            public decimal QtyOut { get; set; }
            
            public string CmpyId { get; set; }
            public string BranchId { get; set; }
            public DateTime CreatedOn { get; set; }
            public string CreatedBy { get; set; }
        }

        // ================= VIEW MODEL =================
        public class ReturnItemVM
        {
            public string purchaseId { get; set; }
            public string productId { get; set; }
            public decimal returnQty { get; set; }
            public string reason { get; set; }
        }
    }
}