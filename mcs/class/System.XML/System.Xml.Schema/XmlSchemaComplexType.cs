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
			attributes = new XmlSchemaObjectCollection();
			block = XmlSchemaDerivationMethod.None;
		}

		#region Attributes

		[DefaultValue(XmlSchemaDerivationMethod.None)]
		[System.Xml.Serialization.XmlAttribute("block")]
		public XmlSchemaDerivationMethod Block
		{
			get{ return  block; }
			set{ block = value; }
		}
		[DefaultValue(false)]
		[System.Xml.Serialization.XmlAttribute("abstract")]
		public bool IsAbstract 
		{
			get{ return  isAbstract; }
			set{ isAbstract = value; }
		}
		[DefaultValue(false)]
		[System.Xml.Serialization.XmlAttribute("mixed")]
		public override bool IsMixed
		{
			get{ return  isMixed; }
			set{ isMixed = value; }
		}
		
		#endregion
		
		#region Elements
		[XmlElement("anyAttribute",Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaAnyAttribute AnyAttribute 
		{
			get{ return  anyAttribute; }
			set{ anyAttribute = value; }
		}

		[XmlElement("attribute",typeof(XmlSchemaAttribute),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("attributeGroup",typeof(XmlSchemaAttributeGroupRef),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaObjectCollection Attributes 
		{
			get{ return attributes; }
		}
		
		[XmlElement("simpleContent",typeof(XmlSchemaSimpleContent),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("complexContent",typeof(XmlSchemaComplexContent),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaContentModel ContentModel 
		{
			get{ return  contentModel; } 
			set{ contentModel = value; }
		}

		[XmlElement("group",typeof(XmlSchemaGroupRef),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("all",typeof(XmlSchemaAll),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("choice",typeof(XmlSchemaChoice),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("sequence",typeof(XmlSchemaSequence),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaParticle Particle 
		{
			get{ return  particle; } 
			set{ particle = value; }
		}

		#endregion

		#region XmlIgnore
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
		[XmlIgnore]
		public XmlSchemaDerivationMethod BlockResolved 
		{
			get{ return blockResolved; }
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
		#endregion

		[MonoTODO]
		internal bool Compile(ValidationEventHandler h, XmlSchemaInfo info)
		{
			return false;
		}
		
		[MonoTODO]
		internal bool Validate(ValidationEventHandler h)
		{
			return false;
		}
	}
}
