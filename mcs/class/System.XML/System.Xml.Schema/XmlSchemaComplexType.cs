// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.ComponentModel;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaComplexType.
	/// </summary>
	public class XmlSchemaComplexType : XmlSchemaType
	{
		private XmlSchemaAnyAttribute anyAttribute;
		private XmlSchemaObjectCollection attributes;
		private XmlSchemaObjectTable attributeUses;
		private XmlSchemaAnyAttribute attributeWildcard;
		private XmlSchemaDerivationMethod block;
		private XmlSchemaDerivationMethod blockResolved;
		private XmlSchemaContentModel contentModel;
		private XmlSchemaContentType contentType;
		private XmlSchemaParticle contentTypeParticle;
		private bool isAbstract;
		private bool isMixed;
		private XmlSchemaParticle particle;

		public XmlSchemaComplexType()
		{
			block = XmlSchemaDerivationMethod.None;
		}

		[XmlElement]
		public XmlSchemaAnyAttribute AnyAttribute 
		{
			get{ return  anyAttribute; }
			set{ anyAttribute = value; }
		}
		[XmlElement]
		public XmlSchemaObjectCollection Attributes 
		{
			get{ return attributes; }
		}
		[XmlIgnore]
		public XmlSchemaObjectTable AttributeUses 
		{
			get{ return attributeUses; }
		}
		[XmlIgnore]
		public XmlSchemaAnyAttribute AttributeWildcard 
		{
			get{ return attributeWildcard; }
		}
		[DefaultValue(XmlSchemaDerivationMethod.None)]
		[XmlAttribute]
		public XmlSchemaDerivationMethod Block 
		{
			get{ return  block; }
			set{ block = value; }
		}
		[XmlIgnore]
		public XmlSchemaDerivationMethod BlockResolved 
		{
			get{ return blockResolved; }
		}
		[XmlElement]
		public XmlSchemaContentModel ContentModel 
		{
			get{ return  contentModel; } 
			set{ contentModel = value; }
		}
		[XmlIgnore]
		public XmlSchemaContentType ContentType 
		{
			get{ return contentType; }
		}
		[XmlIgnore]
		public XmlSchemaParticle ContentTypeParticle 
		{
			get{ return contentTypeParticle; }
		}
		[DefaultValue(true)]
		[XmlAttribute]
		public bool IsAbstract 
		{
			get{ return  isAbstract; }
			set{ isAbstract = value; }
		}
		[DefaultValue(true)]
		[XmlAttribute]
		public override bool IsMixed
		{
			get{ return  isMixed; }
			set{ isMixed = value; }
		}
		[XmlElement]
		public XmlSchemaParticle Particle 
		{
			get{ return  particle; } 
			set{ particle = value; }
		}
	}
}
