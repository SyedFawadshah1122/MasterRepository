using CRUD_Operation.Classes;
using CrudMaster.Models;
using Dapper;
using EmployeeRegistraion.Models;
using System.Data;

namespace CrudMaster.Classes
{
    public class MainRepository
    {
        public readonly DapperContext _context;
        public MainRepository(DapperContext context)
        {
            _context = context;
        }
        public IDbConnection GetConnection()
        {
            var con = _context.CreateConnection();
            con.Open();
            return con;
        }
      
        ///////
        ///
        //NEW WORK



        // ============================================================
        //#region Category
        // ============================================================

        // SAVE CATEGORY 
        public void Savecategory(CategoryVm Model)
        {
            string sql = @"
                INSERT INTO Categories 
                (CategoryId, CategoryName, Description, IsActive, CmpyId, BranchId, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy)
                VALUES 
                (@CategoryId, @CategoryName, @Description, @IsActive, @CmpyId, @BranchId, @CreatedOn, @CreatedBy, @ModifiedOn, @ModifiedBy)
            ";

            using var conn = _context.CreateConnection();
            conn.Open();
            conn.Execute(sql, Model);
        }

        // GET ALL CATEGORIES
        public IEnumerable<CategoryVm> GetCategories()
        {
            string sql = @"SELECT CategoryId, CategoryName, Description, IsActive FROM Categories";

            using var conn = _context.CreateConnection();
            conn.Open();
            return conn.Query<CategoryVm>(sql).ToList();
        }

        // GET CATEGORY BY ID
        public CategoryVm? GetCategoryById(string id)
        {
            string sql = @"SELECT CategoryId, CategoryName, Description, IsActive
                           FROM Categories
                           WHERE CategoryId = @id";

            using var conn = _context.CreateConnection();
            conn.Open();
            return conn.QueryFirstOrDefault<CategoryVm>(sql, new { id });
        }

        // UPDATE CATEGORY
        public void UpdateCategory(CategoryVm model)
        {
            string sql = @"
                UPDATE Categories
                SET CategoryName = @CategoryName,
                    Description = @Description,
                    IsActive = @IsActive
                WHERE CategoryId = @CategoryId
            ";

            using var conn = _context.CreateConnection();
            conn.Open();
            conn.Execute(sql, model);
        }





        // ============================================================
        #region SubCategory
        // ============================================================

        // SAVE SUBCATEGORY 
        public void SaveSubCategory(SubCategoryVm model)
        {
            string sql = @"
                INSERT INTO SubCategories
                (SubCategoryId, CategoryId, SubCategoryName, IsActive, CmpyId, BranchId, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy)
                VALUES
                (@SubCategoryId, @CategoryId, @SubCategoryName, @IsActive, @CmpyId, @BranchId, @CreatedOn, @CreatedBy, @ModifiedOn, @ModifiedBy)
            ";

            using var conn = _context.CreateConnection();
            conn.Open();
            conn.Execute(sql, model);
        }

        // GET ALL SUBCATEGORIES
        public IEnumerable<SubCategoryVm> GetSubCategories()
        {
            string sql = @"SELECT CategoryId, SubCategoryId, SubCategoryName, IsActive 
                           FROM SubCategories";

            using var conn = _context.CreateConnection();
            conn.Open();
            return conn.Query<SubCategoryVm>(sql).ToList();
        }

        // GET SUBCATEGORY BY ID
        public SubCategoryVm? GetSubCategoryById(string id)
        {
            string sql = @"SELECT SubCategoryId, CategoryId, SubCategoryName, IsActive
                           FROM SubCategories
                           WHERE SubCategoryId = @id";

            using var conn = _context.CreateConnection();
            conn.Open();
            return conn.QueryFirstOrDefault<SubCategoryVm>(sql, new { id });
        }

        // for loading subcategories based on category selection
        public IEnumerable<SubCategoryVm> GetSubCategoriesByCategoryId(string categoryId)
        {
            string sql = @"SELECT SubCategoryId, SubCategoryName
                           FROM SubCategories
                           WHERE CategoryId = @categoryId";

            using var conn = _context.CreateConnection();
            conn.Open();
            return conn.Query<SubCategoryVm>(sql, new { categoryId }).ToList();
        }

        // UPDATE SUBCATEGORY
        public void UpdateSubCategory(SubCategoryVm model)
        {
            string sql = @"
                UPDATE SubCategories
                SET CategoryId = @CategoryId,
                    SubCategoryId = @SubCategoryId,
                    SubCategoryName = @SubCategoryName,
                    IsActive = @IsActive
                WHERE SubCategoryId = @SubCategoryId
            ";

            using var conn = _context.CreateConnection();
            conn.Open();
            conn.Execute(sql, model);
        }

        // DELETE CATEGORY
        public void DeleteCategory(string id)
        {
            string sql = @"DELETE FROM Categories WHERE CategoryId = @id";

            using var conn = _context.CreateConnection();
            conn.Open();
            conn.Execute(sql, new { id });
        }

        // DELETE SUBCATEGORY//
        public void DeleteSubCategory(string id)
        {
            string sql = @"DELETE FROM SubCategories WHERE SubCategoryId = @id";
            using var conn = _context.CreateConnection();
            conn.Open();
            conn.Execute(sql, new { id });
        }

        #endregion




        // ============================================================
        #region Products
        // ============================================================

        // CREATE / INSERT PRODUCT
        public void SaveProducts(ProductsVm model)
        {
            string sql = @"
                INSERT INTO Products
                (
                    ProductId, ProductName, CategoryId, SubCategoryId,
                    CostPrice, SellingPrice, StockQty, IsActive,
                    CmpyId, BranchId, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy
                )
                VALUES
                (
                    @ProductId, @ProductName, @CategoryId, @SubCategoryId,
                    @CostPrice, @SellingPrice, @StockQty, @IsActive,
                    @CmpyId, @BranchId, @CreatedOn, @CreatedBy, @ModifiedOn, @ModifiedBy
                )";

            using var conn = GetConnection();
            conn.Execute(sql, model);
        }

        // GET ALL PRODUCTS
        public IEnumerable<ProductsVm> GetProducts()
        {
            string sql = @"
                SELECT ProductId, ProductName, CategoryId, SubCategoryId,
                       CostPrice, SellingPrice, StockQty, IsActive
                FROM Products";

            using var conn = GetConnection();
            return conn.Query<ProductsVm>(sql).ToList();
        }

        // GET PRODUCT BY ID
        public ProductsVm GetProductById(string productId)
        {
            string sql = "SELECT * FROM Products WHERE ProductId = @ProductId";
            using var conn = GetConnection();
            return conn.QueryFirstOrDefault<ProductsVm>(sql, new { ProductId = productId });
        }

        public void UpdateProduct(ProductsVm model)
        {
            string sql = @"
        UPDATE Products
        SET ProductName = @ProductName,
            CategoryId = @CategoryId,
            SubCategoryId = @SubCategoryId,
            CostPrice = @CostPrice,
            SellingPrice = @SellingPrice,
            StockQty = @StockQty,
            IsActive = @IsActive,
            ModifiedOn = @ModifiedOn,
            ModifiedBy = @ModifiedBy
        WHERE ProductId = @ProductId";

            using var conn = _context.CreateConnection();
            conn.Open();
            conn.Execute(sql, model);
        }


        // DELETE PRODUCT
        public void DeleteProduct(string productId)
        {
            string sql = "DELETE FROM Products WHERE ProductId = @ProductId";
            using var conn = GetConnection();
            conn.Execute(sql, new { ProductId = productId });
        }



        #endregion
        //Suppliers SECTION
        // SUPPLIER REGION
        // ============================================================

        public void SaveSupplier(SuppliersVm model)
        {
            string sql = @"
        INSERT INTO Suppliers
        (SupplierId, SupplierName, Mobile, Address, Active,
         CmpyId, BranchId, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy)
        VALUES
        (@SupplierId, @SupplierName, @Mobile, @Address, @Active,
         @CmpyId, @BranchId, @CreatedOn, @CreatedBy, @ModifiedOn, @ModifiedBy)
    ";

            using var conn = _context.CreateConnection();
            conn.Open();
            conn.Execute(sql, model);
        }

        public IEnumerable<SuppliersVm> GetSuppliers()
        {
            string sql = @"
        SELECT SupplierId, SupplierName, Mobile, Address, Active,
               CmpyId, BranchId, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy
        FROM Suppliers
    ";

            using var conn = _context.CreateConnection();
            conn.Open();
            return conn.Query<SuppliersVm>(sql).ToList();
        }

        public SuppliersVm? GetSupplierById(string id)
        {
            string sql = @"
        SELECT SupplierId, SupplierName, mobile, Address, Active,
               CmpyId, BranchId, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy
        FROM Suppliers
        WHERE SupplierId = @id
    ";

            using var conn = _context.CreateConnection();
            conn.Open();
            return conn.QueryFirstOrDefault<SuppliersVm>(sql, new { id });
        }

        public void UpdateSupplier(SuppliersVm model)
        {
            string sql = @"
        UPDATE Suppliers
        SET SupplierName = @SupplierName,
           mobile = @mobile,
            Address = @Address,
            Active = @Active,
            ModifiedOn = @ModifiedOn,
            ModifiedBy = @ModifiedBy
        WHERE SupplierId = @SupplierId
    ";

            using var conn = _context.CreateConnection();
            conn.Open();
            conn.Execute(sql, model);
        }

        public void DeleteSupplier(string id)
        {
            string sql = @"DELETE FROM Suppliers WHERE SupplierId = @id";

            using var conn = _context.CreateConnection();
            conn.Open();
            conn.Execute(sql, new { id });
        }

        /* Quota exceeded. Please try again later. */

    }
}
