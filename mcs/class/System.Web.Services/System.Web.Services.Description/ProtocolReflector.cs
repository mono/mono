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
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public string DefaultNamespace {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public MessageCollection HeaderMessages {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public Message InputMessage {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public LogicalMethodInfo Method {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public WebMethodAttribute MethodAttribute {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public LogicalMethodInfo[] Methods {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}
	
		public Operation Operation {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public OperationBinding OperationBinding {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public Message OutputMessage {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public Port Port {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public PortType PortType {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public abstract string ProtocolName {
			get; 
		}

		public XmlReflectionImporter ReflectionImporter {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public XmlSchemaExporter SchemaExporter {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public XmlSchemas Schemas {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public Service Service {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public ServiceDescription ServiceDescription {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public ServiceDescriptionCollection ServiceDescriptions {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public Type ServiceType {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public string ServiceUrl {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
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

		[MonoTODO]
		public ServiceDescription GetServiceDescription (string ns)
		{
			throw new NotImplementedException ();
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
