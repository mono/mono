// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaAnnotated.
	/// </summary>
	public class XmlSchemaAnnotated : XmlSchemaObject
	{
		private XmlSchemaAnnotation annotation;
		private string id;
		private XmlAttribute[] unhandledAttributes;

		public XmlSchemaAnnotated()
		{}
		[XmlElement]
		public XmlSchemaAnnotation Annotation 
		{ 
			get{ return this.annotation; } 
			set{ this.annotation = value; } 
		}
		[XmlAttribute]
		public string Id 
		{ 
			get{ return this.id; } 
			set{ id = value; } 
		}
		[XmlAnyAttribute]
		public XmlAttribute[] UnhandledAttributes 
		{ 
			get{ return unhandledAttributes; } 
			set{ unhandledAttributes = value; } 
		}
	}
}
