
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
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
