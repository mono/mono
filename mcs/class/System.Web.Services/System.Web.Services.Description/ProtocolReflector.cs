// 
// System.Web.Services.Description.ProtocolReflector.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Collections;

namespace System.Web.Services.Description {
	public abstract class ProtocolReflector {

		#region Fields

		Binding binding;
		string defaultNamespace;
		MessageCollection headerMessages;
		Message inputMessage;
		LogicalMethodInfo[] methods;
		Operation operation;
		OperationBinding operationBinding;
		Message outputMessage;		
		Port port;
		PortType portType;
		string protocolName;
		XmlSchemaExporter schemaExporter;
		Service service;
		ServiceDescription serviceDescription;
		Type serviceType;
		string serviceUrl;
		SoapSchemaExporter soapSchemaExporter;
		MethodStubInfo methodStubInfo;
		TypeStubInfo typeInfo;
		ArrayList extensionReflectors;
		ServiceDescriptionReflector serviceReflector;

		XmlReflectionImporter reflectionImporter;
		SoapReflectionImporter soapReflectionImporter;
		
		CodeIdentifiers portNames;
		
		#endregion // Fields

		#region Constructors
	
		protected ProtocolReflector ()
		{
			defaultNamespace = WebServiceAttribute.DefaultNamespace;
			extensionReflectors = ExtensionManager.BuildExtensionReflectors ();
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
			get { return headerMessages; }	// TODO: set
		}

		public Message InputMessage {
			get { return inputMessage; }
		}

		public LogicalMethodInfo Method {
			get { return methodStubInfo.MethodInfo; }
		}

		public WebMethodAttribute MethodAttribute {
			get { return methodStubInfo.MethodAttribute; }
		}

		public LogicalMethodInfo[] Methods {
			get { return typeInfo.LogicalType.LogicalMethods; }
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

		public XmlReflectionImporter ReflectionImporter 
		{
			get
			{
				if (reflectionImporter == null) {
					reflectionImporter = typeInfo.XmlImporter;
					if (reflectionImporter == null)
						reflectionImporter = new XmlReflectionImporter();
				}
				return reflectionImporter;
			}
		}

		internal SoapReflectionImporter SoapReflectionImporter 
		{
			get
			{
				if (soapReflectionImporter == null) {
					soapReflectionImporter = typeInfo.SoapImporter;
					if (soapReflectionImporter == null)
						soapReflectionImporter = new SoapReflectionImporter();
				}
				return soapReflectionImporter;
			}
		}

		public XmlSchemaExporter SchemaExporter {
			get { return schemaExporter; }
		}

		public SoapSchemaExporter SoapSchemaExporter {
			get { return soapSchemaExporter; }
		}

		public XmlSchemas Schemas {
			get { return serviceReflector.Schemas; }
		}

		public Service Service {
			get { return service; }
		}

		public ServiceDescription ServiceDescription {
			get { return serviceDescription; }
		}

		public ServiceDescriptionCollection ServiceDescriptions {
			get { return serviceReflector.ServiceDescriptions; }
		}

		public Type ServiceType {
			get { return serviceType; }
		}

		public string ServiceUrl {
			get { return serviceUrl; }
		}
		
		internal MethodStubInfo MethodStubInfo {
			get { return methodStubInfo; }
		}
		
		internal TypeStubInfo TypeInfo {
			get { return typeInfo; }
		}


		#endregion // Properties

		#region Methods
		
		internal void Reflect (ServiceDescriptionReflector serviceReflector, Type type, string url, XmlSchemaExporter xxporter, SoapSchemaExporter sxporter)
		{
			portNames = new CodeIdentifiers ();
			this.serviceReflector = serviceReflector;
			serviceUrl = url;
			serviceType = type;
			
			schemaExporter = xxporter;
			soapSchemaExporter = sxporter;
			
			typeInfo = TypeStubManager.GetTypeStub (type, ProtocolName);
			
			ServiceDescription desc = ServiceDescriptions [typeInfo.LogicalType.WebServiceNamespace];
			
			if (desc == null)
			{
				desc = new ServiceDescription ();
				desc.TargetNamespace = typeInfo.LogicalType.WebServiceNamespace;
				desc.Name = typeInfo.LogicalType.WebServiceName;
				ServiceDescriptions.Add (desc);
			}
			
			ImportService (desc, typeInfo, url);			
		}

		void ImportService (ServiceDescription desc, TypeStubInfo typeInfo, string url)
		{
			service = desc.Services [typeInfo.LogicalType.WebServiceName];
			if (service == null)
			{
				service = new Service ();
				service.Name = typeInfo.LogicalType.WebServiceName;
				service.Documentation = typeInfo.LogicalType.Description;
				desc.Services.Add (service);
			}
			
			foreach (BindingInfo binfo in typeInfo.Bindings)
				ImportBinding (desc, service, typeInfo, url, binfo);
		}
		
		void ImportBinding (ServiceDescription desc, Service service, TypeStubInfo typeInfo, string url, BindingInfo binfo)
		{
			port = new Port ();
			port.Name = portNames.AddUnique (binfo.Name, port);
			port.Binding = new XmlQualifiedName (binfo.Name, binfo.Namespace);
			service.Ports.Add (port);

			if (binfo.Namespace != desc.TargetNamespace)
			{
				if (binfo.Location == null || binfo.Location == string.Empty)
				{
					ServiceDescription newDesc = new ServiceDescription();
					newDesc.TargetNamespace = binfo.Namespace;
					newDesc.Name = binfo.Name;
					int id = ServiceDescriptions.Add (newDesc);
					AddImport (desc, binfo.Namespace, GetWsdlUrl (url,id));
					ImportBindingContent (newDesc, typeInfo, url, binfo);
				}
				else
					AddImport (desc, binfo.Namespace, binfo.Location);
			}
			else
				ImportBindingContent (desc, typeInfo, url, binfo);
		}

		void ImportBindingContent (ServiceDescription desc, TypeStubInfo typeInfo, string url, BindingInfo binfo)
		{
			serviceDescription = desc;
			
			binding = new Binding ();
			binding.Name = binfo.Name;
			binding.Type = new XmlQualifiedName (binfo.Name, binfo.Namespace);
			desc.Bindings.Add (binding);
			
			portType = new PortType ();
			portType.Name = binding.Name;
			desc.PortTypes.Add (portType);

			BeginClass ();
			
			foreach (MethodStubInfo method in typeInfo.Methods)
			{
				methodStubInfo = method;
				
				string metBinding = ReflectMethodBinding ();
				if (metBinding != null && (metBinding != binding.Name)) continue;
				
				operation = new Operation ();
				operation.Name = method.OperationName;
				operation.Documentation = method.MethodAttribute.Description;
				
				inputMessage = new Message ();
				inputMessage.Name = method.Name + ProtocolName + "In";
				ServiceDescription.Messages.Add (inputMessage);
				
				outputMessage = new Message ();
				outputMessage.Name = method.Name + ProtocolName + "Out";
				ServiceDescription.Messages.Add (outputMessage);

				OperationInput inOp = new OperationInput ();
				if (method.Name != method.OperationName) inOp.Name = method.Name;
				Operation.Messages.Add (inOp);
				inOp.Message = new XmlQualifiedName (inputMessage.Name, ServiceDescription.TargetNamespace);
				
				OperationOutput outOp = new OperationOutput ();
				if (method.Name != method.OperationName) outOp.Name = method.Name;
				Operation.Messages.Add (outOp);
				outOp.Message = new XmlQualifiedName (outputMessage.Name, ServiceDescription.TargetNamespace);
			
				portType.Operations.Add (operation);
				ImportOperationBinding ();
				
				ReflectMethod ();
				
				foreach (SoapExtensionReflector reflector in extensionReflectors)
				{
					reflector.ReflectionContext = this;
					reflector.ReflectMethod ();
				}
			}

			EndClass ();
		}

		void ImportOperationBinding ()
		{
			operationBinding = new OperationBinding ();
			operationBinding.Name = methodStubInfo.OperationName;
			
			InputBinding inOp = new InputBinding ();
			operationBinding.Input = inOp;
			
			OutputBinding outOp = new OutputBinding ();
			operationBinding.Output = outOp;
			
			if (methodStubInfo.OperationName != methodStubInfo.Name)
				inOp.Name = outOp.Name = methodStubInfo.Name;
			
			binding.Operations.Add (operationBinding);
		}
		
		internal static void AddImport (ServiceDescription desc, string ns, string location)
		{
			Import im = new Import();
			im.Namespace = ns;
			im.Location = location;
			desc.Imports.Add (im);
		}
		
		string GetWsdlUrl (string baseUrl, int id)
		{
			return baseUrl + "?wsdl=" + id;
		}
		
		protected virtual void BeginClass ()
		{
		}

		protected virtual void EndClass ()
		{
		}

		public ServiceDescription GetServiceDescription (string ns)
		{
			return ServiceDescriptions [ns];
		}

		protected abstract bool ReflectMethod ();

		protected virtual string ReflectMethodBinding ()
		{
			return null;
		}

		#endregion
	}
}
