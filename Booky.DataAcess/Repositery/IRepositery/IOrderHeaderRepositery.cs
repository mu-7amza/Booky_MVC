using Booky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booky.DataAcess.Repositery.IRepositery
{
    public interface IOrderHeaderRepositery : IRepositery<OrderHeader>
    {
        void Update(OrderHeader obj);

        void UpdateStatus(int id, string status, string? paymentstatus = null);
        void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId);


    }
}
