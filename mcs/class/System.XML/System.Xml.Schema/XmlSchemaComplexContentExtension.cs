// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaComplexContentExtension.
	/// </summary>
	public class XmlSchemaComplexContentExtension : XmlSchemaContent
	{
		private XmlSchemaAnyAttribute any;
		private XmlSchemaObjectCollection attributes;
		private XmlQualifiedName baseTypeName;
		private XmlSchemaParticle particle;
		public XmlSchemaComplexContentExtension()
		{
			attributes = new XmlSchemaObjectCollection();
		}
		[XmlElement]
		public XmlSchemaAnyAttribute AnyAttribute 
		{
			get{ return any; }
			set{ any = value;}
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
		[XmlElement]
		public XmlSchemaParticle Particle
		{
			get{ return  particle; }
			set{ particle = value; }
		}
	}
}
