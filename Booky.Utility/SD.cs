using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booky.Utility
{
    public static class SD
    {
        public const string ROLE_CUSTOMER = "Customer";
        public const string ROLE_ADMIN = "Admin";
        public const string ROLE_COMPANY = "Company";
        public const string ROLE_EMPLOYEE = "Employee";

        public const string StatusPending = "Pending";
		public const string StatusApproved = "Approved";
		public const string StatusInProcessing = "Processing";
        public const string StatusShipped = "Shipped";
		public const string StatusCancelled = "Cancelled";
		public const string StatusRefunded = "Refunded";

        public const string PaymentStatusPending = "Pending";
		public const string PaymentStatusApproved = "Approved";
		public const string PaymentStatusDelayedPayment = "ApprovedForDelayedPayment";
		public const string PaymentStatusRejected = "Rejected";

        public const string SessionCart = "SessionShoppingCart";




	}
}
