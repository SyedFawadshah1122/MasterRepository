using CRUD_Operation.Classes;
using CrudMaster.Classes;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Master.Controllers
{
    public class CustomerController : Controller
    {
        private readonly DapperContext _context;

        public CustomerController(DapperContext context)
        {
            _context = context;
        }

        // MAIN PAGE VIEW
        public IActionResult Customers()
        {
            return View();
        }

        // SAVE CUSTOMER
        [HttpPost]
        public IActionResult SaveCustomer(string CustomerId, string CustomerName, string Phone, string Address, bool IsActive)
        {
            using var con = _context.CreateConnection();

            con.Execute("sp_SaveCustomer",
                new
                {
                    CustomerId,
                    CustomerName,
                    Phone,
                    Address,
                    IsActive
                },
                commandType: CommandType.StoredProcedure);

            return Json("Saved");
        }


        [HttpGet]
        public IActionResult GetCustomers()
        {
            using var con = _context.CreateConnection();
            var result = con.Query("sp_GetCustomers", commandType: CommandType.StoredProcedure);
            return Json(result);
        }

        // GET CUSTOMER BY ID
        [HttpGet]
        public IActionResult GetCustomerById(string id)
        {
            using var con = _context.CreateConnection();

            var result = con.QuerySingle("sp_GetCustomerById",
                new { CustomerId = id },
                commandType: CommandType.StoredProcedure);

            return Json(result);
        }

        // UPDATE CUSTOMER
        [HttpPost]
        public IActionResult UpdateCustomer(string CustomerId, string CustomerName, string Phone, string Address, bool IsActive)
        {
            using var con = _context.CreateConnection();

            con.Execute("sp_UpdateCustomer",
                new
                {
                    CustomerId,
                    CustomerName,
                    Phone,
                    Address,
                    IsActive
                },
                commandType: CommandType.StoredProcedure);

            return Json("Updated Successfully");
        }

        // DELETE CUSTOMER
        [HttpPost]
        public IActionResult DeleteCustomer(string id)
        {
            using var con = _context.CreateConnection();

            con.Execute("sp_DeleteCustomer",
                new { CustomerId = id },
                commandType: CommandType.StoredProcedure);

            return Json("Deleted Successfully");
        }

        // DATATABLE SUPPORT 
        [HttpGet]
        public IActionResult GetCustomersPaged(int pageNo, int pageSize, string search, string sortCol, string sortOrder)
        {
            using var con = _context.CreateConnection();

            var result = con.QueryMultiple(
                "sp_GetCustomersPaged",
                new
                {
                    PageNo = pageNo,
                    PageSize = pageSize,
                    Search = search ?? "",
                    SortCol = sortCol ?? "CustomerId",
                    SortOrder = sortOrder ?? "ASC"
                },
                commandType: CommandType.StoredProcedure
            );

            int totalRecords = result.ReadFirst<int>();
            var data = result.Read();

            return Json(new
            {
                totalRecords,
                data
            });
        }
    }
}