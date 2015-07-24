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
//	Andreia Gaita	<avidigal@novell.com>


using System;
using System.Collections;
using System.Collections.Generic;
using Mono.WebBrowser.DOM;

namespace System.Windows.Forms
{
	public sealed class HtmlElementCollection: ICollection, IEnumerable
	{
		private List<HtmlElement> elements;
		private Mono.WebBrowser.IWebBrowser webHost;
		private WebBrowser owner;

		internal HtmlElementCollection (WebBrowser owner, Mono.WebBrowser.IWebBrowser webHost, IElementCollection col)
		{
			elements = new List<HtmlElement>();
			foreach (IElement elem in col) {
				elements.Add (new HtmlElement (owner, webHost, elem));
			}
			this.webHost = webHost;
			this.owner = owner;
		}

		private HtmlElementCollection (WebBrowser owner, Mono.WebBrowser.IWebBrowser webHost, List<HtmlElement> elems)
		{
			elements = elems;
			this.webHost = webHost;
			this.owner = owner;
		}

		public int Count
		{
			get { 
				return elements.Count;
			}
		}

		public HtmlElement this[string elementId]
		{
			get {
				foreach (HtmlElement element in elements) {
					if (element.Id.Equals (elementId))
						return element;
				}
				return null;
			}
		}

		public HtmlElement this[int index]
		{
			get {
				if (index > elements.Count || index < 0)
					return null;
				return elements[index];
			}
		}

		public HtmlElementCollection GetElementsByName (string name)
		{
			List<HtmlElement> elems = new List<HtmlElement>();

			foreach (HtmlElement elem in elements) {
				if (elem.HasAttribute ("name") && elem.GetAttribute ("name").Equals (name)) {
					elems.Add (new HtmlElement (owner, webHost, elem.element));
				}
			}
			return new HtmlElementCollection (owner, webHost, elems);
		}

		
		public IEnumerator GetEnumerator ()
		{
			return elements.GetEnumerator ();
		}

		void ICollection.CopyTo (Array dest, int index)
		{
			elements.CopyTo (dest as HtmlElement[], index);
		}

		object ICollection.SyncRoot
		{
			get { return this; }
		}

		bool ICollection.IsSynchronized
		{
			get { return false; }
		}
	}
}
