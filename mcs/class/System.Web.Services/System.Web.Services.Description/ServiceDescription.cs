// 
// System.Web.Services.Description.ServiceDescription.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.IO;
using System.Collections;
using System.Reflection;
using System.Web.Services;
using System.Web.Services.Configuration;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	[XmlFormatExtensionPoint ("Extensions")]
	[XmlRoot ("definitions", Namespace = "http://schemas.xmlsoap.org/wsdl/")]
	public sealed class ServiceDescription : DocumentableItem {

		#region Fields

		public const string Namespace = "http://schemas.xmlsoap.org/wsdl/";

		BindingCollection bindings;
		ServiceDescriptionFormatExtensionCollection extensions;
		ImportCollection imports;
		MessageCollection messages;
		string name;
		PortTypeCollection portTypes;
		string retrievalUrl;
		ServiceDescriptionCollection serviceDescriptions;
		ServiceCollection services;
		string targetNamespace;
		Types types;
		static ServiceDescriptionSerializer serializer;

		#endregion // Fields

		#region Constructors

		static ServiceDescription ()
		{
			serializer = new ServiceDescriptionSerializer ();
		}

		[MonoTODO ("Move namespaces to subtype, use ServiceDescriptionSerializer")]	
		public ServiceDescription ()
		{
			bindings = new BindingCollection (this);
			extensions = new ServiceDescriptionFormatExtensionCollection (this);
			imports = new ImportCollection (this);
			messages = new MessageCollection (this);
			name = String.Empty;		
			portTypes = new PortTypeCollection (this);

			serviceDescriptions = null;
			services = new ServiceCollection (this);
			targetNamespace = String.Empty;
			types = null;
		}
		
		#endregion // Constructors

		#region Properties

		[XmlElement ("import")]
		public ImportCollection Imports {
			get { return imports; }
		}

		[XmlElement ("types")]
		public Types Types {
			get { return types; }
			set { types = value; }
		}

		[XmlElement ("message")]
		public MessageCollection Messages {
			get { return messages; }
		}

		[XmlElement ("portType")]	
		public PortTypeCollection PortTypes {
			get { return portTypes; }
		}
	
		[XmlElement ("binding")]
		public BindingCollection Bindings {
			get { return bindings; }
		}

		[XmlIgnore]
		public ServiceDescriptionFormatExtensionCollection Extensions { 	
			get { return extensions; }
		}

		[XmlAttribute ("name", DataType = "NMTOKEN")]	
		public string Name {
			get { return name; }
			set { name = value; }
		}

		[XmlIgnore]	
		public string RetrievalUrl {
			get { return retrievalUrl; }
			set { retrievalUrl = value; }
		}
	
		[XmlIgnore]	
		public static XmlSerializer Serializer {
			get { return serializer; }
		}

		[XmlIgnore]
		public ServiceDescriptionCollection ServiceDescriptions {
			get { 
				if (serviceDescriptions == null) 
					throw new NullReferenceException ();
				return serviceDescriptions; 
			}
		}

		[XmlElement ("service")]
		public ServiceCollection Services {
			get { return services; }
		}

		[XmlAttribute ("targetNamespace")]
		public string TargetNamespace {
			get { return targetNamespace; }
			set { targetNamespace = value; }
		}

		#endregion // Properties

		#region Methods

		public static bool CanRead (XmlReader reader)
		{
			return serializer.CanDeserialize (reader);
		}

		public static ServiceDescription Read (Stream stream)
		{
			return (ServiceDescription) serializer.Deserialize (stream);
		}

		public static ServiceDescription Read (string fileName)
		{
			return Read (new FileStream (fileName, FileMode.Open));
		}

		public static ServiceDescription Read (TextReader textReader)
		{
			return (ServiceDescription) serializer.Deserialize (textReader);
		}

		public static ServiceDescription Read (XmlReader reader)
		{
			return (ServiceDescription) serializer.Deserialize (reader);
		}

		public void Write (Stream stream)
		{
			serializer.Serialize (stream, this, GetNamespaceList ());
		}

		public void Write (string fileName)
		{
			Write (new FileStream (fileName, FileMode.Create));
		}

		public void Write (TextWriter writer)
		{
			serializer.Serialize (writer, this, GetNamespaceList ());
		}

		public void Write (XmlWriter writer)
		{
			serializer.Serialize (writer, this, GetNamespaceList ());
		}

		internal void SetParent (ServiceDescriptionCollection serviceDescriptions)
		{
			this.serviceDescriptions = serviceDescriptions; 
		}
		
		XmlSerializerNamespaces GetNamespaceList ()
		{
			XmlSerializerNamespaces ns;
			ns = new XmlSerializerNamespaces ();
			ns.Add ("soap", SoapBinding.Namespace);
			ns.Add ("soapenc", "http://schemas.xmlsoap.org/soap/encoding/");
			ns.Add ("s", XmlSchema.Namespace);
			ns.Add ("http", HttpBinding.Namespace);
			ns.Add ("mime", MimeContentBinding.Namespace);
			ns.Add ("tm", MimeTextBinding.Namespace);
			ns.Add ("s0", TargetNamespace);
			return ns;
		}
		
		internal static void WriteExtensions (XmlWriter writer, object ob)
		{
			ServiceDescriptionFormatExtensionCollection extensions = ExtensionManager.GetExtensionPoint (ob);
			if (extensions != null)
			{
				foreach (ServiceDescriptionFormatExtension ext in extensions)
					WriteExtension (writer, ext);
			}
		}
		
		static void WriteExtension (XmlWriter writer, ServiceDescriptionFormatExtension ext)
		{
			Type type = ext.GetType ();
			ExtensionInfo info = ExtensionManager.GetFormatExtensionInfo (type);
			string prefix = info.Prefix;
			
			if (prefix == null || prefix == "") prefix = writer.LookupPrefix (info.Namespace);
			
//				if (prefix != null && prefix != "")
//					Writer.WriteStartElement (prefix, info.ElementName, info.Namespace);
//				else
//					WriteStartElement (info.ElementName, info.Namespace, false);

			info.Serializer.Serialize (writer, ext);
		}
		
		internal static void ReadExtension (XmlReader reader, object ob)
		{
			ServiceDescriptionFormatExtensionCollection extensions = ExtensionManager.GetExtensionPoint (ob);
			if (extensions != null)
			{
				ExtensionInfo info = ExtensionManager.GetFormatExtensionInfo (reader.LocalName, reader.NamespaceURI);
				if (info != null)
				{
					object extension = info.Serializer.Deserialize (reader);
					extensions.Add ((ServiceDescriptionFormatExtension)extension);
					return;
				}
			}
			reader.Skip ();
		}


		#endregion

		internal class ServiceDescriptionSerializer : XmlSerializer 
		{
			protected override void Serialize (object o, XmlSerializationWriter writer)
			{
				ServiceDescriptionWriterBase xsWriter = writer as ServiceDescriptionWriterBase;
				xsWriter.WriteTree ((ServiceDescription)o);
			}
			
			protected override object Deserialize (XmlSerializationReader reader)
			{
				ServiceDescriptionReaderBase xsReader = reader as ServiceDescriptionReaderBase;
				return xsReader.ReadTree ();
			}
			
			protected override XmlSerializationWriter CreateWriter ()
			{
				return new ServiceDescriptionWriterBase ();
			}
			
			protected override XmlSerializationReader CreateReader ()
			{
				return new ServiceDescriptionReaderBase ();
			}
		}		
	}
}
