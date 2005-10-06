//
// System.Web.UI.WebControls.DataControlButton.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.Web;
using System.Web.UI;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Web.UI.WebControls
{
	internal class DataControlButton: Button
	{
		Control container;
		
		public DataControlButton (Control container)
		{
			this.container = container;
		}
		
		public DataControlButton (Control container, string text, string image, string command, string commandArg, bool allowCallback)
		{
			this.container = container;
			Text = text;
			ImageUrl = image;
			CommandName = command;
			CommandArgument = commandArg;
			AllowCallback = allowCallback;
		}
		
		public string ImageUrl {
			get {
				object o = ViewState["iu"];
				if (o != null) return (string) o;
				return String.Empty;
			}
			set {
				ViewState["iu"] = value;
			}
		}
		
		public bool AllowCallback {
			get {
				object o = ViewState["ac"];
				if (o != null) return (bool) o;
				return true;
			}
			set {
				ViewState["ac"] = value;
			}
		}
		
		public virtual ButtonType ButtonType {
			get {
				object ob = ViewState ["ButtonType"];
				if (ob != null) return (ButtonType) ob;
				return ButtonType.Link;
			}
			set {
				ViewState ["ButtonType"] = value;
			}
		}

		protected override void Render (HtmlTextWriter writer)
		{
			if (CommandName.Length > 0 || ButtonType == ButtonType.Button)
			{
				string postScript = null;
				string callScript = null;
				
				IPostBackContainer pcner = container as IPostBackContainer;
				if (pcner != null) {
					PostBackOptions ops = pcner.GetPostBackOptions (this);
					postScript = container.Page.ClientScript.GetPostBackEventReference (ops);
				} else
					postScript = Page.ClientScript.GetPostBackClientEvent (this, "");

				if (CausesValidation && Page.Validators.Count > 0) {
					postScript = Utils.GetClientValidatedEvent (Page) + postScript;
				}
				
				if (AllowCallback) {
					ICallbackContainer ccner = container as ICallbackContainer;
					if (ccner != null)
						callScript = ccner.GetCallbackScript (this, CommandName + "$" + CommandArgument);
				}
			
				if (ButtonType == ButtonType.Link || ButtonType == ButtonType.Image)
				{
					if (ImageUrl.Length > 0) {
						writer.AddAttribute (HtmlTextWriterAttribute.Type, "image");
						writer.AddAttribute (HtmlTextWriterAttribute.Src, ResolveUrl (ImageUrl));
						if (callScript != null)
							writer.AddAttribute (HtmlTextWriterAttribute.Onclick, callScript);
						else
							writer.AddAttribute (HtmlTextWriterAttribute.Onclick, postScript);
						if (Text.Length > 0)
							writer.AddAttribute (HtmlTextWriterAttribute.Alt, Text);
						writer.RenderBeginTag (HtmlTextWriterTag.Input);
						writer.RenderEndTag ();
					}
					else {
						if (callScript != null) {
							writer.AddAttribute (HtmlTextWriterAttribute.Onclick, callScript);
							writer.AddAttribute (HtmlTextWriterAttribute.Href, "javascript:");
						}
						else
							writer.AddAttribute (HtmlTextWriterAttribute.Href, "javascript:" + postScript);
						writer.RenderBeginTag (HtmlTextWriterTag.A);
						writer.Write (Text);
						writer.RenderEndTag ();
					}
				}
				else if (ButtonType == ButtonType.Button)
				{
					if (callScript != null)
						writer.AddAttribute (HtmlTextWriterAttribute.Onclick, callScript);
					else
						writer.AddAttribute (HtmlTextWriterAttribute.Onclick, postScript);
						
					writer.AddAttribute (HtmlTextWriterAttribute.Type, "submit");
					writer.AddAttribute (HtmlTextWriterAttribute.Name, ClientID);
					writer.AddAttribute (HtmlTextWriterAttribute.Value, Text);
					writer.RenderBeginTag (HtmlTextWriterTag.Input);
					writer.RenderEndTag ();
				}
			} else {
				if (ImageUrl.Length > 0) {
					writer.AddAttribute (HtmlTextWriterAttribute.Src, ResolveUrl (ImageUrl));
					if (Text.Length > 0)
						writer.AddAttribute (HtmlTextWriterAttribute.Alt, Text);
					writer.RenderBeginTag (HtmlTextWriterTag.Img);
					writer.RenderEndTag ();
				}
				else {
					writer.Write (Text);
				}
			}
		}
	}
}

#endif
