//
// XPathDocumentNodeChangedAction.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
#if NET_2_0

namespace System.Xml.XPath
{
	public enum XPathDocumentNodeChangedAction
	{
		Deleteed,
		Deleteing,
		Inserted,
		Inserting,
		Rejected,
		Rejecting,
		Updated,
		Updating,
	}
}
#endif
