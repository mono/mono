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
		[XmlElement]
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
