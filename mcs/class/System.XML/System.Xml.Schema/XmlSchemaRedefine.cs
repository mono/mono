// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaRedefine.
	/// </summary>
	public class XmlSchemaRedefine : XmlSchemaExternal
	{
		private XmlSchemaObjectTable attributeGroups;
		private XmlSchemaObjectTable groups;
		private XmlSchemaObjectCollection items;
		private XmlSchemaObjectTable schemaTypes;

		public XmlSchemaRedefine()
		{
			attributeGroups = new XmlSchemaObjectTable();
			groups = new XmlSchemaObjectTable();
			items = new XmlSchemaObjectCollection(this);
			schemaTypes = new XmlSchemaObjectTable();
		}
		[XmlIgnore]
		public XmlSchemaObjectTable AttributeGroups 
		{
			get{ return attributeGroups; }
		}
		[XmlIgnore]
		public XmlSchemaObjectTable Groups 
		{
			get{ return groups; }
		}
		[XmlElement("annotation",typeof(XmlSchemaAnnotation),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("simpleType",typeof(XmlSchemaSimpleType),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("complexType",typeof(XmlSchemaComplexType),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("group",typeof(XmlSchemaGroup),Namespace="http://www.w3.org/2001/XMLSchema")]
		//NOTE: AttributeGroup and not AttributeGroupRef
		[XmlElement("attributeGroup",typeof(XmlSchemaAttributeGroup),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaObjectCollection Items 
		{
			get{ return items; }
		}
		[XmlIgnore]
		public XmlSchemaObjectTable SchemaTypes 
		{
			get{ return schemaTypes; }
		}
	}
}
