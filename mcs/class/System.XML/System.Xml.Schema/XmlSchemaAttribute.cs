// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.ComponentModel;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaAttribute.
	/// </summary>
	public class XmlSchemaAttribute : XmlSchemaAnnotated
	{
		private object attributeType;
		private string defaultValue;
		private string fixedValue;
		private XmlSchemaForm form;
		private string name;
		private XmlQualifiedName qualifiedName;
		private XmlQualifiedName refName;
		private XmlSchemaSimpleType schemaType;
		private XmlQualifiedName schemaTypeName;
		private XmlSchemaUse use;

		public XmlSchemaAttribute()
		{
			//FIXME: Docs says the default is optional.
			//Whereas the MS implementation has default None.
			use = XmlSchemaUse.Optional;
			qualifiedName = XmlQualifiedName.Empty;
			refName = XmlQualifiedName.Empty;
		}

		// Properties
		[XmlIgnore]
		public object AttributeType 
		{ //FIXME: This is not correct. Is it?
			get{ return attributeType; }
		}
		//FIXME: what is the default value?
		[DefaultValue("")]
		[XmlAttribute]
		public string DefaultValue 
		{
			get{ return defaultValue;}
			set
			{ // Default Value and fixed Value are mutually exclusive
				fixedValue = null;
				defaultValue = value;
			}
		}
		[DefaultValue("")]
		[XmlAttribute]
		public string FixedValue 
		{
			get{ return fixedValue;}
			set
			{ // Default Value and fixed Value are mutually exclusive
				defaultValue = null;
				fixedValue = value;
			}
		}
		[DefaultValue(XmlSchemaForm.None)]
		[XmlAttribute]
		public XmlSchemaForm Form 
		{
			get{ return form;}
			set{ form = value;}
		}
		[XmlAttribute]
		public string Name 
		{
			get{ return name;}
			set
			{ // Name and RefName are mutually exclusive
				refName = null;
				name  = value;
			}
		}
		[XmlIgnore]
		public XmlQualifiedName QualifiedName 
		{
			get{ return qualifiedName;}
		}

		[XmlAttribute]
		public XmlQualifiedName RefName 
		{
			get{ return refName;}
			set
			{ // Name and RefName are mutually exclusive
				name = null;
				refName = value; 
			}
		}
		[XmlElement]
		public XmlSchemaSimpleType SchemaType 
		{
			get{ return schemaType;}
			set{ schemaType = value;}
		}
		[XmlAttribute]
		public XmlQualifiedName SchemaTypeName 
		{
			get{ return schemaTypeName;}
			set{ schemaTypeName = value;}
		}
		[DefaultValue(XmlSchemaUse.Optional)]
		[XmlAttribute]
		public XmlSchemaUse Use 
		{
			get{ return use;}
			set{ use = value;}
		}
	}
}
