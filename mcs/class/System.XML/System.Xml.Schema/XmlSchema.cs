// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
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
	[XmlRoot] 
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
		private string version ;


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
		[DefaultValue(XmlSchemaForm.None)]
		[XmlAttribute]
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
		[XmlAttribute]
		public XmlSchemaDerivationMethod BlockDefault 
		{
			get{ return blockDefault;}
			set{ blockDefault = value;}
		}
		[DefaultValue(XmlSchemaForm.None)]
		[XmlAttribute]
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
		[XmlAttribute]
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
		[XmlAttribute]
		public string Id 
		{
			get{ return id;}
			set{ id = value;}
		}
		[XmlElement]
		public XmlSchemaObjectCollection Includes 
		{
			get{ return includes;}
		}
		[XmlIgnore]
		public bool IsCompiled 
		{
			get{ return isCompiled;}
		}
		[XmlElement]
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
		[XmlAttribute]
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
		[XmlAttribute]
		public string Version 
		{
			get{ return version;}
			set{ version = value;}
		}

		// Methods
		[MonoTODO]
		public void Compile(System.Xml.Schema.ValidationEventHandler validationEventHandler)
		{
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
		public static XmlSchema Read(XmlReader reader, ValidationEventHandler validationEventHandler)
		{
			return null;
		}

		public void Write(System.IO.Stream stream)
		{
			Write(new XmlTextWriter(stream,null),null);
		}
		public void Write(System.IO.TextWriter writer)
		{
			Write(new XmlTextWriter(writer),null);
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
			Write(new XmlTextWriter(writer),namespaceManager);
		}
		[MonoTODO]
		public void Write(System.Xml.XmlWriter writer, System.Xml.XmlNamespaceManager namespaceManager)
		{
		}
	}
}
