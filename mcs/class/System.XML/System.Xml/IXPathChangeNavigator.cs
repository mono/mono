//
// IXPathChangeNavigator.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
#if NET_2_0

using System;
using System.Collections;

namespace System.Xml
{
	public interface IXPathChangeNavigator
	{
		void AcceptChange ();

		void RejectChange ();

		IEnumerable SelectChanges (XmlChangeFilters changeType);

		IEnumerable SelectChanges ();

	}

}
#endif
