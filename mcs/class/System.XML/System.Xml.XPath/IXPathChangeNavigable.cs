//
// IXPathChangeNavigable.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
#if NET_2_0

using System;
using System.Collections;

namespace System.Xml.XPath
{
	public interface IXPathChangeNavigable
	{
		XPathChangeNavigator CreateChangeNavigator ();
	}

}
#endif
