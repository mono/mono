// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaElement.
	/// </summary>
	public class XmlSchemaElement : XmlSchemaParticle
	{
		private XmlSchemaDerivationMethod block;
		private XmlSchemaDerivationMethod blockResolved;
		private XmlSchemaObjectCollection constraints;
		private string defaultValue;
		private object elementType;
		private XmlSchemaDerivationMethod final;
		private XmlSchemaDerivationMethod finalResolved;
		private string fixedValue;
		private XmlSchemaForm form;
		private bool isAbstract;
		private bool isNillable;
		private string name;
		private XmlQualifiedName qName;
		private XmlQualifiedName refName;
		private XmlSchemaType schemaType;
		private XmlQualifiedName schemaTypeName;
		private XmlQualifiedName substitutionGroup;

		public XmlSchemaElement()
		{
			block = XmlSchemaDerivationMethod.None;
			final = XmlSchemaDerivationMethod.None;
			constraints = new XmlSchemaObjectCollection();
			qName = XmlQualifiedName.Empty;
			substitutionGroup = XmlQualifiedName.Empty;
		}

		#region Attributes
		
		[DefaultValue(false)]
		[System.Xml.Serialization.XmlAttribute("abstract")]
		public bool IsAbstract 
		{
			get{ return  isAbstract; }
			set{ isAbstract = value; }
		}

		[DefaultValue(XmlSchemaDerivationMethod.None)]
		[System.Xml.Serialization.XmlAttribute("block")]
		public XmlSchemaDerivationMethod Block 
		{
			get{ return  block; }
			set{ block = value; }
		}
		
		[DefaultValue(null)]
		[System.Xml.Serialization.XmlAttribute("default")]
		public string DefaultValue 
		{
			get{ return  defaultValue; }
			set{ defaultValue = value; }
		}
		
		[DefaultValue(XmlSchemaDerivationMethod.None)]
		[System.Xml.Serialization.XmlAttribute("final")]
		public XmlSchemaDerivationMethod Final 
		{
			get{ return  final; }
			set{ final = value; }
		}

		[DefaultValue(null)]
		[System.Xml.Serialization.XmlAttribute("fixed")]
		public string FixedValue 
		{
			get{ return  fixedValue; }
			set{ fixedValue = value; }
		}
		[DefaultValue(XmlSchemaForm.None)]
		[System.Xml.Serialization.XmlAttribute("form")]
		public XmlSchemaForm Form 
		{
			get{ return  form; }
			set{ form = value; }
		}

		[DefaultValue(false)]
		[System.Xml.Serialization.XmlAttribute("nillable")]
		public bool IsNillable 
		{
			get{ return  isNillable; }
			set{ isNillable = value; }
		}

		[DefaultValue(null)]
		[System.Xml.Serialization.XmlAttribute("name")]
		public string Name 
		{
			get{ return  name; }
			set{ name = value; }
		}
		[System.Xml.Serialization.XmlAttribute("ref")]
		public XmlQualifiedName RefName 
		{
			get{ return  refName; }
			set{ refName = value;}
		}
		
		[System.Xml.Serialization.XmlAttribute("type")]
		public XmlQualifiedName SchemaTypeName 
		{
			get{ return  schemaTypeName; }
			set{ schemaTypeName = value; }
		}

		[System.Xml.Serialization.XmlAttribute("substitutionGroup")]
		public XmlQualifiedName SubstitutionGroup
		{
			get{ return  substitutionGroup; }
			set{ substitutionGroup = value; }
		}


		#endregion

		#region Elements
		
		[XmlElement("unique",typeof(XmlSchemaUnique),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("key",typeof(XmlSchemaKey),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("keyref",typeof(XmlSchemaKeyref),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaObjectCollection Constraints 
		{
			get{ return constraints; }
		}

		[XmlElement("simpleType",typeof(XmlSchemaSimpleType),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("complexType",typeof(XmlSchemaComplexType),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaType SchemaType 
		{
			get{ return  schemaType; }
			set{ schemaType = value; }
		}
		
		#endregion

		#region XmlIgnore
		[XmlIgnore]
		public XmlSchemaDerivationMethod BlockResolved 
		{
			get{ return blockResolved; }
		}

		[XmlIgnore]
		public object ElementType 
		{
			get{ return elementType; }
		}
		
		[XmlIgnore]
		public XmlSchemaDerivationMethod FinalResolved 
		{
			get{ return finalResolved; }
		}

		[XmlIgnore]
		public XmlQualifiedName QualifiedName 
		{
			get{ return qName; }
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
