//
// System.Web.UI.WebControls.DataControlButton.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005-2010 Novell, Inc (http://www.novell.com)
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
using System;
using System.Web;
using System.Web.UI;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;

namespace System.Web.UI.WebControls
{
	internal interface IDataControlButton : IButtonControl
	{
		Control Container { get; set;}
		string ImageUrl { get;set;}
		bool AllowCallback { get; set;}
		ButtonType ButtonType { get;}
	}

	[SupportsEventValidation]
	internal class DataControlButton : Button, IDataControlButton
	{
		public static IDataControlButton CreateButton (ButtonType type, Control container, string text, string image, string command, string commandArg, bool allowCallback)
		{
			IDataControlButton btn;

			switch (type) {
				case ButtonType.Link:
					btn = new DataControlLinkButton ();
					break;
				case ButtonType.Image:
					btn = new DataControlImageButton ();
					btn.ImageUrl = image;
					break;
				default:
					btn = new DataControlButton ();
					break;
			}

			btn.Container = container;
			btn.CommandName = command;
			btn.CommandArgument = commandArg;
			btn.Text = text;
			btn.CausesValidation = false;
			btn.AllowCallback = allowCallback;

			return btn;
		}

		Control _container;

		public Control Container {
			get { return _container; }
			set { _container = value; }
		}

		public string ImageUrl {
			get { return String.Empty; }
			set { }
		}

		public bool AllowCallback {
			get { return ViewState.GetBool ("AllowCallback", true); }
			set { ViewState ["AllowCallback"] = value; }
		}

		public ButtonType ButtonType {
			get { return ButtonType.Button; }
		}

		public override bool UseSubmitBehavior {
			get { return false; }
			set { throw new NotSupportedException (); }
		}

		internal override string GetClientScriptEventReference ()
		{
			if (AllowCallback) {
				ICallbackContainer ccner = Container as ICallbackContainer;
				if (ccner != null)
					return ccner.GetCallbackScript (this, CommandName + "$" + CommandArgument);
			}
			return base.GetClientScriptEventReference ();
		}

		protected override PostBackOptions GetPostBackOptions ()
		{
			IPostBackContainer pcner = Container as IPostBackContainer;
			if (pcner != null)
				return pcner.GetPostBackOptions (this);
			return base.GetPostBackOptions ();
		}
	}

	internal class DataControlLinkButton : LinkButton, IDataControlButton
	{
		Control _container;

		public Control Container {
			get { return _container; }
			set { _container = value; }
		}

		public string ImageUrl {
			get { return String.Empty; }
			set { }
		}

		public bool AllowCallback {
			get { return ViewState.GetBool ("AllowCallback", true); }
			set { ViewState ["AllowCallback"] = value; }
		}

		public ButtonType ButtonType {
			get { return ButtonType.Link; }
		}

		protected internal override void Render (HtmlTextWriter writer)
		{
			EnsureForeColor ();
			if (AllowCallback) {
				ICallbackContainer ccner = Container as ICallbackContainer;
				if (ccner != null)
					OnClientClick = ClientScriptManager.EnsureEndsWithSemicolon (OnClientClick) + ccner.GetCallbackScript (this, CommandName + "$" + CommandArgument);
			}

			base.Render (writer);
		}

		void EnsureForeColor ()
		{
			if (ForeColor != Color.Empty)
				return;

			for (Control parent = Parent; parent != null; parent = parent.Parent) {
				WebControl wc = parent as WebControl;
				if (wc != null && wc.ForeColor != Color.Empty) {
					ForeColor = wc.ForeColor;
					break;
				}
				if (parent == Container)
					break;
			}
		}

		protected override PostBackOptions GetPostBackOptions ()
		{
			IPostBackContainer pcner = Container as IPostBackContainer;
			if (pcner != null)
				return pcner.GetPostBackOptions (this);
			return base.GetPostBackOptions ();
		}

	}

	internal class DataControlImageButton : ImageButton, IDataControlButton
	{
		Control _container;

		public Control Container {
			get { return _container; }
			set { _container = value; }
		}

		public bool AllowCallback {
			get { return ViewState.GetBool ("AllowCallback", true); }
			set { ViewState ["AllowCallback"] = value; }
		}

		public ButtonType ButtonType {
			get { return ButtonType.Image; }
		}

		internal override string GetClientScriptEventReference ()
		{
			if (AllowCallback) {
				ICallbackContainer ccner = Container as ICallbackContainer;
				if (ccner != null)
					return ccner.GetCallbackScript (this, CommandName + "$" + CommandArgument);
			}
			return base.GetClientScriptEventReference ();
		}

		protected override PostBackOptions GetPostBackOptions ()
		{
			IPostBackContainer pcner = Container as IPostBackContainer;
			if (pcner != null)
				return pcner.GetPostBackOptions (this);
			return base.GetPostBackOptions ();
		}

	}
}

