﻿using MultiShop.Models;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace MultiShop.Controllers
{
	public class PaymentController : Controller
	{
		MultiShopDbContext db = new MultiShopDbContext();
		private PayPal.Api.Payment payment;
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult PaymentWithPaypal(Order model, string Cancel = null)
		{
			String cutPhone = model.Description.Substring(1, 9);
			String phoneTo = "+84" + cutPhone;
			const String accountSID = "ACe8acfac4979129af98e8e393e35eca5e";
			const String authToken = "921a8ede611b813030d3ec9384d936ed";
			TwilioClient.Init(accountSID, authToken);
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
										| SecurityProtocolType.Tls11
										| SecurityProtocolType.Tls12
										| SecurityProtocolType.Ssl3;

			SqlConnection con = new SqlConnection("Server=DRAGON-PC\\SQLEXPRESS;Database=MultiShop;Integrated Security=True;");
			SqlCommand cmdf = new SqlCommand("UPDATE Orders SET Result  = 'Failed' WHERE (CustomerId = @CustID) and (OrderDate = @date)", con);
			SqlCommand cmds = new SqlCommand("UPDATE Orders SET Result  = 'Success' WHERE (CustomerId = @CustID)  and (OrderDate = @date)", con);
			APIContext apiContext = PaypalConfiguration.GetAPIContext();

			try
			{
				//A resource representing a Payer that funds a payment Payment Method as paypal  
				//Payer Id will be returned when payment proceeds or click to pay  
				string payerId = Request.Params["PayerID"];

				if (string.IsNullOrEmpty(payerId))
				{
					//this section will be executed first because PayerID doesn't exist  
					//it is returned by the create function call of the payment class  
					// Creating a payment  
					// baseURL is the url on which paypal sendsback the data.  
					string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Payment/PaymentWithPayPal?";
					//here we are generating guid for storing the paymentID received in session  
					//which will be used in the payment execution  
					var guid = Convert.ToString((new Random()).Next(100000));
					//CreatePayment function gives us the payment approval url  
					//on which payer is redirected for paypal account payment  
					var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid);
					//var createdPayment = this.CreatePayment(apiContext, baseURI);
					//get links returned from paypal in response to Create function call  
					var links = createdPayment.links.GetEnumerator();
					string paypalRedirectUrl = string.Empty;
					while (links.MoveNext())
					{
						Links lnk = links.Current;
						if (lnk.rel.ToLower().Trim().Equals("approval_url"))
						{
							//saving the payapalredirect URL to which user will be redirected for payment  
							paypalRedirectUrl = lnk.href;
						}
					}

					// saving the paymentID in the key guid  
					Session.Add(guid, createdPayment.id);
					return Redirect(paypalRedirectUrl);
				}
				else
				{
					// This function exectues after receving all parameters for the payment  
					var guid = Request.Params["guid"];
					// Session.Add("PayerId", payerId);
					var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
					//If executed payment failed then we will show payment failure message to user  
					if (executedPayment.state.ToLower() != "approved")
					{
						cmdf.Parameters.AddWithValue("@CustID", model.CustomerId);
						cmdf.Parameters.AddWithValue("@date", model.OrderDate);
						con.Open();
						cmdf.ExecuteNonQuery();
						con.Close();

						// Gửi SMS xác nhận thanh toán thất bại
						
						var fail = MessageResource.Create(
							body: "Đơn hàng thanh toán bằng PayPal của bạn đã thất bại! Vui lòng kiểm tra số dư trong Paypal.",
							from: new Twilio.Types.PhoneNumber("+16123548927"),
							to: new Twilio.Types.PhoneNumber(phoneTo)
						);
						return View("FailureView");
					}
				}
			}
			catch (Exception ex)
			{
				// Session.Remove("PayerID");
				// Session.Remove("guid");

				cmdf.Parameters.AddWithValue("@CustID", model.CustomerId);
				cmdf.Parameters.AddWithValue("@date", model.OrderDate);
				con.Open();
				cmdf.ExecuteNonQuery();
				con.Close();

				// Gửi SMS xác nhận thanh toán thất bại
				var failed = MessageResource.Create(
					body: "Đơn hàng thanh toán bằng PayPal của bạn đã thất bại! Vui lòng kiểm tra số dư trong Paypal.",
					from: new Twilio.Types.PhoneNumber("+16123548927"),
					to: new Twilio.Types.PhoneNumber(phoneTo)
				);
				return View("FailureView");
			}

			cmds.Parameters.AddWithValue("@CustID", model.CustomerId);
			cmds.Parameters.AddWithValue("@date", model.OrderDate);
			con.Open();
			cmds.ExecuteNonQuery();
			con.Close();

			// Gửi SMS xác nhận thanh toán thành công
			var success = MessageResource.Create(
				body: "Đơn hàng thanh toán bằng PayPal của bạn đã thành công! Chúng tôi sẽ giao hàng trong thời gian sớm nhất.",
				from: new Twilio.Types.PhoneNumber("+16123548927"),
				to: new Twilio.Types.PhoneNumber(phoneTo)
			);

			//on successful payment, show success page to user.  
			return View("SucessView");
		}

		private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
		{
			var paymentExecution = new PaymentExecution() { payer_id = payerId };
			this.payment = new Payment() { id = paymentId };
			return this.payment.Execute(apiContext, paymentExecution);
		}

		private Payment CreatePayment(APIContext apiContext, string redirectUrl)
		{

			//create itemlist and add item objects to it
			var itemList = new ItemList() { items = new List<Item>() };

			var cart = ShoppingCart.Cart;
			foreach (var p in cart.Items)
			{

				var product = db.Products.Find(p.Id);

				itemList.items.Add(new Item()
				{
					name = product.Name,
					currency = "USD",
					price = p.UnitPrice.ToString(),
					quantity = p.Quantity.ToString(),
				});
			}
			//Adding Item Details like name, currency, price etc


			var payer = new Payer()
			{
				payment_method = "paypal",
				payer_info = new PayerInfo
				{
					first_name = "Zaphod",
					last_name = "Beeblebrox"
				}


			};

			// Configure Redirect Urls here with RedirectUrls object
			var redirUrls = new RedirectUrls()
			{
				cancel_url = redirectUrl,
				return_url = redirectUrl
			};

			// Adding Tax, shipping and Subtotal details
			var details = new Details()
			{
				tax = "1",
				shipping = "1",
				subtotal = "1"
			};

			//Final amount with details
			var amount = new Amount()
			{
				currency = "USD",
				total = ShoppingCart.Cart.Total.ToString(), // Total must be equal to sum of tax, shipping and subtotal.

			};

			var transactionList = new List<Transaction>();
			// Adding description about the transaction
			transactionList.Add(new Transaction()
			{
				description = "Transaction description",
				invoice_number = Convert.ToString((new Random()).Next(100000)), //Generate an Invoice No
				amount = amount,
				item_list = itemList
			});


			this.payment = new Payment()
			{
				intent = "sale",
				payer = payer,
				transactions = transactionList,
				redirect_urls = redirUrls
			};

			// Create a payment using a APIContext
			return this.payment.Create(apiContext);
		}
	}
}