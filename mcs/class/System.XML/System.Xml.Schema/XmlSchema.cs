//
// System.Xml.Schema.XmlSchema.cs
//
// Author:
//	Dwivedi, Ajay kumar  Adwiv@Yahoo.com
//	Atsushi Enomoto  ginga@kit.hi-ho.ne.jp
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
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.ComponentModel;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchema.
	/// </summary>
	[XmlRoot ("schema",Namespace=XmlSchema.Namespace)]
	public class XmlSchema : XmlSchemaObject
	{
		//public constants
		public const string Namespace = "http://www.w3.org/2001/XMLSchema";
		public const string InstanceNamespace = "http://www.w3.org/2001/XMLSchema-instance";
		internal const string XdtNamespace = "http://www.w3.org/2003/11/xpath-datatypes";

		//private fields
		private XmlSchemaForm attributeFormDefault ;
		private XmlSchemaObjectTable attributeGroups ;
		private XmlSchemaObjectTable attributes ;
		private XmlSchemaDerivationMethod blockDefault ;
		private XmlSchemaForm elementFormDefault ;
		private XmlSchemaObjectTable elements ;
		private XmlSchemaDerivationMethod finalDefault ;
		private XmlSchemaObjectTable groups ;
		private string id ;
		private XmlSchemaObjectCollection includes ;
		private XmlSchemaObjectCollection items ;
		private XmlSchemaObjectTable notations ;
		private XmlSchemaObjectTable schemaTypes ;
		private XmlSchemaObjectTable named_identities;
		private Hashtable ids;
		private string targetNamespace ;
		private XmlAttribute[] unhandledAttributes ;
		private string version;

		// other post schema compilation infoset
		private XmlSchemaSet schemas;

		private XmlNameTable nameTable;

		internal bool missedSubComponents;

		// Only compilation-time use
		private XmlSchemaObjectCollection compilationItems;

		// Compiler specific things
		const string xmlname = "schema";

		public XmlSchema ()
		{
			attributeFormDefault= XmlSchemaForm.None;
			blockDefault = XmlSchemaDerivationMethod.None;
			elementFormDefault = XmlSchemaForm.None;
			finalDefault = XmlSchemaDerivationMethod.None;
			includes = new XmlSchemaObjectCollection();
			isCompiled = false;
			items = new XmlSchemaObjectCollection();
			attributeGroups = new XmlSchemaObjectTable();
			attributes = new XmlSchemaObjectTable();
			elements = new XmlSchemaObjectTable();
			groups = new XmlSchemaObjectTable();
			notations = new XmlSchemaObjectTable();
			schemaTypes = new XmlSchemaObjectTable();
			named_identities = new XmlSchemaObjectTable ();
			ids = new Hashtable ();
			compilationItems = new XmlSchemaObjectCollection ();
		}

		#region Properties

		[DefaultValue (XmlSchemaForm.None)]
		[System.Xml.Serialization.XmlAttribute ("attributeFormDefault")]
		public XmlSchemaForm AttributeFormDefault
		{
			get{ return attributeFormDefault; }
			set{ this.attributeFormDefault = value;}
		}

		[DefaultValue (XmlSchemaDerivationMethod.None)]
		[System.Xml.Serialization.XmlAttribute ("blockDefault")]
		public XmlSchemaDerivationMethod BlockDefault
		{
			get{ return blockDefault;}
			set{ blockDefault = value;}
		}

		[DefaultValue (XmlSchemaDerivationMethod.None)]
		[System.Xml.Serialization.XmlAttribute ("finalDefault")]
		public XmlSchemaDerivationMethod FinalDefault
		{
			get{ return finalDefault; }
			set{ finalDefault = value; }
		}

		[DefaultValue (XmlSchemaForm.None)]
		[System.Xml.Serialization.XmlAttribute ("elementFormDefault")]
		public XmlSchemaForm ElementFormDefault
		{
			get{ return elementFormDefault; }
			set{ elementFormDefault = value; }
		}

		[System.Xml.Serialization.XmlAttribute ("targetNamespace", DataType="anyURI")]
		public string TargetNamespace
		{
			get{ return targetNamespace; }
			set{ targetNamespace = value; }
		}

		[System.Xml.Serialization.XmlAttribute ("version", DataType="token")]
		public string Version
		{
			get{ return version; }
			set{ version = value; }
		}

		[XmlElement ("include",typeof(XmlSchemaInclude))]
		[XmlElement ("import",typeof(XmlSchemaImport))]
		[XmlElement ("redefine",typeof(XmlSchemaRedefine))]
		public XmlSchemaObjectCollection Includes
		{
			get{ return includes;}
		}

		[XmlElement ("simpleType", typeof (XmlSchemaSimpleType))]
		[XmlElement ("complexType", typeof (XmlSchemaComplexType))]
		[XmlElement ("group", typeof (XmlSchemaGroup))]
			//Only Schema's attributeGroup has type XmlSchemaAttributeGroup.
			//Others (complextype, restrictions etc) must have XmlSchemaAttributeGroupRef
		[XmlElement ("attributeGroup", typeof (XmlSchemaAttributeGroup))]
		[XmlElement ("element", typeof (XmlSchemaElement))]
		[XmlElement ("attribute", typeof (XmlSchemaAttribute))]
		[XmlElement ("notation", typeof (XmlSchemaNotation))]
		[XmlElement ("annotation", typeof (XmlSchemaAnnotation))]
		public XmlSchemaObjectCollection Items
		{
			get{ return items; }
		}

		[XmlIgnore]
		public bool IsCompiled
		{
			get{ return this.CompilationId != Guid.Empty; }
		}

		[XmlIgnore]
		public XmlSchemaObjectTable Attributes
		{
			get{ return attributes; }
		}

		[XmlIgnore]
		public XmlSchemaObjectTable AttributeGroups
		{
			get{ return attributeGroups; }
		}

		[XmlIgnore]
		public XmlSchemaObjectTable SchemaTypes
		{
			get{ return schemaTypes; }
		}

		[XmlIgnore]
		public XmlSchemaObjectTable Elements
		{
			get{ return elements; }
		}

		[System.Xml.Serialization.XmlAttribute ("id", DataType="ID")]
		public string Id
		{
			get{ return id; }
			set{ id = value; }
		}

		[XmlAnyAttribute]
		public XmlAttribute [] UnhandledAttributes
		{
			get {
				if (unhandledAttributeList != null) {
					unhandledAttributes = (XmlAttribute []) unhandledAttributeList.ToArray (typeof (XmlAttribute));
					unhandledAttributeList = null;
				}
				return unhandledAttributes;
			}
			set {
				unhandledAttributes = value;
				unhandledAttributeList = null;
			}
		}

		[XmlIgnore]
		public XmlSchemaObjectTable Groups
		{
			get{ return groups; }
		}

		[XmlIgnore]
		public XmlSchemaObjectTable Notations
		{
			get{ return notations; }
		}

		internal XmlSchemaObjectTable NamedIdentities {
			get { return named_identities; }
		}

		internal XmlSchemaSet Schemas
		{
			get { return schemas; }
		}

		internal Hashtable IDCollection {
			get { return ids; }
		}
		#endregion

		#region Compile

		// Methods
		/// <summary>
		/// This compile method does two things:
		/// 1. It compiles and fills the PSVI dataset
		/// 2. Validates the schema by calling Validate method.
		/// Every XmlSchemaObject has a Compile Method which gets called.
		/// </summary>
		/// <remarks>
		///             1. blockDefault must be one of #all | List of (extension | restriction | substitution)
		///             2. finalDefault must be one of (#all | List of (extension | restriction| union| list))
		///             3. id must be of type ID
		///             4. targetNamespace should be any uri
		///             5. version should be a normalizedString
		/// </remarks>
#if NET_2_0
		[Obsolete ("Use XmlSchemaSet.Compile() instead.")]
#endif
		public void Compile (ValidationEventHandler handler)
		{
			Compile (handler, new XmlUrlResolver ());
		}

#if NET_2_0
		[Obsolete ("Use XmlSchemaSet.Compile() instead.")]
#endif
#if NET_1_1
		public void Compile (ValidationEventHandler handler, XmlResolver resolver)
#else
		internal void Compile (ValidationEventHandler handler, XmlResolver resolver)
#endif
		{
			XmlSchemaSet xss = new XmlSchemaSet ();
			if (handler != null)
				xss.ValidationEventHandler += handler;
			xss.XmlResolver = resolver;
			xss.Add (this);
			xss.Compile ();
		}

		// It is used by XmlSchemaCollection.Add() and XmlSchemaSet.Remove().
		internal void CompileSubset (ValidationEventHandler handler, XmlSchemaSet col, XmlResolver resolver)
		{
			var handledUris = new List<CompiledSchemaMemo> ();
			CompileSubset (handler, col, resolver, handledUris);
		}

		// It is used by XmlSchemaSet.Compile().
		internal void CompileSubset (ValidationEventHandler handler, XmlSchemaSet col, XmlResolver resolver, List<CompiledSchemaMemo> handledUris)
		{
			if (SourceUri != null && SourceUri.Length > 0) {
				// if it has line info and are the same as one of existing info, then skip it.
				if (Contains (handledUris))
					return;
				handledUris.Add (new CompiledSchemaMemo () {  SourceUri = this.SourceUri, LineNumber = this.LineNumber, LinePosition = this.LinePosition});
			}
			DoCompile (handler, handledUris, col, resolver);
		}

		bool Contains (List<CompiledSchemaMemo> handledUris)
		{
			foreach (var i in handledUris)
				if (i.SourceUri.Equals (SourceUri) && i.LineNumber != 0 && i.LineNumber == LineNumber && i.LinePosition == LinePosition)
					return true;
			return false;
		}

		void SetParent ()
		{
			for (int i = 0; i < Items.Count; i++)
				Items [i].SetParent (this);
			for (int i = 0; i < Includes.Count; i++)
				Includes [i].SetParent (this);
		}

		void DoCompile (ValidationEventHandler handler, List<CompiledSchemaMemo> handledUris, XmlSchemaSet col, XmlResolver resolver)
		{
			SetParent ();
			CompilationId = col.CompilationId;
			schemas = col;
			if (!schemas.Contains (this)) // e.g. xs:import
				schemas.Add (this);

			attributeGroups.Clear ();
			attributes.Clear ();
			elements.Clear ();
			groups.Clear ();
			notations.Clear ();
			schemaTypes.Clear ();
			named_identities.Clear ();
			ids.Clear ();
			compilationItems.Clear ();

			//1. Union and List are not allowed in block default
			if (BlockDefault != XmlSchemaDerivationMethod.All) {
				if((BlockDefault & XmlSchemaDerivationMethod.List)!=0 )
					error(handler, "list is not allowed in blockDefault attribute");
				if((BlockDefault & XmlSchemaDerivationMethod.Union)!=0 )
					error(handler, "union is not allowed in blockDefault attribute");
			}

			//2. Substitution is not allowed in finaldefault.
			if (FinalDefault != XmlSchemaDerivationMethod.All) {
				if((FinalDefault & XmlSchemaDerivationMethod.Substitution)!=0 )
					error(handler, "substitution is not allowed in finalDefault attribute");
			}

			//3. id must be of type ID
			XmlSchemaUtil.CompileID(Id, this, IDCollection, handler);

			//4. targetNamespace should be of type anyURI or absent
			if (TargetNamespace != null) {
				if (TargetNamespace.Length == 0)
					error (handler, "The targetNamespace attribute cannot have have empty string as its value.");

				if(!XmlSchemaUtil.CheckAnyUri (TargetNamespace))
					error(handler, TargetNamespace+" is not a valid value for targetNamespace attribute of schema");
			}

			//5. version should be of type normalizedString
			if (!XmlSchemaUtil.CheckNormalizedString(Version))
				error(handler, Version + "is not a valid value for version attribute of schema");

			// Compile the content of this schema

			for (int i = 0; i < Items.Count; i++) {
				compilationItems.Add (Items [i]);
			}

			// First, we run into inclusion schemas to collect 
			// compilation target items into compiledItems.
			for (int i = 0; i < Includes.Count; i++)
				ProcessExternal (handler, handledUris, resolver, Includes [i] as XmlSchemaExternal, col);

			// Compilation phase.
			// At least each Compile() must give unique (qualified) name for each component.
			// It also checks self-resolvable properties correctness.
			// Post compilation schema information contribution is not done here.
			// It should be done by Validate().
			for (int i = 0; i < compilationItems.Count; i++) {
				XmlSchemaObject obj = compilationItems [i];
				if(obj is XmlSchemaAnnotation) {
					int numerr = ((XmlSchemaAnnotation)obj).Compile (handler, this);
					errorCount += numerr;
				} else if (obj is XmlSchemaAttribute) {
					XmlSchemaAttribute attr = (XmlSchemaAttribute) obj;
					int numerr = attr.Compile (handler, this);
					errorCount += numerr;
					if(numerr == 0)
					{
						XmlSchemaUtil.AddToTable (Attributes, attr, attr.QualifiedName, handler);
					}
				} else if (obj is XmlSchemaAttributeGroup) {
					XmlSchemaAttributeGroup attrgrp = (XmlSchemaAttributeGroup) obj;
					int numerr = attrgrp.Compile(handler, this);
					errorCount += numerr;
					if (numerr == 0)
						XmlSchemaUtil.AddToTable (
							AttributeGroups,
							attrgrp,
							attrgrp.QualifiedName,
							handler);
				} else if (obj is XmlSchemaComplexType) {
					XmlSchemaComplexType ctype = (XmlSchemaComplexType) obj;
					int numerr = ctype.Compile (handler, this);
					errorCount += numerr;
					if (numerr == 0)
						XmlSchemaUtil.AddToTable (
							schemaTypes,
							ctype,
							ctype.QualifiedName,
							handler);
				} else if (obj is XmlSchemaSimpleType) {
					XmlSchemaSimpleType stype = (XmlSchemaSimpleType) obj;
					stype.islocal = false; //This simple type is toplevel
					int numerr = stype.Compile (handler, this);
					errorCount += numerr;
					if (numerr == 0)
						XmlSchemaUtil.AddToTable (
							SchemaTypes,
							stype,
							stype.QualifiedName,
							handler);
				} else if (obj is XmlSchemaElement) {
					XmlSchemaElement elem = (XmlSchemaElement) obj;
					elem.parentIsSchema = true;
					int numerr = elem.Compile (handler, this);
					errorCount += numerr;
					if (numerr == 0)
						XmlSchemaUtil.AddToTable (
							Elements,
							elem,
							elem.QualifiedName,
							handler);
				} else if (obj is XmlSchemaGroup) {
					XmlSchemaGroup grp = (XmlSchemaGroup) obj;
					int numerr = grp.Compile (handler, this);
					errorCount += numerr;
					if (numerr == 0)
						XmlSchemaUtil.AddToTable (
							Groups,
							grp,
							grp.QualifiedName,
							handler);
				} else if (obj is XmlSchemaNotation) {
					XmlSchemaNotation ntn = (XmlSchemaNotation) obj;
					int numerr = ntn.Compile (handler, this);
					errorCount += numerr;
					if (numerr == 0)
						XmlSchemaUtil.AddToTable (
							Notations,
							ntn,
							ntn.QualifiedName,
							handler);
				} else {
					ValidationHandler.RaiseValidationEvent (
						handler,
						null,
						String.Format ("Object of Type {0} is not valid in Item Property of Schema", obj.GetType ().Name),
						null,
						this,
						null,
						XmlSeverityType.Error);
				}
			}
		}

		private string GetResolvedUri (XmlResolver resolver, string relativeUri)
		{
			Uri baseUri = null;
			if (this.SourceUri != null && this.SourceUri != String.Empty)
				baseUri = new Uri (this.SourceUri);
			Uri abs = resolver.ResolveUri (baseUri, relativeUri);
#if NET_2_0
			return abs != null ? abs.OriginalString : String.Empty;
#else
 			return abs != null ? abs.ToString () : String.Empty;
#endif
		}

		void ProcessExternal (ValidationEventHandler handler, List<CompiledSchemaMemo> handledUris, XmlResolver resolver, XmlSchemaExternal ext, XmlSchemaSet col)
		{
			if (ext == null) {
				error (handler, String.Format ("Object of Type {0} is not valid in Includes Property of XmlSchema", ext.GetType().Name));
				return;
			}

			// The only case we want to handle where the SchemaLocation is null is if the external is an import.
			XmlSchemaImport import = ext as XmlSchemaImport;
			if (ext.SchemaLocation == null && import == null)
				return;
			
			XmlSchema includedSchema = null;
			if (ext.SchemaLocation != null)
			{
				Stream stream = null;
				string url = null;
				if (resolver != null) {
					url = GetResolvedUri (resolver, ext.SchemaLocation);
					foreach (var i in handledUris)
						if (i.SourceUri.Equals (url))
							// This schema is already handled, so simply skip (otherwise, duplicate definition errrors occur.
							return;
					handledUris.Add (new CompiledSchemaMemo () { SourceUri = url });
					try {
						stream = resolver.GetEntity (new Uri (url), null, typeof (Stream)) as Stream;
					} catch (Exception) {
					// LAMESPEC: This is not good way to handle errors, but since we cannot know what kind of XmlResolver will come, so there are no mean to avoid this ugly catch.
						warn (handler, "Could not resolve schema location URI: " + url);
						stream = null;
					}
				}
		
				// Process redefinition children in advance.
				XmlSchemaRedefine redefine = ext as XmlSchemaRedefine;
				if (redefine != null) {
					for (int j = 0; j < redefine.Items.Count; j++) {
						XmlSchemaObject redefinedObj = redefine.Items [j];
						redefinedObj.isRedefinedComponent = true;
						redefinedObj.isRedefineChild = true;
						if (redefinedObj is XmlSchemaType ||
							redefinedObj is XmlSchemaGroup ||
							redefinedObj is XmlSchemaAttributeGroup)
							compilationItems.Add (redefinedObj);
						else
							error (handler, "Redefinition is only allowed to simpleType, complexType, group and attributeGroup.");
					}
				}

				if (stream == null) {
					// It is missing schema components.
					missedSubComponents = true;
					return;
				} else {
					XmlTextReader xtr = null;
					try {
						xtr = new XmlTextReader (url, stream, nameTable);
						includedSchema = XmlSchema.Read (xtr, handler);
					} finally {
						if (xtr != null)
							xtr.Close ();
					}
					includedSchema.schemas = schemas;
				}
				includedSchema.SetParent ();
				ext.Schema = includedSchema;
			}

			// Set - actual - target namespace for the included schema * before compilation*.
			if (import != null) {
				if (ext.Schema == null && ext.SchemaLocation == null) {
					// if a schema location wasn't specified, check the other schemas we have to see if one of those
					// is a match.
					foreach(XmlSchema schema in col.Schemas())
					{
						if (schema.TargetNamespace == import.Namespace)
						{
							includedSchema = schema;
							includedSchema.schemas = schemas;
							includedSchema.SetParent ();
							ext.Schema = includedSchema;
							break;
						}
					}
					// handle case where target namespace doesn't exist in schema collection - i.e can't find it at all
					if (includedSchema == null)
						return;
				} else if (includedSchema != null) {
					if (TargetNamespace == includedSchema.TargetNamespace) {
						error (handler, "Target namespace must be different from that of included schema.");
						return;
					} else if (includedSchema.TargetNamespace != import.Namespace) {
						error (handler, "Attribute namespace and its importing schema's target namespace must be the same.");
						return;
					}
				}
			} else if (includedSchema != null) {
				if (TargetNamespace == null && 
					includedSchema.TargetNamespace != null) {
					includedSchema.error (handler, String.Format ("On {0} element, targetNamespace is required to include a schema which has its own target namespace", ext.GetType ().Name));
					return;
				}
				else if (TargetNamespace != null && 
					includedSchema.TargetNamespace == null)
					includedSchema.TargetNamespace = TargetNamespace;
			}

			// Do not compile included schema here.
			if (includedSchema != null)
				AddExternalComponentsTo (includedSchema, compilationItems, handler, handledUris, resolver, col);
		}


		void AddExternalComponentsTo (XmlSchema s, XmlSchemaObjectCollection items, ValidationEventHandler handler, List<CompiledSchemaMemo> handledUris, XmlResolver resolver, XmlSchemaSet col)
		{
			foreach (XmlSchemaExternal ext in s.Includes)
				s.ProcessExternal (handler, handledUris, resolver, ext, col);
			foreach (XmlSchemaObject obj in s.compilationItems)
				items.Add (obj);
			// Items might be already resolved (recursive schema imports), or might not be (other cases), so we add items only when appropriate here. (duplicate check is anyways done elsewhere)
			foreach (XmlSchemaObject obj in s.Items)
				if (!items.Contains (obj))
					items.Add (obj);
		}

		internal bool IsNamespaceAbsent (string ns)
		{
			return !schemas.Contains (ns);
		}

		#endregion

		internal XmlSchemaAttribute FindAttribute (XmlQualifiedName name)
		{
			XmlSchemaAttribute a;
			foreach (XmlSchema schema in schemas.Schemas ()) {
				a = schema.Attributes [name] as XmlSchemaAttribute;
				if (a != null)
					return a;
			}
			return null;
		}

		internal XmlSchemaAttributeGroup FindAttributeGroup (XmlQualifiedName name)
		{
			XmlSchemaAttributeGroup a;
			foreach (XmlSchema schema in schemas.Schemas ()) {
				a = schema.AttributeGroups [name] as XmlSchemaAttributeGroup;
				if (a != null)
					return a;
			}
			return null;
		}

		internal XmlSchemaElement FindElement (XmlQualifiedName name)
		{
			XmlSchemaElement a;
			foreach (XmlSchema schema in schemas.Schemas ()) {
				a = schema.Elements [name] as XmlSchemaElement;
				if (a != null)
					return a;
			}
			return null;
		}

		internal XmlSchemaType FindSchemaType (XmlQualifiedName name)
		{
			XmlSchemaType a;
			foreach (XmlSchema schema in schemas.Schemas ()) {
				a = schema.SchemaTypes [name] as XmlSchemaType;
				if (a != null)
					return a;
			}
			return null;
		}

		internal void Validate (ValidationEventHandler handler)
		{
			ValidationId = CompilationId;

			// Validate
			foreach (XmlSchemaAttribute attr in Attributes.Values)
				errorCount += attr.Validate (handler, this);
			foreach (XmlSchemaAttributeGroup attrgrp in AttributeGroups.Values)
				errorCount += attrgrp.Validate (handler, this);
			foreach (XmlSchemaType type in SchemaTypes.Values)
				errorCount += type.Validate (handler, this);
			foreach (XmlSchemaElement elem in Elements.Values)
				errorCount += elem.Validate (handler, this);
			foreach (XmlSchemaGroup grp in Groups.Values)
				errorCount += grp.Validate (handler, this);
			foreach (XmlSchemaNotation ntn in Notations.Values)
				errorCount += ntn.Validate (handler, this);

			if (errorCount == 0)
				isCompiled = true;
			errorCount = 0;
		}

		#region Read

		// We cannot use xml deserialization, since it does not provide line info, qname context, and so on.
		public static XmlSchema Read (TextReader reader, ValidationEventHandler validationEventHandler)
		{
			return Read (new XmlTextReader (reader),validationEventHandler);
		}
		public static XmlSchema Read (Stream stream, ValidationEventHandler validationEventHandler)
		{
			return Read (new XmlTextReader (stream),validationEventHandler);
		}

		public static XmlSchema Read (XmlReader rdr, ValidationEventHandler validationEventHandler)
		{
			XmlSchemaReader reader = new XmlSchemaReader (rdr, validationEventHandler);

			if (reader.ReadState == ReadState.Initial)
				reader.ReadNextElement ();

			int startDepth = reader.Depth;

			do
			{
				switch(reader.NodeType)
				{
				case XmlNodeType.Element:
					if(reader.LocalName == "schema")
					{
						XmlSchema schema = new XmlSchema ();
						schema.nameTable = rdr.NameTable;

						schema.LineNumber = reader.LineNumber;
						schema.LinePosition = reader.LinePosition;
						schema.SourceUri = reader.BaseURI;

						ReadAttributes(schema, reader, validationEventHandler);
						//IsEmptyElement does not behave properly if reader is
						//positioned at an attribute.
						reader.MoveToElement();
						if(!reader.IsEmptyElement)
						{
							ReadContent(schema, reader, validationEventHandler);
						}
						else
							rdr.Skip ();

						return schema;
					}
					else
						//Schema can't be generated. Throw an exception
						error (validationEventHandler, "The root element must be schema", null);
					break;
				default:
					error(validationEventHandler, "This should never happen. XmlSchema.Read 1 ",null);
					break;
				}
			} while(reader.Depth > startDepth && reader.ReadNextElement());

			// This is thrown regardless of ValidationEventHandler existence.
			throw new XmlSchemaException ("The top level schema must have namespace " + XmlSchema.Namespace, null);
		}

		private static void ReadAttributes(XmlSchema schema, XmlSchemaReader reader, ValidationEventHandler h)
		{
			Exception ex;

			reader.MoveToElement();
			while(reader.MoveToNextAttribute())
			{
				switch(reader.Name)
				{
					case "attributeFormDefault" :
						schema.attributeFormDefault = XmlSchemaUtil.ReadFormAttribute(reader,out ex);
						if(ex != null)
							error(h, reader.Value + " is not a valid value for attributeFormDefault.", ex);
						break;
					case "blockDefault" :
						schema.blockDefault = XmlSchemaUtil.ReadDerivationAttribute(reader,out ex, "blockDefault",
							XmlSchemaUtil.ElementBlockAllowed);
						if(ex != null)
							error (h, ex.Message, ex);
						break;
					case "elementFormDefault":
						schema.elementFormDefault = XmlSchemaUtil.ReadFormAttribute(reader, out ex);
						if(ex != null)
							error(h, reader.Value + " is not a valid value for elementFormDefault.", ex);
						break;
					case "finalDefault":
						schema.finalDefault = XmlSchemaUtil.ReadDerivationAttribute(reader, out ex, "finalDefault",
							XmlSchemaUtil.FinalAllowed);
						if(ex != null)
							error (h, ex.Message , ex);
						break;
					case "id":
						schema.id = reader.Value;
						break;
					case "targetNamespace":
						schema.targetNamespace = reader.Value;
						break;
					case "version":
						schema.version = reader.Value;
						break;
					default:
						if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
							error(h, reader.Name + " attribute is not allowed in schema element",null);
						else
						{
							XmlSchemaUtil.ReadUnhandledAttribute(reader,schema);
						}
						break;
				}
			}
		}

		private static void ReadContent(XmlSchema schema, XmlSchemaReader reader, ValidationEventHandler h)
		{
			reader.MoveToElement();
			if(reader.LocalName != "schema" && reader.NamespaceURI != XmlSchema.Namespace && reader.NodeType != XmlNodeType.Element)
				error(h, "UNREACHABLE CODE REACHED: Method: Schema.ReadContent, " + reader.LocalName + ", " + reader.NamespaceURI,null);

			//(include | import | redefine | annotation)*,
			//((simpleType | complexType | group | attributeGroup | element | attribute | notation | annotation)*
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchema.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1)
				{
					if(reader.LocalName == "include")
					{
						XmlSchemaInclude include = XmlSchemaInclude.Read(reader,h);
						if(include != null)
							schema.includes.Add(include);
						continue;
					}
					if(reader.LocalName == "import")
					{
						XmlSchemaImport import = XmlSchemaImport.Read(reader,h);
						if(import != null)
							schema.includes.Add(import);
						continue;
					}
					if(reader.LocalName == "redefine")
					{
						XmlSchemaRedefine redefine = XmlSchemaRedefine.Read(reader,h);
						if(redefine != null)
							schema.includes.Add(redefine);
						continue;
					}
					if(reader.LocalName == "annotation")
					{
						XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
						if(annotation != null)
							schema.items.Add(annotation);
						continue;
					}
				}
				if(level <=2)
				{
					level = 2;
					if(reader.LocalName == "simpleType")
					{
						XmlSchemaSimpleType stype = XmlSchemaSimpleType.Read(reader,h);
						if(stype != null)
							schema.items.Add(stype);
						continue;
					}
					if(reader.LocalName == "complexType")
					{
						XmlSchemaComplexType ctype = XmlSchemaComplexType.Read(reader,h);
						if(ctype != null)
							schema.items.Add(ctype);
						continue;
					}
					if(reader.LocalName == "group")
					{
						XmlSchemaGroup group = XmlSchemaGroup.Read(reader,h);
						if(group != null)
							schema.items.Add(group);
						continue;
					}
					if(reader.LocalName == "attributeGroup")
					{
						XmlSchemaAttributeGroup attributeGroup = XmlSchemaAttributeGroup.Read(reader,h);
						if(attributeGroup != null)
							schema.items.Add(attributeGroup);
						continue;
					}
					if(reader.LocalName == "element")
					{
						XmlSchemaElement element = XmlSchemaElement.Read(reader,h);
						if(element != null)
							schema.items.Add(element);
						continue;
					}
					if(reader.LocalName == "attribute")
					{
						XmlSchemaAttribute attr = XmlSchemaAttribute.Read(reader,h);
						if(attr != null)
							schema.items.Add(attr);
						continue;
					}
					if(reader.LocalName == "notation")
					{
						XmlSchemaNotation notation = XmlSchemaNotation.Read(reader,h);
						if(notation != null)
							schema.items.Add(notation);
						continue;
					}
					if(reader.LocalName == "annotation")
					{
						XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
						if(annotation != null)
							schema.items.Add(annotation);
						continue;
					}
				}
				reader.RaiseInvalidElementError();
			}
		}
		#endregion

		#region write

		public void Write(System.IO.Stream stream)
		{
			Write(stream,null);
		}
		public void Write(System.IO.TextWriter writer)
		{
			Write(writer,null);
		}
		public void Write(System.Xml.XmlWriter writer)
		{
			Write(writer,null);
		}
		public void Write(System.IO.Stream stream, System.Xml.XmlNamespaceManager namespaceManager)
		{
			Write(new XmlTextWriter(stream,null),namespaceManager);
		}
		public void Write(System.IO.TextWriter writer, System.Xml.XmlNamespaceManager namespaceManager)
		{
			XmlTextWriter xwriter = new XmlTextWriter(writer);
			xwriter.Formatting = Formatting.Indented;
			Write(xwriter,namespaceManager);
		}

		public void Write (System.Xml.XmlWriter writer, System.Xml.XmlNamespaceManager namespaceManager)
		{
			XmlSerializerNamespaces nss = new XmlSerializerNamespaces ();

			if (namespaceManager != null) {
				foreach (string name in namespaceManager) {
					//xml and xmlns namespaces are added by default in namespaceManager.
					//So we should ignore them
					if (name !="xml" && name != "xmlns")
						nss.Add (name, namespaceManager.LookupNamespace (name));
				}
			}

			if (Namespaces != null && Namespaces.Count > 0) {
				XmlQualifiedName [] qnames = Namespaces.ToArray ();
				foreach (XmlQualifiedName qn in qnames)
					nss.Add (qn.Name, qn.Namespace);
				string p = String.Empty;
				bool loop = true;
				for (int idx = 1; loop; idx++) {
					loop = false;
					foreach (XmlQualifiedName qn in qnames)
						if (qn.Name == p) {
							p = "q" + idx;
							loop = true;
							break;
						}
				}
				nss.Add (p, XmlSchema.Namespace);
			}

			if (nss.Count == 0) {
				// Add the xml schema namespace. (It is done 
				// only when no entry exists in Namespaces).
				nss.Add ("xs", XmlSchema.Namespace);
				if (TargetNamespace != null && TargetNamespace.Length != 0)
					nss.Add ("tns", TargetNamespace);
			}

			XmlSchemaSerializer xser = new XmlSchemaSerializer ();
			XmlSerializerNamespaces backup = Namespaces;
			try {
				Namespaces = null;
				xser.Serialize (writer, this, nss);
			} finally {
				Namespaces = backup;
			}
			writer.Flush();
		}
		#endregion
	}

	class XmlSchemaSerializer : XmlSerializer
	{
		protected override void Serialize (object o, XmlSerializationWriter writer)
		{
			XmlSchemaSerializationWriter w = writer as XmlSchemaSerializationWriter;
			w.WriteRoot_XmlSchema ((XmlSchema) o);
		}

		protected override XmlSerializationWriter CreateWriter ()
		{
			return new XmlSchemaSerializationWriter ();
		}
	}
	
	class CompiledSchemaMemo
	{
		public string SourceUri;
		public int LineNumber;
		public int LinePosition;
	}
}
