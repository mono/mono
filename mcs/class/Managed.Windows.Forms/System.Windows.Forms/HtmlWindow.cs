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
// Copyright (c) 2007, 2008 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

#if NET_2_0

using System;
using System.Drawing;
using Mono.WebBrowser.DOM;

namespace System.Windows.Forms
{
	public sealed class HtmlWindow {

		private IWindow window;
		private Mono.WebBrowser.IWebBrowser webHost;
		internal HtmlWindow (Mono.WebBrowser.IWebBrowser webHost, IWindow iWindow)
		{
			this.window = iWindow;
			this.webHost = webHost;
		}
	
#region Properties
		public string Name {
			get { return this.window.Name; }
			set { this.window.Name = value; }
		}
		
		public HtmlWindow Parent {
			get { return new HtmlWindow (webHost, this.window.Parent); }
		}
#endregion
		
#region Methods
		public void Alert (string message) 
		{			
			MessageBox.Show ("Alert", message);		
		}

		public bool Confirm (string message) 
		{			
			DialogResult ret = MessageBox.Show (message, "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
			return ret == DialogResult.OK;
		}
		
		public string Prompt (string message, string defaultValue)
		{
			WebBrowserDialogs.Prompt prompt = new WebBrowserDialogs.Prompt ("Prompt", message, defaultValue);
			DialogResult ret = prompt.Show ();
			return prompt.Text;
		}
		
		public void Navigate (string urlString)
		{
			webHost.Navigation.Go (urlString);
		}

		public void Navigate (Uri url)
		{
			webHost.Navigation.Go (url.ToString ());
		}
		
		public void ScrollTo (Point point)
		{
			ScrollTo (point.X, point.Y);
		}

		public void ScrollTo (int x, int y)
		{
			this.window.ScrollTo (x, y);
		}

		[MonoTODO("Blank opens in current window at the moment. Missing media and search implementations. No options implemented")]
		public HtmlWindow Open (Uri url, string target, string options, bool replace)
		{
			return Open (url.ToString(), target, options, replace);
		}

		[MonoTODO("Blank opens in current window at the moment. Missing media and search implementations. No options implemented")]
		public HtmlWindow Open (string url, string target, string options, bool replace) 
		{
			switch (target) {
				case "_blank":
					this.window.Open (url);
				break;
				case "_media":
				break;
				case "_parent":
					this.window.Parent.Open (url);
				break;
				case "_search":
				break;
				case "_self":
					this.window.Open (url);
				break;
				case "_top":
					this.window.Top.Open (url);
				break;
			}
			return this;
		}
		
		[MonoTODO("Opens in current window at the moment.")]
		public HtmlWindow OpenNew (string url, string options)
		{
			return Open (url, "_blank", options, false);
		}

		[MonoTODO("Opens in current window at the moment.")]
		public HtmlWindow OpenNew (Uri url, string options)
		{
			return OpenNew (url.ToString (), options);
		}
		
#endregion
		
		
#region Standard stuff
		public override int GetHashCode () 
		{ 
			return window.GetHashCode (); 
		}
	
		public override bool Equals (object obj) {
			return this == (HtmlWindow) obj;
		}
		
		public static bool operator ==(HtmlWindow left, HtmlWindow right) {
			if ((object)left == (object)right) {
				return true;
			}

			if ((object)left == null || (object)right == null) {
				return false;
			}

			return left.Equals (right); 
		}

		public static bool operator !=(HtmlWindow left, HtmlWindow right) {
			return !(left == right);
		}
#endregion
	}
}

#endif
