//
// System.Xml.XPath.XPathComparer
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2003 Atsushi Enomoto
//

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
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace System.Xml.XPath
{
	internal class XPathIteratorComparer : IComparer
	{
		public static XPathIteratorComparer Instance = new XPathIteratorComparer ();
		private XPathIteratorComparer ()
		{
		}

		public int Compare (object o1, object o2)
		{
			XPathNodeIterator nav1 = o1 as XPathNodeIterator;
			XPathNodeIterator nav2 = o2 as XPathNodeIterator;
			if (nav1 == null)
				return -1;
			if (nav2 == null)
				return 1;
			switch (nav1.Current.ComparePosition (nav2.Current)) {
			case XmlNodeOrder.Same:
				return 0;
			case XmlNodeOrder.After:
				return -1;
			default:
				return 1;
			}
		}
	}

#if NET_2_0
	internal class XPathNavigatorComparer : IComparer, IEqualityComparer
#else
	internal class XPathNavigatorComparer : IComparer
#endif
	{
		public static XPathNavigatorComparer Instance = new XPathNavigatorComparer ();
		private XPathNavigatorComparer ()
		{
		}

		public int Compare (object o1, object o2)
		{
			XPathNavigator nav1 = o1 as XPathNavigator;
			XPathNavigator nav2 = o2 as XPathNavigator;
			if (nav1 == null)
				return -1;
			if (nav2 == null)
				return 1;
			switch (nav1.ComparePosition (nav2)) {
			case XmlNodeOrder.Same:
				return 0;
			case XmlNodeOrder.After:
				return 1;
			default:
				return -1;
			}
		}

#if NET_2_0
		bool IEqualityComparer.Equals (object o1, object o2)
		{
			XPathNavigator nav1 = o1 as XPathNavigator;
			XPathNavigator nav2 = o2 as XPathNavigator;
			return nav1 != null && nav2 != null && nav1.IsSamePosition (nav2);
		}

		int IEqualityComparer.GetHashCode (object obj)
		{
			return obj.GetHashCode ();
		}
#endif
	}
}
