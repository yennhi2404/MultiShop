﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using MultiShop.Helpers;

namespace MultiShop.Controllers
{
    
    public class HomeController : Controller
    {
        MultiShopDbContext db = new MultiShopDbContext();

		public ActionResult Index()
        {
			string url = "https://vnexpress.net/rss/giai-tri.rss";
			ViewBag.listItems = RSSHelper.read(url);
            var model = db.Categories
                .Where(c => c.Products.Count >= 4)
                .OrderBy(c => Guid.NewGuid()).ToList();

			return View(model);
        }

        public ActionResult Search()
        {
            var name = Request["term"];

            var data = db.Products
                .Where(p => p.Name.Contains(name))
                .Select(p => p.Name).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult About()
        {
			string url = "https://vnexpress.net/rss/giai-tri.rss";
			ViewBag.listItems = RSSHelper.read(url);
			ViewBag.Message = "Giới thiệu về Multishop";
            return View();
        }

        public ActionResult Contact()
        {
			string url = "https://vnexpress.net/rss/giai-tri.rss";
			ViewBag.listItems = RSSHelper.read(url);
			ViewBag.Message = "Liên hệ";
            return View();
        }
        
        public ActionResult Category()
        {
			
			var model = db.Categories;
            return PartialView("_Category",model);
        }

        public ActionResult Special()
        {
			
			var model = db.Products.Where(p=>p.Special==true).Take(5);
            return PartialView("_Special", model);
        }
        
        public ActionResult Saleoff()
        {		
			var model = db.Products.Where(p => p.Discount>0).Take(1);
            return PartialView("_Saleoff", model);
        }
	}
}