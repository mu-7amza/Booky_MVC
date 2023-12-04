using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booky.DataAcess.Repositery.IRepositery
{
    public interface IUnitOfWork
    {
        ICategoryRepositery Category { get; }
        IProductRepositery Product { get; }
        ICompanyRepositery Company { get; }
        IShoppingCartRepositery ShoppingCart { get; }
        IApplicationUserRepositery ApplicationUser { get; }
        IOrderHeaderRepositery OrderHeader { get; }
        IOrderDetailsRepositery OrderDetails { get; }
        IProductImageRepositery ProductImage { get; }
        void Save();
    }
}
