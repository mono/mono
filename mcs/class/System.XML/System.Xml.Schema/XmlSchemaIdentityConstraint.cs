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
		[XmlElement]
		public XmlSchemaObjectCollection Fields 
		{
			get{ return fields; }
		}
		[XmlAttribute]
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
		[XmlElement]
		public XmlSchemaXPath Selector 
		{
			get{ return  selector; } 
			set{ selector = value; }
		}
	}
}
