﻿using Booky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booky.DataAcess.Repositery.IRepositery
{
    public interface IShoppingCartRepositery : IRepositery<ShoppingCart>
    {
        void Update(ShoppingCart obj);
 
    }
}
