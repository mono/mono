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
using System.Xml;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
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
		XmlSerializer serializer;
		ServiceDescriptionCollection serviceDescriptions;
		ServiceCollection services;
		string targetNamespace;
		Types types;

		#endregion // Fields

		#region Constructors
	
		public ServiceDescription ()
		{
			bindings = new BindingCollection (this);
			extensions = new ServiceDescriptionFormatExtensionCollection (this);
			imports = new ImportCollection (this);
			messages = new MessageCollection (this);
			name = String.Empty;		
			portTypes = new PortTypeCollection (this);
			retrievalUrl = String.Empty;
			serializer = null;
			serviceDescriptions = null;
			services = new ServiceCollection (this);
			targetNamespace = String.Empty;
			types = null;
		}
		
		#endregion // Constructors

		#region Properties

		public BindingCollection Bindings {
			get { return bindings; }
		}

		public ServiceDescriptionFormatExtensionCollection Extensions { 	
			get { return extensions; }
		}

		public ImportCollection Imports {
			get { return imports; }
		}

		public MessageCollection Messages {
			get { return messages; }
		}
	
		public string Name {
			get { return name; }
			set { name = value; }
		}
	
		public PortTypeCollection PortTypes {
			get { return portTypes; }
		}
		
		public string RetrievalUrl {
			get { return retrievalUrl; }
			set { retrievalUrl = value; }
		}
		
		public static XmlSerializer Serializer {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public ServiceDescriptionCollection ServiceDescriptions {
			get { 
				if (serviceDescriptions == null) 
					throw new NullReferenceException ();
				return serviceDescriptions; 
			}
		}

		public ServiceCollection Services {
			get { return services; }
		}

		public string TargetNamespace {
			get { return targetNamespace; }
			set { targetNamespace = value; }
		}

		public Types Types {
			get { return types; }
			set { types = value; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public static bool CanRead (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ServiceDescription Read (Stream stream)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ServiceDescription Read (string fileName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ServiceDescription Read (TextReader textReader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ServiceDescription Read (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (Stream stream)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (string fileName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (TextWriter writer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (XmlWriter writer)
		{
			throw new NotImplementedException ();
		}

		internal void SetParent (ServiceDescriptionCollection serviceDescriptions)
		{
			this.serviceDescriptions = serviceDescriptions; 
		}

		#endregion
	}
}
