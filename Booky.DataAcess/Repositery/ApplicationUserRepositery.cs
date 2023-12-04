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
    public class ApplicationUserRepositery : Repositery<ApplicationUser>, IApplicationUserRepositery
    {
        private ApplicationDbContext _db;
        public ApplicationUserRepositery(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ApplicationUser applicationUser)
        {
            _db.ApplicationUsers.Update(applicationUser);
        }
    }
}
