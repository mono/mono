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
		private string targetNamespace ;
		private XmlAttribute[] unhandledAttributes ;
		private string version;

		// other post schema compilation infoset
		private Hashtable idCollection;
		private XmlSchemaObjectTable namedIdentities;
		private XmlSchemaSet schemas;

		private XmlNameTable nameTable;

		internal bool missedSubComponents;

		// Only compilation-time use
		private XmlSchemaObjectCollection compilationItems;
		private Hashtable handledUris;

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
			idCollection = new Hashtable ();
			namedIdentities = new XmlSchemaObjectTable();
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

		[System.Xml.Serialization.XmlAttribute ("targetNamespace")]
		public string TargetNamespace
		{
			get{ return targetNamespace; }
			set{ targetNamespace = value; }
		}

		[System.Xml.Serialization.XmlAttribute ("version")]
		public string Version
		{
			get{ return version; }
			set{ version = value; }
		}

		[XmlElement ("include",typeof(XmlSchemaInclude), Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement ("import",typeof(XmlSchemaImport), Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement ("redefine",typeof(XmlSchemaRedefine), Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaObjectCollection Includes
		{
			get{ return includes;}
		}

		[XmlElement ("simpleType", typeof (XmlSchemaSimpleType), Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement ("complexType", typeof (XmlSchemaComplexType), Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement ("group", typeof (XmlSchemaGroup),Namespace="http://www.w3.org/2001/XMLSchema")]
			//Only Schema's attributeGroup has type XmlSchemaAttributeGroup.
			//Others (complextype, restrictions etc) must have XmlSchemaAttributeGroupRef
		[XmlElement ("attributeGroup", typeof (XmlSchemaAttributeGroup), Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement ("element", typeof (XmlSchemaElement), Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement ("attribute", typeof (XmlSchemaAttribute), Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement ("notation", typeof (XmlSchemaNotation), Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement ("annotation", typeof (XmlSchemaAnnotation), Namespace="http://www.w3.org/2001/XMLSchema")]
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

		[System.Xml.Serialization.XmlAttribute ("id")]
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

		internal Hashtable IDCollection
		{
			get { return idCollection; }
		}

		internal XmlSchemaObjectTable NamedIdentities
		{
			get { return namedIdentities; }
		}

		internal XmlSchemaSet Schemas
		{
			get { return schemas; }
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
		public void Compile (ValidationEventHandler handler)
		{
			Compile (handler, new XmlUrlResolver ());
		}

#if NET_1_1
		public void Compile (ValidationEventHandler handler, XmlResolver resolver)
#else
		internal void Compile (ValidationEventHandler handler, XmlResolver resolver)
#endif
		{
			Compile (handler, new Stack (), this, null, resolver);
		}

		internal void Compile (ValidationEventHandler handler, XmlSchemaSet col, XmlResolver resolver)
		{
			Compile (handler, new Stack (), this, col, resolver);
		}

		private void Compile (ValidationEventHandler handler, Stack schemaLocationStack, XmlSchema rootSchema, XmlSchemaSet col, XmlResolver resolver)
		{
			if (rootSchema != this) {
				CompilationId = rootSchema.CompilationId;
				schemas = rootSchema.schemas;
			}
			else {
				schemas = col;
				if (schemas == null) {
					schemas = new XmlSchemaSet ();
					schemas.CompilationId = Guid.NewGuid ();
				}
				CompilationId = schemas.CompilationId;
				this.idCollection.Clear ();
			}
			if (!schemas.Contains (this)) // e.g. xs:import
				schemas.Add (this);

			attributeGroups.Clear ();
			attributes.Clear ();
			elements.Clear ();
			groups.Clear ();
			notations.Clear ();
			schemaTypes.Clear ();
			namedIdentities.Clear ();

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
			XmlSchemaUtil.CompileID(Id, this, this.IDCollection, handler);

			//4. targetNamespace should be of type anyURI or absent
			if (TargetNamespace != null) {
				if(!XmlSchemaUtil.CheckAnyUri (TargetNamespace))
					error(handler, TargetNamespace+" is not a valid value for targetNamespace attribute of schema");
			}

			//5. version should be of type normalizedString
			if (!XmlSchemaUtil.CheckNormalizedString(Version))
				error(handler, Version + "is not a valid value for version attribute of schema");

			// Compile the content of this schema

			compilationItems = new XmlSchemaObjectCollection ();
			for (int i = 0; i < Items.Count; i++) {
#if NET_2_0
				Items [i].Parent = this;
#endif
				compilationItems.Add (Items [i]);
			}
			if (this == rootSchema)
				handledUris = new Hashtable ();

			// First, we run into inclusion schemas to collect 
			// compilation target items into compiledItems.
			for (int i = 0; i < Includes.Count; i++) {
#if NET_2_0
				Includes [i].Parent = this;
#endif
				XmlSchemaExternal ext = Includes [i] as XmlSchemaExternal;
				if (ext == null) {
					error (handler, String.Format ("Object of Type {0} is not valid in Includes Property of XmlSchema", Includes [i].GetType().Name));
					continue;
				}

				if (ext.SchemaLocation == null) 
					continue;

				Stream stream = null;
				string url = null;
				if (resolver != null) {
					url = GetResolvedUri (resolver, ext.SchemaLocation);
					if (schemaLocationStack.Contains (url)) {
						// Just skip nested inclusion. 
						// The spec is "carefully written"
						// not to handle it as an error.
//						error (handler, "Nested inclusion was found: " + url);
						// must skip this inclusion
						continue;
					}
					if (rootSchema.handledUris.Contains (url))
						// This schema is already handled, so simply skip (otherwise, duplicate definition errrors occur.
						continue;
					rootSchema.handledUris.Add (url, url);
					try {
						stream = resolver.GetEntity (new Uri (url), null, typeof (Stream)) as Stream;
					} catch (Exception) {
					// LAMESPEC: This is not good way to handle errors, but since we cannot know what kind of XmlResolver will come, so there are no mean to avoid this ugly catch.
						warn (handler, "Could not resolve schema location URI: " + url);
						stream = null;
					}
				}

				// Process redefinition children in advance.
				XmlSchemaRedefine redefine = Includes [i] as XmlSchemaRedefine;
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

				XmlSchema includedSchema = null;
				if (stream == null) {
					// It is missing schema components.
					missedSubComponents = true;
					continue;
				} else {
					schemaLocationStack.Push (url);
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

				// Set - actual - target namespace for the included schema * before compilation*.
				XmlSchemaImport import = ext as XmlSchemaImport;
				if (import != null) {
					if (TargetNamespace == includedSchema.TargetNamespace) {
						error (handler, "Target namespace must be different from that of included schema.");
						continue;
					} else if (includedSchema.TargetNamespace != import.Namespace) {
						error (handler, "Attribute namespace and its importing schema's target namespace must be the same.");
						continue;
					}
				} else {
					if (TargetNamespace == null && 
						includedSchema.TargetNamespace != null) {
						error (handler, "Target namespace is required to include a schema which has its own target namespace");
						continue;
					}
					else if (TargetNamespace != null && 
						includedSchema.TargetNamespace == null)
						includedSchema.TargetNamespace = TargetNamespace;
				}

				// Compile included schema.
				includedSchema.idCollection = this.IDCollection;
				includedSchema.Compile (handler, schemaLocationStack, rootSchema, col, resolver);
				schemaLocationStack.Pop ();

				if (import != null)
					rootSchema.schemas.Add (includedSchema);

				// Note that we use compiled items. Items
				// may not exist in Items, since included
				// schema also includes another schemas.
				foreach (DictionaryEntry entry in includedSchema.Attributes)
					compilationItems.Add ((XmlSchemaObject) entry.Value);
				foreach (DictionaryEntry entry in includedSchema.Elements)
					compilationItems.Add ((XmlSchemaObject) entry.Value);
				foreach (DictionaryEntry entry in includedSchema.SchemaTypes)
					compilationItems.Add ((XmlSchemaObject) entry.Value);
				foreach (DictionaryEntry entry in includedSchema.AttributeGroups)
					compilationItems.Add ((XmlSchemaObject) entry.Value);
				foreach (DictionaryEntry entry in includedSchema.Groups)
					compilationItems.Add ((XmlSchemaObject) entry.Value);
				foreach (DictionaryEntry entry in includedSchema.Notations)
					compilationItems.Add ((XmlSchemaObject) entry.Value);
			}

			// Compilation phase.
			// At least each Compile() must gives unique (qualified) name for each component.
			// It also checks self-resolvable properties correct.
			// Post compilation schema information contribution is not required here.
			// It should be done by Validate().
			for (int i = 0; i < compilationItems.Count; i++) {
				XmlSchemaObject obj = compilationItems [i];
				if(obj is XmlSchemaAnnotation) {
					int numerr = ((XmlSchemaAnnotation)obj).Compile (handler, this);
					errorCount += numerr;
				} else if (obj is XmlSchemaAttribute) {
					XmlSchemaAttribute attr = (XmlSchemaAttribute) obj;
					attr.ParentIsSchema = true;
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
					ctype.ParentIsSchema = true;
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

			if (rootSchema == this)
				Validate(handler);

			if (errorCount == 0)
				isCompiled = true;
			errorCount = 0;
		}

		private string GetResolvedUri (XmlResolver resolver, string relativeUri)
		{
			Uri baseUri = null;
			if (this.SourceUri != null && this.SourceUri != String.Empty)
				baseUri = new Uri (this.SourceUri);
			return resolver.ResolveUri (baseUri, relativeUri).ToString ();
		}

		internal bool IsNamespaceAbsent (string ns)
		{
			return !schemas.Contains (ns);
		}

		#endregion

		private void Validate (ValidationEventHandler handler)
		{
			ValidationId = CompilationId;

			// Firstly Element needs to be filled their substitution group info
			foreach (XmlSchemaElement elem in Elements.Values)
				elem.FillSubstitutionElementInfo ();

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

						if (rdr.NodeType == XmlNodeType.EndElement)
							rdr.Read ();
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
				if (nss == null)
					nss = new XmlSerializerNamespaces ();
				foreach (string name in namespaceManager) {
					//xml and xmlns namespaces are added by default in namespaceManager.
					//So we should ignore them
					if (name !="xml" && name != "xmlns")
						nss.Add (name, namespaceManager.LookupNamespace (name));
				}
			}

			if (Namespaces != null && Namespaces.Count > 0) {
				nss.Add (String.Empty, XmlSchema.Namespace);
				foreach (XmlQualifiedName qn in Namespaces.ToArray ()) {
					nss.Add (qn.Name, qn.Namespace);
				}
			}

			if (nss.Count == 0) {
				// Add the xml schema namespace. (It is done 
				// only when no entry exists in Namespaces).
				nss.Add ("xs", XmlSchema.Namespace);
				if (TargetNamespace != null)
					nss.Add ("tns", TargetNamespace);
			}

			XmlSchemaSerializer xser = new XmlSchemaSerializer ();
			xser.Serialize (writer, this, nss);
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
}
