//
// XmlAttributeOverrides.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
//

using System;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for XmlAttributeOverrides.
	/// </summary>
	public class XmlAttributeOverrides
	{
		
		public XmlAttributes this[Type type]
		{
			[MonoTODO]
			get{ throw new NotImplementedException (); }
		}

		public XmlAttributes this[Type type, string member] 
		{
			[MonoTODO]
			get{ throw new NotImplementedException (); }
		}

		[MonoTODO]
		public void Add (Type type, XmlAttributes attributes)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Add( Type type, string member, XmlAttributes attributes)
		{
			throw new NotImplementedException ();
		}

	}
}
