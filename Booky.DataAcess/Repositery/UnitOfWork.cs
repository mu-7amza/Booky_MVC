using Booky.DataAccess.Data;
using Booky.DataAcess.Repositery.IRepositery;
using Booky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booky.DataAcess.Repositery
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        public ICategoryRepositery Category { get; private set; }
        public IProductRepositery Product { get; private set; }
        public ICompanyRepositery Company { get; private set; }
        public IShoppingCartRepositery ShoppingCart { get; private set; }
        public IApplicationUserRepositery ApplicationUser { get; private set; }
        public IOrderDetailsRepositery OrderDetails { get; private set; }
        public IOrderHeaderRepositery OrderHeader { get; private set; }
        public IProductImageRepositery ProductImage { get; private set; }

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Category = new CategoryRepositery(_db);
            Product = new ProductRepositery(_db);
            Company = new CompanyRepositery(_db);
            ShoppingCart = new ShoppingCartRepositery(_db);
            ApplicationUser = new ApplicationUserRepositery(_db);
            OrderDetails = new OrderDetailsRepositery(_db);
            OrderHeader = new OrderHeaderRepositery(_db);
            ProductImage = new ProductImageRepositery(_db);
        }
        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
