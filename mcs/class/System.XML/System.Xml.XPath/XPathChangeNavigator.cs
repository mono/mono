//
// XPathChangeNavigator.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
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
