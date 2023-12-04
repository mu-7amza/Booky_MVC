using Booky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booky.DataAcess.Repositery.IRepositery
{
    public interface IApplicationUserRepositery : IRepositery<ApplicationUser>
    {
        public void Update(ApplicationUser applicationUser);
    }
}
