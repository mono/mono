// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaAttributeGroup.
	/// </summary>
	public class XmlSchemaAttributeGroup : XmlSchemaAnnotated
	{
		private XmlSchemaAnyAttribute any;
		private XmlSchemaObjectCollection attributes;
		private string name;
		private XmlSchemaAttributeGroup redefined;

		public XmlSchemaAttributeGroup()
		{
			attributes  = new XmlSchemaObjectCollection();
			//FIXME:
			redefined  = this;
		}

		[XmlElement("anyAttribute",Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaAnyAttribute AnyAttribute 
		{
			get{ return any;}
			set{ any = value;}
		}

		[XmlElement("attribute",typeof(XmlSchemaAttribute),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("attributeGroup",typeof(XmlSchemaAttributeGroupRef),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaObjectCollection Attributes 
		{
			get{ return attributes;}
		}

		[System.Xml.Serialization.XmlAttribute("name")]
		public string Name 
		{
			get{ return name;}
			set{ name = value;}
		}

		//Undocumented property
		[XmlIgnore]
		public XmlSchemaAttributeGroup RedefinedAttributeGroup 
		{
			get{ return redefined;}
		}
	}
}
