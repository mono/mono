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


using System;
using Mono.WebBrowser.DOM;

namespace System.Windows.Forms
{
	public sealed class HtmlHistory : IDisposable
	{
		private bool disposed;
		private Mono.WebBrowser.IWebBrowser webHost;
		private Mono.WebBrowser.DOM.IHistory history;

		internal HtmlHistory (Mono.WebBrowser.IWebBrowser webHost, 
		                      Mono.WebBrowser.DOM.IHistory history)
		{
			this.webHost = webHost;
			this.history = history;
		}
		
		#region IDisposable Members

		private void Dispose (bool disposing)
		{
			if (!disposed) {
				disposed = true;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		#endregion

		#region Properties
		public int Length {
			get { return webHost.Navigation.HistoryCount; }
		}

		[MonoTODO ("Not supported, will throw NotSupportedException")]
		public object DomHistory {
			get { throw new NotSupportedException ("Retrieving a reference to an mshtml interface is not supported. Sorry."); } 
		}
		#endregion


		public void Back (int numberBack)
		{
			history.Back (numberBack);
		}
		
		public void Forward (int numberForward)
		{
			history.Forward (numberForward);
		}
		
		public void Go (int relativePosition)
		{
			history.GoToIndex (relativePosition);
		}

		public void Go (string urlString)
		{
			history.GoToUrl (urlString);
		}

		public void Go (Uri url)
		{
			history.GoToUrl (url.ToString ());
		}
	}
}
