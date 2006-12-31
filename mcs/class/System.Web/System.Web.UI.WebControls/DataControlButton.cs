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
using System.Drawing;

namespace System.Web.UI.WebControls
{
	internal class DataControlButton: Button
	{
		Control container;
		
		public DataControlButton (Control container)
		{
			this.container = container;
			CausesValidation = false;
		}
		
		public DataControlButton (Control container, string text, string image, string command, string commandArg, bool allowCallback)
			: this (container)
		{
			Text = text;
			ImageUrl = image;
			CommandName = command;
			CommandArgument = commandArg;
			AllowCallback = allowCallback;
		}
		
		public Control Container {
			get { return container; }
			set { container = value; }
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

		protected internal override void Render (HtmlTextWriter writer)
		{
			if (CommandName.Length > 0 && ButtonType == ButtonType.Link) {
				EnsureForeColor ();
			}

			if (CommandName.Length > 0 || CommandArgument.Length > 0 || ButtonType == ButtonType.Button)
			{
				string postScript = null;
				string callScript = null;
				PostBackOptions ops = null;

				IPostBackContainer pcner = container as IPostBackContainer;
				if (pcner != null) {
					ops = pcner.GetPostBackOptions (this);
					ops.RequiresJavaScriptProtocol = ButtonType == ButtonType.Link;
				}
				else
					ops = GetPostBackOptions ();

				postScript = Page.ClientScript.GetPostBackEventReference (ops, !Page.IsCallback);
				
				if (AllowCallback) {
					ICallbackContainer ccner = container as ICallbackContainer;
					if (ccner != null)
						callScript = ccner.GetCallbackScript (this, CommandName + "$" + CommandArgument);
				}
			
				ControlStyle.AddAttributesToRender (writer);
				
				if (ButtonType == ButtonType.Image) {
					writer.AddAttribute (HtmlTextWriterAttribute.Type, "image");
					writer.AddAttribute (HtmlTextWriterAttribute.Src, ResolveClientUrl (ImageUrl));
					if (callScript != null)
						writer.AddAttribute (HtmlTextWriterAttribute.Onclick, callScript);
					else
						writer.AddAttribute (HtmlTextWriterAttribute.Onclick, postScript);
					if (Text.Length > 0)
						writer.AddAttribute (HtmlTextWriterAttribute.Alt, Text);
					writer.AddStyleAttribute (HtmlTextWriterStyle.BorderWidth, "0px");
					writer.RenderBeginTag (HtmlTextWriterTag.Input);
					writer.RenderEndTag ();
				}
				if (ButtonType == ButtonType.Link) {
					if (callScript != null) {
						writer.AddAttribute (HtmlTextWriterAttribute.Onclick, callScript);
					}
					writer.AddAttribute (HtmlTextWriterAttribute.Href, postScript);
					writer.RenderBeginTag (HtmlTextWriterTag.A);
					writer.Write (Text);
					writer.RenderEndTag ();
				}
				if (ButtonType == ButtonType.Button) {
					if (callScript != null)
						writer.AddAttribute (HtmlTextWriterAttribute.Onclick, callScript);
					else
						writer.AddAttribute (HtmlTextWriterAttribute.Onclick, postScript);

					writer.AddAttribute (HtmlTextWriterAttribute.Type, "button");
					writer.AddAttribute (HtmlTextWriterAttribute.Name, ClientID);
					writer.AddAttribute (HtmlTextWriterAttribute.Value, Text);
					writer.RenderBeginTag (HtmlTextWriterTag.Input);
					writer.RenderEndTag ();
				}
			}
			else {
				if (ImageUrl.Length > 0) {
					ControlStyle.AddAttributesToRender (writer);
					writer.AddAttribute (HtmlTextWriterAttribute.Src, ResolveClientUrl (ImageUrl));
					if (Text.Length > 0)
						writer.AddAttribute (HtmlTextWriterAttribute.Alt, Text);
					writer.AddStyleAttribute (HtmlTextWriterStyle.BorderWidth, "0px");
					writer.RenderBeginTag (HtmlTextWriterTag.Img);
					writer.RenderEndTag ();
				}
				else {
					if (!ControlStyle.IsEmpty) {
						ControlStyle.AddAttributesToRender (writer);
					}
					writer.RenderBeginTag (HtmlTextWriterTag.Span);
					writer.Write (Text);
					writer.RenderEndTag ();
				}
			}
		}
		
		protected override PostBackOptions GetPostBackOptions () {
			PostBackOptions options = new PostBackOptions (this);
			options.Argument = "";
			options.RequiresJavaScriptProtocol = ButtonType == ButtonType.Link;
			options.ClientSubmit = true;
			options.PerformValidation = CausesValidation && Page != null && Page.AreValidatorsUplevel (ValidationGroup);
			if (options.PerformValidation)
				options.ValidationGroup = ValidationGroup;

			return options;
		}

		private void EnsureForeColor () {
			if (ForeColor != Color.Empty)
				return;

			for (Control parent = Parent; parent != null; parent = parent.Parent) {
				WebControl wc = parent as WebControl;
				if (wc != null && wc.ForeColor != Color.Empty) {
					ForeColor = wc.ForeColor;
					break;
				}
				if (parent == container)
					break;
			}
		}
	}
}

#endif
