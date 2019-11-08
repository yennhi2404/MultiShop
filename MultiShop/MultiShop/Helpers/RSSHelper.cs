using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.XPath;
using MultiShop.Models.EShopV20;

namespace MultiShop.Helpers
{
	public class RSSHelper
	{
		public static List<Item> read(string url)
		{
			List<Item> listItems = new List<Item>();
			try
			{
				int i = 0;
				XPathDocument doc = new XPathDocument(url);
				XPathNavigator navigator = doc.CreateNavigator();
				XPathNodeIterator nodes = navigator.Select("//item");
				while (nodes.MoveNext())
				{
					if (i <= 10)
					{
						XPathNavigator node = nodes.Current;
						listItems.Add(new Item
						{
							Guid = node.SelectSingleNode("guid").Value,
							Link = node.SelectSingleNode("link").Value,
							PubDate = node.SelectSingleNode("pubDate").Value,
							Title = node.SelectSingleNode("title").Value
						});
						i++;
					}
				}
			}
			catch
			{
				listItems = null;
			}
			return listItems;
		}
	}
}