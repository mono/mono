/**
 * Namespace:   System.Web.UI.Design.WebControls
 * Class:       AdRotatorDesigner
 *
 * Author:      Gaurav Vaish
 * Maintainer:  gvaish_mono@lycos.com
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.IO;
using System.Web.UI.WebControls;
using System.Web.UI.Design;

namespace System.Web.UI.Design.WebControls
{
	public class AdRotatorDesigner : ControlDesigner
	{
		public AdRotatorDesigner() : base()
		{
		}

		public override string GetDesignTimeHtml()
		{
			if(Component != null && Component is AdRotator)
			{
				AdRotator      toDesign = (AdRotator)Component;
				HtmlTextWriter writer   = new HtmlTextWriter(new StringWriter());
				HyperLink      link     = new HyperLink();
				Image          disp     = new Image();

				link.ID          = toDesign.ID;
				link.NavigateUrl = String.Empty;
				link.Target      = toDesign.Target;
				link.AccessKey   = toDesign.AccessKey;
				link.Enabled     = toDesign.Enabled;
				link.TabIndex    = toDesign.TabIndex;

				disp.ApplyStyle(toDesign.ControlStyle);
				disp.ImageUrl      = String.Empty;
				disp.AlternateText = toDesign.ID;
				disp.ToolTip       = toDesign.ToolTip;

				link.RenderBeginTag(writer);
				link.RenderControl(writer);
				link.RenderEndTag(writer);

				return writer.ToString();
			}
			return String.Empty;
		}
	}
}
