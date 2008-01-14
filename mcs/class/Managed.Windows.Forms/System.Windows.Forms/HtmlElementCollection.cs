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
//	Andreia Gaita	<avidigal@novell.com>

#if NET_2_0

using System;
using System.Collections;
using Mono.WebBrowser.DOM;

namespace System.Windows.Forms
{
	[MonoTODO ("Needs Implementation")]
	public sealed class HtmlElementCollection: ICollection, IEnumerable
	{
		private IElementCollection collection;
		private HtmlElement[] elements;

		internal HtmlElementCollection (IElementCollection col)
		{
			collection = col;
		}

		private HtmlElementCollection (HtmlElement[] elems)
		{
			elements = elems;
		}

		public int Count
		{
			get { 
				if (collection != null)
					return collection.Count;
				if (elements != null)
					return elements.Length;
				return 0;
			}
		}

		public HtmlElement this[string elementId]
		{
			get {
				if (collection != null) {
					foreach (IElement elem in collection) {
						if (elem.HasAttribute ("id") && elem.GetAttribute ("id").Equals (elementId)) {
							return new HtmlElement (elem);
						}
					}					
				}
				if (elements != null) {
					for (int i = 0; i < elements.Length; i++) {
						if (elements[i].Id.Equals (elementId))
							return elements[i];
					}
				}
				return null;
			}
		}

		public HtmlElement this[int index]
		{
			get {
				if (collection != null)
					return new HtmlElement(collection[index]);
				if (elements != null)
					return elements[index];
				return null;
			}
		}

		public HtmlElementCollection GetElementsByName (string name)
		{
			HtmlElement[] elems = new HtmlElement[this.Count];
			int count = 0;
			foreach (IElement elem in collection) {
				if (elem.HasAttribute ("name") && elem.GetAttribute ("name").Equals (name)) {
					elems[count] = new HtmlElement (elem);
					count++;
				}
			}
			HtmlElement[] resized = new HtmlElement [count];
			elems.CopyTo (resized, 0);
			return new HtmlElementCollection (resized);
		}

		[MonoTODO ("Needs Implementation")]
		public IEnumerator GetEnumerator ()
		{
			return null;
		}

		void ICollection.CopyTo (Array dest, int index)
		{
			throw new NotImplementedException ();
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

#endif