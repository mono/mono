/**
 * Namespace:   System.Web.UI.Design.WebControls
 * Class:       PanelDesigner
 *
 * Author:      Gaurav Vaish
 * Maintainer:  gvaish_mono@lycos.com
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Web.UI.WebControls;
using System.Web.UI.Design;

namespace System.Web.UI.Design.WebControls
{
	public class PanelDesigner : ReadWriteControlDesigner
	{
		public PanelDesigner()
		{
		}
		
		protected override void MapPropertyToStyle(string propName,
		                                           object varPropValue)
		{
			if(propName != null)
			{
				try
				{
					bool hasBeenSet = false;
					if(propName == "BackImageUrl")
					{
						string url = varPropValue.ToString();
						if(url.Length > 0)
							url = "url(" + url + ")";
						// FIXME: CSS Specs read "background-image",
						// while MS implementation puts "backgroundImage".
						// Is it a MS implementation bug?
						Behavior.SetStyleAttribute("backgroundImage",
						                            true, url, true);
						hasBeenSet = true;
					} else if(propName == "HorizonalAlign")
					{
						HorizontalAlign alignment = (HorizontalAlign)varPropValue;
						if(alignment != HorizontalAlign.NotSet)
						{
							string value = Enum.Format(typeof(HorizontalAlign), 
							                           varPropValue, "G");
							Behavior.SetStyleAttribute("textAlign",
							                            true, value, true);
							hasBeenSet = true;
						}
					}
					if(!hasBeenSet)
						base.MapPropertyToStyle(propName, varPropValue);
						
				} catch(Exception) { }
			}
		}
		
		protected override void OnBehaviorAttached()
		{
			base.OnBehaviorAttached();
			Panel toDesign = (Panel)Component;
			MapPropertyToStyle("BackImageUrl", toDesign.BackImageUrl);
			MapPropertyToStyle("HorizontalAlign", toDesign.HorizontalAlign);
		}
	}
}
