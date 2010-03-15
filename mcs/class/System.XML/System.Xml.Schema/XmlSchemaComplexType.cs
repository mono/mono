//
// System.Xml.Schema.XmlSchemaComplexType.cs
//
// Authors:
//	Dwivedi, Ajay kumar  Adwiv@Yahoo.com
//	Enomoto, Atsushi     ginga@kit.hi-ho.ne.jp
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
		private XmlSchemaParticle validatableParticle;
		private XmlSchemaParticle contentTypeParticle;
		private bool isAbstract;
		private bool isMixed;
		private XmlSchemaParticle particle;
		private XmlSchemaContentType resolvedContentType;

		internal bool ValidatedIsAbstract;
		internal bool ParentIsSchema {
			get { return Parent is XmlSchema; }
		}

		const string xmlname = "complexType";

		private static XmlSchemaComplexType anyType;

		internal static XmlSchemaComplexType AnyType {
			get {
				if (anyType == null) {
					anyType = new XmlSchemaComplexType ();
					anyType.Name = "anyType";
					anyType.QNameInternal = new XmlQualifiedName ("anyType", XmlSchema.Namespace);
					if (XmlSchemaUtil.StrictMsCompliant)
						anyType.validatableParticle = XmlSchemaParticle.Empty; // This code makes validator handles these schemas incorrectly: particlesIb001, mgM013, mgH014, ctE004, ctD004
					else
						anyType.validatableParticle = XmlSchemaAny.AnyTypeContent;

					anyType.contentTypeParticle = anyType.validatableParticle;
					anyType.DatatypeInternal = XmlSchemaSimpleType.AnySimpleType;
					anyType.isMixed = true;
					anyType.resolvedContentType = XmlSchemaContentType.Mixed;
				}
				return anyType;
			}
		}

		internal static readonly XmlQualifiedName AnyTypeName = new XmlQualifiedName ("anyType", XmlSchema.Namespace);

		public XmlSchemaComplexType ()
		{
			attributes = new XmlSchemaObjectCollection();
			block = XmlSchemaDerivationMethod.None;
			attributeUses = new XmlSchemaObjectTable();
			validatableParticle = XmlSchemaParticle.Empty;
			contentTypeParticle = validatableParticle;
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
				
		[XmlElement("simpleContent",typeof(XmlSchemaSimpleContent))]
		[XmlElement("complexContent",typeof(XmlSchemaComplexContent))]
		public XmlSchemaContentModel ContentModel 
		{
			get{ return  contentModel; } 
			set{ contentModel = value; }
		}

		//LAMESPEC: The default value for particle in Schema is of Type EmptyParticle (internal?)
		[XmlElement("group",typeof(XmlSchemaGroupRef))]
		[XmlElement("all",typeof(XmlSchemaAll))]
		[XmlElement("choice",typeof(XmlSchemaChoice))]
		[XmlElement("sequence",typeof(XmlSchemaSequence))]
		public XmlSchemaParticle Particle 
		{
			get{ return  particle; } 
			set{ particle = value; }
		}

		[XmlElement("attribute",typeof(XmlSchemaAttribute))]
		[XmlElement("attributeGroup",typeof(XmlSchemaAttributeGroupRef))]
		public XmlSchemaObjectCollection Attributes 
		{
			get{ return attributes; }
		}

		[XmlElement("anyAttribute")]
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

		internal XmlSchemaParticle ValidatableParticle 
		{
			get{ return contentTypeParticle; }
		}

		internal override void SetParent (XmlSchemaObject parent)
		{
			base.SetParent (parent);
			if (ContentModel != null)
				ContentModel.SetParent (this);
			if (Particle != null)
				Particle.SetParent (this);
			if (AnyAttribute != null)
				AnyAttribute.SetParent (this);
			foreach (XmlSchemaObject obj in Attributes)
				obj.SetParent (this);
		}

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
		internal override int Compile (ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (CompilationId == schema.CompilationId)
				return errorCount;

			ValidatedIsAbstract = isAbstract;
			attributeUses.Clear();

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
					this.QNameInternal = new XmlQualifiedName(Name, AncestorSchema.TargetNamespace);
				
				if(Block != XmlSchemaDerivationMethod.None)
				{
					if(Block == XmlSchemaDerivationMethod.All)
					{
						blockResolved = XmlSchemaDerivationMethod.All;
					}
					else
					{
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

				XmlSchemaSimpleContent smodel = ContentModel as XmlSchemaSimpleContent;
				if(smodel != null)
				{
					XmlSchemaSimpleContentExtension sscx = smodel.Content as XmlSchemaSimpleContentExtension;
					if (sscx == null) {
						XmlSchemaSimpleContentRestriction sscr = smodel.Content as XmlSchemaSimpleContentRestriction;
						if (sscr != null) {
							if (sscr.BaseType != null) {
								sscr.BaseType.Compile (h, schema);
								BaseXmlSchemaTypeInternal = sscr.BaseType;
							}
						}
					}
				}
			}
			else
			{
				if (Particle != null)
					errorCount += Particle.Compile (h, schema);

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

		Guid CollectProcessId;

		private void CollectSchemaComponent (ValidationEventHandler h, XmlSchema schema)
		{
			if (CollectProcessId == schema.CompilationId)
				return;
			// Below are already contributed by Compile():
			// {name}, {namespace} => QualifiedName, QNameInternal
			// {abstract} => ValidatedIsAbstract
			// {prohibited substitutions} => BlockResolved
			// {final} => FinalResolved
			// {annotations} => Annotation (XmlSchemaAnnotated)

			// Below are different properties depending on simpleContent | complexContent.
			// {base type definition}
			// {derivation method}
			// {attribute uses} => AttributeUses (later)
			// {attribute wildcard} => AttributeWildcard (later)
			// {content type}


			// {base type definition} => baseSchemaTypeInternal (later)
			if (contentModel != null) {
				BaseSchemaTypeName = contentModel.Content != null ? contentModel.Content.GetBaseTypeName () : XmlQualifiedName.Empty;

				BaseXmlSchemaTypeInternal = schema.FindSchemaType(BaseSchemaTypeName);
			}
			// Resolve redefine.
			if (this.isRedefineChild && BaseXmlSchemaType != null && this.QualifiedName == BaseSchemaTypeName) {
				XmlSchemaType redType = (XmlSchemaType) redefinedObject;
				if (redType == null)
					error (h, "Redefinition base type was not found.");
				else
					BaseXmlSchemaTypeInternal = redType;
			}

			// {derivation method} => resolvedDerivedBy
			if (contentModel != null && contentModel.Content != null) {
				resolvedDerivedBy =
					contentModel.Content.IsExtension ?
					XmlSchemaDerivationMethod.Extension :
					XmlSchemaDerivationMethod.Restriction;
			}
			else
				resolvedDerivedBy = XmlSchemaDerivationMethod.Empty;
		}

		void FillContentTypeParticle (ValidationEventHandler h, XmlSchema schema)
		{
			if (CollectProcessId == schema.CompilationId)
				return;
			CollectProcessId = schema.CompilationId;

			var ct = BaseXmlSchemaType as XmlSchemaComplexType;
			if (ct != null)
				ct.FillContentTypeParticle (h, schema);

			// {content type} => ContentType and ContentTypeParticle (later)
			if (ContentModel != null) {
				CollectContentTypeFromContentModel (h, schema);
			} else
				CollectContentTypeFromImmediateContent ();

			contentTypeParticle = validatableParticle.GetOptimizedParticle (true);
			if (contentTypeParticle == XmlSchemaParticle.Empty && resolvedContentType == XmlSchemaContentType.ElementOnly)
				resolvedContentType = XmlSchemaContentType.Empty;
		}

		#region {content type}
		private void CollectContentTypeFromImmediateContent ()
		{
			// leave resolvedDerivedBy as Empty
			if (Particle != null)
				validatableParticle = Particle;
			if (this == AnyType) {
				resolvedContentType = XmlSchemaContentType.Mixed;
				return;
			}

			if (validatableParticle == XmlSchemaParticle.Empty) {
				// note that this covers "Particle == null" case
				if (this.IsMixed)
					resolvedContentType = XmlSchemaContentType.TextOnly;
				else
					resolvedContentType = XmlSchemaContentType.Empty;
			} else {
				if (this.IsMixed)
					resolvedContentType = XmlSchemaContentType.Mixed;
				else
					resolvedContentType = XmlSchemaContentType.ElementOnly;
			}
			if (this != AnyType)
				BaseXmlSchemaTypeInternal = XmlSchemaComplexType.AnyType;
		}

		private void CollectContentTypeFromContentModel (ValidationEventHandler h, XmlSchema schema)
		{
			if (ContentModel.Content == null) {
				// basically it is error. Recover by specifying empty content.
				validatableParticle = XmlSchemaParticle.Empty;
				resolvedContentType = XmlSchemaContentType.Empty;
				return;
			}

			if (ContentModel.Content is XmlSchemaComplexContentExtension)
				CollectContentTypeFromComplexExtension (h, schema);
			if (ContentModel.Content is XmlSchemaComplexContentRestriction)
				CollectContentTypeFromComplexRestriction ();
		}

		private void CollectContentTypeFromComplexExtension (ValidationEventHandler h, XmlSchema schema)
		{
			XmlSchemaComplexContentExtension cce = (XmlSchemaComplexContentExtension) ContentModel.Content;
			XmlSchemaComplexType baseComplexType = this.BaseXmlSchemaType as XmlSchemaComplexType;
			if (baseComplexType != null)
				baseComplexType.CollectSchemaComponent (h ,schema);

			// It must exist, but consider validation error case.
			if (BaseSchemaTypeName == XmlSchemaComplexType.AnyTypeName)
				baseComplexType = XmlSchemaComplexType.AnyType;

			// On error case, it simply rejects any contents
			if (baseComplexType == null) {
				validatableParticle = XmlSchemaParticle.Empty;
				resolvedContentType = XmlSchemaContentType.Empty;
				return;
			}

			// 3.4.2 complex content {content type}
			// FIXME: this part is looking different than the spec. sections.
			if (cce.Particle == null || cce.Particle == XmlSchemaParticle.Empty) {
				// - 2.1
				if (baseComplexType == null) {
					// Basically it is an error. Considering ValidationEventHandler.
					validatableParticle = XmlSchemaParticle.Empty;
					resolvedContentType = XmlSchemaContentType.Empty;
				} else {
					validatableParticle = baseComplexType.ValidatableParticle;
					resolvedContentType = baseComplexType.resolvedContentType;
					// Bug #501814
					if (resolvedContentType == XmlSchemaContentType.Empty)
						resolvedContentType = GetComplexContentType (contentModel);
				}
			} else if (baseComplexType.validatableParticle == XmlSchemaParticle.Empty
				|| baseComplexType == XmlSchemaComplexType.AnyType) {
				// - 2.2
				validatableParticle = cce.Particle;
				resolvedContentType = GetComplexContentType (contentModel);
			} else {
				// - 2.3 : create a new sequences that merges both contents.
				XmlSchemaSequence seq = new XmlSchemaSequence ();
				this.CopyInfo (seq);
				seq.Items.Add (baseComplexType.validatableParticle);
				seq.Items.Add (cce.Particle);
				seq.Compile (h, schema);
				seq.Validate (h, schema);
				validatableParticle = seq;
				resolvedContentType = GetComplexContentType (contentModel);
			}
			if (validatableParticle == null)
				validatableParticle = XmlSchemaParticle.Empty;
		}

		private void CollectContentTypeFromComplexRestriction ()
		{
			XmlSchemaComplexContentRestriction ccr = (XmlSchemaComplexContentRestriction) ContentModel.Content;
			// 3.4.2 complex content schema component {content type}
			// - 1.1.1
			bool isEmptyParticle = false;
			if (ccr.Particle == null) 
				isEmptyParticle = true;
			else {
				XmlSchemaGroupBase gb = ccr.Particle as XmlSchemaGroupBase;
				if (gb != null) {
					// - 1.1.2
					if (!(gb is XmlSchemaChoice) && gb.Items.Count == 0)
						isEmptyParticle = true;
					// - 1.1.3
					else if (gb is XmlSchemaChoice && gb.Items.Count == 0 && gb.ValidatedMinOccurs == 0)
						isEmptyParticle = true;
				}
			}
			if (isEmptyParticle) {
				resolvedContentType = XmlSchemaContentType.Empty;
				validatableParticle = XmlSchemaParticle.Empty;
			} else {
				// - 1.2.1
				resolvedContentType = GetComplexContentType (contentModel);
				// - 1.2.2
				validatableParticle = ccr.Particle;
			}
		}

		// 3.4.2 Complex Content Schema Component {content type} 1.2.1
		private XmlSchemaContentType GetComplexContentType (XmlSchemaContentModel content)
		{
			if (this.IsMixed || ((XmlSchemaComplexContent) content).IsMixed)
				return XmlSchemaContentType.Mixed;
			else
				return XmlSchemaContentType.ElementOnly;
		}
		#endregion

		//
		// We have to validate:
		//
		//	- 3.4.3 Complex Type Definition Representation OK
		//	- 3.4.6 Type Definition Properties Correct
		//	- 3.4.6 Derivation Valid (Extension)
		//	- 3.4.6 Derivation Valid (Restriction, Complex)
		//
		// There are many schema errata:
		// http://www.w3.org/2001/05/xmlschema-errata#Errata1
		//
		// E1-43 Derivation Valid (Restriction, Complex) 5.
		// E1-21 Derivation Valid (Restriction, Complex) 4.3.
		// E1-17 Type Derivation OK (Complex) 2.1.
		//
		// And E1-38, E1-37, E1-30, E1-27
		//
		internal override int Validate (ValidationEventHandler h, XmlSchema schema)
		{
			if (IsValidated (schema.ValidationId))
				return errorCount;
			// FIXME: omitting it causes StackOverflowException
			// among the compilation of element and types, but
			// it may result in insufficient results.
			ValidationId = schema.ValidationId;

			CollectSchemaComponent (h, schema);

			ValidateBaseXmlSchemaType (h, schema);
			
			ValidateParticle (h, schema);
			
			FillContentTypeParticle (h, schema);

			// 3.4.6: Properties Correct
			// Term. 1 => 3.4.1 already done by CollectSchemaComponent()
			//	      except for {attribute uses} and {attribute wildcard}
			// Term. 2, 3 and 4 goes to ValidateContentModel().
			// Term. 5 follows in this method.
			//
			if (ContentModel != null)
				ValidateContentModel (h, schema);
			else
				ValidateImmediateAttributes (h, schema);

			// Additional support for 3.8.6 All Group Limited
			if (ContentTypeParticle != null) {
				XmlSchemaAll termAll = contentTypeParticle.GetOptimizedParticle (true) as XmlSchemaAll;
				if (termAll != null && (termAll.ValidatedMaxOccurs != 1 || contentTypeParticle.ValidatedMaxOccurs != 1)) // here contentTypeParticle is used to check occurence.
					error (h, "Particle whose term is -all- and consists of complex type content particle must have maxOccurs = 1.");
			}

#if NET_2_0
			if (schema.Schemas.CompilationSettings != null &&
				schema.Schemas.CompilationSettings.EnableUpaCheck)
#endif
			// This check is optional only after 2.0
			contentTypeParticle.ValidateUniqueParticleAttribution (new XmlSchemaObjectTable (),
				new ArrayList (), h, schema);
			contentTypeParticle.ValidateUniqueTypeAttribution (
				new XmlSchemaObjectTable (), h, schema);

			// 3.4.6 Properties Correct :: 5 (Two distinct ID attributes)
			XmlSchemaAttribute idAttr = null;
			foreach (DictionaryEntry entry in attributeUses) {
				XmlSchemaAttribute attr = (XmlSchemaAttribute) entry.Value;
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

		private void ValidateImmediateAttributes (ValidationEventHandler h, XmlSchema schema)
		{
			// {attribute uses}
			// also checks 3.4.6 Properties Correct :: 4 (Distinct attributes)
			attributeUses = new XmlSchemaObjectTable ();
			XmlSchemaUtil.ValidateAttributesResolved (attributeUses,
				h, schema, attributes, anyAttribute, ref attributeWildcard, null, false);
		}
		
		private void ValidateBaseXmlSchemaType (ValidationEventHandler h, XmlSchema schema)
		{
			if (ContentModel != null && BaseXmlSchemaTypeInternal != null)
				errorCount += BaseXmlSchemaTypeInternal.Validate (h, schema);
		}

		private void ValidateParticle (ValidationEventHandler h, XmlSchema schema)
		{	
			if (ContentModel == null && Particle != null) {
				errorCount += particle.Validate (h, schema);
				XmlSchemaGroupRef pgrp = Particle as XmlSchemaGroupRef;
				if (pgrp != null) {
					if (pgrp.TargetGroup != null)
						errorCount += pgrp.TargetGroup.Validate (h,schema);
					// otherwise, it might be missing sub components.
					else if (!schema.IsNamespaceAbsent (pgrp.RefName.Namespace))
						error (h, "Referenced group " + pgrp.RefName + " was not found in the corresponding schema.");
				}
			}
		}

		private void ValidateContentModel (ValidationEventHandler h, XmlSchema schema)
		{
			errorCount += contentModel.Validate (h, schema);
			
			XmlSchemaType baseType = BaseXmlSchemaTypeInternal;

			// Here we check 3.4.6 Properties Correct :: 2. and 3.
			XmlSchemaComplexContentExtension cce = contentModel.Content as XmlSchemaComplexContentExtension;
			XmlSchemaComplexContentRestriction ccr = contentModel.Content as XmlSchemaComplexContentRestriction;
			XmlSchemaSimpleContentExtension sce = contentModel.Content as XmlSchemaSimpleContentExtension;
			XmlSchemaSimpleContentRestriction scr = contentModel.Content as XmlSchemaSimpleContentRestriction;

			XmlSchemaAnyAttribute localAnyAttribute = null;
			XmlSchemaAnyAttribute baseAnyAttribute = null;

			// 3.4.6 Properties Correct :: 3. Circular definition prohibited.
			if (ValidateRecursionCheck ())
				error (h, "Circular definition of schema types was found.");
			if (baseType != null) {
				// Fill "Datatype" property.
				this.DatatypeInternal = baseType.Datatype;
			} else if (BaseSchemaTypeName == XmlSchemaComplexType.AnyTypeName)
				DatatypeInternal = XmlSchemaSimpleType.AnySimpleType;
			else if (XmlSchemaUtil.IsBuiltInDatatypeName (BaseSchemaTypeName)) {
				DatatypeInternal = XmlSchemaDatatype.FromName (BaseSchemaTypeName);
			}

			XmlSchemaComplexType baseComplexType = baseType as XmlSchemaComplexType;
			XmlSchemaSimpleType baseSimpleType = baseType as XmlSchemaSimpleType;

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
				if (BaseSchemaTypeName == XmlSchemaComplexType.AnyTypeName)
					baseComplexType = XmlSchemaComplexType.AnyType;
				else if (XmlSchemaUtil.IsBuiltInDatatypeName (BaseSchemaTypeName))
					error (h, "Referenced base schema type is XML Schema datatype.");
				else if (baseComplexType == null && !schema.IsNamespaceAbsent (BaseSchemaTypeName.Namespace))
					error (h, "Referenced base schema type " + BaseSchemaTypeName + " was not complex type or not found in the corresponding schema.");
			}
			// Common to simpleContent 
			else {
				// ContentType of {content type}
				resolvedContentType = XmlSchemaContentType.TextOnly;

				// 3.4.3 Complex Type Definition Representation OK :: 1.
				// base
				if (BaseSchemaTypeName == XmlSchemaComplexType.AnyTypeName)
					baseComplexType = XmlSchemaComplexType.AnyType;

				if (baseComplexType != null && baseComplexType.ContentType != XmlSchemaContentType.TextOnly) {
					error (h, "Base schema complex type of a simple content must be simple content type. Base type is " + BaseSchemaTypeName);
				} else if (sce == null && (baseSimpleType != null && BaseSchemaTypeName.Namespace != XmlSchema.Namespace)) {
					error (h, "If a simple content is not an extension, base schema type must be complex type. Base type is " + BaseSchemaTypeName);
				} else if (XmlSchemaUtil.IsBuiltInDatatypeName (BaseSchemaTypeName)) {
					// do nothing for particle.
				}
				// otherwise, it might be missing sub components.
				else if (baseType == null && !schema.IsNamespaceAbsent (BaseSchemaTypeName.Namespace))
					error (h, "Referenced base schema type " + BaseSchemaTypeName + " was not found in the corresponding schema.");

				// 3.4.3 Complex Type Definition Representation OK :: 2.
				// Note that baseSimpleType is also allowed as to Errata E1-27 (http://www.w3.org/2001/05/xmlschema-errata)
				if (baseComplexType != null) {
					if (baseComplexType.ContentType == XmlSchemaContentType.TextOnly) {
						// 2.1.1
					// Here "baseComplexType.Particle != null" is required for error-ignorant case
					} else if (scr != null && baseComplexType.ContentType == XmlSchemaContentType.Mixed && baseComplexType.Particle != null && baseComplexType.Particle.ValidateIsEmptiable () && scr.BaseType != null) {
						// 2.1.2 && 2.2: OK
					}
					else
						error (h, "Base complex type of a simple content restriction must be text only.");
				} else {
					if (sce != null && baseComplexType == null) {
						// 2.1.3 : OK
					}
					else
						error (h, "Not allowed base type of a simple content restriction.");
				}
			}

			// complexType/complexContent/extension
			if (cce != null) {
				// I don't think 3.4.6 Derivation Valid (Extension) :: 1.2
				// is constraining anything here, since 3.4.2 {attribute uses}
				// defines as to include base type's attribute uses.
				localAnyAttribute = cce.AnyAttribute;
				if (baseComplexType != null) {
					foreach (DictionaryEntry entry in baseComplexType.AttributeUses) {
						XmlSchemaAttribute attr = (XmlSchemaAttribute) entry.Value;
						XmlSchemaUtil.AddToTable (attributeUses, attr, attr.QualifiedName, h);
					}
					baseAnyAttribute = baseComplexType.AttributeWildcard;
				}
				// attributes
				errorCount += XmlSchemaUtil.ValidateAttributesResolved (
					this.attributeUses, h, schema, cce.Attributes, 
					cce.AnyAttribute , ref attributeWildcard, null, true);

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

				// attributes
				localAnyAttribute = ccr.AnyAttribute;
				this.attributeWildcard = localAnyAttribute;
				if (baseComplexType != null)
					baseAnyAttribute = baseComplexType.AttributeWildcard;
				if (baseAnyAttribute != null && localAnyAttribute != null)
					// 1.3 attribute wildcard subset. (=> 3.10.6)
					localAnyAttribute.ValidateWildcardSubset (baseAnyAttribute, h, schema);

				// 3.4.2 Complex Type Definition with complex content Schema Component
				// and its {attribute uses} and {attribute wildcard} are done here (descendantly)
				errorCount += XmlSchemaUtil.ValidateAttributesResolved (
					this.attributeUses, h, schema, ccr.Attributes, 
					ccr.AnyAttribute, ref attributeWildcard, null, false);
				foreach (DictionaryEntry entry in baseComplexType.AttributeUses) {
					XmlSchemaAttribute attr = (XmlSchemaAttribute) entry.Value;
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
					sce.AnyAttribute, ref attributeWildcard, null, true);

				// Attributes
				// I don't think 3.4.6 Derivation Valid (Extension) :: 1.2
				// is constraining anything here, since 3.4.2 {attribute uses}
				// defines as to include base type's attribute uses.
				localAnyAttribute = sce.AnyAttribute;

				if (baseComplexType != null) {
					baseAnyAttribute = baseComplexType.AttributeWildcard;

					foreach (DictionaryEntry entry in baseComplexType.AttributeUses) {
						XmlSchemaAttribute attr = (XmlSchemaAttribute) entry.Value;
#if BUGGY_MS_COMPLIANT
						if (attr.Use != XmlSchemaUse.Prohibited)
							XmlSchemaUtil.AddToTable (attributeUses, attr, attr.QualifiedName, h);
#endif
						XmlSchemaUtil.AddToTable (attributeUses, attr, attr.QualifiedName, h);
					}
				}
				if (baseAnyAttribute != null && localAnyAttribute != null)
					// 1.3 attribute wildcard subset. (=> 3.10.6)
					localAnyAttribute.ValidateWildcardSubset (baseAnyAttribute, h, schema);
			}
			// complexType/simpleContent/restriction
			if (scr != null) {
				// Attributes
				baseAnyAttribute = baseComplexType != null ? baseComplexType.AttributeWildcard : null;

				localAnyAttribute = scr.AnyAttribute;
				if (localAnyAttribute != null && baseAnyAttribute != null)
					// 1.3 attribute wildcard subset. (=> 3.10.6)
					localAnyAttribute.ValidateWildcardSubset (baseAnyAttribute, h, schema);
				// 3.4.6 :: 5.1. Beware that There is an errata for 5.1!!
				// http://www.w3.org/2001/05/xmlschema-errata#Errata1

				// 3.4.2 Complex Type Definition with simple content Schema Component
				// and its {attribute uses} and {attribute wildcard} are done here (descendantly)
				errorCount += XmlSchemaUtil.ValidateAttributesResolved (
					this.attributeUses, h, schema, scr.Attributes, 
					scr.AnyAttribute, ref attributeWildcard, null, false);
			}

			// Common process of AttributeWildcard.
			if (localAnyAttribute != null) {
				this.attributeWildcard = localAnyAttribute;
			}
			else
				this.attributeWildcard = baseAnyAttribute;
		}

		// 3.4.6 Type Derivation OK (Complex)
		internal void ValidateTypeDerivationOK (object b, ValidationEventHandler h, XmlSchema schema)
		{
			// AnyType derives from AnyType itself.
			if (this == XmlSchemaComplexType.AnyType && BaseXmlSchemaType == this)
				return;

			XmlSchemaType bst = b as XmlSchemaType;
			if (b == this)	// 1 and 2.1
				return;
			if (bst != null && (resolvedDerivedBy & bst.FinalResolved) != 0) // 1.
				error (h, "Derivation type " + resolvedDerivedBy + " is prohibited by the base type.");
			// FIXME: here BaseSchemaType should be 
			// BaseXmlSchemaType, however for some case it 
			// seems not working.
			if (BaseSchemaType == b) // 2.2
				return;
			if (BaseSchemaType == null || BaseXmlSchemaType == XmlSchemaComplexType.AnyType) { // 2.3.1
				error (h, "Derived type's base schema type is anyType.");
				return;
			}
			// 2.3.2.1
			XmlSchemaComplexType dbct = BaseXmlSchemaType as XmlSchemaComplexType;
			if (dbct != null) {
				dbct.ValidateTypeDerivationOK (b, h, schema);
				return;
			}
			// 2.3.2.2
			XmlSchemaSimpleType dbst = BaseXmlSchemaType as XmlSchemaSimpleType;
			if (dbst != null) {
				dbst.ValidateTypeDerivationOK (b, h, schema, true);
				return;
			}
		}

		// 3.4.6 Derivation Valid (Extension) - Term. 1 (Complex Type)
		internal void ValidateComplexBaseDerivationValidExtension (XmlSchemaComplexType baseComplexType,
			ValidationEventHandler h, XmlSchema schema)
		{
			// 1.1
			if ((baseComplexType.FinalResolved & XmlSchemaDerivationMethod.Extension) != 0)
				error (h, "Derivation by extension is prohibited.");
			// 1.2
			foreach (DictionaryEntry entry in baseComplexType.AttributeUses) {
				XmlSchemaAttribute ba = (XmlSchemaAttribute) entry.Value;
				XmlSchemaAttribute da = AttributeUses [ba.QualifiedName] as XmlSchemaAttribute;
				if (da == null)
					error (h, "Invalid complex type derivation by extension was found. Missing attribute was found: " + ba.QualifiedName + " .");
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
				if (this.ContentType != baseComplexType.ContentType)
//				if (this.GetContentType (false) != baseComplexType.GetContentType (false))
					error (h, "Base complex type has different content type " + baseComplexType.ContentType + ".");
				// 1.4.2.2.2.2 => 3.9.6 Particle Valid (Extension)
				else if (this.contentTypeParticle == null ||
					!this.contentTypeParticle.ParticleEquals (baseComplexType.ContentTypeParticle)) {
					XmlSchemaSequence seq = contentTypeParticle as XmlSchemaSequence;
					if (contentTypeParticle != XmlSchemaParticle.Empty && (seq == null || contentTypeParticle.ValidatedMinOccurs != 1 || contentTypeParticle.ValidatedMaxOccurs != 1))
						error (h, "Invalid complex content extension was found.");
					else {
						// Identical sequence item should be checked, but
						// I think it is naturally achieved as coded above.
					}
				}
			}
		}

		// 3.4.6 Derivation Valid (Extension) - Term. 2 (Simple Type)
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
			foreach (DictionaryEntry entry in this.AttributeUses) {
				XmlSchemaAttribute attr = (XmlSchemaAttribute) entry.Value;
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
			if (this.AttributeWildcard != null && baseType != XmlSchemaComplexType.AnyType) {
				if (baseType.AttributeWildcard == null)
					error (h, "Invalid attribute derivation by restriction because of attribute wildcard.");
				else
					AttributeWildcard.ValidateWildcardSubset (baseType.AttributeWildcard, h, schema);
			}

			// 5.
			if (this == AnyType)
				return;
			if (contentTypeParticle == XmlSchemaParticle.Empty) {
				// 5.1
				if (ContentType != XmlSchemaContentType.Empty) {
					// TODO: 5.1.1
//					XmlSchemaSimpleType baseST = baseType as XmlSchemaSimpleType;
					// 5.1.2
					if (baseType.ContentType == XmlSchemaContentType.Mixed && !baseType.ContentTypeParticle.ValidateIsEmptiable ())
						error (h, "Invalid content type derivation.");

				} else {
					// 5.2
					if (baseType.ContentTypeParticle != XmlSchemaParticle.Empty &&
						!baseType.ContentTypeParticle.ValidateIsEmptiable ())
						error (h, "Invalid content type derivation.");
				}
			} else {
				// 5.3 => 3.9.6 Particle Valid (Restriction)
				if (baseType.ContentTypeParticle != null) {
					// 3.9.6 - 1 : same particle.
					// 3.9.6 - 2 is covered by using ActualParticle.
					if (!contentTypeParticle.ParticleEquals (baseType.ContentTypeParticle))
						contentTypeParticle.ValidateDerivationByRestriction (baseType.ContentTypeParticle, h, schema, true);
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
						error (h,"some invalid values for block attribute were found",innerex);
				}
				else if(reader.Name == "final")
				{
					ctype.Final = XmlSchemaUtil.ReadDerivationAttribute(reader,out innerex, "final",
						XmlSchemaUtil.FinalAllowed);
					if(innerex != null)
						error (h,"some invalid values for final attribute were found",innerex);
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
