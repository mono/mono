//
// XmlMapping.cs: 
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
	/// Summary description for XmlMapping.
	/// </summary>
	public abstract class XmlMapping
	{
		private string elementName;
		private string ns;
		private string typeName;

		public XmlMapping ()
		{
		}

		public string ElementName {
			get { 
				return elementName;
			}
		}
		public string Namespace {
			get {
				return ns;
			}
		}
		public string TypeName {
			get { 
				return typeName; 
			} 
		}
	}
}
