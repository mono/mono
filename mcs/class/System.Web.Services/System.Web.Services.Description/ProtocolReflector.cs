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
		ServiceDescriptionCollection serviceDescriptions;
		Type serviceType;
		string serviceUrl;
		SoapSchemaExporter soapSchemaExporter;
		Types types;
		MethodStubInfo methodStubInfo;
		TypeStubInfo typeInfo;
		ArrayList extensionReflectors;

		#endregion // Fields

		#region Constructors
	
		protected ProtocolReflector ()
		{
			defaultNamespace = WebServiceAttribute.DefaultNamespace;
			extensionReflectors = ExtensionManager.BuildExtensionReflectors ();
			types = new Types ();
			serviceDescriptions = new ServiceDescriptionCollection ();
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
			get { return typeInfo.XmlImporter;; }
		}

		internal SoapReflectionImporter SoapReflectionImporter {
			get { return typeInfo.SoapImporter;; }
		}

		public XmlSchemaExporter SchemaExporter {
			get { return schemaExporter; }
		}

		public SoapSchemaExporter SoapSchemaExporter {
			get { return soapSchemaExporter; }
		}

		public XmlSchemas Schemas {
			get { return types.Schemas; }
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
		
		internal MethodStubInfo MethodStubInfo {
			get { return methodStubInfo; }
		}
		
		internal TypeStubInfo TypeInfo {
			get { return typeInfo; }
		}


		#endregion // Properties

		#region Methods
		
		internal void Reflect (Type type, string url)
		{
			serviceUrl = url;
			serviceType = type;
			
			schemaExporter = new XmlSchemaExporter (types.Schemas);
			soapSchemaExporter = new SoapSchemaExporter (types.Schemas);
			
			typeInfo = TypeStubManager.GetTypeStub (type);
			
			ServiceDescription desc = new ServiceDescription ();
			desc.TargetNamespace = typeInfo.WebServiceNamespace;
			desc.Name = typeInfo.WebServiceName;
			serviceDescriptions.Add (desc);
			
			methods = new LogicalMethodInfo[typeInfo.Methods.Count];
			for (int n=0; n<typeInfo.Methods.Count; n++)
				methods [n] = ((MethodStubInfo) typeInfo.Methods [n]).MethodInfo;
			
			ImportService (desc, typeInfo, url);
			
			if (serviceDescriptions.Count == 1)
				desc.Types = types;
			else
			{
				foreach (ServiceDescription d in serviceDescriptions)
				{
					d.Types = new Types();
					for (int n=0; n<types.Schemas.Count; n++)
						AddImport (d, types.Schemas[n].TargetNamespace, GetSchemaUrl (url, n));
				}
			}
		}

		void ImportService (ServiceDescription desc, TypeStubInfo typeInfo, string url)
		{
			service = new Service ();
			service.Name = typeInfo.WebServiceName;
			service.Documentation = typeInfo.Description;

			desc.Services.Add (service);
			
			foreach (BindingInfo binfo in typeInfo.Bindings)
				ImportBinding (desc, service, typeInfo, url, binfo);
		}
		
		void ImportBinding (ServiceDescription desc, Service service, TypeStubInfo typeInfo, string url, BindingInfo binfo)
		{
			port = new Port ();
			port.Name = binfo.Name;
			port.Binding = new XmlQualifiedName (binfo.Name, binfo.Namespace);
			service.Ports.Add (port);

			if (binfo.Namespace != desc.TargetNamespace)
			{
				if (binfo.Location == null || binfo.Location == string.Empty)
				{
					ServiceDescription newDesc = new ServiceDescription();
					newDesc.TargetNamespace = binfo.Namespace;
					int id = serviceDescriptions.Add (newDesc);
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
				operation.Name = method.Name;
				operation.Documentation = method.Description;
				
				OperationInput inOp = new OperationInput ();
				inOp.Message = ImportMessage (operation.Name + "SoapIn", method.InputMembersMapping, method, out inputMessage);
				operation.Messages.Add (inOp);
				
				OperationOutput outOp = new OperationOutput ();
				outOp.Message = ImportMessage (operation.Name + "SoapOut", method.OutputMembersMapping, method, out outputMessage);
				operation.Messages.Add (outOp);

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
			operationBinding.Name = methodStubInfo.Name;
			
			InputBinding inOp = new InputBinding ();
			operationBinding.Input = inOp;
			
			OutputBinding outOp = new OutputBinding ();
			operationBinding.Output = outOp;
			
			binding.Operations.Add (operationBinding);
		}
		
		XmlQualifiedName ImportMessage (string name, XmlMembersMapping members, MethodStubInfo method, out Message msg)
		{
			msg = new Message ();
			msg.Name = name;
			
			if (method.ParameterStyle == SoapParameterStyle.Wrapped)
			{
				MessagePart part = new MessagePart ();
				part.Name = "parameters";
				XmlQualifiedName qname = new XmlQualifiedName (members.ElementName, members.Namespace);
				if (method.Use == SoapBindingUse.Literal) part.Element = qname;
				else part.Type = qname;
				msg.Parts.Add (part);
			}
			else
			{
				for (int n=0; n<members.Count; n++)
				{
					MessagePart part = new MessagePart ();
					part.Name = members[n].MemberName;
					
					if (method.Use == SoapBindingUse.Literal) {
						part.Element = new XmlQualifiedName (members[n].MemberName, members[n].Namespace);
					}
					else {
						string namesp = members[n].TypeNamespace;
						if (namesp == "") namesp = XmlSchema.Namespace;
						part.Type = new XmlQualifiedName (members[n].TypeName, namesp);
					}
					msg.Parts.Add (part);
				}
			}
			
			serviceDescription.Messages.Add (msg);
			
			if (method.Use == SoapBindingUse.Literal)
				schemaExporter.ExportMembersMapping (members);
			else
				soapSchemaExporter.ExportMembersMapping (members);
			
			return new XmlQualifiedName (name, members.Namespace);
		}

		void AddImport (ServiceDescription desc, string ns, string location)
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
		
		string GetSchemaUrl (string baseUrl, int id)
		{
			return baseUrl + "?schema=" + id;
		}
		
		protected virtual void BeginClass ()
		{
		}

		protected virtual void EndClass ()
		{
		}

		public ServiceDescription GetServiceDescription (string ns)
		{
			return serviceDescriptions [ns];
		}

		protected abstract bool ReflectMethod ();

		protected virtual string ReflectMethodBinding ()
		{
			return null;
		}

		#endregion
	}
}
