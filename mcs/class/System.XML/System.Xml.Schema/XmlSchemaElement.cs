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
		private string targetNamespace;
		private string hash;

		private static string xmlname = "element";

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

		[DefaultValue(null)]
		[System.Xml.Serialization.XmlAttribute("name")]
		public string Name 
		{
			get{ return  name; }
			set{ name = value; }
		}

		[DefaultValue(false)]
		[System.Xml.Serialization.XmlAttribute("nillable")]
		public bool IsNillable 
		{
			get{ return  isNillable; }
			set{ isNillable = value; }
		}

		[System.Xml.Serialization.XmlAttribute("ref")]
		public XmlQualifiedName RefName 
		{
			get{ return  refName; }
			set{ refName = value;}
		}

		[System.Xml.Serialization.XmlAttribute("substitutionGroup")]
		public XmlQualifiedName SubstitutionGroup
		{
			get{ return  substitutionGroup; }
			set{ substitutionGroup = value; }
		}
		
		[System.Xml.Serialization.XmlAttribute("type")]
		public XmlQualifiedName SchemaTypeName 
		{
			get{ return  schemaTypeName; }
			set{ schemaTypeName = value; }
		}

		[XmlElement("simpleType",typeof(XmlSchemaSimpleType),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("complexType",typeof(XmlSchemaComplexType),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaType SchemaType 
		{
			get{ return  schemaType; }
			set{ schemaType = value; }
		}

		[XmlElement("unique",typeof(XmlSchemaUnique),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("key",typeof(XmlSchemaKey),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("keyref",typeof(XmlSchemaKeyref),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaObjectCollection Constraints 
		{
			get{ return constraints; }
		}

		[XmlIgnore]
		public XmlQualifiedName QualifiedName 
		{
			get{ return qName; }
		}

		[XmlIgnore]
		public object ElementType 
		{
			get{ return elementType; }
		}
		
		[XmlIgnore]
		public XmlSchemaDerivationMethod BlockResolved 
		{
			get{ return blockResolved; }
		}
		
		[XmlIgnore]
		public XmlSchemaDerivationMethod FinalResolved 
		{
			get{ return finalResolved; }
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
		internal int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;

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
					this.qName = new XmlQualifiedName(this.name, schema.TargetNamespace);
				
				if(form != XmlSchemaForm.None)
					error(h,"form must be absent");
				if(MinOccursString != null)
					error(h,"minOccurs must be absent");
				if(MaxOccursString != null)
					error(h,"maxOccurs must be absent");

				XmlSchemaDerivationMethod allfinal = (XmlSchemaDerivationMethod.Extension | XmlSchemaDerivationMethod.Restriction);
				if(final == XmlSchemaDerivationMethod.All)
					finalResolved = allfinal;
				else if(final == XmlSchemaDerivationMethod.None)
					finalResolved = allfinal;
				else 
				{
					if((final & ~allfinal) != 0)
						warn(h,"some values for final are invalid in this context");
					finalResolved = final & allfinal;
				}

				XmlSchemaDerivationMethod allblock = XmlSchemaDerivationMethod.Extension | 
					XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Substitution;

				if(block == XmlSchemaDerivationMethod.All)
					blockResolved = allblock;
				else if(block == XmlSchemaDerivationMethod.None)
					blockResolved = allblock;
				else
				{
					if((block & ~allblock) != 0)
						warn(h,"Some of the values for block are invalid in this context");
					blockResolved = block & allblock;
				}

				if(schemaType != null && schemaTypeName != null && !schemaTypeName.IsEmpty)
				{
					error(h,"both schemaType and content can't be present");
				}

				//Even if both are present, read both of them.
				if(schemaType != null)
				{
					if(schemaType is XmlSchemaSimpleType)
					{
						errorCount += ((XmlSchemaSimpleType)schemaType).Compile(h,schema);
					}
					else if(schemaType is XmlSchemaComplexType)
					{
						errorCount += ((XmlSchemaComplexType)schemaType).Compile(h,schema);
					}
					else
						error(h,"only simpletype or complextype is allowed");
				}
				if(schemaTypeName != null && !schemaTypeName.IsEmpty)
				{
					if(!XmlSchemaUtil.CheckQName(SchemaTypeName))
						error(h,"SchemaTypeName must be an XmlQualifiedName");
				}
				if(SubstitutionGroup != null && !SubstitutionGroup.IsEmpty)
				{
					if(!XmlSchemaUtil.CheckQName(SubstitutionGroup))
						error(h,"SubstitutionGroup must be a valid XmlQualifiedName");
				}

				foreach(XmlSchemaObject obj in constraints)
				{
					if(obj is XmlSchemaUnique)
						errorCount += ((XmlSchemaUnique)obj).Compile(h,schema);
					else if(obj is XmlSchemaKey)
						errorCount += ((XmlSchemaKey)obj).Compile(h,schema);
					else if(obj is XmlSchemaKeyref)
						errorCount += ((XmlSchemaKeyref)obj).Compile(h,schema);
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

				//FIXME: Should we reset the values
				if(MinOccurs > MaxOccurs)
					error(h,"minOccurs must be less than or equal to maxOccurs");

				if(refName == null || RefName.IsEmpty)
				{
					if(form == XmlSchemaForm.Qualified || (form == XmlSchemaForm.None && schema.ElementFormDefault == XmlSchemaForm.Qualified))
						this.targetNamespace = schema.TargetNamespace;
					else
						this.targetNamespace = string.Empty;

					if(this.name == null)	//b1
						error(h,"Required attribute name must be present");
					else if(!XmlSchemaUtil.CheckNCName(this.name)) // b1.2
						error(h,"attribute name must be NCName");
					else
						this.qName = new XmlQualifiedName(this.name, this.targetNamespace);
				
					XmlSchemaDerivationMethod allblock = XmlSchemaDerivationMethod.Extension | 
						XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Substitution;

					if(block == XmlSchemaDerivationMethod.All)
						blockResolved = allblock;
					else if(block == XmlSchemaDerivationMethod.None)
						blockResolved = allblock;
					else
					{
						if((block & ~allblock) != 0)
							warn(h,"Some of the values for block are invalid in this context");
						blockResolved = block & allblock;
					}

					if(schemaType != null && schemaTypeName != null && !schemaTypeName.IsEmpty)
					{
						error(h,"both schemaType and content can't be present");
					}

					//Even if both are present, read both of them.
					if(schemaType != null)
					{
						if(schemaType is XmlSchemaSimpleType)
						{
							errorCount += ((XmlSchemaSimpleType)schemaType).Compile(h,schema);
						}
						else if(schemaType is XmlSchemaComplexType)
						{
							errorCount += ((XmlSchemaComplexType)schemaType).Compile(h,schema);
						}
						else
							error(h,"only simpletype or complextype is allowed");
					}
					if(schemaTypeName != null && !schemaTypeName.IsEmpty)
					{
						if(!XmlSchemaUtil.CheckQName(SchemaTypeName))
							error(h,"SchemaTypeName must be an XmlQualifiedName");
					}
					if(SubstitutionGroup != null && !SubstitutionGroup.IsEmpty)
					{
						if(!XmlSchemaUtil.CheckQName(SubstitutionGroup))
							error(h,"SubstitutionGroup must be a valid XmlQualifiedName");
					}

					foreach(XmlSchemaObject obj in constraints)
					{
						if(obj is XmlSchemaUnique)
							errorCount += ((XmlSchemaUnique)obj).Compile(h,schema);
						else if(obj is XmlSchemaKey)
							errorCount += ((XmlSchemaKey)obj).Compile(h,schema);
						else if(obj is XmlSchemaKeyref)
							errorCount += ((XmlSchemaKeyref)obj).Compile(h,schema);
					}
				}
				else
				{
					if(!XmlSchemaUtil.CheckQName(RefName))
						error(h,"RefName must be a XmlQualifiedName");

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

					qName = RefName;
					schema.MissingElementTypeRefs.Add (this, qName);
				}
			}

			// PSVI contribution for XmlSchemaElement
			if(refName == null || RefName.IsEmpty) {
				if (this.schemaType != null)
					this.elementType = schemaType;
				else {
					XmlSchemaType xsType = schema.SchemaTypes [schemaTypeName] as XmlSchemaType;
					if (xsType != null)
						this.elementType = xsType;
					else
						this.elementType = XmlSchemaDatatype.FromName (schemaTypeName);
				}
			}
		
			XmlSchemaUtil.CompileID(Id,this,schema.IDCollection,h);

			this.CompilationId = schema.CompilationId;
			return errorCount;
		}

		internal void SetReferedElementInfo (XmlSchemaElement element)
		{
			this.elementType = element.elementType;
		}
		
		[MonoTODO]
		internal int Validate(ValidationEventHandler h)
		{
			return errorCount;
		}

		//<element
		//  abstract = boolean : false
		//  block = (#all | List of (extension | restriction | substitution)) 
		//  default = string
		//  final = (#all | List of (extension | restriction)) 
		//  fixed = string
		//  form = (qualified | unqualified)
		//  id = ID
		//  maxOccurs = (nonNegativeInteger | unbounded)  : 1
		//  minOccurs = nonNegativeInteger : 1
		//  name = NCName
		//  nillable = boolean : false
		//  ref = QName
		//  substitutionGroup = QName
		//  type = QName
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?, ((simpleType | complexType)?, (unique | key | keyref)*))
		//</element>

		internal static XmlSchemaElement Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaElement element = new XmlSchemaElement();
			Exception innerex;
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaElement.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			element.LineNumber = reader.LineNumber;
			element.LinePosition = reader.LinePosition;
			element.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "abstract")
				{
					element.IsAbstract = XmlSchemaUtil.ReadBoolAttribute(reader,out innerex);
					if(innerex != null)
						error(h,reader.Value + " is invalid value for abstract",innerex);
				}
				else if(reader.Name == "block")
				{
					element.block = XmlSchemaUtil.ReadDerivationAttribute(reader,out innerex, "block");
					if(innerex != null)
						warn(h,"some invalid values for block attribute were found",innerex);
				}
				else if(reader.Name == "default")
				{
					element.defaultValue = reader.Value;
				}
				else if(reader.Name == "final")
				{
					element.Final = XmlSchemaUtil.ReadDerivationAttribute(reader,out innerex, "block");
					if(innerex != null)
						warn(h,"some invalid values for final attribute were found",innerex);
				}
				else if(reader.Name == "fixed")
				{
					element.fixedValue = reader.Value;
				}
				else if(reader.Name == "form")
				{
					element.form = XmlSchemaUtil.ReadFormAttribute(reader,out innerex);
					if(innerex != null)
						error(h,reader.Value + " is an invalid value for form attribute",innerex);
				}
				else if(reader.Name == "id")
				{
					element.Id = reader.Value;
				}
				else if(reader.Name == "maxOccurs")
				{
					try
					{
						element.MaxOccursString = reader.Value;
					}
					catch(Exception e)
					{
						error(h,reader.Value + " is an invalid value for maxOccurs",e);
					}
				}
				else if(reader.Name == "minOccurs")
				{
					try
					{
						element.MinOccursString = reader.Value;
					}
					catch(Exception e)
					{
						error(h,reader.Value + " is an invalid value for minOccurs",e);
					}
				}
				else if(reader.Name == "name")
				{
					element.Name = reader.Value;
				}
				else if(reader.Name == "nillable")
				{
					element.IsNillable = XmlSchemaUtil.ReadBoolAttribute(reader,out innerex);
					if(innerex != null)
						error(h,reader.Value + "is not a valid value for nillable",innerex);
				}
				else if(reader.Name == "ref")
				{
					element.refName = XmlSchemaUtil.ReadQNameAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for ref attribute",innerex);
				}
				else if(reader.Name == "substitutionGroup")
				{
					element.substitutionGroup = XmlSchemaUtil.ReadQNameAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for substitutionGroup attribute",innerex);
				}
				else if(reader.Name == "type")
				{
					element.SchemaTypeName = XmlSchemaUtil.ReadQNameAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for type attribute",innerex);
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for element",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,element);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return element;

			//  Content: annotation?, 
			//			(simpleType | complexType)?, 
			//			(unique | key | keyref)*
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaElement.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2; //Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						element.Annotation = annotation;
					continue;
				}
				if(level <= 2)
				{
					if(reader.LocalName == "simpleType")
					{
						level = 3;
						XmlSchemaSimpleType simple = XmlSchemaSimpleType.Read(reader,h);
						if(simple != null)
							element.SchemaType = simple;
						continue;
					}
					if(reader.LocalName == "complexType")
					{
						level = 3;
						XmlSchemaComplexType complex = XmlSchemaComplexType.Read(reader,h);
						if(complex != null)
						{
							element.SchemaType = complex;
						}
						continue;
					}
				}
				if(level <= 3)
				{
					if(reader.LocalName == "unique")
					{
						level = 3;
						XmlSchemaUnique unique = XmlSchemaUnique.Read(reader,h);
						if(unique != null)
							element.constraints.Add(unique);
						continue;
					}
					else if(reader.LocalName == "key")
					{
						level = 3;
						XmlSchemaKey key = XmlSchemaKey.Read(reader,h);
						if(key != null)
							element.constraints.Add(key);
						continue;
					}
					else if(reader.LocalName == "keyref")
					{
						level = 3;
						XmlSchemaKeyref keyref = XmlSchemaKeyref.Read(reader,h);
						if(keyref != null)
							element.constraints.Add(keyref);
						continue;
					}
				}
				reader.RaiseInvalidElementError();
			}
			return element;
		}
	}
}
