using Booky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Booky.DataAcess.Repositery.IRepositery
{
    public interface IRepositery<T> where T : class
    {
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperities = null);
        T Get(Expression<Func<T,bool>> Filter , string? includeProperities = null , bool tracked = false);
        void Add ( T entity );
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);




    }

}
