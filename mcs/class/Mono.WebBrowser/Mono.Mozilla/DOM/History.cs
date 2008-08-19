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
// Copyright (c) 2008 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections;
using Mono.WebBrowser;
using Mono.WebBrowser.DOM;

namespace Mono.Mozilla.DOM
{
	internal class History : DOMObject, IHistory
	{
		private Navigation navigation;
		
		public History (WebBrowser control, Navigation navigation)
			: base (control)
		{
			this.navigation = navigation;
		}
		
		public int Count { 
			get { return navigation.HistoryCount; }
		}
		
		public void Back (int count)
		{
			navigation.Go (count * -1, true);
		}
		
		public void Forward (int count)
		{
			navigation.Go (count, true);
		}
			
		public void GoToIndex (int index)
		{
			navigation.Go  (index);
		}
		public void GoToUrl (string url)
		{
			int index = -1;
			nsISHistory history;
			navigation.navigation.getSessionHistory(out history);
			int count = Count;
			nsIHistoryEntry entry;
			for (int i = 0; i < count; i++) {
				nsIURI uri;
				history.getEntryAtIndex(i, false, out entry);
				entry.getURI (out uri);
				AsciiString spec = new AsciiString(String.Empty);
				uri.getSpec (spec.Handle);
				if (string.Compare (spec.ToString (), url, true) == 0) {
					index = i;
					break;
				}
			}
			if (index > -1)
				this.GoToIndex (index);
		}
	}
}
