//
// IXPathEditable.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
#if NET_2_0

using System;

namespace System.Xml.XPath
{
	public interface IXPathEditable
	{
		XPathEditableNavigator CreateEditor ();
	}
}
#endif
