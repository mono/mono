//
// XPathDocument2ChangedEventAction.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
#if NET_1_2

namespace System.Xml
{
	public enum XPathDocument2ChangedEventAction
	{
		Inserted,
		Removed,
		Changed,
		Inserting,
		Removing,
		Changing,
		Rejecting,
		Rejected
	}
}
#endif
