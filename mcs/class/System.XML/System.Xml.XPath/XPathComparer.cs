//
// System.Xml.XPath.XPathComparer
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2003 Atsushi Enomoto
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
			BaseIterator nav1 = o1 as BaseIterator;
			BaseIterator nav2 = o2 as BaseIterator;
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

	internal class XPathNavigatorComparer : IComparer
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
	}
}
