/**
 * Namespace:   System.Web.UI.Design.WebControls
 * Class:       CheckboxDesigner
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
	public class CheckBoxDesigner : ControlDesigner
	{
		public CheckBoxDesigner() : base()
		{
		}
		
		public override string GetDesignTimeHtml()
		{
			if(Component != null && Component is CheckBox)
			{
				CheckBox cbx = (CheckBox) Component;
				cbx.Text     = cbx.Text.Trim();
				if(cbx.Text.Length == 0)
				{
					cbx.Text = "[" + cbx.ID + "]";
				}
				return base.GetDesignTimeHtml();
			}
			return String.Empty;
		}
	}
}
