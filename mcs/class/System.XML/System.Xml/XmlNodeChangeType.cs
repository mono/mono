//
// XmlNodeChangeType.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
#if NET_1_2

namespace System.Xml
{
	public enum XmlNodeChangeType
	{
		Updated,
		Inserted,
		Deleted,
		Unchanged
	}
}
#endif
