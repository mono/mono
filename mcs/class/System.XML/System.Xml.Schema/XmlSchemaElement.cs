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
			constraints = new XmlSchemaObjectCollection();
			final = XmlSchemaDerivationMethod.None;
			finalResolved = XmlSchemaDerivationMethod.None;
			name = string.Empty;
			qName = XmlQualifiedName.Empty;
			substitutionGroup = XmlQualifiedName.Empty;
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
		public XmlSchemaObjectCollection Constraints 
		{
			get{ return constraints; }
		}
		[DefaultValue("")]
		[XmlAttribute]
		public string DefaultValue 
		{
			get{ return  defaultValue; }
			set{ defaultValue = value; }
		}
		[XmlIgnore]
		public object ElementType 
		{
			get{ return elementType; }
		}
		[DefaultValue(XmlSchemaDerivationMethod.None)]
		[XmlAttribute]
		public XmlSchemaDerivationMethod Final 
		{
			get{ return  final; }
			set{ final = value; }
		}
		[XmlIgnore]
		public XmlSchemaDerivationMethod FinalResolved 
		{
			get{ return finalResolved; }
		}
		[DefaultValue("")]
		[XmlAttribute]
		public string FixedValue 
		{
			get{ return  fixedValue; }
			set{ fixedValue = value; }
		}
		[DefaultValue(XmlSchemaForm.None)]
		[XmlAttribute]
		public XmlSchemaForm Form 
		{
			get{ return  form; }
			set{ form = value; }
		}
		[DefaultValue(true)]
		[XmlAttribute]
		public bool IsAbstract 
		{
			get{ return  isAbstract; }
			set{ isAbstract = value; }
		}
		[DefaultValue(false)]
		[XmlAttribute]
		public bool IsNillable 
		{
			get{ return  isNillable; }
			set{ isNillable = value; }
		}
		[DefaultValue("")]
		[XmlAttribute]
		public string Name 
		{
			get{ return  name; }
			set{ name = value; }
		}
		[XmlIgnore]
		public XmlQualifiedName QualifiedName 
		{
			get{ return qName; }
		}
		[XmlAttribute]
		public XmlQualifiedName RefName 
		{
			get{ return  refName; }
			set{ refName = value;}
		}
		[XmlElement]
		public XmlSchemaType SchemaType 
		{
			get{ return  schemaType; }
			set{ schemaType = value; }
		}
		[XmlAttribute]
		public XmlQualifiedName SchemaTypeName 
		{
			get{ return  schemaTypeName; }
			set{ schemaTypeName = value; }
		}
		[XmlAttribute]
		public XmlQualifiedName SubstitutionGroup 
		{
			get{ return  substitutionGroup; }
			set{ substitutionGroup = value; }
		}
	}
}
