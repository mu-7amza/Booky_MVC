using Booky.Models.ViewModels;

namespace Booky.Models.ViewModels
{
    public class ShoppingCartVm
    {
        public IEnumerable<ShoppingCart> Items { get; set; }
        public OrderHeader OrderHeader { get; set; }
        public string? Error {  get; set; }
    }
}
