//
// XmlAttributeEventArgs.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
//

using System.Xml;
using System;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for XmlAttributeEventArgs.
	/// </summary>
	public class XmlAttributeEventArgs : EventArgs
	{
		[MonoTODO]
		public XmlAttribute Attr { 
			get { throw new NotImplementedException(); }
		}
		[MonoTODO]
		public int LineNumber {
			get { throw new NotImplementedException(); }
		}
		[MonoTODO]
		public int LinePosition {
			get { throw new NotImplementedException(); }
		}
		[MonoTODO]
		public object ObjectBeingDeserialized {
			get{ throw new NotImplementedException(); }
		}

	}
}
