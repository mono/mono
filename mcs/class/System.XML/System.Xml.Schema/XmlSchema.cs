// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.ComponentModel;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchema.
	/// </summary>
	[XmlRoot("schema",Namespace="http://www.w3.org/2001/XMLSchema")]
	public class XmlSchema : XmlSchemaObject
	{
		//public constants
		public const string InstanceNamespace = "http://www.w3.org/2001/XMLSchema-instance";
		public const string Namespace = "http://www.w3.org/2001/XMLSchema";

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
		private bool isCompiled ;
		private XmlSchemaObjectCollection items ;
		private XmlSchemaObjectTable notations ;
		private XmlSchemaObjectTable schemaTypes ;
		private string targetNamespace ;
		private XmlAttribute[] unhandledAttributes ;
		private string version;
		private string language;

		// Compiler specific things
		private XmlSchemaInfo info;

		public XmlSchema()
		{
			attributeFormDefault= XmlSchemaForm.None;
			blockDefault		= XmlSchemaDerivationMethod.None;
			elementFormDefault	= XmlSchemaForm.None;
			finalDefault		= XmlSchemaDerivationMethod.None;
			includes			= new XmlSchemaObjectCollection();
			isCompiled			= false;
			items				= new XmlSchemaObjectCollection();
			attributeGroups		= new XmlSchemaObjectTable();
			attributes			= new XmlSchemaObjectTable();
			elements			= new XmlSchemaObjectTable();
			groups				= new XmlSchemaObjectTable();
			notations			= new XmlSchemaObjectTable();
			schemaTypes			= new XmlSchemaObjectTable();
		}

		#region Properties

		[DefaultValue(XmlSchemaForm.None)]
		[System.Xml.Serialization.XmlAttribute("attributeFormDefault")]
		public XmlSchemaForm AttributeFormDefault 
		{
			get{ return attributeFormDefault; }
			set{ this.attributeFormDefault = value;}
		}

		[XmlIgnore]
		public XmlSchemaObjectTable AttributeGroups 
		{
			get{ return attributeGroups; }
		}
		
		[XmlIgnore]
		public XmlSchemaObjectTable Attributes 
		{
			get{ return attributes;}
		}
		
		[DefaultValue(XmlSchemaDerivationMethod.None)]
		[System.Xml.Serialization.XmlAttribute("blockDefault")]
		public XmlSchemaDerivationMethod BlockDefault 
		{
			get{ return blockDefault;}
			set{ blockDefault = value;}
		}
		
		[DefaultValue(XmlSchemaForm.None)]
		[System.Xml.Serialization.XmlAttribute("elementFormDefault")]
		public XmlSchemaForm ElementFormDefault 
		{
			get{ return elementFormDefault;}
			set{ elementFormDefault = value;}
		}

		[XmlIgnore]
		public XmlSchemaObjectTable Elements 
		{
			get{ return elements;}
		}

		[DefaultValue(XmlSchemaDerivationMethod.None)]
		[System.Xml.Serialization.XmlAttribute("finalDefault")]
		public XmlSchemaDerivationMethod FinalDefault 
		{
			get{ return finalDefault;}
			set{ finalDefault = value;}
		}

		[XmlIgnore]
		public XmlSchemaObjectTable Groups 
		{
			get{ return groups;}
		}

		[System.Xml.Serialization.XmlAttribute("id")]
		public string Id 
		{
			get{ return id;}
			set{ id = value;}
		}
		
		[XmlElement("include",typeof(XmlSchemaInclude),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("import",typeof(XmlSchemaImport),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("redefine",typeof(XmlSchemaRedefine),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaObjectCollection Includes 
		{
			get{ return includes;}
		}
		
		[XmlIgnore]
		public bool IsCompiled 
		{
			get{ return isCompiled;}
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
		public XmlSchemaObjectTable Notations 
		{
			get{ return notations;}
		}
		
		[XmlIgnore]
		public XmlSchemaObjectTable SchemaTypes 
		{
			get{ return schemaTypes; }
		}
		
		[System.Xml.Serialization.XmlAttribute("targetNamespace")]
		public string TargetNamespace 
		{
			get{ return targetNamespace;}
			set{ targetNamespace = value;}
		}
		
		[XmlAnyAttribute]
		public XmlAttribute[] UnhandledAttributes 
		{
			get{ return unhandledAttributes;}
			set{ unhandledAttributes = value;}
		}
		
		[System.Xml.Serialization.XmlAttribute("version")]
		public string Version 
		{
			get{ return version;}
			set{ version = value;}
		}

		// New attribute defined in W3C schema element
		[System.Xml.Serialization.XmlAttribute("xml:lang")]
		public string Language
		{
			get{ return  language; }
			set{ language = value; }
		}

		#endregion

		// Methods
		/// <summary>
		/// This compile method does two things:
		/// 1. It compiles and fills the PSVI dataset
		/// 2. Validates the schema by calling Validate method.
		/// Every XmlSchemaObject has a Compile Method which gets called.
		/// </summary>
		/// <remarks>
		///		1. blockDefault must be one of #all | List of (extension | restriction | substitution)
		///		2. finalDefault must be one of (#all | List of (extension | restriction| union| list))
		///		3. id must be of type ID
		///		4. targetNamespace should be any uri
		///		5. version should be a token
		///		6. xml:lang should be a language
		///		
		/// </remarks>
		[MonoTODO]
		public void Compile(ValidationEventHandler handler)
		{
			//1. Union and List are not allowed in block default
			if(this.blockDefault != XmlSchemaDerivationMethod.All)
			{
				if((this.blockDefault & XmlSchemaDerivationMethod.List)!=0 )
					error(handler, "list is not allowed in blockDefault attribute");
				if((this.blockDefault & XmlSchemaDerivationMethod.Union)!=0 )
					error(handler, "union is not allowed in blockDefault attribute");
			}
			//2. Substitution is not allowed in finaldefault.
			if(this.finalDefault != XmlSchemaDerivationMethod.All)
			{
				if((this.finalDefault & XmlSchemaDerivationMethod.Substitution)!=0 )
					error(handler, "substitution is not allowed in finalDefault attribute");
			}
			//3. id must be of type ID
			if(this.id != null && !XmlSchemaUtil.CheckID(this.id))
				error(handler, "id attribute is not a valid ID");

			//4. targetNamespace should be of type anyURI
			if(!XmlSchemaUtil.CheckAnyUri(this.targetNamespace))
				error(handler, "targetNamespace is not a valid URI");

			//5. version should be of type TOKEN
			if(!XmlSchemaUtil.CheckToken(this.version))
				error(handler, "version is not a valid token");

			//6. xml:lang must be a language
			if(!XmlSchemaUtil.CheckLanguage(this.language))
				error(handler, "xml:lang is not a valid language");

			// Create the xmlschemainfo object which we use to pass variables like targetnamespace;
			info = new XmlSchemaInfo();
			if(this.targetNamespace != null && XmlSchemaUtil.CheckAnyUri(this.targetNamespace))
				info.targetNS = this.TargetNamespace;
			
			if(this.ElementFormDefault != XmlSchemaForm.Qualified)
				info.formDefault = XmlSchemaForm.Unqualified;
			else
				info.formDefault = XmlSchemaForm.Qualified;

			if(FinalDefault == XmlSchemaDerivationMethod.All)
				info.finalDefault = XmlSchemaDerivationMethod.All;
			else // If finalDefault is None, info's finalDefault is set to empty
				info.finalDefault = (FinalDefault & (XmlSchemaDerivationMethod.Extension | XmlSchemaDerivationMethod.Restriction));

			if(BlockDefault == XmlSchemaDerivationMethod.All)
				info.blockDefault = XmlSchemaDerivationMethod.All;
			else // If finalDefault is None, info's blockDefault is set to empty
				info.blockDefault = (blockDefault & (XmlSchemaDerivationMethod.Extension |
									XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Substitution));

			// Compile the content of this schema
			foreach(XmlSchemaObject obj in Includes)
			{
				if(obj is XmlSchemaExternal)
				{
					//FIXME: Kuch to karo! (Do Something ;)
				}
				else
				{
					error(handler,"Object of Type "+obj.GetType().Name+" is not valid in Includes Property of Schema");
				}
			}
			foreach(XmlSchemaObject obj in Items)
			{
				if(obj is XmlSchemaAnnotation)
				{
					if(((XmlSchemaAnnotation)obj).Compile(handler,info) == 0)
					{
						//FIXME: What PSVI set do we add this to?
					}
				}
				else if(obj is XmlSchemaAttribute)
				{
					XmlSchemaAttribute attr = (XmlSchemaAttribute) obj;
					attr.parentIsSchema = true;
					if(attr.Compile(handler,info) == 0)
					{
						Attributes.Add(attr.QualifiedName, attr);
					}
				}
				else if(obj is XmlSchemaAttributeGroup)
				{
					XmlSchemaAttributeGroup attrgrp = (XmlSchemaAttributeGroup) obj;
					if(attrgrp.Compile(handler,info) == 0)
					{
						AttributeGroups.Add(attrgrp.QualifiedName, attrgrp);
					}
				}
				else if(obj is XmlSchemaComplexType)
				{
					XmlSchemaComplexType ctype = (XmlSchemaComplexType) obj;
					ctype.istoplevel = true;
					if(ctype.Compile(handler,info) == 0)
					{
						schemaTypes.Add(ctype.QualifiedName, ctype);
					}
				}
				else if(obj is XmlSchemaSimpleType)
				{
					XmlSchemaSimpleType stype = (XmlSchemaSimpleType) obj;
					stype.islocal = false; //This simple type is toplevel
					if(stype.Compile(handler,info) == 0)
					{
						SchemaTypes.Add(stype.QualifiedName, stype);
					}
				}
				else if(obj is XmlSchemaElement)
				{
					XmlSchemaElement elem = (XmlSchemaElement) obj;
					elem.parentIsSchema = true;
					if(elem.Compile(handler,info) == 0)
					{
						Elements.Add(elem.QualifiedName,elem);
					}
				}
				else if(obj is XmlSchemaGroup)
				{
					XmlSchemaGroup grp = (XmlSchemaGroup) obj;
					if(grp.Compile(handler,info) == 0)
					{
						Groups.Add(grp.QualifiedName,grp);
					}
				}
				else if(obj is XmlSchemaNotation)
				{
					XmlSchemaNotation ntn = (XmlSchemaNotation) obj;
					if(ntn.Compile(handler,info) == 0)
					{
						Notations.Add(ntn.QualifiedName, ntn);
					}
				}
				else
				{
					ValidationHandler.RaiseValidationError(handler,this,
						"Object of Type "+obj.GetType().Name+" is not valid in Item Property of Schema");
				}
			}
			Validate(handler);
		}

		[MonoTODO]
		protected void Validate(ValidationEventHandler handler)
		{

			foreach(XmlSchemaObject obj in Includes)
			{
			}

			//				foreach(XmlSchemaAnnotation ann in ??????)
			//				{
			//					ann.Validate(handler);
			//				}
			foreach(XmlSchemaAttribute attr in Attributes.Values)
			{
				attr.Validate(handler);
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
					((XmlSchemaSimpleType)type).Validate(handler);
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

		public static XmlSchema Read(TextReader reader, ValidationEventHandler validationEventHandler)
		{
			return Read(new XmlTextReader(reader),validationEventHandler);
		}
		public static XmlSchema Read(Stream stream, ValidationEventHandler validationEventHandler)
		{
			return Read(new XmlTextReader(stream),validationEventHandler);
		}

		[MonoTODO]
		//FIXME: Use the validationeventhandler. Probably needs the parser
		public static XmlSchema Read(XmlReader reader, ValidationEventHandler validationEventHandler)
		{
			XmlSerializer xser = new XmlSerializer(typeof(XmlSchema));
			return (XmlSchema) xser.Deserialize(reader);
		}
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
		[MonoTODO]
		public void Write(System.Xml.XmlWriter writer, System.Xml.XmlNamespaceManager namespaceManager)
		{
			XmlSerializerNamespaces xns;
			
			if(Namespaces != null)
			{
				xns = new XmlSerializerNamespaces(this.Namespaces);
			}
			else
			{
				xns = new XmlSerializerNamespaces();
			}

			if(namespaceManager != null)
			{
				foreach(string name in namespaceManager)
				{
					//xml and xmlns namespaced are added by default in namespaceManager. 
					//So we should ignore them
					if(name!="xml" && name != "xmlns")
						xns.Add(name,namespaceManager.LookupNamespace(name));
				}
			}
			
			this.Namespaces = xns;
			
			XmlSerializer xser = new XmlSerializer(typeof(XmlSchema));
			xser.Serialize(writer,this,xns);
			writer.Flush();
		}
		
		internal void error(ValidationEventHandler handle,string message)
		{
			ValidationHandler.RaiseValidationError(handle,this,message);
		}
	}
}
