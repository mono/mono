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
		//Compilation fields
		private string targetNamespace;
		internal bool parentIsSchema = false;
		internal XmlSchema schema = null;
		internal bool errorOccured = false;
		
		public XmlSchemaAttribute()
		{
			//FIXME: Docs says the default is optional.
			//Whereas the MS implementation has default None.
			form	= XmlSchemaForm.None;
			use		= XmlSchemaUse.None;
			schemaTypeName	= XmlQualifiedName.Empty;
			qualifiedName	= XmlQualifiedName.Empty;
			refName			= XmlQualifiedName.Empty;
		}

		// Properties
		[XmlIgnore]
		public object AttributeType 
		{ //FIXME: This is not correct. Is it?
			get{ return attributeType; }
		}

		[DefaultValue(null)]
		[System.Xml.Serialization.XmlAttribute("default")]
		public string DefaultValue 
		{
			get{ return defaultValue;}
			set
			{ // Default Value and fixed Value are mutually exclusive
				fixedValue = null;
				defaultValue = value;
			}
		}

		[DefaultValue(null)]
		[System.Xml.Serialization.XmlAttribute("fixed")]
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
		[System.Xml.Serialization.XmlAttribute("form")]
		public XmlSchemaForm Form 
		{
			get{ return form;}
			set{ form = value;}
		}

		[System.Xml.Serialization.XmlAttribute("name")]
		public string Name 
		{
			get{ return name;}
			set
			{
				name  = value;
			}
		}
		[XmlIgnore]
		public XmlQualifiedName QualifiedName 
		{
			get{ return qualifiedName;}
		}

		[System.Xml.Serialization.XmlAttribute("ref")]
		public XmlQualifiedName RefName 
		{
			get{ return refName;}
			set
			{
				refName = value; 
			}
		}

		[XmlElement("simpleType",Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaSimpleType SchemaType 
		{
			get{ return schemaType;}
			set{ schemaType = value;}
		}
		
		[System.Xml.Serialization.XmlAttribute("type")]
		public XmlQualifiedName SchemaTypeName 
		{
			get{ return schemaTypeName;}
			set{ schemaTypeName = value;}
		}

		[DefaultValue(XmlSchemaUse.None)]
		[System.Xml.Serialization.XmlAttribute("use")]
		public XmlSchemaUse Use 
		{
			get{ return use;}
			set{ use = value;}
		}
		/// <remarks>
		/// For an attribute:
		///  a) If the parent is schema 
		///		1-5		are from <xs:complexType name="topLevelAttribute"> in the Schema for Schema
		///		6-8		are from  "Constraints on XML Representations of Attribute Declarations"
		///		9-10	are from "Attribute Declaration Schema Component"
		///		11-16	are from "Constraints on Attribute Declaration Schema Components"
		///		1. ref	must be absent
		///		2. form must be absent
		///		3. use	must be absent
		///		4. name must be present and of type NCName
		///		5. *NO CHECK REQUIRED* Only simple types and annotation are allowed as content
		///		6. default and fixed must not both be present. 
		///		7. *NO CHECK REQUIRED* If default and use are both present... (Not possible since use is absent)
		///		8. type and <simpleType> must not both be present.
		///		9. Target Namespace should be schema's targetnamespace or absent
		///		10. Type Definiton coressponds to <simpletype> element, or type value, or absent
		///		11. *TO UNDERSTAND* Missing Sub-components
		///		12. value constraint must be of the same datatype as of type
		///		13. if the type definition is ID then there should be no value constraint.
		///		14. name must not be xmlns
		///		15. Targetnamespace must not be xsi. This implies the target namespace of schema can't be xsi if toplevel attributes are used.
		///		16. *Exception to rule 15* inbuilt attributes: xsi:nil, xsi:type, xsi:schemaLocation, xsi: noNamespaceSchemaLocation
		///	b) *TODO*: If the parent is complextype and ref is not set
		///	c) *TODO*: if the parent is not schema and ref is set
		/// </remarks>
		[MonoTODO]
		//FIXME: Should we set a property to null if an error occurs? Or should we stop the validation?
		internal bool Compile(ValidationEventHandler h, XmlSchemaInfo info)
		{
			if(parentIsSchema)//a
			{
				if(this.refName!= null && !this.refName.IsEmpty) // a.1
					error(h,"ref must be absent in the top level <attribute>");
				if(this.form!= XmlSchemaForm.None)	// a.2
					error(h,"form must be absent in the top level <attribute>");
				if(this.use!= XmlSchemaUse.None)		// a.3
					error(h,"use must be absent in the top level <attribute>");
				if(this.name == null)	//a.4
					error(h,"name must be present if attribute has schema as its parent");
				// FIXME: A better way to check NCName? Something like IsNCName()?
				else if(this.name.IndexOf(":") != -1) // a.4.2
					error(h,"attribute name must be NCName");
				else if(this.name == "xmlns") // a.14 
					error(h,"attribute name can't be xmlns");
				else
					this.qualifiedName = new XmlQualifiedName(this.name, info.targetNS);	
	
				// TODO: a.10, a.11, a.12, a.13
				if(this.defaultValue != null && this.fixedValue != null) // a.6
					error(h,"default and fixed must not both be present in an Attribute");
				this.targetNamespace = this.schema.TargetNamespace; // a.9
				if(this.schemaType != null)
				{
					if(this.schemaTypeName != null && !this.SchemaTypeName.IsEmpty) // a.8
						error(h,"attribute can't have both a type and <simpleType> content");
					else 
					{
						this.SchemaType.islocal = true;
						this.schemaType.Compile(h,info); 
					}
				}
				if(this.targetNamespace == XmlSchema.InstanceNamespace && this.name != "nil" && this.name != "type" 
						&& this.name != "schemaLocation" && this.name != "noNamespaceSchemaLocation") // a.15, a.16
					error(h,"targetNamespace can't be " + XmlSchema.InstanceNamespace);

			}
			// TODO: else
			return errorOccured;
		}
		
		[MonoTODO]
		internal bool Validate(ValidationEventHandler h)
		{
			return false;
		}
		
		internal void error(ValidationEventHandler handle,string message)
		{
			this.errorOccured = true;
			ValidationHandler.RaiseValidationError(handle,this,message);
		}
	}
}
