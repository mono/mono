using System;
using System.Web;

namespace XmlSiteMapProviderBug
{
	public class CustomXmlSitemapProvider : XmlSiteMapProvider
	{
		public override SiteMapNode FindSiteMapNode(string rawUrl)
		{
			var node = base.FindSiteMapNode(rawUrl);
			if (node != null)
			{
				var page = HttpContext.Current.Handler as System.Web.UI.Page;
				if (page != null)
				{
					page.Title = node.Title;
				}
				return node;
			}
			else
			{
				return base.RootNode;
			}
		}
	}
}

