// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaIdentityConstraint.
	/// </summary>
	public class XmlSchemaIdentityConstraint : XmlSchemaAnnotated
	{
		private XmlSchemaObjectCollection fields;
		private string name;
		private XmlQualifiedName qName;
		private XmlSchemaXPath selector;

		public XmlSchemaIdentityConstraint()
		{
			fields = new XmlSchemaObjectCollection();
			qName = XmlQualifiedName.Empty;
		}

		[XmlElement("field",typeof(XmlSchemaXPath),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaObjectCollection Fields 
		{
			get{ return fields; }
		}

		[XmlElement("selector",typeof(XmlSchemaXPath),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaXPath Selector 
		{
			get{ return  selector; } 
			set{ selector = value; }
		}
		
		[System.Xml.Serialization.XmlAttribute("name")]
		public string Name 
		{
			get{ return  name; } 
			set{ name = value; }
		}
		
		[XmlIgnore]
		public XmlQualifiedName QualifiedName 
		{
			get{ return  qName; }
		}
	}
}
