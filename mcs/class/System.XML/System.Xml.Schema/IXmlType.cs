//
// IXmlType.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell, Inc.
//

#if NET_2_0
namespace System.Xml.Schema
{
	public interface IXmlType
	{
		Type DefaultType { get; }

		XmlQualifiedName QualifiedName { get; }

		XmlValueConverter ValueConverter { get; }
	}
}
#endif
