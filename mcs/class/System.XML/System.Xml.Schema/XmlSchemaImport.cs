// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaImport.
	/// </summary>
	public class XmlSchemaImport : XmlSchemaExternal
	{
		private XmlSchemaAnnotation annotation;
		private string nameSpace;
		public XmlSchemaImport()
		{
		}

		[XmlElement("annotation",Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaAnnotation Annotation 
		{
			get{ return  annotation; } 
			set{ annotation = value; }
		}
		
		[System.Xml.Serialization.XmlAttribute("namespace")]
		public string Namespace 
		{
			get{ return  nameSpace; } 
			set{ nameSpace = value; }
		}
	}
}
