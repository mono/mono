// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaSimpleContentExtension.
	/// </summary>
	public class XmlSchemaSimpleContentExtension : XmlSchemaContent
	{

		private XmlSchemaAnyAttribute any;
		private XmlSchemaObjectCollection attributes;
		private XmlQualifiedName baseTypeName;

		public XmlSchemaSimpleContentExtension()
		{
			baseTypeName = XmlQualifiedName.Empty;
			attributes	 = new XmlSchemaObjectCollection();
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
		[XmlAttribute]
		public XmlQualifiedName BaseTypeName 
		{
			get{ return  baseTypeName; }
			set{ baseTypeName = value; }
		}
	}
}
