/**
 * Namespace:   System.Web.UI.Design.WebControls
 * Class:       AdRotatorDesigner
 *
 * Author:      Gaurav Vaish
 * Maintainer:  mastergaurav AT users DOT sf DOT net
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Web.UI.WebControls;
using System.Web.UI.Design;

namespace System.Web.UI.Design.WebControls
{
	public class AdRotatorDesigner : ControlDesigner
	{
		public AdRotatorDesigner() : base()
		{
		}

		[MonoTODO]
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

				image.ApplyStyle(toDesign.ControlStyle);
				image.ImageUrl      = String.Empty;
				image.AlternateText = toDesign.ID;
				image.ToolTip       = toDesign.ToolTip;

				link.RenderBeginTag(writer);
				link.RenderControl(writer);
				link.RenderEndTag(writer);

				return writer.ToString();
			}
		}
	}
}
