// 
// System.Web.Services.Description.ProtocolReflector.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	public abstract class ProtocolReflector {

		#region Fields

		Binding binding;
		string defaultNamespace;
		MessageCollection headerMessages;
		Message inputMessage;
		LogicalMethodInfo method;
		WebMethodAttribute methodAttribute;
		LogicalMethodInfo[] methods;
		Operation operation;
		OperationBinding operationBinding;
		Message outputMessage;		
		Port port;
		PortType portType;
		string protocolName;
		XmlReflectionImporter reflectionImporter;
		XmlSchemaExporter schemaExporter;
		XmlSchemas schemas;
		Service service;
		ServiceDescription serviceDescription;
		ServiceDescriptionCollection serviceDescriptions;
		Type serviceType;
		string serviceUrl;

		#endregion // Fields

		#region Constructors
	
		[MonoTODO]	
		protected ProtocolReflector ()
		{
			throw new NotImplementedException ();
		}
		
		#endregion // Constructors

		#region Properties

		public Binding Binding {
			get { return binding; }
		}

		public string DefaultNamespace {
			get { return defaultNamespace; }
		}

		public MessageCollection HeaderMessages {
			get { return headerMessages; }
		}

		public Message InputMessage {
			get { return inputMessage; }
		}

		public LogicalMethodInfo Method {
			get { return method; }
		}

		public WebMethodAttribute MethodAttribute {
			get { return methodAttribute; }
		}

		public LogicalMethodInfo[] Methods {
			get { return methods; }
		}
	
		public Operation Operation {
			get { return operation; }
		}

		public OperationBinding OperationBinding {
			get { return operationBinding; }
		}

		public Message OutputMessage {
			get { return outputMessage; }
		}

		public Port Port {
			get { return port; }
		}

		public PortType PortType {
			get { return portType; }
		}

		public abstract string ProtocolName {
			get; 
		}

		public XmlReflectionImporter ReflectionImporter {
			get { return reflectionImporter; }
		}

		public XmlSchemaExporter SchemaExporter {
			get { return schemaExporter; }
		}

		public XmlSchemas Schemas {
			get { return schemas; }
		}

		public Service Service {
			get { return service; }
		}

		public ServiceDescription ServiceDescription {
			get { return serviceDescription; }
		}

		public ServiceDescriptionCollection ServiceDescriptions {
			get { return serviceDescriptions; }
		}

		public Type ServiceType {
			get { return serviceType; }
		}

		public string ServiceUrl {
			get { return serviceUrl; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		protected virtual void BeginClass ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void EndClass ()
		{
			throw new NotImplementedException ();
		}

		public ServiceDescription GetServiceDescription (string ns)
		{
			return serviceDescriptions [ns];
		}

		protected abstract bool ReflectMethod ();

		[MonoTODO]
		protected virtual string ReflectMethodBinding ()
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
