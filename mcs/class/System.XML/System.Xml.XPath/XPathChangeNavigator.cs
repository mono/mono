//
// XPathChangeNavigator.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//
#if NET_2_0

using System;
using System.Collections;

namespace System.Xml.XPath
{
	public abstract class XPathChangeNavigator
		: XPathNavigator, IXPathChangeNavigable
	{
		protected XPathChangeNavigator ()
		{
		}

		// Properties

		public abstract XmlNodeChangeType NodeChangeType { get; }

		public abstract object OriginalTypedValue { get; }

		public abstract string OriginalValue { get; }

		// Methods

		public virtual void AcceptChange ()
		{
			AcceptChange (XmlChangeFilters.AllChanges);
		}

		public abstract void AcceptChange (XmlChangeFilters filters);

		public virtual void AcceptChangesOnSubtree ()
		{
			AcceptChangesOnSubtree (XmlChangeFilters.AllChanges);
		}

		[MonoTODO]
		public virtual void AcceptChangesOnSubtree (XmlChangeFilters filters)
		{
			throw new NotImplementedException ();
		}

		public virtual XPathChangeNavigator CreateChangeNavigator ()
		{
			return (XPathChangeNavigator) Clone ();
		}

		public virtual void RejectChange ()
		{
			RejectChange (XmlChangeFilters.AllChanges);
		}

		public abstract void RejectChange (XmlChangeFilters filters);

		public virtual XPathNodeIterator SelectChanges ()
		{
			return SelectChanges (XmlChangeFilters.AllChanges);
		}

		public abstract XPathNodeIterator SelectChanges (XmlChangeFilters changeTypes);
	}
}

#endif
