/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls.Adapters
 * Class     : HtmlCommandAdapter
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System;
using System.Collections.Specialized;
using System.Web.Mobile;
using System.Web.UI.MobileControls;

namespace System.Web.UI.MobileControls.Adapters
{
	public class HtmlCommandAdapter : HtmlControlAdapter
	{
		public HtmlCommandAdapter() : base()
		{
		}

		protected new Command Control
		{
			get
			{
				return base.Control as Command;
			}
		}

		public override bool LoadPostData(string postKey,
		               NameValueCollection data,
		               object privateControlData, out bool dataChanged)
		{
			dataChanged = false;
			bool retVal = false;
			if(Control != null)
			{
				string id = Control.UniqueID;
				string ctrl = data[id + ".x"];
				string evnt = data[id + ".y"];
				if(ctrl != null && evnt != null && ctrl.Length > 0 && evnt.Length > 0)
				{
					dataChanged = true;
					retVal = true;
				}
			}
			return retVal;
		}

		public override void Render(HtmlMobileTextWriter writer)
		{
			bool supportsImgSbt = false;
			bool supportsJavascript = false;
			Form mobileForm = null;

			if(Control != null)
			{
				if(Control.ImageUrl != String.Empty && Device.SupportsImageSubmit)
				{
					supportsImgSbt = true;
					if(Control.Format == CommandFormat.Link)
					{
						if(Device.JavaScript)
						{
							supportsJavascript = true;
						}
					}
				}
			}
			if(supportsJavascript)
			{
				writer.EnterStyle(Style);
				mobileForm = Control.Form;
				if(mobileForm.Action.Length > 0)
				{
					writer.Write("<a href=\"javascript:document.");
					writer.Write(mobileForm.ClientID);
					writer.Write(".submit()\"");
					base.AddAttributes(writer);
					writer.Write(">");
					writer.WriteText(Control.Text, false);
					writer.WriteEndTag("a");
				} else
				{
					base.RenderBeginLink(writer, Constants.FormIDPrefix + mobileForm.UniqueID);
					writer.WriteText(Control.Text, true);
					base.RenderEndLink(writer);
				}
				writer.ExitStyle(Style, Control.BreakAfter);
			} else
			{
				writer.EnterLayout(Style);
				writer.WriteBeginTag("input");
				writer.WriteAttribute("name", Control.UniqueID);
				if(supportsImgSbt)
				{
					writer.WriteAttribute("type", "image");
					writer.WriteAttribute("src", Control.ResolveUrl(Control.ImageUrl));
					writer.WriteAttribute("alt", Control.Text);
				} else
				{
					writer.WriteAttribute("type", "submit");
					writer.Write("value=\"");
					writer.WriteText(Control.Text, true);
				}
				writer.Write("\"");
				base.AddAttributes(writer);
				writer.Write("/>");
				writer.ExitLayout(Style, Control.BreakAfter);
			}
		}
	}
}
