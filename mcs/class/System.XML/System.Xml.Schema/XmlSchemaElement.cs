//
// System.Xml.Schema.XmlSchemaElement.cs
//
// Authors:
//	Dwivedi, Ajay kumar  Adwiv@Yahoo.com
//	Enomoto, Atsushi     ginga@kit.hi-ho.ne.jp
//
using System;
using System.Collections;
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
		private string validatedDefaultValue;
		private string validatedFixedValue;
		internal bool actualIsAbstract;
		internal bool actualIsNillable;
		private ArrayList substitutingElements = new ArrayList ();
		XmlSchemaElement referencedElement;

		// Post compilation items. It should be added on all schema components.
		XmlSchema schema;

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
			get {
				if (referencedElement != null)
					return referencedElement.ElementType;
				else
					return elementType;
			}
		}
		
		[XmlIgnore]
		public XmlSchemaDerivationMethod BlockResolved 
		{
			get{
				if (referencedElement != null)
					return referencedElement.BlockResolved;
				else
					return blockResolved;
			}
		}
		
		[XmlIgnore]
		public XmlSchemaDerivationMethod FinalResolved 
		{
			get{
				if (referencedElement != null)
					return referencedElement.FinalResolved;
				else
					return finalResolved;
			}
		}

		// Post compilation default value (normalized)
		internal string ValidatedDefaultValue
		{
			get{
				if (referencedElement != null)
					return referencedElement.ValidatedDefaultValue;
				else
					return validatedDefaultValue;
			}
		}

		// Post compilation fixed value (normalized)
		internal string ValidatedFixedValue 
		{
			get{
				if (referencedElement != null)
					return referencedElement.ValidatedFixedValue;
				else
					return validatedFixedValue;
			}
		}

		internal ArrayList SubstitutingElements
		{
			get {
				if (referencedElement != null)
					return referencedElement.SubstitutingElements;
				else
					return this.substitutingElements;
			}
		}

		#endregion

		/*
		// FIXME: using this causes stack overflow...
		internal override XmlSchemaParticle ActualParticle {
			get {
				if (this.SubstitutingElements != null && this.SubstitutingElements.Count > 0) {
					XmlSchemaChoice choice = new XmlSchemaChoice ();
					choice.Compile (null, schema); // compute Validated Min/Max Occurs.
					choice.CompiledItems.Add (this);
					for (int i = 0; i < SubstitutingElements.Count; i++) {
						XmlSchemaElement se = SubstitutingElements [i] as XmlSchemaElement;
						choice.CompiledItems.Add (se);
					}
					return choice;
				}
				else
					return this;
			}
		}
		*/

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
		internal override int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;
			this.schema = schema;

			if(this.defaultValue != null && this.fixedValue != null)
				error(h,"both default and fixed can't be present");

			if(parentIsSchema || isRedefineChild)
			{
				if(this.refName != null && !RefName.IsEmpty)
					error(h,"ref must be absent");

				if(this.name == null)	//b1
					error(h,"Required attribute name must be present");
				else if(!XmlSchemaUtil.CheckNCName(this.name)) // b1.2
					error(h,"attribute name must be NCName");
				else
					this.qName = new XmlQualifiedName (this.name, schema.TargetNamespace);

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
					finalResolved = XmlSchemaDerivationMethod.Empty;
				else 
				{
//					if((final & ~allfinal) != 0)
					if ((final | XmlSchemaUtil.FinalAllowed) != XmlSchemaUtil.FinalAllowed)
						warn(h,"some values for final are invalid in this context");
					finalResolved = final & allfinal;
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
				// This is not W3C REC 3.3.3 requirement
//				if(isAbstract)
//					error(h,"abstract must be absent");

				CompileOccurence (h, schema);

				if(refName == null || RefName.IsEmpty)
				{
					if(form == XmlSchemaForm.Qualified || (form == XmlSchemaForm.None && schema.ElementFormDefault == XmlSchemaForm.Qualified))
						this.targetNamespace = schema.TargetNamespace;
					else
						this.targetNamespace = "";

					if(this.name == null)	//b1
						error(h,"Required attribute name must be present");
					else if(!XmlSchemaUtil.CheckNCName(this.name)) // b1.2
						error(h,"attribute name must be NCName");
					else
						this.qName = new XmlQualifiedName(this.name, this.targetNamespace);
				
					/*
					XmlSchemaDerivationMethod allblock = XmlSchemaDerivationMethod.Extension | 
						XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Substitution;

					if(block == XmlSchemaDerivationMethod.All)
						blockResolved = allblock;
//					else if(block == XmlSchemaDerivationMethod.None)
//						blockResolved = allblock;
					else
					{
						if((block & ~allblock) != 0)
							warn(h,"Some of the values for block are invalid in this context");
						blockResolved = block & allblock;
					}
					*/

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
				}
			}

			switch (block) {
			case XmlSchemaDerivationMethod.All:
				blockResolved = XmlSchemaDerivationMethod.All;
				break;
			case XmlSchemaDerivationMethod.None:
				blockResolved = XmlSchemaDerivationMethod.Empty;
				break;
			default:
				if ((block | XmlSchemaUtil.ElementBlockAllowed) != XmlSchemaUtil.ElementBlockAllowed)
					warn(h,"Some of the values for block are invalid in this context");
				blockResolved = block;
				break;
			}

			if (Constraints != null) {
				XmlSchemaObjectTable table = new XmlSchemaObjectTable ();
				foreach (XmlSchemaIdentityConstraint c in Constraints) {
					XmlSchemaUtil.AddToTable (table, c, c.QualifiedName, h);
				}
			}

			XmlSchemaUtil.CompileID(Id,this,schema.IDCollection,h);

			this.CompilationId = schema.CompilationId;
			return errorCount;
		}

		[MonoTODO]
		internal override int Validate(ValidationEventHandler h, XmlSchema schema)
		{
			if (IsValidated (schema.CompilationId))
				return errorCount;

			// See XML Schema Structures 3.6 for the complete description.

			// Element Declaration Properties Correct
			// 1. = 3.3.1 (modulo 5.3)

			// 3.3.1:
			// {annotation} is as is.
			// {name}, {target namespace}, {scope}, {disallowed substitution},
			// {substitution group exclusions} (handled the same as 'disallowed substitution')
			// and {identity-constraint-definitions} are Compile()d.
			// {value constraint} is going to be filled in step 2.

			// actual {nillable}, {abstract} 
			this.actualIsNillable = IsNillable;
			this.actualIsAbstract = IsAbstract;

			// {type} from here
			XmlSchemaDatatype datatype = null;
			if (schemaType != null)
				elementType = schemaType;
			else if (SchemaTypeName != XmlQualifiedName.Empty) {
				XmlSchemaType type = schema.SchemaTypes [SchemaTypeName] as XmlSchemaType;
				// If el is null, then it is missing sub components .
				if (type != null) {
					type.Validate (h, schema);
					elementType = type;
				}
				else if (SchemaTypeName == XmlSchemaComplexType.AnyTypeName)
					elementType = XmlSchemaComplexType.AnyType;
				else if (SchemaTypeName.Namespace == XmlSchema.Namespace) {
					datatype = XmlSchemaDatatype.FromName (SchemaTypeName);
					if (datatype == null)
						error (h, "Invalid schema datatype was specified.");
					else
						elementType = datatype;
				}
				// otherwise, it might be missing sub components.
				else if (!schema.IsNamespaceAbsent (SchemaTypeName.Namespace))
					error (h, "Referenced element schema type " + SchemaTypeName + " was not found in the corresponding schema.");
			}
			else if (RefName != XmlQualifiedName.Empty)
			{
				XmlSchemaElement refElem = schema.Elements [RefName] as XmlSchemaElement;
				// If el is null, then it is missing sub components .
				if (refElem != null) {
					this.referencedElement = refElem;
					errorCount += refElem.Validate (h, schema);
					elementType = refElem.ElementType;
					this.validatedDefaultValue = refElem.validatedDefaultValue;
					this.validatedFixedValue = refElem.validatedFixedValue;
					this.actualIsAbstract = refElem.IsAbstract;
					this.actualIsNillable = refElem.IsNillable;
				}
				// otherwise, it might be missing sub components.
				else if (!schema.IsNamespaceAbsent (RefName.Namespace))
					error (h, "Referenced element " + RefName + " was not found in the corresponding schema.");
			}
			
			// Otherwise the -ur type- definition.
			if (elementType == null)
				elementType = XmlSchemaComplexType.AnyType;

			XmlSchemaType xsType = elementType as XmlSchemaType;
			if (xsType != null) {
				errorCount += xsType.Validate (h, schema);
				datatype = xsType.Datatype;
			}
			// basic {type} is now filled, except for derivation by {substitution group}.

			// {substitution group affiliation}
			// 3. subsitution group's type derivation check.
			if (this.SubstitutionGroup != XmlQualifiedName.Empty) {
				XmlSchemaElement substElem = schema.Elements [SubstitutionGroup] as XmlSchemaElement;
				// If el is null, then it is missing sub components .
				if (substElem != null) {
					substElem.Validate (h, schema);
					XmlSchemaType substSchemaType = substElem.ElementType as XmlSchemaType;
					if (substSchemaType != null) {
						// 3.3.6 Properties Correct 3.
						if ((substElem.FinalResolved & XmlSchemaDerivationMethod.Substitution) != 0)
							error (h, "Substituted element blocks substitution.");
						if (xsType != null && (substElem.FinalResolved & xsType.DerivedBy) != 0)
							error (h, "Invalid derivation was found. Substituted element prohibits this derivation method: " + xsType.DerivedBy + ".");
					}
					XmlSchemaComplexType xsComplexType = xsType as XmlSchemaComplexType;
					if (xsComplexType != null)
						xsComplexType.ValidateTypeDerivationOK (substElem.ElementType, h, schema);
					else {
						XmlSchemaSimpleType xsSimpleType = xsType as XmlSchemaSimpleType;
						if (xsSimpleType != null)
							xsSimpleType.ValidateTypeDerivationOK (substElem.ElementType, h, schema, true);
					}
					substElem.substitutingElements.Add (this);
				}
				// otherwise, it might be missing sub components.
				else if (!schema.IsNamespaceAbsent (SubstitutionGroup.Namespace))
					error (h, "Referenced element type " + SubstitutionGroup + " was not found in the corresponding schema.");
			}

			// 2. ElementDefaultValid
			// 4. ID with {value constraint} is prohibited.
			if (defaultValue != null || fixedValue != null) {
				ValidateElementDefaultValidImmediate (h, schema);
				if (datatype != null && // Such situation is basically an error. For ValidationEventHandler.
					datatype.TokenizedType == XmlTokenizedType.ID)
					error (h, "Element type is ID, which does not allows default or fixed values.");
			}

			// Identity constraints (3.11.3 / 3.11.6)
			foreach (XmlSchemaIdentityConstraint ident in Constraints)
				ident.Validate (h, schema);

			ValidationId = schema.ValidationId;
			return errorCount;
		}

		internal override bool ParticleEquals (XmlSchemaParticle other)
		{
			XmlSchemaElement element = other as XmlSchemaElement;
			if (element == null)
				return false;
			if (this.ValidatedMaxOccurs != element.ValidatedMaxOccurs ||
				this.ValidatedMinOccurs != element.ValidatedMinOccurs)
				return false;
			if (this.QualifiedName != element.QualifiedName ||
				this.ElementType != element.ElementType ||
				this.Constraints.Count != element.Constraints.Count)
				return false;
			for (int i = 0; i < this.Constraints.Count; i++) {
				XmlSchemaIdentityConstraint c1 = Constraints [i] as XmlSchemaIdentityConstraint;
				XmlSchemaIdentityConstraint c2 = element.Constraints [i] as XmlSchemaIdentityConstraint;
				if (c1.QualifiedName != c2.QualifiedName ||
					c1.Selector.XPath != c2.Selector.XPath ||
					c1.Fields.Count != c2.Fields.Count)
					return false;
				for (int f = 0; f < c1.Fields.Count; f++) {
					XmlSchemaXPath f1 = c1.Fields [f] as XmlSchemaXPath;
					XmlSchemaXPath f2 = c2.Fields [f] as XmlSchemaXPath;
					if (f1.XPath != f2.XPath)
						return false;
				}
			}
			if (this.BlockResolved != element.BlockResolved ||
				this.FinalResolved != element.FinalResolved ||
				this.ValidatedDefaultValue != element.ValidatedDefaultValue ||
				this.ValidatedFixedValue != element.ValidatedFixedValue)
				return false;
			return true;
		}

		internal override void ValidateDerivationByRestriction (XmlSchemaParticle baseParticle,
			ValidationEventHandler h, XmlSchema schema)
		{
			// element - NameAndTypeOK
			XmlSchemaElement baseElement = baseParticle as XmlSchemaElement;
			if (baseElement != null) {
				ValidateDerivationByRestrictionNameAndTypeOK (baseElement, h, schema);
				return;
			}

			// any - NSCompat
			XmlSchemaAny baseAny = baseParticle as XmlSchemaAny;
			if (baseAny != null) {
				// NSCompat
				baseAny.ValidateWildcardAllowsNamespaceName (this.QualifiedName.Namespace, h, schema, true);
				ValidateOccurenceRangeOK (baseAny, h, schema);
				return;
			}

//*
			// choice - RecurseAsIfGroup
			XmlSchemaGroupBase gb = null;
			if (baseParticle is XmlSchemaSequence)
				gb = new XmlSchemaSequence ();
			else if (baseParticle is XmlSchemaChoice)
				gb = new XmlSchemaChoice ();
			else if (baseParticle is XmlSchemaAll)
				gb = new XmlSchemaAll ();

			if (gb != null) {
				gb.Items.Add (this);
				gb.Compile (h, schema);
				gb.Validate (h, schema);
				// It looks weird, but here we never think about 
				// _pointlessness_ of this groupbase particle.
				gb.ValidateDerivationByRestriction (baseParticle, h, schema);
				return;
			}
//*/
		}

		private void ValidateDerivationByRestrictionNameAndTypeOK (XmlSchemaElement baseElement,
			ValidationEventHandler h, XmlSchema schema)
		{
			// 1.
			if (this.QualifiedName != baseElement.QualifiedName)
				error (h, "Invalid derivation by restriction of particle was found. Both elements must have the same name.");
			// 2.
			if (this.isNillable && !baseElement.isNillable)
				error (h, "Invalid element derivation by restriction of particle was found. Base element is not nillable and derived type is nillable.");
			// 3.
			ValidateOccurenceRangeOK (baseElement, h, schema);
			// 4.
			if (baseElement.ValidatedFixedValue != null &&
				baseElement.ValidatedFixedValue != this.ValidatedFixedValue)
				error (h, "Invalid element derivation by restriction of particle was found. Both fixed value must be the same.");
			// 5. TODO: What is "identity constraints subset" ???

			// 6. 
			if ((baseElement.BlockResolved | this.BlockResolved) != this.BlockResolved)
				error (h, "Invalid derivation by restriction of particle was found. Derived element must contain all of the base element's block value.");
			// 7.
			if (baseElement.ElementType != null) {
				XmlSchemaComplexType derivedCType = this.ElementType as XmlSchemaComplexType;
				if (derivedCType != null)
					// W3C REC says that it is Type Derivation OK to be check, but
					// in fact it should be DerivationValid (Restriction, Complex).
					derivedCType.ValidateDerivationValidRestriction (
						baseElement.ElementType as XmlSchemaComplexType, h, schema);
					// derivedCType.ValidateTypeDerivationOK (baseElement.ElementType, h, schema);
				else {
					XmlSchemaSimpleType derivedSType = this.ElementType as XmlSchemaSimpleType;
					if (derivedSType != null)
						derivedSType.ValidateTypeDerivationOK (baseElement.ElementType, h, schema, true);
					else if (baseElement.ElementType != XmlSchemaComplexType.AnyType && baseElement.ElementType != this.ElementType)
						error (h, "Invalid element derivation by restriction of particle was found. Both primitive types differ.");
				}
			}
		}

		internal override void CheckRecursion (int depth, ValidationEventHandler h, XmlSchema schema)
		{
			XmlSchemaComplexType ct = this.ElementType as XmlSchemaComplexType;
			if (ct == null || ct.ContentTypeParticle == null)
				return;
			ct.ContentTypeParticle.CheckRecursion (depth + 1, h, schema);
		}

		internal override void ValidateUniqueParticleAttribution (XmlSchemaObjectTable qnames, ArrayList nsNames,
			ValidationEventHandler h, XmlSchema schema)
		{
			if (qnames.Contains (this.QualifiedName))
				error (h, "Ambiguous element label was detected: " + this.QualifiedName);
			else {
				foreach (XmlSchemaAny any in nsNames) {
					if (any.ValidatedMaxOccurs == 0)
						continue;
					if (any.HasValueAny ||
						any.HasValueLocal && this.QualifiedName.Namespace == "" ||
						any.HasValueOther && this.QualifiedName.Namespace != this.targetNamespace ||
						any.HasValueTargetNamespace && this.QualifiedName.Namespace == this.targetNamespace) {
						error (h, "Ambiguous element label which is contained by -any- particle was detected: " + this.QualifiedName);
						break;
					} else {
						bool bad = false;
						foreach (string ns in any.ResolvedNamespaces) {
							if (ns == this.QualifiedName.Namespace) {
								bad = true;
								break;
							}
						}
						if (bad) {
							error (h, "Ambiguous element label which is contained by -any- particle was detected: " + this.QualifiedName);
							break;
						}
					}
				}
				qnames.Add (this.QualifiedName, this);
			}
		}

		internal override void ValidateUniqueTypeAttribution (XmlSchemaObjectTable labels,
			ValidationEventHandler h, XmlSchema schema)
		{
			XmlSchemaElement labeled = labels [this.QualifiedName] as XmlSchemaElement;
			if (labeled == null)
				labels.Add (this.QualifiedName, this);
			else if (labeled.ElementType != this.ElementType)
				error (h, "Different types are specified on the same named elements in the same sequence. Element name is " + QualifiedName);
		}


		// 3.3.6 Element Default Valid (Immediate)
		private void ValidateElementDefaultValidImmediate (ValidationEventHandler h, XmlSchema schema)
		{
			// This presumes that ElementType is already filled.

			XmlSchemaDatatype datatype = elementType as XmlSchemaDatatype;
			XmlSchemaSimpleType simpleType = elementType as XmlSchemaSimpleType;
			if (simpleType != null)
				datatype = simpleType.Datatype;

			if (datatype == null) {
				XmlSchemaComplexType complexType = elementType as XmlSchemaComplexType;
				switch (complexType.ContentType) {
				case XmlSchemaContentType.Empty:
				case XmlSchemaContentType.ElementOnly:
					error (h, "Element content type must be simple type or mixed.");
					break;
				}
				datatype = XmlSchemaSimpleType.AnySimpleType;
			}

			XmlNamespaceManager nsmgr = null;
			if (datatype.TokenizedType == XmlTokenizedType.QName) {
				if (this.Namespaces != null)
					foreach (XmlQualifiedName qname in Namespaces.ToArray ())
						nsmgr.AddNamespace (qname.Name, qname.Namespace);
			}

			try {
				if (defaultValue != null) {
					validatedDefaultValue = datatype.Normalize (defaultValue);
					datatype.ParseValue (validatedDefaultValue, null, nsmgr);
				}
			} catch (Exception ex) {
				// FIXME: This is not a good way to handle exception, but
				// I think there is no remedy for such Framework specification.
				error (h, "The Element's default value is invalid with respect to its type definition.", ex);
			}
			try {
				if (fixedValue != null) {
					validatedFixedValue = datatype.Normalize (fixedValue);
					datatype.ParseValue (validatedFixedValue, null, nsmgr);
				}
			} catch (Exception ex) {
				// FIXME: This is not a good way to handle exception.
				error (h, "The Element's fixed value is invalid with its type definition.", ex);
			}
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
					element.block = XmlSchemaUtil.ReadDerivationAttribute(reader,out innerex, "block",
						XmlSchemaUtil.ElementBlockAllowed);
					if(innerex != null)
						warn(h,"some invalid values for block attribute were found",innerex);
				}
				else if(reader.Name == "default")
				{
					element.defaultValue = reader.Value;
				}
				else if(reader.Name == "final")
				{
					element.Final = XmlSchemaUtil.ReadDerivationAttribute(reader,out innerex, "final",
						XmlSchemaUtil.FinalAllowed);
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
