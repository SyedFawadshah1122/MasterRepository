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
                            SL.LedgerId AS DetailId,
                            SL.PurchaseId,
                            SL.ProductId,
                            P.ProductName,
                            SL.QtyIn AS purchasedQty,
                            SL.BalanceQty AS balanceQty,
                            PD.CostPrice,
                            PD.Barcode,
                            SL.CmpyId,
                            SL.BranchId
                        FROM StockLedger SL
                        INNER JOIN Products P ON SL.ProductId = P.ProductId
                        LEFT JOIN PurchaseDetail PD 
                            ON SL.ProductId = PD.ProductId 
                            AND SL.PurchaseId = PD.PurchaseId
                        WHERE SL.PurchaseId = @PurchaseId
                        ORDER BY SL.LedgerId";

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
                    purchasedQty = Convert.ToDecimal(dr["purchasedQty"]),
                    balanceQty = Convert.ToDecimal(dr["balanceQty"]),
                    costPrice = dr["CostPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["CostPrice"]),
                    barcode = dr["Barcode"] == DBNull.Value ? "" : dr["Barcode"].ToString()
                });
            }

            con.Close();

            return Json(new { data = list });
        }

        // =========================
        // ADD PURCHASE RETURN
        // =========================
        [HttpPost]
        public IActionResult AddBulkPurchaseReturn([FromBody] List<PurchaseReturnModel> model)
        {
            if (model == null || model.Count == 0)
            {
                return Json(new { success = false, message = "No data found" });
            }

            using (var db = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                db.Open();

                string returnId = "RET-" + DateTime.Now.ToString("yyyyMMddHHmmss");

                foreach (var item in model)
                {
                    var param = new DynamicParameters();

                    param.Add("@ReturnId", returnId);
                    param.Add("@PurchaseId", item.PurchaseId);
                    param.Add("@ProductId", item.ProductId);
                    param.Add("@Qty", item.ReturnQty);
                    param.Add("@CostPrice", item.CostPrice);
                    param.Add("@Barcode", item.Barcode);
                    param.Add("@SupplierId", item.SupplierId);

                    param.Add("@CmpyId", "1");
                    param.Add("@BranchId", "1");
                    param.Add("@CreatedBy", "Admin");

                    db.Execute("AddBulkPurchaseReturn", param, commandType: CommandType.StoredProcedure);
                }
            }

            return Json(new { success = true, message = "Purchase Return Saved Successfully" });
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

                return Json(new { data = data });
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

        public class PurchaseReturnModel
        {
            public string PurchaseId { get; set; }

            public string ProductId { get; set; }

            public decimal ReturnQty { get; set; }

            public decimal CostPrice { get; set; }

            public string Barcode { get; set; }

            public string SupplierId { get; set; }
        }
    }
}