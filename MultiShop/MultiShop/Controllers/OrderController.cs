using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MultiShop.Helpers;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace MultiShop.Controllers
{
    public class OrderController : Controller
    {
        MultiShopDbContext db = new MultiShopDbContext();
		//
		// GET: /Order/
		public ActionResult Checkout()
        {
			string url = "https://vnexpress.net/rss/giai-tri.rss";
			ViewBag.listItems = RSSHelper.read(url);
			var model = new Order();
            model.CustomerId = User.Identity.Name;
            model.OrderDate = DateTime.Now;
            model.Amount = ShoppingCart.Cart.Total;
			
			return View(model);
        }
		[HttpPost]
		public ActionResult Purchase(Order model)
        {
            //db.Orders.Add(model);

            var cart = ShoppingCart.Cart;
            foreach (var p in cart.Items)
            {
                var d = new OrderDetail
                {
                    Order = model,
                    ProductId = p.Id,
                    UnitPrice = p.UnitPrice,
                    Discount = p.Discount,
                    Quantity = p.Quantity
                };

                db.OrderDetails.Add(d);
            }
            db.SaveChanges();

			

			// Thanh toán trực tuyến
			//var api = new WebApiClient<AccountInfo>();
			//var data = new AccountInfo { 
			//    Id=Request["BankAccount"],
			//    Balance = cart.Total
			//};
			//api.Put("api/Bank/nn", data);
			//return RedirectToAction("Detail", new { id = model.Id });
			return RedirectToAction("PaymentWithPayPal", "Payment",model);
		}
		
		public ActionResult Detail(int id)
        {
			string url = "https://vnexpress.net/rss/giai-tri.rss";
			ViewBag.listItems = RSSHelper.read(url);
			var order = db.Orders.Find(id);
            return View(order);
        }
		
		public ActionResult List()
        {
			string url = "https://vnexpress.net/rss/giai-tri.rss";
			ViewBag.listItems = RSSHelper.read(url);
			var orders = db.Orders
                .Where(o => o.CustomerId == User.Identity.Name);
            return View(orders);
        }
	}
}