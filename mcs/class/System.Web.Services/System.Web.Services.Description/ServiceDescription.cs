// 
// System.Web.Services.Description.ServiceDescription.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.IO;
using System.Web.Services;
using System.Web.Services.Configuration;
using System.Xml;
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

		static XmlSerializer serializer;
		static SoapReflectionImporter importer;
		static XmlTypeMapping mapping;


		#endregion // Fields

		#region Constructors

		static ServiceDescription ()
		{
			importer = new SoapReflectionImporter ();
			mapping = importer.ImportTypeMapping (typeof (ServiceDescription));
			serializer = new XmlSerializer (mapping);
		}
	
		public ServiceDescription ()
		{
			bindings = new BindingCollection (this);
			extensions = new ServiceDescriptionFormatExtensionCollection (this);
			imports = new ImportCollection (this);
			messages = new MessageCollection (this);
			name = String.Empty;		
			portTypes = new PortTypeCollection (this);
			retrievalUrl = String.Empty;

			SoapReflectionImporter importer = new SoapReflectionImporter ();
			XmlTypeMapping mapping = importer.ImportTypeMapping (typeof (ServiceDescription));
			serializer = new XmlSerializer (mapping);

			serviceDescriptions = null;
			services = new ServiceCollection (this);
			targetNamespace = String.Empty;
			types = null;
		}
		
		#endregion // Constructors

		#region Properties

		[XmlElement ("binding")]
		public BindingCollection Bindings {
			get { return bindings; }
		}

		[XmlIgnore]
		public ServiceDescriptionFormatExtensionCollection Extensions { 	
			get { return extensions; }
		}

		[XmlElement ("import")]
		public ImportCollection Imports {
			get { return imports; }
		}

		[XmlElement ("message")]
		public MessageCollection Messages {
			get { return messages; }
		}

		[XmlAttribute ("name", DataType = "NMTOKEN")]	
		public string Name {
			get { return name; }
			set { name = value; }
		}

		[XmlElement ("portType")]	
		public PortTypeCollection PortTypes {
			get { return portTypes; }
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

		[XmlElement ("type")]
		public Types Types {
			get { return types; }
			set { types = value; }
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
			serializer.Serialize (stream, this);
		}

		public void Write (string fileName)
		{
			Write (new FileStream (fileName, FileMode.Create));
		}

		public void Write (TextWriter writer)
		{
			serializer.Serialize (writer, this);
		}

		public void Write (XmlWriter writer)
		{
			serializer.Serialize (writer, this);
		}

		internal void SetParent (ServiceDescriptionCollection serviceDescriptions)
		{
			this.serviceDescriptions = serviceDescriptions; 
		}

		#endregion
	}
}
