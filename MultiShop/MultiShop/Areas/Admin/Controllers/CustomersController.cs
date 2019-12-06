using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;

namespace MultiShop.Areas.Admin.Controllers
{
    public class CustomersController : Controller
    {
        private MultiShopDbContext db = new MultiShopDbContext();

        // GET: Admin/Customers
        public ActionResult Index()
        {
            return View(db.Customers.ToList());
        }

		public ActionResult ExportToExcel()
		{
			//Chuẩn bị dữ liệu
			var data = db.Customers.Select(kh => new
			{
				Email = kh.Email,
				FullName = kh.Fullname
			});

			//Xuất ra Excel dùng EPLus
			var stream = new MemoryStream();
			using (var package = new ExcelPackage(stream))
			{
				var sheet = package.Workbook.Worksheets.Add("Customer");

				//sheet.Cells[1, 1].Value = "A";
				sheet.Cells.LoadFromCollection(data, true);

				package.Save();
			}
			stream.Position = 0;

			return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "KhachHang.xlsx");
		}
	}
}