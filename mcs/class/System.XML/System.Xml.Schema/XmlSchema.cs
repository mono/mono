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
		[XmlIgnore]
		public const string InstanceNamespace = "http://www.w3.org/2001/XMLSchema-instance";
		[XmlIgnore]
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

		public XmlSchema()
		{
			attributeFormDefault= XmlSchemaForm.None;
			blockDefault		= XmlSchemaDerivationMethod.None;
			elementFormDefault	= XmlSchemaForm.None;
			finalDefault		= XmlSchemaDerivationMethod.None;
			includes			= new XmlSchemaObjectCollection();
			isCompiled			= false;
			items				= new XmlSchemaObjectCollection();
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
			get{ return schemaTypes;}
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
		[MonoTODO]
		public void Compile(ValidationEventHandler validationEventHandler)
		{
			attributeGroups = null;

		}

		public static XmlSchema Read(TextReader reader, ValidationEventHandler validationEventHandler)
		{
			return Read(new XmlTextReader(reader),validationEventHandler);
		}
		public static XmlSchema Read(Stream stream, ValidationEventHandler validationEventHandler)
		{
			return Read(new XmlTextReader(stream),validationEventHandler);
		}
		//<ToBeRemoved>
		private static void Serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
		{
			Console.WriteLine("Unknown Attribute");
			Console.WriteLine("\t" + e.Attr.Name + " " + e.Attr.InnerXml);
			Console.WriteLine("\t LineNumber: " + e.LineNumber);
			Console.WriteLine("\t LinePosition: " + e.LinePosition);
		}
		private static void Serializer_UnknownElement(object sender, XmlElementEventArgs e)
		{
			Console.WriteLine("Unknown Element");
			Console.WriteLine("\t" + e.Element.Name + " " + e.Element.InnerXml);
			Console.WriteLine("\t LineNumber: " + e.LineNumber);
			Console.WriteLine("\t LinePosition: " + e.LinePosition);
		}
		private static void Serializer_UnknownNode(object sender, XmlNodeEventArgs e)
		{
			Console.WriteLine("Unknown Node");
			Console.WriteLine("\t" + e.Name + " " + e.Text);
			Console.WriteLine("\t LineNumber: " + e.LineNumber);
			Console.WriteLine("\t LinePosition: " + e.LinePosition);
		}
		private static void Serializer_UnknownAttribute(object sender, UnreferencedObjectEventArgs e)
		{
			Console.WriteLine("Unknown");
			Console.WriteLine("\t" + e.UnreferencedId);
			Console.WriteLine("\t" + e.UnreferencedObject);
		}
		//</ToBeRemoved>
		[MonoTODO]
		public static XmlSchema Read(XmlReader reader, ValidationEventHandler validationEventHandler)
		{
			XmlSerializer xser = new XmlSerializer(typeof(XmlSchema));
			//<ToBeRemoved>
			xser.UnknownAttribute +=  new XmlAttributeEventHandler(Serializer_UnknownAttribute);
			xser.UnknownElement +=  new XmlElementEventHandler(Serializer_UnknownElement);
			xser.UnknownNode +=  new XmlNodeEventHandler(Serializer_UnknownNode);
			xser.UnreferencedObject +=  new UnreferencedObjectEventHandler(Serializer_UnknownAttribute);
			//</ToBeRemoved>
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
			// This is why the Write was not writing schema with line breaks
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
	}
}
