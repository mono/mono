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
using System.Collections;
using System.Collections.Generic;
using Mono.WebBrowser.DOM;

namespace System.Windows.Forms
{
	public class HtmlWindowCollection: ICollection, IEnumerable
	{
		private List<HtmlWindow> windows;
		

		internal HtmlWindowCollection (WebBrowser owner, Mono.WebBrowser.IWebBrowser webHost, IWindowCollection col)
		{
			windows = new List<HtmlWindow>();
			foreach (IWindow window in col)
				windows.Add (new HtmlWindow (owner, webHost, window));
		}
		
		public int Count {
			get {
				return windows.Count;
			}
		}

		public HtmlWindow this [string windowId] {
			get {
				foreach (HtmlWindow window in windows)
					if (window.Name.Equals (windowId))
						return window;
				return null;
			}
		}
		
		public HtmlWindow this [int index] {
			get {
				if (index > windows.Count || index < 0)
					return null;
				return windows [index];
			}
		}

		public IEnumerator GetEnumerator ()
		{
			return windows.GetEnumerator ();
		}

		void ICollection.CopyTo (Array dest, int index)
		{
			windows.CopyTo (dest as HtmlWindow[], index);
		}

		object ICollection.SyncRoot {
			get { return this; }
		}

		bool ICollection.IsSynchronized {
			get { return false; }
		}
	}
}
