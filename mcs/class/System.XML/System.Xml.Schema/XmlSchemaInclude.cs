// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaInclude.
	/// </summary>
	public class XmlSchemaInclude : XmlSchemaExternal
	{
		private XmlSchemaAnnotation annotation;
		public XmlSchemaInclude()
		{
		}
		[XmlElement]
		public XmlSchemaAnnotation Annotation 
		{
			get{ return  annotation; } 
			set{ annotation = value; }
		}
	}
}
