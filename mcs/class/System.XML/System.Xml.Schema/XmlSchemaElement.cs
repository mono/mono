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
		internal bool parentIsSchema = false;
		private int errorCount;
		private string targetNamespace;

		public XmlSchemaElement()
		{
			block = XmlSchemaDerivationMethod.None;
			final = XmlSchemaDerivationMethod.None;
			constraints = new XmlSchemaObjectCollection();
			qName = XmlQualifiedName.Empty;
			refName = XmlQualifiedName.Empty;
			schemaTypeName = XmlQualifiedName.Empty;
			substitutionGroup = XmlQualifiedName.Empty;

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

		/// <remarks>
		/// a) If Element has parent as schema:
		///		1. name must be present and of type NCName.
		///		2. ref must be absent
		///		3. form must be absent
		///		4. minOccurs must be absent
		///		5. maxOccurs must be absent
		///	b) If Element has parent is not schema and ref is absent
		///		1. name must be present and of type NCName.
		///		2. if form equals qualified or form is absent and schema's formdefault is qualifed,
		///		   targetNamespace is schema's targetnamespace else empty.
		///		3. type and either <simpleType> or <complexType> are mutually exclusive
		///		4. default and fixed must not both be present.
		///		5. substitutiongroup must be absent
		///		6. final must be absent
		///		7. abstract must be absent
		///	c) if the parent is not schema and ref is set
		///		1. name must not be present
		///		2. all of <simpleType>,<complexType>,  <key>, <keyref>, <unique>, nillable, 
		///		   default, fixed, form, block and type,  must be absent.
		///	    3. substitutiongroup is prohibited
		///		4. final is prohibited
		///		5. abstract is prohibited
		///		6. default and fixed must not both be present.(Actually both are absent)
		/// </remarks>	
		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchemaInfo info)
		{
			
			if(this.defaultValue != null && this.fixedValue != null)
				error(h,"both default and fixed can't be present");

			if(parentIsSchema)
			{
				if(this.refName != null && !RefName.IsEmpty)
					error(h,"ref must be absent");
				
				if(this.name == null)	//b1
					error(h,"Required attribute name must be present");
				else if(!XmlSchemaUtil.CheckNCName(this.name)) // b1.2
					error(h,"attribute name must be NCName");
				else
					this.qName = new XmlQualifiedName(this.name, info.targetNS);
				
				if(form != XmlSchemaForm.None)
					error(h,"form must be absent");
				if(MinOccursString != null)
					error(h,"minOccurs must be absent");
				if(MaxOccursString != null)
					error(h,"maxOccurs must be absent");

				
				if(final == XmlSchemaDerivationMethod.All)
					finalResolved = XmlSchemaDerivationMethod.All;
				else if(final == XmlSchemaDerivationMethod.None)
					finalResolved = info.blockDefault & (XmlSchemaDerivationMethod.Extension | XmlSchemaDerivationMethod.Restriction);
				else 
					finalResolved = final & (XmlSchemaDerivationMethod.Extension | XmlSchemaDerivationMethod.Restriction);

				if(block == XmlSchemaDerivationMethod.All)
					blockResolved = XmlSchemaDerivationMethod.All;
				else if(block == XmlSchemaDerivationMethod.None)
					blockResolved = info.blockDefault & (XmlSchemaDerivationMethod.Extension | 
						XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Substitution);
				else 
					blockResolved = block & (XmlSchemaDerivationMethod.Extension | 
						XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Substitution);

				if(schemaType != null && schemaTypeName != null && !schemaTypeName.IsEmpty)
				{
					error(h,"both schemaType and content can't be present");
				}
				else
				{
					if(schemaType != null)
					{
						if(schemaType is XmlSchemaSimpleType)
							errorCount += ((XmlSchemaSimpleType)schemaType).Compile(h,info);
						else if(schemaType is XmlSchemaComplexType)
							errorCount += ((XmlSchemaComplexType)schemaType).Compile(h,info);
						else
							error(h,"only simpletype or complextype is allowed");
					}
					else
					{
						if(schemaTypeName == null || schemaTypeName.IsEmpty)
							error(h,"one of schemaType or schemaTypeName must be present");
					}
				}

				foreach(XmlSchemaObject obj in constraints)
				{
					if(obj is XmlSchemaUnique)
						((XmlSchemaUnique)obj).Compile(h,info);
					else if(obj is XmlSchemaKey)
						((XmlSchemaKey)obj).Compile(h,info);
					else if(obj is XmlSchemaKeyref)
						((XmlSchemaKeyref)obj).Compile(h,info);
				}
			}
			else
			{
				if(substitutionGroup != null && !substitutionGroup.IsEmpty)
					error(h,"substitutionGroup must be absent");
				if(final != XmlSchemaDerivationMethod.None)
					error(h,"final must be absent");
				if(isAbstract)
					error(h,"abstract must be absent");

				if(refName == null || RefName.IsEmpty)
				{
					if(form == XmlSchemaForm.Qualified || (form == XmlSchemaForm.None && info.formDefault == XmlSchemaForm.Qualified))
						this.targetNamespace = info.targetNS;
					else
						this.targetNamespace = string.Empty;

					if(this.name == null)	//b1
						error(h,"Required attribute name must be present");
					else if(!XmlSchemaUtil.CheckNCName(this.name)) // b1.2
						error(h,"attribute name must be NCName");
					else
						this.qName = new XmlQualifiedName(this.name, this.targetNamespace);
				
					if(block == XmlSchemaDerivationMethod.All)
						blockResolved = XmlSchemaDerivationMethod.All;
					else if(block == XmlSchemaDerivationMethod.None)
						blockResolved = info.blockDefault & (XmlSchemaDerivationMethod.Extension | 
							XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Substitution);
					else 
						blockResolved = block & (XmlSchemaDerivationMethod.Extension | 
							XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Substitution);

					if(schemaType != null && schemaTypeName != null && !schemaTypeName.IsEmpty)
					{
						error(h,"both schemaType and content can't be present");
					}
					else
					{
						if(schemaType != null)
						{
							if(schemaType is XmlSchemaSimpleType)
								errorCount += ((XmlSchemaSimpleType)schemaType).Compile(h,info);
							else if(schemaType is XmlSchemaComplexType)
								errorCount += ((XmlSchemaComplexType)schemaType).Compile(h,info);
							else
								error(h,"only simpletype or complextype is allowed");
						}
						else
						{
							if(schemaTypeName == null || schemaTypeName.IsEmpty)
								error(h,"one of schemaType or schemaTypeName must be present");
						}
					}

					foreach(XmlSchemaObject obj in constraints)
					{
						if(obj is XmlSchemaUnique)
							((XmlSchemaUnique)obj).Compile(h,info);
						else if(obj is XmlSchemaKey)
							((XmlSchemaKey)obj).Compile(h,info);
						else if(obj is XmlSchemaKeyref)
							((XmlSchemaKeyref)obj).Compile(h,info);
					}
				}
				else
				{
					if(name != null)
						error(h,"name must not be present when ref is present");
					if(Constraints.Count != 0)
						error(h,"key, keyref and unique must be absent");
					if(isNillable)
						error(h,"nillable must be absent");
					if(defaultValue != null)
						error(h,"default must be absent");
					if(fixedValue != null)
						error(h,"fixed must be null");
					if(form != XmlSchemaForm.None)
						error(h,"form must be absent");
					if(block != XmlSchemaDerivationMethod.None)
						error(h,"block must be absent");
					if(schemaTypeName != null && !schemaTypeName.IsEmpty)
						error(h,"type must be absent");
					if(SchemaType != null)
						error(h,"simpleType or complexType must be absent");
				}
			}
			if(this.Id != null && !XmlSchemaUtil.CheckID(Id))
				error(h, "id must be a valid ID");
			return errorCount;
		}
		
		[MonoTODO]
		internal int Validate(ValidationEventHandler h)
		{
			return errorCount;
		}

		internal void error(ValidationEventHandler handle,string message)
		{
			errorCount++;
			ValidationHandler.RaiseValidationError(handle,this,message);
		}
	}
}
