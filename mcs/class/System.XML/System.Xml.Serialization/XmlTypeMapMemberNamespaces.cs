//
// XmlTypeMapMemberNamespaces.cs: 
//
// Author:
//   Atsushi Enomoto
//
// (C) 2003 Atsushi Enomoto
//

using System;
using System.Xml.Schema;

namespace System.Xml.Serialization
{
	// XmlTypeMapMemberNamespaces
	// A member of a class that must be used to add namespace declarations.
	// It must be limited at most 1 in a class.

	internal class XmlTypeMapMemberNamespaces: XmlTypeMapMember
	{
		public XmlTypeMapMemberNamespaces()
		{
		}
	}
}
