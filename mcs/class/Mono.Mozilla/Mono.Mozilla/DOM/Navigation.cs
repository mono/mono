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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using System.Runtime.InteropServices;
using System.Text;
using Mono.WebBrowser;
using Mono.WebBrowser.DOM;

namespace Mono.Mozilla.DOM
{
	internal class Navigation: INavigation
	{

		private nsIWebNavigation webNav;
		private IWebBrowser control;
		public Navigation (IWebBrowser control, nsIWebNavigation webNav)
		{
			this.webNav = webNav;
			this.control = control;
		}

		#region INavigation Members

		public bool CanGoBack {
			get {
				bool canGoBack;
				webNav.CanGoBack (out canGoBack);
				return canGoBack;
			}
		}

		public bool CanGoForward {
			get {
				bool canGoForward;
				webNav.CanGoForward (out canGoForward);
				return canGoForward;
			}
		}

		public bool Back ()
		{
			return Base.Back (control);
		}

		public bool Forward ()
		{
			return Base.Forward (control);
		}

		public void Home ()
		{
			Base.Home (control);
		}

		public void Reload ()
		{
			Base.Reload (control, ReloadOption.None);
		}

		public void Reload (ReloadOption option)
		{
			Base.Reload (control, option);
		}

		public void Stop ()
		{
			Base.Stop (control);
		}

		#endregion
	}
}
