//
// IXPathNavigator.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
#if NET_1_2

using System;
using System.Collections;

namespace System.Xml
{

	public interface IXPathNavigator
	{
		IXPathNavigator Clone ();

		XmlNodeOrder ComparePosition (IXPathNavigator other);

		bool IsDescendant (IXPathNavigator other);

		bool IsSamePosition (IXPathNavigator other);

		IXPathNavigator MoveTo (IXPathNavigator other);

		IXPathNavigator MoveToAttribute (string localName, string namespaceName, bool atomizedNames);
		IXPathNavigator MoveToChild (string localName, string namespaceName, bool atomizedNames);

		IXPathNavigator MoveToDescendantOf (IXPathNavigator root, string localName, string namespaceName, bool atomizedNames);

		IXPathNavigator MoveToDescendantOf (IXPathNavigator root, XmlInfoItemType type);

		IXPathNavigator MoveToFirstAttribute ();

		IXPathNavigator MoveToFirstChild ();

		IXPathNavigator MoveToFirstNamespace (XmlNamespaceScope scope);

		IXPathNavigator MoveToFirstValue ();

		IXPathNavigator MoveToId (string id);

		IXPathNavigator MoveToNextAttribute ();

		IXPathNavigator MoveToNextNamespace (XmlNamespaceScope scope);

		IXPathNavigator MoveToNextSibling ();

		IXPathNavigator MoveToNextValue ();

		IXPathNavigator MoveToParent ();

		IXPathNavigator MoveToRoot ();

		IXPathNavigator MoveToSibling (string localName, string namespaceName, bool atomizedNames);
		IXPathNavigator MoveToSibling (XmlInfoItemType type);
	}

}
#endif
