// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaComplexContentRestriction.
	/// </summary>
	public class XmlSchemaComplexContentRestriction : XmlSchemaContent
	{
		private XmlSchemaAnyAttribute any;
		private XmlSchemaObjectCollection attributes;
		private XmlQualifiedName baseTypeName;
		private XmlSchemaParticle particle;

		public XmlSchemaComplexContentRestriction()
		{
			baseTypeName = XmlQualifiedName.Empty;
			attributes	 = new XmlSchemaObjectCollection();
		}

		[System.Xml.Serialization.XmlAttribute("base")]
		public XmlQualifiedName BaseTypeName 
		{
			get{ return  baseTypeName; }
			set{ baseTypeName = value; }
		}

		[XmlElement("anyAttribute",Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaAnyAttribute AnyAttribute 
		{
			get{ return  any; }
			set{ any = value; }
		}

		[XmlElement("attribute",typeof(XmlSchemaAttribute),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("attributeGroup",typeof(XmlSchemaAttributeGroupRef),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaObjectCollection Attributes 
		{
			get{ return attributes; }
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
	}
}
