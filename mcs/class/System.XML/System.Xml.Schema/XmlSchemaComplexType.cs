//
// System.Xml.Schema.XmlSchemaComplexType.cs
//
// Authors:
//	Dwivedi, Ajay kumar  Adwiv@Yahoo.com
//	Enomoto, Atsushi     ginga@kit.hi-ho.ne.jp
//
using System;
using System.Collections;
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
		private XmlSchemaParticle contentTypeParticle;
		private bool isAbstract;
		private bool isMixed;
		private XmlSchemaParticle particle;
		private XmlSchemaContentType resolvedContentType;

		internal bool ValidatedIsAbstract;
		internal bool ParentIsSchema = false;

		private static string xmlname = "complexType";

		private static XmlSchemaComplexType anyType;

		internal static XmlSchemaComplexType AnyType {
			get {
				if (anyType == null) {
					anyType = new XmlSchemaComplexType ();
					anyType.Name = "";	// In MS.NET, it is not "anyType"
					anyType.QNameInternal = XmlQualifiedName.Empty;	// Not xs:anyType as well.
					anyType.contentTypeParticle = XmlSchemaAny.AnyTypeContent;
//					anyType.baseSchemaTypeInternal = anyType;
					anyType.datatypeInternal = XmlSchemaSimpleType.AnySimpleType;
					anyType.isMixed = true;
				}
				return anyType;
			}
		}

		internal static readonly XmlQualifiedName AnyTypeName = new XmlQualifiedName ("anyType", XmlSchema.Namespace);

		public XmlSchemaComplexType()
		{
			attributes = new XmlSchemaObjectCollection();
			block = XmlSchemaDerivationMethod.None;
			attributeUses = new XmlSchemaObjectTable();
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
		[DefaultValue(false)]
		[System.Xml.Serialization.XmlAttribute("mixed")]
		public override bool IsMixed
		{
			get{ return  isMixed; }
			set{ isMixed = value; }
		}
		
		#endregion
		
		#region Elements
				
		[XmlElement("simpleContent",typeof(XmlSchemaSimpleContent),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("complexContent",typeof(XmlSchemaComplexContent),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaContentModel ContentModel 
		{
			get{ return  contentModel; } 
			set{ contentModel = value; }
		}

		//LAMESPEC: The default value for particle in Schema is of Type EmptyParticle (internal?)
		[XmlElement("group",typeof(XmlSchemaGroupRef),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("all",typeof(XmlSchemaAll),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("choice",typeof(XmlSchemaChoice),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("sequence",typeof(XmlSchemaSequence),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaParticle Particle 
		{
			get{ return  particle; } 
			set{ particle = value; }
		}

		[XmlElement("attribute",typeof(XmlSchemaAttribute),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("attributeGroup",typeof(XmlSchemaAttributeGroupRef),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaObjectCollection Attributes 
		{
			get{ return attributes; }
		}

		[XmlElement("anyAttribute",Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaAnyAttribute AnyAttribute 
		{
			get{ return  anyAttribute; }
			set{ anyAttribute = value; }
		}
		#endregion

		#region XmlIgnore
		[XmlIgnore]
		public XmlSchemaContentType ContentType 
		{
			get{ return resolvedContentType; }
		}
		[XmlIgnore]
		[MonoTODO ("Derivation is not supported yet.")]
		public XmlSchemaParticle ContentTypeParticle 
		{
			get{ return contentTypeParticle; }
		}
		[XmlIgnore]
		public XmlSchemaDerivationMethod BlockResolved 
		{
			get{ return blockResolved; }
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
		#endregion

		/// <remarks>
		/// 1. If ContentModel is present, neither particle nor Attributes nor AnyAttribute can be present.
		/// 2. If particle is present, 
		/// a. For a topLevelComplexType
		///		1. name must be present and type NCName
		///		2. if block is #all, blockdefault is #all, else List of (extension | restriction)
		///		3. if final is #all, finaldefault is #all, else List of (extension | restriction)
		///	b. For a local Complex type 
		///		1. abstract must be false
		///		2. Name must be absent
		///		3. final must be absent
		///		4. block must be absent
		///		
		/// </remarks>
		[MonoTODO]
		internal override int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return errorCount;

			ValidatedIsAbstract = isAbstract;

			if (isRedefinedComponent) {
				if (Annotation != null)
					Annotation.isRedefinedComponent = true;
				if (AnyAttribute != null)
					AnyAttribute.isRedefinedComponent = true;
				foreach (XmlSchemaObject obj in Attributes)
					obj.isRedefinedComponent = true;
				if (ContentModel != null)
					ContentModel.isRedefinedComponent = true;
				if (Particle != null)
					Particle.isRedefinedComponent = true;
			}

			// block/final resolution
			if(ParentIsSchema || isRedefineChild)
			{
				if(this.Name == null || this.Name == string.Empty)
					error(h,"name must be present in a top level complex type");
				else if(!XmlSchemaUtil.CheckNCName(Name))
					error(h,"name must be a NCName");
				else
					this.QNameInternal = new XmlQualifiedName(Name, schema.TargetNamespace);
				
				if(Block != XmlSchemaDerivationMethod.None)
				{
					if(Block == XmlSchemaDerivationMethod.All)
					{
						blockResolved = XmlSchemaDerivationMethod.All;
					}
					else
					{
						//TODO: Check what all is not allowed
						if ((Block & XmlSchemaUtil.ComplexTypeBlockAllowed) != Block)
							error (h, "Invalid block specification.");
						blockResolved = Block & XmlSchemaUtil.ComplexTypeBlockAllowed;
					}
				}
				else
				{
					switch (schema.BlockDefault) {
					case XmlSchemaDerivationMethod.All:
						blockResolved = XmlSchemaDerivationMethod.All;
						break;
					case XmlSchemaDerivationMethod.None:
						blockResolved = XmlSchemaDerivationMethod.Empty;
						break;
					default:
						blockResolved = schema.BlockDefault & XmlSchemaUtil.ComplexTypeBlockAllowed;
						break;
					}
				}

				if(Final != XmlSchemaDerivationMethod.None)
				{
					if(Final == XmlSchemaDerivationMethod.All)
						finalResolved = XmlSchemaDerivationMethod.All;
					else if ((Final & XmlSchemaUtil.FinalAllowed) != Final)
						error (h, "Invalid final specification.");
					else
						finalResolved = Final;
				}
				else
				{
					switch (schema.FinalDefault) {
					case XmlSchemaDerivationMethod.All:
						finalResolved = XmlSchemaDerivationMethod.All;
						break;
					case XmlSchemaDerivationMethod.None:
						finalResolved = XmlSchemaDerivationMethod.Empty;
						break;
					default:
						finalResolved = schema.FinalDefault & XmlSchemaUtil.FinalAllowed;
						break;
					}
				}
			}
			else // Not Top Level
			{
				if(isAbstract)
					error(h,"abstract must be false in a local complex type");
				if(Name != null)
					error(h,"name must be absent in a local complex type");
				if(Final != XmlSchemaDerivationMethod.None)
					error(h,"final must be absent in a local complex type");
				if(block != XmlSchemaDerivationMethod.None)
					error(h,"block must be absent in a local complex type");
			}

			// Process contents and BaseSchemaType
			if(contentModel != null)
			{
				if(anyAttribute != null || Attributes.Count != 0 || Particle != null)
					error(h,"attributes, particles or anyattribute is not allowed if ContentModel is present");
				errorCount += contentModel.Compile (h, schema);

				XmlQualifiedName baseTypeName = null;
				XmlSchemaSimpleContent smodel = ContentModel as XmlSchemaSimpleContent;
				if(smodel != null)
				{
					XmlSchemaSimpleContentExtension sscx = smodel.Content as XmlSchemaSimpleContentExtension;
					if (sscx != null)
						baseTypeName = sscx.BaseTypeName;
					else {
						XmlSchemaSimpleContentRestriction sscr = smodel.Content as XmlSchemaSimpleContentRestriction;
						if (sscr != null) {
							baseTypeName = sscr.BaseTypeName;
							if (sscr.BaseType != null) {
								sscr.BaseType.Compile (h, schema);
								baseSchemaTypeInternal = sscr.BaseType;
							}
						}
					}
					contentTypeParticle = XmlSchemaParticle.Empty;
				}
				else
				{
					XmlSchemaComplexContent cmodel = (XmlSchemaComplexContent) ContentModel;
					XmlSchemaComplexContentExtension sccx = cmodel.Content as XmlSchemaComplexContentExtension;
					if (sccx != null) {
						contentTypeParticle = sccx.Particle;
						baseTypeName = sccx.BaseTypeName;
					}
					else {
						XmlSchemaComplexContentRestriction sccr = (XmlSchemaComplexContentRestriction) cmodel.Content;
						if (sccr != null) {
							contentTypeParticle = sccr.Particle;
							baseTypeName = sccr.BaseTypeName;
						}
					}
				}
			}
			else
			{
				if(Particle is XmlSchemaGroupRef)
				{
					XmlSchemaGroupRef xsgr = (XmlSchemaGroupRef)Particle;
					errorCount += xsgr.Compile(h,schema);
				}
				else if(Particle is XmlSchemaAll)
				{
					XmlSchemaAll xsa = (XmlSchemaAll)Particle;
					errorCount += xsa.Compile(h,schema);
				}
				else if(Particle is XmlSchemaChoice)
				{
					XmlSchemaChoice xsc = (XmlSchemaChoice)Particle;
					errorCount += xsc.Compile(h,schema);
				}
				else if(Particle is XmlSchemaSequence)
				{
					XmlSchemaSequence xss = (XmlSchemaSequence)Particle;
					errorCount += xss.Compile(h,schema);
				}
				this.contentTypeParticle = Particle;

				if(this.anyAttribute != null)
				{
					AnyAttribute.Compile(h,schema);
				}
				foreach(XmlSchemaObject obj in Attributes)
				{
					if(obj is XmlSchemaAttribute)
					{
						XmlSchemaAttribute attr = (XmlSchemaAttribute) obj;
						errorCount += attr.Compile(h,schema);
					}
					else if(obj is XmlSchemaAttributeGroupRef)
					{
						XmlSchemaAttributeGroupRef atgrp = (XmlSchemaAttributeGroupRef) obj;
						errorCount += atgrp.Compile(h,schema);
					}
					else
						error(h,obj.GetType() +" is not valid in this place::ComplexType");
				}
			}

			XmlSchemaUtil.CompileID(Id, this, schema.IDCollection, h);
			this.CompilationId = schema.CompilationId;
			return errorCount;
		}

		[MonoTODO]
		internal override int Validate(ValidationEventHandler h, XmlSchema schema)
		{
			if (IsValidated (schema.ValidationId))
				return errorCount;
			// FIXME: omitting it causes StackOverflowException
			// among the compilation of element and types, but
			// it may result in insufficient results.
			ValidationId = schema.ValidationId;

			// Term. 1 of 3.4.6 = 3.4.1 : Complex Type Definitions Properties Correct
			// Term. 2 and 3 goes ValidateContentModel().
			// Term. 4 and 5 follows in this method.
			//
			// Schema component to CLR type property mapping:
			// {derivation method} => resolvedDerivedBy
			// {annotations} are as is.
			// {name}, {namespace} => QualifiedName
			// {final} and {prohibited substitutions} are Compile()d.
			// {abstract} => ValidatedIsAbstract

			// Below are different properties depending on simpleContent | complexContent.
			// {base type definition} => BaseSchemaType (later)
			// {attribute uses} => AttributeUses (later)
			// {content type} => ContentType and ContentTypeParticle (later)

			// TODO: Beware of E1-27 of http://www.w3.org/2001/05/xmlschema-errata#Errata1
			if (contentModel != null)
				ValidateContentModel (h, schema);
			else {
				if (particle != null)
					ValidateParticle (h, schema);
				// contentModel never has them.
				ValidateImmediateAttributes (h, schema);
			}
			// Additional support for 3.8.6 All Group Limited
			if (contentTypeParticle != null) {
				XmlSchemaAll termAll = contentTypeParticle.ActualParticle as XmlSchemaAll;
				if (termAll != null && contentTypeParticle.ValidatedMaxOccurs != 1)
					error (h, "Particle whose term is -all- and consists of complex type content particle must have maxOccurs = 1.");
			}

			// {content type} is going to be finished.
			if (contentTypeParticle == null)
				contentTypeParticle = XmlSchemaParticle.Empty;
			contentTypeParticle.ValidateUniqueParticleAttribution (new XmlSchemaObjectTable (),
				new ArrayList (), h, schema);
			contentTypeParticle.ValidateUniqueTypeAttribution (
				new XmlSchemaObjectTable (), h, schema);
			resolvedContentType = GetContentType ();

			// 3.4.6 Properties Correct :: 5 (Two distinct ID attributes)
			XmlSchemaAttribute idAttr = null;
			foreach (XmlSchemaAttribute attr in attributeUses) {
				XmlSchemaDatatype dt = attr.AttributeType as XmlSchemaDatatype;
				if (dt != null && dt.TokenizedType != XmlTokenizedType.ID)
					continue;
				if (dt == null)
					dt = ((XmlSchemaSimpleType) attr.AttributeType).Datatype;
				if (dt != null && dt.TokenizedType == XmlTokenizedType.ID) {
					if (idAttr != null)
						error (h, "Two or more ID typed attribute declarations in a complex type are found.");
					else
						idAttr = attr;
				}
			}

			ValidationId = schema.ValidationId;
			return errorCount;
		}

		private void ValidateParticle (ValidationEventHandler h, XmlSchema schema)
		{
			// {content type} as a particle.
			errorCount += particle.Validate (h, schema);
			contentTypeParticle = Particle;
			XmlSchemaGroupRef pgrp = Particle as XmlSchemaGroupRef;
			if (pgrp != null) {
				if (pgrp.TargetGroup != null)
					errorCount += pgrp.TargetGroup.Validate (h,schema);
				// otherwise, it might be missing sub components.
				else if (!schema.IsNamespaceAbsent (pgrp.RefName.Namespace))
					error (h, "Referenced group " + pgrp.RefName + " was not found in the corresponding schema.");
			}
		}

		private void ValidateImmediateAttributes (ValidationEventHandler h, XmlSchema schema)
		{
			// {attribute uses}
			// also checks 3.4.6 Properties Correct :: 4 (Distinct attributes)
			attributeUses = new XmlSchemaObjectTable ();
			XmlSchemaUtil.ValidateAttributesResolved (attributeUses,
				h, schema, attributes, anyAttribute, ref attributeWildcard, null);
		}

		private void ValidateContentModel (ValidationEventHandler h, XmlSchema schema)
		{
			// Here we check 3.4.6 Properties Correct :: 2. and 3.
			errorCount += contentModel.Validate (h, schema);
			XmlSchemaComplexContentExtension cce = contentModel.Content as XmlSchemaComplexContentExtension;
			XmlSchemaComplexContentRestriction ccr = contentModel.Content as XmlSchemaComplexContentRestriction;
			XmlSchemaSimpleContentExtension sce = contentModel.Content as XmlSchemaSimpleContentExtension;
			XmlSchemaSimpleContentRestriction scr = contentModel.Content as XmlSchemaSimpleContentRestriction;

			XmlSchemaAnyAttribute localAnyAttribute = null;
			XmlSchemaAnyAttribute baseAnyAttribute = null;

			XmlQualifiedName baseTypeName = null;
			if (cce != null)
				baseTypeName = cce.BaseTypeName;
			else if (ccr != null)
				baseTypeName = ccr.BaseTypeName;
			else if (sce != null)
				baseTypeName = sce.BaseTypeName;
			else
				baseTypeName = scr.BaseTypeName;

			XmlSchemaType baseType = schema.SchemaTypes [baseTypeName] as XmlSchemaType;
			// Resolve redefine.
			if (this.isRedefineChild && baseType != null && this.QualifiedName == baseTypeName) {
				baseType = (XmlSchemaType) redefinedObject;
				if (baseType == null)
					error (h, "Redefinition base type was not found.");
				baseType.Validate (h, schema);
			}
			// 3.4.6 Properties Correct :: 3. Circular definition prohibited.
			if (ValidateRecursionCheck ())
				error (h, "Circular definition of schema types was found.");
			if (baseType != null) {
				baseType.Validate (h, schema);
				// Fill "Datatype" property.
				this.datatypeInternal = baseType.Datatype;
			} else if (baseTypeName == XmlSchemaComplexType.AnyTypeName)
				datatypeInternal = XmlSchemaSimpleType.AnySimpleType;
			else if (baseTypeName.Namespace == XmlSchema.Namespace) {
				datatypeInternal = XmlSchemaDatatype.FromName (baseTypeName);
			}

			// {derivation method}
			XmlSchemaComplexType baseComplexType = baseType as XmlSchemaComplexType;
			XmlSchemaSimpleType baseSimpleType = baseType as XmlSchemaSimpleType;
			if (cce != null || sce != null)
				resolvedDerivedBy = XmlSchemaDerivationMethod.Extension;
			else
				resolvedDerivedBy = XmlSchemaDerivationMethod.Restriction;

			// 3.4.6 Derivation Valid (common to Extension and Restriction, Complex) :: 1.
			if (baseType != null && (baseType.FinalResolved & resolvedDerivedBy) != 0)
					error (h, "Specified derivation is specified as final by derived schema type.");

			// 3.4.6 Properties Correct :: 2.
			// Simple {base type definition} and restriction {derivation method} not allowed.
			if (baseSimpleType != null && resolvedDerivedBy == XmlSchemaDerivationMethod.Restriction)
				error (h, "If the base schema type is a simple type, then this type must be extension.");

			// Common to complexContent
			if (cce != null || ccr != null) {
				// 3.4.3 Complex Type Definition Representation OK :: 1.
				// base
				if (baseTypeName == XmlSchemaComplexType.AnyTypeName)
					baseComplexType = XmlSchemaComplexType.AnyType;
				else if (baseTypeName.Namespace == XmlSchema.Namespace)
					error (h, "Referenced base schema type is XML Schema datatype.");
				else if (baseComplexType == null && !schema.IsNamespaceAbsent (baseTypeName.Namespace))
					error (h, "Referenced base schema type " + baseTypeName + " was not complex type or not found in the corresponding schema.");
			}
			// Common to simpleContent 
			else {
				// 3.4.3 Complex Type Definition Representation OK :: 1.
				// base
				if (baseTypeName == XmlSchemaComplexType.AnyTypeName)
					baseComplexType = XmlSchemaComplexType.AnyType;

				if (baseComplexType != null && baseComplexType.ContentType != XmlSchemaContentType.TextOnly) {
					error (h, "Base schema complex type of a simple content must be simple content type. Base type is " + baseTypeName);
				} else if (sce == null && (baseSimpleType != null && baseTypeName.Namespace != XmlSchema.Namespace)) {
					error (h, "If a simple content is not an extension, base schema type must be complex type. Base type is " + baseTypeName);
				} else if (baseTypeName.Namespace == XmlSchema.Namespace) {
					if (XmlSchemaDatatype.FromName (baseTypeName) == null)
						error (h, "Invalid schema data type was specified: " + baseTypeName);
					// do nothing for particle.
				}
				// otherwise, it might be missing sub components.
				else if (baseType == null && !schema.IsNamespaceAbsent (baseTypeName.Namespace))// && schema.Schemas [baseTypeName.Namespace] != null)
					error (h, "Referenced base schema type " + baseTypeName + " was not found in the corresponding schema.");
			}

			// complexType/complexContent/extension
			if (cce != null) {
				// ContentTypeParticle
				if (baseComplexType == null) {
					// Basically it is an error. Considering ValidationEventHandler.
				}
				else if (baseComplexType.ContentTypeParticle == XmlSchemaParticle.Empty
					|| baseComplexType == XmlSchemaComplexType.AnyType)
					contentTypeParticle = cce.Particle;
				else if (cce.Particle == null || cce.Particle == XmlSchemaParticle.Empty)
					contentTypeParticle = baseComplexType.ContentTypeParticle;
				else {
					// create a new sequences that merges both contents.
					XmlSchemaSequence seq = new XmlSchemaSequence ();
					seq.Items.Add (baseComplexType.ContentTypeParticle);
					seq.Items.Add (cce.Particle);
					seq.Compile (h, schema);
					seq.Validate (h, schema);
					contentTypeParticle = seq;
				}

				// I don't think 3.4.6 Derivation Valid (Extension) :: 1.2
				// is constraining anything here, since 3.4.2 {attribute uses}
				// defines as to include base type's attribute uses.
				localAnyAttribute = cce.AnyAttribute;
				if (baseComplexType != null) {
					foreach (XmlSchemaAttribute attr in baseComplexType.AttributeUses)
						XmlSchemaUtil.AddToTable (attributeUses, attr, attr.QualifiedName, h);
					baseAnyAttribute = baseComplexType.AttributeWildcard;
				}
				// attributes
				errorCount += XmlSchemaUtil.ValidateAttributesResolved (
					this.attributeUses, h, schema, cce.Attributes, 
					cce.AnyAttribute , ref attributeWildcard, null);

				// After adding them, test extension validity.
				if (baseComplexType != null)
					this.ValidateComplexBaseDerivationValidExtension (baseComplexType, h, schema);
				else if (baseSimpleType != null)
					this.ValidateSimpleBaseDerivationValidExtension (baseSimpleType, h, schema);
			}
			// complexType/complexContent/restriction
			if (ccr != null) {
				// For ValidationEventHandler.
				if (baseComplexType == null)
					baseComplexType = XmlSchemaComplexType.AnyType;

				// ContentTypeParticles (It must contain base type's particle).
				if (ccr.Particle != null) {
					ccr.Particle.Validate (h, schema);
					contentTypeParticle = ccr.Particle;
				}
				else
					contentTypeParticle = XmlSchemaParticle.Empty;

				localAnyAttribute = ccr.AnyAttribute;
				this.attributeWildcard = localAnyAttribute;
				if (baseComplexType != null)
					baseAnyAttribute = baseComplexType.AttributeWildcard;
				if (baseAnyAttribute != null && localAnyAttribute != null)
					// 1.3 attribute wildcard subset. (=> 3.10.6)
					localAnyAttribute.ValidateWildcardSubset (baseAnyAttribute, h, schema);

				// FIXME: Check 3.4.2 Complex Type Definition with complex content Schema Component
				// and its {attribute uses} and {attribute wildcard}
				errorCount += XmlSchemaUtil.ValidateAttributesResolved (
					this.attributeUses, h, schema, ccr.Attributes, 
					ccr.AnyAttribute, ref attributeWildcard, null);
				foreach (XmlSchemaAttribute attr in baseComplexType.AttributeUses) {
					if (attributeUses [attr.QualifiedName] == null)
						XmlSchemaUtil.AddToTable (attributeUses, attr, attr.QualifiedName, h);
				}

				// Derivation Valid (Restriction, Complex) :: 5.
				// Also see E1-15 of http://www.w3.org/2001/05/xmlschema-errata#Errata1
				// 5.1 shouled be in scr (XmlSchemaSimpleContentRestriction)
				this.ValidateDerivationValidRestriction (baseComplexType, h, schema);
			}
			// complexType/simpleContent/extension
			if (sce != null) {
				errorCount += XmlSchemaUtil.ValidateAttributesResolved (
					this.attributeUses, h, schema, sce.Attributes, 
					sce.AnyAttribute, ref attributeWildcard, null);

				// Attributes
				// I don't think 3.4.6 Derivation Valid (Extension) :: 1.2
				// is constraining anything here, since 3.4.2 {attribute uses}
				// defines as to include base type's attribute uses.
				localAnyAttribute = sce.AnyAttribute;

				if (baseComplexType != null) {
					baseAnyAttribute = baseComplexType.AttributeWildcard;

					foreach (XmlSchemaAttribute attr in baseComplexType.AttributeUses)
						XmlSchemaUtil.AddToTable (attributeUses, attr, attr.QualifiedName, h);
				}
				if (baseAnyAttribute != null && localAnyAttribute != null)
					// 1.3 attribute wildcard subset. (=> 3.10.6)
					localAnyAttribute.ValidateWildcardSubset (baseAnyAttribute, h, schema);
			}
			// complexType/simpleContent/restriction
			if (scr != null) {
				if (baseComplexType == null) {
					// 3.4.3 :: 2.
					error (h, "Base type of a simple content restriction must be a complex type.");
				} else {
					// Attributes
					baseAnyAttribute = baseComplexType.AttributeWildcard;

					localAnyAttribute = scr.AnyAttribute;
					if (localAnyAttribute != null && baseAnyAttribute != null)
						// 1.3 attribute wildcard subset. (=> 3.10.6)
						localAnyAttribute.ValidateWildcardSubset (baseAnyAttribute, h, schema);
					// TODO: 3.4.6 :: 5.1. Beware that There is an errata for 5.1!!
					// http://www.w3.org/2001/05/xmlschema-errata#Errata1

					// FIXME: Check 3.4.2 Complex Type Definition with simple content Schema Component
					// and its {attribute uses} and {attribute wildcard}
					errorCount += XmlSchemaUtil.ValidateAttributesResolved (
						this.attributeUses, h, schema, scr.Attributes, 
						scr.AnyAttribute, ref attributeWildcard, null);
				}
			}

			// Common process of AttributeWildcard.
			// TODO: Check 3.4.2 {attribute wildcard} to fill the complete wildcard.
			if (localAnyAttribute != null) {
				this.attributeWildcard = localAnyAttribute;
			}
			else
				this.attributeWildcard = baseAnyAttribute;
			this.baseSchemaTypeInternal = baseType;
		}

		private void AddExtensionAttributes (XmlSchemaObjectCollection attributes,
			XmlSchemaAnyAttribute anyAttribute, ValidationEventHandler h, XmlSchema schema)
		{
		}

		// It was formerly placed directly in ContentType property.
		// I get it out, since ContentType is _post_ compilation property value.
		private XmlSchemaContentType GetContentType ()
		{
			if (this.isMixed)
				return XmlSchemaContentType.Mixed;
			XmlSchemaComplexContent xcc = 
				ContentModel as XmlSchemaComplexContent;
			if (xcc != null && xcc.IsMixed)
				return XmlSchemaContentType.Mixed;

			XmlSchemaSimpleContent xsc = ContentModel as XmlSchemaSimpleContent;
			if (xsc != null)
				return XmlSchemaContentType.TextOnly;

			return contentTypeParticle != XmlSchemaParticle.Empty ?
				XmlSchemaContentType.ElementOnly :
				XmlSchemaContentType.Empty;
		}

		// 3.4.6 Type Derivation OK (Complex)
		internal void ValidateTypeDerivationOK (object b, ValidationEventHandler h, XmlSchema schema)
		{
			// AnyType derives from AnyType itself.
			if (this == XmlSchemaComplexType.AnyType && BaseSchemaType == this)
				return;

			XmlSchemaType bst = b as XmlSchemaType;
			if (b == this)	// 1 and 2.1
				return;
			if (bst != null && (resolvedDerivedBy & bst.FinalResolved) != 0) // 1
				error (h, "Derivation type " + resolvedDerivedBy + " is prohibited by the base type.");
			if (BaseSchemaType == b) // 2.2
				return;
			if (BaseSchemaType == XmlSchemaComplexType.AnyType) { // 2.3.1
				error (h, "Derived type's base schema type is anyType.");
				return;
			}
			// 2.3.2.1
			XmlSchemaComplexType dbct = BaseSchemaType as XmlSchemaComplexType;
			if (dbct != null) {
				dbct.ValidateTypeDerivationOK (b, h, schema);
				return;
			}
			// 2.3.2.2
			XmlSchemaSimpleType dbst = BaseSchemaType as XmlSchemaSimpleType;
			if (dbst != null) {
				dbst.ValidateTypeDerivationOK (b, h, schema, true);
				return;
			}
		}

		// Term. 1 of 3.4.6 Derivation Valid (Extension)
		internal void ValidateComplexBaseDerivationValidExtension (XmlSchemaComplexType baseComplexType,
			ValidationEventHandler h, XmlSchema schema)
		{
			// 1.1
			if ((baseComplexType.FinalResolved & XmlSchemaDerivationMethod.Extension) != 0)
				error (h, "Derivation by extension is prohibited.");
			// 1.2
			foreach (XmlSchemaAttribute ba in baseComplexType.AttributeUses) {
				XmlSchemaAttribute da = AttributeUses [ba.QualifiedName] as XmlSchemaAttribute;
				if (da == null)
					error (h, "Invalid complex type derivation by extension was found. Missing attribute was found: " + ba.QualifiedName + " .");
				// TODO: How to evaluate "equal" type ...?
			}
			// 1.3 -> 3.10.6 Wildcard Subset.
			if (AnyAttribute != null) {
				if (baseComplexType.AnyAttribute == null)
					error (h, "Invalid complex type derivation by extension was found. Base complex type does not have an attribute wildcard.");
				else
					baseComplexType.AnyAttribute.ValidateWildcardSubset (AnyAttribute, h, schema);
			}

			// 1.4 => 1.4.2 (1.4.1 would be included in SimpleContentExtention).
			// 1.4.2.1
//			if (contentTypeParticle == null)
//				error (h, "Extended complex type's content type must not be empty.");
			// 1.4.2.2.1
			if (baseComplexType.ContentType != XmlSchemaContentType.Empty) {
				// 1.4.2.2.2.1
				if (this.GetContentType () == baseComplexType.ContentType) {
					// nothing to do
				}
				// 1.4.2.2.2.2
				// 3.9.6 Particle Valid (Extension)
				else if (this.contentTypeParticle != baseComplexType.ContentTypeParticle) {
					XmlSchemaSequence seq = contentTypeParticle as XmlSchemaSequence;
					if (contentTypeParticle.ValidatedMinOccurs != 1 ||
						contentTypeParticle.ValidatedMaxOccurs != 1 ||
						seq == null)
						error (h, "Invalid complex content extension was found.");
					else {
						// Identical sequence item should be checked, but
						// I think it is naturally achieved as coded above.
					}

				}
				else
					error (h, "Invalid complex content extension was found. Extended complex type has different content type from base type.");
			}
		}

		// Term. 2 of 3.4.6 Derivation Valid (Extension)
		internal void ValidateSimpleBaseDerivationValidExtension (object baseType,
			ValidationEventHandler h, XmlSchema schema)
		{
			XmlSchemaSimpleType st = baseType as XmlSchemaSimpleType;
			if (st != null && (st.FinalResolved & XmlSchemaDerivationMethod.Extension) != 0)
				error (h, "Extension is prohibited by the base type.");

			XmlSchemaDatatype dt = baseType as XmlSchemaDatatype;
			if (dt == null)
				dt = st.Datatype;
			if (dt != this.Datatype)
				error (h, "To extend simple type, a complex type must have the same content type as the base type.");

			/*
			switch (resolvedContentType) {
			case XmlSchemaContentType.Mixed:
			case XmlSchemaContentType.TextOnly:
				XmlSchemaSimpleType st = baseType as XmlSchemaSimpleType;
				if ((st == null && Datatype != baseType) ||
					(st != null && st.Datatype != Datatype))
					goto case XmlSchemaContentType.ElementOnly;
				if (st != null
					&& (st.FinalResolved & XmlSchemaDerivationMethod.Extension) != 0)
					error (h, "Extension is prohibited by the base type.");
				break;
			case XmlSchemaContentType.ElementOnly:
			case XmlSchemaContentType.Empty:
				error (h, "To extend simple type, a complex type must have the same content type as the base type.");
				break;
			}
			*/
		}

		internal void ValidateDerivationValidRestriction (XmlSchemaComplexType baseType,
			ValidationEventHandler h, XmlSchema schema)
		{
			// 1.
			if (baseType == null) {
				error (h, "Base schema type is not a complex type.");
				return;
			}
			if ((baseType.FinalResolved & XmlSchemaDerivationMethod.Restriction) != 0) {
				error (h, "Prohibited derivation by restriction by base schema type.");
				return;
			}

			// 2.
			foreach (XmlSchemaAttribute attr in this.AttributeUses) {
				XmlSchemaAttribute baseAttr = baseType.AttributeUses [attr.QualifiedName] as XmlSchemaAttribute;
				if (baseAttr != null) {
					// 2.1
					// 2.1.1
					if (baseAttr.ValidatedUse != XmlSchemaUse.Optional && attr.ValidatedUse != XmlSchemaUse.Required)
						error (h, "Invalid attribute derivation by restriction was found for " + attr.QualifiedName + " .");
					// 2.1.2
					XmlSchemaSimpleType attrSimpleType = attr.AttributeType as XmlSchemaSimpleType;
					XmlSchemaSimpleType baseAttrSimpleType = baseAttr.AttributeType as XmlSchemaSimpleType;
					bool typeError = false;
					if (attrSimpleType != null)
						attrSimpleType.ValidateDerivationValid (baseAttrSimpleType, null, h, schema);
					else if (attrSimpleType == null && baseAttrSimpleType != null)
						typeError = true;
					else {
						Type t1 = attr.AttributeType.GetType ();
						Type t2 = baseAttr.AttributeType.GetType ();
						if (t1 != t2 && t1.IsSubclassOf (t2))
							typeError = true;
					}
					if (typeError)
						error (h, "Invalid attribute derivation by restriction because of its type: " + attr.QualifiedName + " .");
					// 2.1.3
					if (baseAttr.ValidatedFixedValue != null && attr.ValidatedFixedValue != baseAttr.ValidatedFixedValue)
						error (h, "Invalid attribute derivation by restriction because of its fixed value constraint: " + attr.QualifiedName + " .");
				} else {
					// 2.2
					if (baseType.AttributeWildcard != null)
						if (!baseType.AttributeWildcard.ValidateWildcardAllowsNamespaceName (
							attr.QualifiedName.Namespace, schema) &&
							!schema.IsNamespaceAbsent (attr.QualifiedName.Namespace))
							error (h, "Invalid attribute derivation by restriction was found for " + attr.QualifiedName + " .");
				}
			}
			// I think 3. is considered in 2.
			// 4.
			if (this.AttributeWildcard != null) {
				if (baseType.AttributeWildcard == null)
					error (h, "Invalid attribute derivation by restriction because of attribute wildcard.");
				else
					AttributeWildcard.ValidateWildcardSubset (baseType.AttributeWildcard, h, schema);
			}

			// 5.
			if (contentTypeParticle == XmlSchemaParticle.Empty) {
				// TODO: 5.1
				// 5.2
				if (baseType.ContentTypeParticle != XmlSchemaParticle.Empty &&
					!baseType.ContentTypeParticle.ValidateIsEmptiable ())
				error (h, "Invalid content type derivation.");
			} else {
				// 5.3 => 3.9.6 Particle Valid (Restriction)
				if (baseType.ContentTypeParticle != null) {
					// 3.9.6 - 1 : same particle.
					// 3.9.6 - 2 is covered by using ActualParticle.
					if (!contentTypeParticle.ActualParticle.ParticleEquals (baseType.ContentTypeParticle.ActualParticle))
						contentTypeParticle.ActualParticle.ValidateDerivationByRestriction (
							baseType.ContentTypeParticle.ActualParticle, h, schema);
				}
			}
		}

#region Read
		//<complexType
		//  abstract = boolean : false
		//  block = (#all | List of (extension | restriction)) 
		//  final = (#all | List of (extension | restriction)) 
		//  id = ID
		//  mixed = boolean : false
		//  name = NCName
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?, (simpleContent | complexContent | ((group | all | choice | sequence)?, ((attribute | attributeGroup)*, anyAttribute?))))
		//</complexType>
		internal static XmlSchemaComplexType Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaComplexType ctype = new XmlSchemaComplexType();
			reader.MoveToElement();
			Exception innerex;

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaComplexType.Read, name="+reader.Name,null);
				reader.SkipToEnd();
				return null;
			}

			ctype.LineNumber = reader.LineNumber;
			ctype.LinePosition = reader.LinePosition;
			ctype.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "abstract")
				{
					ctype.IsAbstract = XmlSchemaUtil.ReadBoolAttribute(reader,out innerex);
					if(innerex != null)
						error(h,reader.Value + " is invalid value for abstract",innerex);
				}
				else if(reader.Name == "block")
				{
					ctype.block = XmlSchemaUtil.ReadDerivationAttribute(reader,out innerex, "block",
						XmlSchemaUtil.ComplexTypeBlockAllowed);
					if(innerex != null)
						warn(h,"some invalid values for block attribute were found",innerex);
				}
				else if(reader.Name == "final")
				{
					ctype.Final = XmlSchemaUtil.ReadDerivationAttribute(reader,out innerex, "final",
						XmlSchemaUtil.FinalAllowed);
					if(innerex != null)
						warn(h,"some invalid values for final attribute were found",innerex);
				}
				else if(reader.Name == "id")
				{
					ctype.Id = reader.Value;
				}
				else if(reader.Name == "mixed")
				{
					ctype.isMixed = XmlSchemaUtil.ReadBoolAttribute(reader,out innerex);
					if(innerex != null)
						error(h,reader.Value + " is invalid value for mixed",innerex);
				}
				else if(reader.Name == "name")
				{
					ctype.Name = reader.Value;
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for complexType",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,ctype);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return ctype;

			//Content: 1. annotation?, 
			//		   2. simpleContent | 2. complexContent | 
			//			(3.(group | all | choice | sequence)?, (4.(attribute | attributeGroup)*, 5.anyAttribute?)))
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaComplexType.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2; //Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						ctype.Annotation = annotation;
					continue;
				}
				if(level <=2)
				{
					if(reader.LocalName == "simpleContent")
					{
						level = 6;
						XmlSchemaSimpleContent simple = XmlSchemaSimpleContent.Read(reader,h);
						if(simple != null)
							ctype.ContentModel = simple;
						continue;
					}
					if(reader.LocalName == "complexContent")
					{
						level = 6;
						XmlSchemaComplexContent complex = XmlSchemaComplexContent.Read(reader,h);
						if(complex != null)
							ctype.contentModel = complex;
						continue;
					}
				}
				if(level <= 3)
				{
					if(reader.LocalName == "group")
					{
						level = 4;
						XmlSchemaGroupRef group = XmlSchemaGroupRef.Read(reader,h);
						if(group != null)
							ctype.particle = group;
						continue;
					}
					if(reader.LocalName == "all")
					{
						level = 4;
						XmlSchemaAll all = XmlSchemaAll.Read(reader,h);
						if(all != null)
							ctype.particle = all;
						continue;
					}
					if(reader.LocalName == "choice")
					{
						level = 4;
						XmlSchemaChoice choice = XmlSchemaChoice.Read(reader,h);
						if(choice != null)
							ctype.particle = choice;
						continue;
					}
					if(reader.LocalName == "sequence")
					{
						level = 4;
						XmlSchemaSequence sequence = XmlSchemaSequence.Read(reader,h);
						if(sequence != null)
							ctype.particle = sequence;
						continue;
					}
				}
				if(level <= 4)
				{
					if(reader.LocalName == "attribute")
					{
						level = 4;
						XmlSchemaAttribute attr = XmlSchemaAttribute.Read(reader,h);
						if(attr != null)
							ctype.Attributes.Add(attr);
						continue;
					}
					if(reader.LocalName == "attributeGroup")
					{
						level = 4;
						XmlSchemaAttributeGroupRef attr = XmlSchemaAttributeGroupRef.Read(reader,h);
						if(attr != null)
							ctype.attributes.Add(attr);
						continue;
					}
				}
				if(level <= 5 && reader.LocalName == "anyAttribute")
				{
					level = 6;
					XmlSchemaAnyAttribute anyattr = XmlSchemaAnyAttribute.Read(reader,h);
					if(anyattr != null)
						ctype.AnyAttribute = anyattr;
					continue;
				}
				reader.RaiseInvalidElementError();
			}
			return ctype;
		}
#endregion
	}
}
