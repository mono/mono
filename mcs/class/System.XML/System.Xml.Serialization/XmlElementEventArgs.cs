//
// XmlElementEventArgs.cs: 
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
	/// Summary description for XmlElementEventArgs.
	/// </summary>
	public class XmlElementEventArgs : EventArgs
	{
		[MonoTODO]
		public XmlElement Element { 
			get{ throw new NotImplementedException(); }
		}
		[MonoTODO]
		public int LineNumber {
			get{ throw new NotImplementedException(); }
		}
		[MonoTODO]
		public int LinePosition {
			get{ throw new NotImplementedException(); }
		}
		[MonoTODO]
		public object ObjectBeingDeserialized {
			get{ throw new NotImplementedException(); }
		}
	}
}
