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
    public class OrderDetailsRepositery : Repositery<OrderDetail>, IOrderDetailsRepositery
    {
        private ApplicationDbContext _db;
        public OrderDetailsRepositery(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderDetail obj)
        {
            _db.OrderDetails.Update(obj);
        }

    }
}
