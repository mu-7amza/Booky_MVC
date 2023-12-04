using Booky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booky.DataAcess.Repositery.IRepositery
{
    public interface IOrderDetailsRepositery : IRepositery<OrderDetail>
    {
        void Update(OrderDetail obj);
 
    }
}
