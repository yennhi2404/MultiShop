using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MultiShop
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				name: "san-pham",
				url: "san-pham/{metatile}-{CategoryID}",
				defaults: new { controller = "Product", action = "Category", id = UrlParameter.Optional },
				namespaces: new[] { "MultiShop.Controllers" }
			);
			routes.MapRoute(
				name: "chi-tiet-san-pham",
				url: "chi-tiet-san-pham/{name}_{id}",
				defaults: new { controller = "Product", action = "Detail", id = UrlParameter.Optional },
				namespaces: new[] { "MultiShop.Controllers" }
			);
			routes.MapRoute(
				name: "gio-hang",
				url: "gio-hang",
				defaults: new { controller = "Cart", action = "Index", id = UrlParameter.Optional },
				namespaces: new[] { "MultiShop.Controllers" }
			);
			routes.MapRoute(
				name: "dang-ky",
				url: "dang-ky",
				defaults: new { controller = "Account", action = "Register", id = UrlParameter.Optional },
				namespaces: new[] { "MultiShop.Controllers" }
			);
			routes.MapRoute(
				name: "thanh-toan",
				url: "thanh-toan",
				defaults: new { controller = "Order", action = "Checkout", id = UrlParameter.Optional },
				namespaces: new[] { "MultiShop.Controllers" }
			);
			routes.MapRoute(
				name: "lien-he",
				url: "lien-he",
				defaults: new { controller = "Home", action = "Contact", id = UrlParameter.Optional },
				namespaces: new[] { "MultiShop.Controllers" }
			);
			routes.MapRoute(
				name: "thong-tin-ve-multishop",
				url: "thong-tin-ve-multishop",
				defaults: new { controller = "Home", action = "About", id = UrlParameter.Optional },
				namespaces: new[] { "MultiShop.Controllers" }
			);
			routes.MapRoute(
				name: "tim-kiem",
				url: "tim-kiem",
				defaults: new { controller = "Product", action = "Search", id = UrlParameter.Optional },
				namespaces: new[] { "MultiShop.Controllers" }
			);
			routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "MultiShop.Controllers" }
            );
        }
    }
}
