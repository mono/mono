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
			items = new XmlSchemaObjectCollection();
		}

		[System.Xml.Serialization.XmlAttribute("id")]
		public string Id 
		{
			get{ return  id; } 
			set{ id = value; }
		}
		
		[XmlElement("appinfo",typeof(XmlSchemaAppInfo),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("documentation",typeof(XmlSchemaDocumentation),Namespace="http://www.w3.org/2001/XMLSchema")]
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
