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
		
		[XmlElement("annotation",Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaAnnotation Annotation 
		{ 
			get{ return  annotation; } 
			set{ annotation = value; } 
		}
		
		[System.Xml.Serialization.XmlAttribute("id")]
		public string Id 
		{ 
			get{ return  id; } 
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
