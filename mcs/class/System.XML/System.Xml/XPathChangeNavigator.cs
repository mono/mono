//
// XPathChangeNavigator.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
//
#if NET_1_2

using System;
using System.Collections;

namespace System.Xml
{
	public abstract class XPathChangeNavigator 
		: XPathNavigator2, IXPathChangeNavigator
	{
		protected XPathChangeNavigator ()
		{
		}

		public abstract void AcceptChange (); 

		public abstract XmlNodeChangeType NodeChangeType { get; }

		public abstract XPathEditor CreateXmlEditor ();

		public abstract void RejectChange ();

		public abstract IEnumerable SelectChanges (XmlChangeFilters changeTypes);
		public abstract IEnumerable SelectChanges ();
	}
}

#endif
