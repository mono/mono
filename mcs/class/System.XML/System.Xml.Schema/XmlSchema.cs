//
// System.Xml.Schema.XmlSchema.cs
//
// Author:
//	Dwivedi, Ajay kumar  Adwiv@Yahoo.com
//	Atsushi Enomoto  ginga@kit.hi-ho.ne.jp
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
        [XmlRoot("schema",Namespace=XmlSchema.Namespace)]
        public class XmlSchema : XmlSchemaObject
        {
                //public constants
                public const string Namespace = "http://www.w3.org/2001/XMLSchema";
                public const string InstanceNamespace = "http://www.w3.org/2001/XMLSchema-instance";

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
                private string language;

		// post schema compilation infoset
		private Hashtable idCollection;
		private Hashtable missingBaseSchemaTypeRefs;
		private ArrayList missingElementTypeRefs;
		private XmlSchemaCollection schemas;

                // Compiler specific things
                private static string xmlname = "schema";

                public XmlSchema()
                {
                        attributeFormDefault= XmlSchemaForm.None;
                        blockDefault            = XmlSchemaDerivationMethod.None;
                        elementFormDefault      = XmlSchemaForm.None;
                        finalDefault            = XmlSchemaDerivationMethod.None;
                        includes                        = new XmlSchemaObjectCollection();
                        isCompiled                      = false;
                        items                           = new XmlSchemaObjectCollection();
                        attributeGroups         = new XmlSchemaObjectTable();
                        attributes                      = new XmlSchemaObjectTable();
                        elements                        = new XmlSchemaObjectTable();
                        groups                          = new XmlSchemaObjectTable();
                        notations                       = new XmlSchemaObjectTable();
                        schemaTypes                     = new XmlSchemaObjectTable();
			idCollection                    = new Hashtable ();
			missingBaseSchemaTypeRefs       = new Hashtable ();
			missingElementTypeRefs          = new ArrayList ();
                }

                #region Properties

                [DefaultValue(XmlSchemaForm.None)]
                [System.Xml.Serialization.XmlAttribute("attributeFormDefault")]
                public XmlSchemaForm AttributeFormDefault
                {
                        get{ return attributeFormDefault; }
                        set{ this.attributeFormDefault = value;}
                }

                [DefaultValue(XmlSchemaDerivationMethod.None)]
                [System.Xml.Serialization.XmlAttribute("blockDefault")]
                public XmlSchemaDerivationMethod BlockDefault
                {
                        get{ return blockDefault;}
                        set{ blockDefault = value;}
                }

                [DefaultValue(XmlSchemaDerivationMethod.None)]
                [System.Xml.Serialization.XmlAttribute("finalDefault")]
                public XmlSchemaDerivationMethod FinalDefault
                {
                        get{ return finalDefault;}
                        set{ finalDefault = value;}
                }

                [DefaultValue(XmlSchemaForm.None)]
                [System.Xml.Serialization.XmlAttribute("elementFormDefault")]
                public XmlSchemaForm ElementFormDefault
                {
                        get{ return elementFormDefault;}
                        set{ elementFormDefault = value;}
                }

                [System.Xml.Serialization.XmlAttribute("targetNamespace")]
                public string TargetNamespace
                {
                        get{ return targetNamespace;}
                        set{ targetNamespace = value;}
                }

                [System.Xml.Serialization.XmlAttribute("version")]
                public string Version
                {
                        get{ return version;}
                        set{ version = value;}
                }

                [XmlElement("include",typeof(XmlSchemaInclude),Namespace="http://www.w3.org/2001/XMLSchema")]
                [XmlElement("import",typeof(XmlSchemaImport),Namespace="http://www.w3.org/2001/XMLSchema")]
                [XmlElement("redefine",typeof(XmlSchemaRedefine),Namespace="http://www.w3.org/2001/XMLSchema")]
                public XmlSchemaObjectCollection Includes
                {
                        get{ return includes;}
                }

                [XmlElement("simpleType",typeof(XmlSchemaSimpleType),Namespace="http://www.w3.org/2001/XMLSchema")]
                [XmlElement("complexType",typeof(XmlSchemaComplexType),Namespace="http://www.w3.org/2001/XMLSchema")]
                [XmlElement("group",typeof(XmlSchemaGroup),Namespace="http://www.w3.org/2001/XMLSchema")]
                        //Only Schema's attributeGroup has type XmlSchemaAttributeGroup.
                        //Others (complextype, restrictions etc) must have XmlSchemaAttributeGroupRef
                [XmlElement("attributeGroup",typeof(XmlSchemaAttributeGroup),Namespace="http://www.w3.org/2001/XMLSchema")]
                [XmlElement("element",typeof(XmlSchemaElement),Namespace="http://www.w3.org/2001/XMLSchema")]
                [XmlElement("attribute",typeof(XmlSchemaAttribute),Namespace="http://www.w3.org/2001/XMLSchema")]
                [XmlElement("notation",typeof(XmlSchemaNotation),Namespace="http://www.w3.org/2001/XMLSchema")]
                [XmlElement("annotation",typeof(XmlSchemaAnnotation),Namespace="http://www.w3.org/2001/XMLSchema")]
                public XmlSchemaObjectCollection Items
                {
                        get{ return items;}
                }

                [XmlIgnore]
                public bool IsCompiled
                {
                        get{ return this.CompilationId != Guid.Empty;}
                }

                [XmlIgnore]
                public XmlSchemaObjectTable Attributes
                {
                        get{ return attributes;}
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
                        get{ return elements;}
                }

                [System.Xml.Serialization.XmlAttribute("id")]
                public string Id
                {
                        get{ return id;}
                        set{ id = value;}
                }

                [XmlAnyAttribute]
                public XmlAttribute[] UnhandledAttributes
                {
                        get
                        {
                                if(unhandledAttributeList != null)
                                {
                                        unhandledAttributes = (XmlAttribute[]) unhandledAttributeList.ToArray(typeof(XmlAttribute));
                                        unhandledAttributeList = null;
                                }
                                return unhandledAttributes;
                        }
                        set
                        {
                                unhandledAttributes = value;
                                unhandledAttributeList = null;
                        }
                }

                [XmlIgnore]
                public XmlSchemaObjectTable Groups
                {
                        get{ return groups;}
                }

                [XmlIgnore]
                public XmlSchemaObjectTable Notations
                {
                        get{ return notations;}
                }

                // New attribute defined in W3C schema element
                [System.Xml.Serialization.XmlAttribute("xml:lang")]
                public string Language
                {
                        get{ return  language; }
                        set{ language = value; }
                }

		internal Hashtable IDCollection
		{
			get { return idCollection; }
		}

		internal Hashtable MissingBaseSchemaTypeRefs
		{
			get { return missingBaseSchemaTypeRefs; }
		}

		internal ArrayList MissingElementTypeRefs
		{
			get { return missingElementTypeRefs; }
		}

		internal XmlSchemaCollection Schemas
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
                ///             6. xml:lang should be a language
                /// </remarks>
                [MonoTODO]
                public void Compile(ValidationEventHandler handler)
		{
			Compile (handler, new Stack (), this);
			isCompiled = true;
		}

		private void Compile (ValidationEventHandler handler, Stack schemaLocationStack, XmlSchema rootSchema)
                {
			CompilationId = Guid.NewGuid ();
			schemas = new XmlSchemaCollection ();
			schemas.Add (this);

			attributeGroups = new XmlSchemaObjectTable ();
			attributes = new XmlSchemaObjectTable ();
			elements = new XmlSchemaObjectTable ();
			groups = new XmlSchemaObjectTable ();
			missingElementTypeRefs.Clear ();
			missingBaseSchemaTypeRefs.Clear ();
			notations = new XmlSchemaObjectTable ();
			schemaTypes = new XmlSchemaObjectTable ();

			//1. Union and List are not allowed in block default
                        if(BlockDefault != XmlSchemaDerivationMethod.All)
                        {
                                if((BlockDefault & XmlSchemaDerivationMethod.List)!=0 )
                                        error(handler, "list is not allowed in blockDefault attribute");
                                if((BlockDefault & XmlSchemaDerivationMethod.Union)!=0 )
                                        error(handler, "union is not allowed in blockDefault attribute");
                        }

                        //2. Substitution is not allowed in finaldefault.
                        if(FinalDefault != XmlSchemaDerivationMethod.All)
                        {
                                if((FinalDefault & XmlSchemaDerivationMethod.Substitution)!=0 )
                                        error(handler, "substitution is not allowed in finalDefault attribute");
                        }

                        //3. id must be of type ID
                        XmlSchemaUtil.CompileID(Id, this, this.IDCollection, handler);

                        //4. targetNamespace should be of type anyURI or absent
                        if(TargetNamespace != null)
                        {
                                if(!XmlSchemaUtil.CheckAnyUri(TargetNamespace))
                                        error(handler, TargetNamespace+" is not a valid value for targetNamespace attribute of schema");
                        }

                        //5. version should be of type normalizedString
                        if(!XmlSchemaUtil.CheckNormalizedString(Version))
                                error(handler, Version + "is not a valid value for version attribute of schema");

                        //6. xml:lang must be a language
                        if(!XmlSchemaUtil.CheckLanguage(Language))
                                error(handler, Language + " is not a valid language");

                        // Compile the content of this schema

			XmlSchemaObjectCollection compilationItems = new XmlSchemaObjectCollection ();
			foreach (XmlSchemaObject obj in Items)
				compilationItems.Add (obj);

                        foreach(XmlSchemaObject obj in Includes)
                        {
				XmlSchemaExternal ext = obj as XmlSchemaExternal;
                                if(ext != null)
                                {
					if (ext.SchemaLocation == null) continue;
					string url = GetResolvedUri (ext.SchemaLocation);
					XmlSchemaInclude include = ext as XmlSchemaInclude;
					if (include != null) {
						if (schemaLocationStack.Contains (url)) {
							error(handler, "Nested inclusion was found: " + url);
							// must skip this inclusion
							continue;
						}
						schemaLocationStack.Push (url);
						XmlSchema includedSchema = XmlSchema.Read (new XmlTextReader (url), handler);
						includedSchema.schemas = schemas;
						includedSchema.Compile (handler, schemaLocationStack, rootSchema);
						schemaLocationStack.Pop ();
						foreach (XmlSchemaObject includedObj in includedSchema.Items)
							compilationItems.Add (includedObj);
					}
					XmlSchemaRedefine redefine = obj as XmlSchemaRedefine;
					if (redefine != null) {

	                                        //FIXME: Kuch to karo! (Do Something ;)
						throw new NotImplementedException ();

						if (schemaLocationStack.Contains (url)) {
							error(handler, "Nested inclusion was found: " + url);
							// must skip this inclusion
							continue;
						}
					}
					XmlSchemaImport import = obj as XmlSchemaImport;
					if (import != null) {
						if (schemaLocationStack.Contains (url)) {
							error(handler, "Nested inclusion was found: " + url);
							// must skip this inclusion
							continue;
						}
						schemaLocationStack.Push (url);
						XmlSchema includedSchema = XmlSchema.Read (new XmlTextReader (url), handler);
						includedSchema.schemas = schemas;
						includedSchema.Compile (handler, schemaLocationStack, rootSchema);
						schemaLocationStack.Pop ();
						// TODO: check targetNamespace constraints
						rootSchema.schemas.Add (includedSchema);
						foreach (XmlSchemaElement element in includedSchema.Elements)
							elements.Add (element.QualifiedName, element);
						foreach (XmlSchemaAttribute attribute in includedSchema.Attributes)
							attributes.Add (attribute.QualifiedName, attribute);
						foreach (XmlSchemaAttributeGroup attrGroup in includedSchema.AttributeGroups)
							attributeGroups.Add (attrGroup.QualifiedName, attrGroup);
						foreach (XmlSchemaGroup group in includedSchema.Groups)
							groups.Add (group.QualifiedName, group);
						foreach (XmlSchemaGroup group in includedSchema.Groups)
							groups.Add (group.QualifiedName, group);
						foreach (XmlSchemaNotation notation in includedSchema.Notations)
							notations.Add (notation.QualifiedName, notation);
						foreach (XmlSchemaType schemaType in includedSchema.SchemaTypes)
							schemaTypes.Add (schemaType.QualifiedName, schemaType);
					}
                                }
                                else
                                {
                                        error(handler,"Object of Type "+obj.GetType().Name+" is not valid in Includes Property of XmlSchema");
                                }
                        }
			// It is hack, but types are required before element/attributes.
			foreach (XmlSchemaObject obj in compilationItems)
			{
                                if(obj is XmlSchemaComplexType)
                                {
                                        XmlSchemaComplexType ctype = (XmlSchemaComplexType) obj;
                                        ctype.istoplevel = true;
                                        int numerr = ctype.Compile(handler, this);
                                        errorCount += numerr;
                                        if(numerr == 0)
                                        {
                                                schemaTypes.Add(ctype.QualifiedName, ctype);
                                        }
                                }
                                else if(obj is XmlSchemaSimpleType)
                                {
                                        XmlSchemaSimpleType stype = (XmlSchemaSimpleType) obj;
                                        stype.islocal = false; //This simple type is toplevel
                                        int numerr = stype.Compile(handler, this);
                                        errorCount += numerr;
                                        if(numerr == 0)
                                        {
                                                SchemaTypes.Add(stype.QualifiedName, stype);
                                        }
                                }
			}
			// Add missing references.
			foreach (XmlSchemaComplexType cType in missingBaseSchemaTypeRefs.Keys)
				cType.BaseSchemaTypeInternal = 
					SchemaTypes [missingBaseSchemaTypeRefs [cType] as XmlQualifiedName];

			foreach(XmlSchemaObject obj in compilationItems)
                        {
                                if(obj is XmlSchemaAnnotation)
                                {
                                        int numerr = ((XmlSchemaAnnotation)obj).Compile(handler, this);
                                        errorCount += numerr;
                                        if( numerr == 0)
                                        {
                                                //FIXME: What PSVI set do we add this to?
                                        }
                                }
                                else if(obj is XmlSchemaAttribute)
                                {
                                        XmlSchemaAttribute attr = (XmlSchemaAttribute) obj;
                                        attr.parentIsSchema = true;
                                        int numerr = attr.Compile(handler, this);
                                        errorCount += numerr;
                                        if(numerr == 0)
                                        {
                                                Attributes.Set(attr.QualifiedName, attr);
                                        }
                                }
                                else if(obj is XmlSchemaAttributeGroup)
                                {
                                        XmlSchemaAttributeGroup attrgrp = (XmlSchemaAttributeGroup) obj;
                                        int numerr = attrgrp.Compile(handler, this);
                                        errorCount += numerr;
                                        if(numerr == 0)
                                        {
                                                AttributeGroups.Set(attrgrp.QualifiedName, attrgrp);
                                        }
                                }
                                else if(obj is XmlSchemaComplexType)
                                {
					// Do nothing here.
                                }
                                else if(obj is XmlSchemaSimpleType)
                                {
					// Do nothing here.
                                }
                                else if(obj is XmlSchemaElement)
                                {
                                        XmlSchemaElement elem = (XmlSchemaElement) obj;
                                        elem.parentIsSchema = true;
                                        int numerr = elem.Compile(handler, this);
                                        errorCount += numerr;
                                        if(numerr == 0)
                                        {
                                                Elements.Set(elem.QualifiedName,elem);
                                        }
                                }
                                else if(obj is XmlSchemaGroup)
                                {
                                        XmlSchemaGroup grp = (XmlSchemaGroup) obj;
                                        int numerr = grp.Compile(handler, this);
                                        errorCount += numerr;
                                        if(numerr == 0)
                                        {
                                                Groups.Set(grp.QualifiedName,grp);
                                        }
                                }
                                else if(obj is XmlSchemaNotation)
                                {
                                        XmlSchemaNotation ntn = (XmlSchemaNotation) obj;
                                        int numerr = ntn.Compile(handler, this);
                                        errorCount += numerr;
                                        if(numerr == 0)
                                        {
                                                Notations.Set(ntn.QualifiedName, ntn);
                                        }
                                }
                                else
                                {
                                        ValidationHandler.RaiseValidationError(handler,this,
                                                "Object of Type "+obj.GetType().Name+" is not valid in Item Property of Schema");
                                }
                        }

			if (rootSchema == this)
			{
				// First, fill type information for type reference
				foreach (XmlSchemaElement element in missingElementTypeRefs)
				{
					if (element.SchemaTypeName != XmlQualifiedName.Empty)
					{
						XmlSchemaType type = FindSchemaType (element.SchemaTypeName);
						// If el is null, then it is missing sub components ... ?
						element.SetSchemaType (type);
					}
				}
				// Then, fill type information for the type references for the referencing elements
				foreach (XmlSchemaElement element in missingElementTypeRefs)
				{
					if (element.RefName != XmlQualifiedName.Empty)
					{
						XmlSchemaElement refElem = FindElement (element.RefName);
						// If el is null, then it is missing sub components ... ?
						if (refElem != null) element.SetSchemaType (refElem.ElementType);
					}
				}
			}

			Validate(handler);
                }

		private string GetResolvedUri (string relativeUri)
		{
			Uri baseUri = null;
			if (this.SourceUri != null && this.SourceUri != String.Empty)
				baseUri = new Uri (this.SourceUri);
			return new XmlUrlResolver ().ResolveUri (baseUri, relativeUri).ToString ();
		}

                #endregion

                [MonoTODO]
                private void Validate(ValidationEventHandler handler)
                {
                        foreach(XmlSchemaAttribute attr in Attributes.Values)
                        {
                                attr.Validate(handler, this);
                        }
                        foreach(XmlSchemaAttributeGroup attrgrp in AttributeGroups.Values)
                        {
                                attrgrp.Validate(handler);
                        }
                        foreach(XmlSchemaType type in SchemaTypes.Values)
                        {
                                if(type is XmlSchemaComplexType)
                                {
                                        ((XmlSchemaComplexType)type).Validate(handler);
                                }
                                else
                                        ((XmlSchemaSimpleType)type).Validate(handler, this);
                        }
                        foreach(XmlSchemaElement elem in Elements.Values)
                        {
                                elem.Validate(handler);
                        }
                        foreach(XmlSchemaGroup grp in Groups.Values)
                        {
                                grp.Validate(handler);
                        }
                        foreach(XmlSchemaNotation ntn in Notations.Values)
                        {
                                ntn.Validate(handler);
                        }
                }

		internal XmlSchemaElement FindElement (XmlQualifiedName name)
		{
			if (name.Namespace != TargetNamespace)
				return null;

			XmlSchema target = schemas [name.Namespace];
			if (target == null)
				return null;

			foreach (XmlSchemaObject obj in target.Items) {
				XmlSchemaElement elem = obj as XmlSchemaElement;
				if (elem != null && elem.Name == name.Name)
					return elem;
			}
			return null;
		}

		internal XmlSchemaType FindSchemaType (XmlQualifiedName name)
		{
			if (name.Namespace != TargetNamespace)
				return null;

			XmlSchema target = schemas [name.Namespace];
			if (target == null)
				return null;

			foreach (XmlSchemaObject obj in target.Items) {
				XmlSchemaType type = obj as XmlSchemaType;
				if (type != null && type.Name == name.Name)
					return type;
			}
			return null;
		}

                #region Read

                public static XmlSchema Read(TextReader reader, ValidationEventHandler validationEventHandler)
                {
                        return Read(new XmlTextReader(reader),validationEventHandler);
                }
                public static XmlSchema Read(Stream stream, ValidationEventHandler validationEventHandler)
                {
                        return Read(new XmlTextReader(stream),validationEventHandler);
                }

		[MonoTODO ("Use ValidationEventHandler")]
                public static XmlSchema Read(XmlReader rdr, ValidationEventHandler validationEventHandler)
                {
			string baseURI = rdr.BaseURI;
                        XmlSerializer xser = new XmlSerializer (typeof (XmlSchema));
                        XmlSchema schema = (XmlSchema) xser.Deserialize (rdr);
			schema.SourceUri = baseURI;
			schema.Compile (validationEventHandler);
			return schema;
/*
			XmlSchemaReader reader = new XmlSchemaReader(rdr, validationEventHandler);

                        while(reader.ReadNextElement())
                        {
                                switch(reader.NodeType)
                                {
                                        case XmlNodeType.Element:
                                                if(reader.LocalName == "schema")
                                                {
                                                        XmlSchema schema = new XmlSchema();

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
                                                        return schema;
                                                }
                                                else
                                                {
                                                        //Schema can't be generated. Throw an exception
                                                        throw new XmlSchemaException("The root element must be schema", null);
                                                }
                                        default:
                                                error(validationEventHandler, "This should never happen. XmlSchema.Read 1 ",null);
                                                break;
                                }
                        }
                        throw new XmlSchemaException("The top level schema must have namespace "+XmlSchema.Namespace, null);
*/
                }
/*
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
                                                schema.blockDefault = XmlSchemaUtil.ReadDerivationAttribute(reader,out ex, "blockDefault");
                                                if(ex != null)
                                                        warn(h, ex.Message, ex);
                                                break;
                                        case "elementFormDefault":
                                                schema.elementFormDefault = XmlSchemaUtil.ReadFormAttribute(reader, out ex);
                                                if(ex != null)
                                                        error(h, reader.Value + " is not a valid value for elementFormDefault.", ex);
                                                break;
                                        case "finalDefault":
                                                schema.finalDefault = XmlSchemaUtil.ReadDerivationAttribute(reader, out ex, "finalDefault");
                                                if(ex != null)
                                                        warn(h, ex.Message , ex);
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
                                        case "xml:lang":
                                                schema.language = reader.Value;
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
*/
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
                public void Write(System.Xml.XmlWriter writer, System.Xml.XmlNamespaceManager namespaceManager)
                {
                        if(Namespaces == null)
                        {
                                Namespaces = new XmlSerializerNamespaces();
                        }
                        //Add the xml schema namespace.
                        if(Namespaces.Count == 0)
                        {
                                Namespaces.Add("xs", XmlSchema.Namespace);
                                if (TargetNamespace != null && TargetNamespace != String.Empty)
                                        Namespaces.Add("tns", TargetNamespace);
                        }
                        if(namespaceManager != null)
                        {
                                foreach(string name in namespaceManager)
                                {
                                        //xml and xmlns namespaced are added by default in namespaceManager.
                                        //So we should ignore them
                                        if(name!="xml" && name != "xmlns")
                                                Namespaces.Add(name,namespaceManager.LookupNamespace(name));
                                }
                        }

                        XmlSerializer xser = new XmlSerializer(typeof(XmlSchema));
                        xser.Serialize(writer,this,Namespaces);
                        writer.Flush();
                }
                #endregion
        }
}
