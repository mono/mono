// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaAnnotation.
	/// </summary>
	public class XmlSchemaAnnotation : XmlSchemaObject
	{
		private string id;
		private XmlSchemaObjectCollection items;
		private XmlAttribute[] unhandledAttributes;

		public XmlSchemaAnnotation()
		{
		}
		[XmlAttribute]
		public string Id 
		{
			get{ return  id; } 
			set{ id = value; }
		}
		[XmlElement]
		public XmlSchemaObjectCollection Items 
		{
			get{ return items; }
		}
		[XmlAnyAttribute]
		public XmlAttribute[] UnhandledAttributes 
		{
			get{ return  unhandledAttributes; } 
			set{ unhandledAttributes = value; }
		}
	}
}
