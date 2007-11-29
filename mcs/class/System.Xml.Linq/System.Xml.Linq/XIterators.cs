//
// Authors:
//   Atsushi Enomoto
//
// Copyright 2007 Novell (http://www.novell.com)
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using XPI = System.Xml.Linq.XProcessingInstruction;


namespace System.Xml.Linq
{
	/*
	// Iterators
	internal class XFilterIterator <T> : IEnumerable <T>
	{
		IEnumerable <object> source;
		XName name;

		public XFilterIterator (IEnumerable <object> source, XName name)
		{
			this.source = source;
			this.name = name;
		}

		public IEnumerator <T> GetEnumerator ()
		{
			foreach (object o in source) {
				if (! (o is T))
					continue;
				if (name != null) {
					if (o is XElement) {
						if (((XElement) o).Name != name)
							continue;
					}
					else if (o is XAttribute) {
						if (((XAttribute) o).Name != name)
							continue;
					}
				}
				yield return (T) o;
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
	}

	internal class XDescendantIterator <T> : IEnumerable <T>
	{
		IEnumerable <T> source;

		public XDescendantIterator (IEnumerable <object> source)
		{
			this.source = new XFilterIterator <T> (source, null);
		}

		public IEnumerator <T> GetEnumerator ()
		{
			foreach (T t1 in source) {
				yield return t1;
				XContainer xc = t1 as XContainer;
				if (xc == null || xc.FirstChild == null)
					continue;
				foreach (T t2 in new XDescendantIterator <T> (xc.Content ()))
					yield return t2;
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
	}
	*/

#if !LIST_BASED
	internal class XChildrenIterator : IEnumerable <object>
	{
		XContainer source;
		XNode n;

		public XChildrenIterator (XContainer source)
		{
			this.source = source;
		}

		public IEnumerator <object> GetEnumerator ()
		{
			if (n == null) {
				n = source.FirstNode;
				if (n == null)
					yield break;
			}
			do {
				yield return n;
				n = n.NextNode;
			} while (n != source.LastNode);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
	}
#endif
}

#endif
