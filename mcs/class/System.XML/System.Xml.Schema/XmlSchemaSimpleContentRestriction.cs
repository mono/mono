// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaSimpleContentRestriction.
	/// </summary>
	public class XmlSchemaSimpleContentRestriction : XmlSchemaContent
	{
		
		private XmlSchemaAnyAttribute any;
		private XmlSchemaObjectCollection attributes;
		private XmlSchemaSimpleType baseType;
		private XmlQualifiedName baseTypeName;
		private XmlSchemaObjectCollection facets;

		public XmlSchemaSimpleContentRestriction()
		{
			baseTypeName = XmlQualifiedName.Empty;
			attributes	 = new XmlSchemaObjectCollection();
			facets		 = new XmlSchemaObjectCollection();
		}

		[XmlElement]
		public XmlSchemaAnyAttribute AnyAttribute 
		{
			get{ return  any; }
			set{ any = value; }
		}
		[XmlElement]
		public XmlSchemaObjectCollection Attributes 
		{
			get{ return attributes; }
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
