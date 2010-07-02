using System;
using System.Web;

namespace XmlSiteMapProviderBug
{
	public class CustomXmlSitemapProvider : XmlSiteMapProvider
	{
		public override SiteMapNode FindSiteMapNode(string rawUrl)
		{
			SiteMapNode node = base.FindSiteMapNode(rawUrl);
			if (node != null)
			{
				System.Web.UI.Page page = HttpContext.Current.Handler as System.Web.UI.Page;
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

