/**
 * Namespace: System.Web.UI.WebControls
 * Class:     LinkButtonInternal
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Drawing;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	internal class LinkButtonInternal : LinkButton
	{
		public LinkButtonInternal() : base()
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
				Color   foreCol;
				int     ctr = 0;
				//FIXME: this-> LinkButton-> WebControl
				while(ctr < 2)
				{
					ctrl = ctrl.Parent;
					foreCol = ((WebControl)ctrl).ForeColor;
					if(foreCol != Color.Empty)
					{
						ForeColor = foreCol;
						return;
					}
					ctr++;
				}
			}
		}
	}
}
