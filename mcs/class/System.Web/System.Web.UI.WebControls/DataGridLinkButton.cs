/**
 * Namespace: System.Web.UI.WebControls
 * Class:     DataGridLinkButton
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish_mono@lycos.com
 * Contact: <gvaish_mono@lycos.com>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	class DataGridLinkButton : LinkButton
	{
		public DataGridLinkButton() : base()
		{
		}

		protected override void Render(HtmlTextWriter writer)
		{
			SetForeColor();
			base.Render(writer);
		}

		private void SetForeColor()
		{
			if(!ControlStyle.IsSet(System.Web.UI.WebControls.Style.FORECOLOR))
			{
				Control ctrl = this;
				int level = 0;
				while(level < 3)
				{
					ctrl = ctrl.Parent;
					Color foreColor = ((WebControl)ctrl).ForeColor;
					if(foreColor != Color.Empty)
					{
						ForeColor = foreColor;
						return;
					}
					level++;
				}
			}
		}
	}
}
