/**
 * Namespace:   System.Web.UI.Design.WebControls
 * Class:       ButtonDesigner
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
	public class ButtonDesigner : TextControlDesigner
	{
		public ButtonDesigner(): base()
		{
		}

		public override string GetDesignTimeHtml()
		{
			if(Component != null && Component is Button)
			{
				Button btn = (Button)Component;
				btn.Text   = btn.Text.Trim();
				if(btn.Text.Length == 0)
				{
					btn.Text = "[" + btn.ID + "]";
				}
				return base.GetDesignTimeHtml();
			}
			return String.Empty;
		}
	}
}
