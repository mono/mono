// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;


namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaSimpleTypeRestriction.
	/// </summary>
	public class XmlSchemaSimpleTypeRestriction : XmlSchemaSimpleTypeContent
	{
		private XmlSchemaSimpleType baseType;
		private XmlQualifiedName baseTypeName;
		private XmlSchemaObjectCollection facets;
		public XmlSchemaSimpleTypeRestriction()
		{
			facets = new XmlSchemaObjectCollection();
		}
		[XmlElement]
		public XmlSchemaSimpleType BaseType 
		{
			get{ return  baseType; } 
			set{ baseType = value; }
		}
		[XmlAttribute]
		public XmlQualifiedName BaseTypeName 
		{
			get{ return  baseTypeName; } 
			set{ baseTypeName = value; }
		}
		[XmlElement]
		public XmlSchemaObjectCollection Facets 
		{
			get{ return facets; }
		}
	}
}
