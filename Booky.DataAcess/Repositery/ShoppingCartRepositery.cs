using Booky.DataAccess.Data;
using Booky.DataAcess.Repositery.IRepositery;
using Booky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Booky.DataAcess.Repositery
{
    public class ShoppingCartRepositery : Repositery<ShoppingCart>, IShoppingCartRepositery
    {
        private ApplicationDbContext _db;
        public ShoppingCartRepositery(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ShoppingCart obj)
        {
            _db.ShoppingCarts.Update(obj);
        }

    }
}
