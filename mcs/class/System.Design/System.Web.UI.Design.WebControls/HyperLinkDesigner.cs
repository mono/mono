/**
 * Namespace:   System.Web.UI.Design.WebControls
 * Class:       HyperLinkDesigner
 *
 * Author:      Gaurav Vaish
 * Maintainer:  gvaish_mono@lycos.com
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI.Design;

namespace System.Web.UI.Design.WebControls
{
	public class HyperLinkDesigner : TextControlDesigner
	{
		public HyperLinkDesigner() : base()
		{
		}

		[MonoTODO]
		public override string GetDesignTimeHtml()
		{
			if(Component != null && Component is HyperLink)
			{
				HyperLink link   = (HyperLink) Component;
				link.Text        = link.Text.Trim();
				link.ImageUrl    = link.ImageUrl.Trim();
				link.NavigateUrl = link.NavigateUrl.Trim();
				bool textOrImage = (link.Text.Length > 0 ||
				                    link.ImageUrl.Length > 0);
				bool nav         = link.NavigateUrl.Length > 0;
				if(!textOrImage)
				{
					link.Text        = "[" + link.ID + "]";
					if(!nav)
					{
						link.NavigateUrl = "url";
					}
				}

				// FIXME: Unable to get the essence of "Remarks"
				// in the MSDN documentation. Need to write a program
				// to test what's happening.
				throw new NotImplementedException();

				//return base.GetDesignTimeHtml();
			}
			return String.Empty;
		}
	}
}
