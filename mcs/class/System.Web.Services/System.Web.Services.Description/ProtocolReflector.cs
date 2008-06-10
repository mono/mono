// 
// System.Web.Services.Description.ProtocolReflector.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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

		internal ServiceDescriptionReflector Parent {
			get { return serviceReflector; }
		}

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

		internal SoapSchemaExporter SoapSchemaExporter {
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
			bool bindingFull = true;

			if (binfo.Namespace != desc.TargetNamespace)
			{
				if (binfo.Location == null || binfo.Location == string.Empty)
				{
					ServiceDescription newDesc = new ServiceDescription();
					newDesc.TargetNamespace = binfo.Namespace;
					newDesc.Name = binfo.Name;
					bindingFull = ImportBindingContent (newDesc, typeInfo, url, binfo);
					if (bindingFull) {
						int id = ServiceDescriptions.Add (newDesc);
						AddImport (desc, binfo.Namespace, GetWsdlUrl (url,id));
					}
				}
				else {
					AddImport (desc, binfo.Namespace, binfo.Location);
					bindingFull = true;
				}
			}
			else
				bindingFull = ImportBindingContent (desc, typeInfo, url, binfo);
				
			if (bindingFull)
			{
				port.Binding = new XmlQualifiedName (binding.Name, binfo.Namespace);
				
				int n = 0;
				string name = binfo.Name; 
				bool found;
				do {

					found = false;
					foreach (Port p in service.Ports)
						if (p.Name == name) { found = true; n++; name = binfo.Name + n; break; }
				}
				while (found);
				port.Name = name;
				service.Ports.Add (port);
			}

#if NET_2_0
			if (binfo.WebServiceBindingAttribute != null && binfo.WebServiceBindingAttribute.ConformsTo != WsiProfiles.None && String.IsNullOrEmpty (binfo.WebServiceBindingAttribute.Name)) {
				BasicProfileViolationCollection violations = new BasicProfileViolationCollection ();
				desc.Types.Schemas.Add (Schemas);
				ServiceDescriptionCollection col = new ServiceDescriptionCollection ();
				col.Add (desc);
				ConformanceCheckContext ctx = new ConformanceCheckContext (col, violations);
				ctx.ServiceDescription = desc;
				ConformanceChecker[] checkers = WebServicesInteroperability.GetCheckers (binfo.WebServiceBindingAttribute.ConformsTo);
				foreach (ConformanceChecker checker in checkers) {
					ctx.Checker = checker;
					WebServicesInteroperability.Check (ctx, checker, binding);
					if (violations.Count > 0)
						throw new InvalidOperationException (violations [0].ToString ());
				}
			}
#endif	
		}

		bool ImportBindingContent (ServiceDescription desc, TypeStubInfo typeInfo, string url, BindingInfo binfo)
		{
			serviceDescription = desc;
			
			// Look for an unused name
			
			int n=0;
			string name = binfo.Name;
			bool found;
			do
			{
				found = false;
				foreach (Binding bi in desc.Bindings)
					if (bi.Name == name) { found = true; n++; name = binfo.Name+n; break; }
			}
			while (found);
			
			// Create the binding
			
			binding = new Binding ();
			binding.Name = name;
			binding.Type = new XmlQualifiedName (binding.Name, binfo.Namespace);
#if NET_2_0
			if (binfo.WebServiceBindingAttribute != null && binfo.WebServiceBindingAttribute.EmitConformanceClaims) {
				XmlDocument doc = new XmlDocument ();
				XmlElement docElement = doc.CreateElement ("wsdl", "documentation", "http://schemas.xmlsoap.org/wsdl/");
				XmlElement claimsElement = doc.CreateElement ("wsi", "Claim", "http://ws-i.org/schemas/conformanceClaim/");
				claimsElement.Attributes.Append (doc.CreateAttribute ("conformsTo")).Value = "http://ws-i.org/profiles/basic/1.1";
				docElement.AppendChild (claimsElement);
				binding.DocumentationElement = docElement;
			}
#endif
			
			portType = new PortType ();
			portType.Name = binding.Name;

			BeginClass ();

			foreach (SoapExtensionReflector reflector in extensionReflectors)
			{
				reflector.ReflectionContext = this;
				reflector.ReflectDescription ();
			}

			foreach (MethodStubInfo method in typeInfo.Methods)
			{
				methodStubInfo = method;
				
				string metBinding = ReflectMethodBinding ();
				if (typeInfo.GetBinding (metBinding) != binfo) continue;
				
				operation = new Operation ();
				operation.Name = method.OperationName;
				operation.Documentation = method.MethodAttribute.Description;

				// FIXME: SOAP 1.1 and SOAP 1.2 should share
				// the same message definitions.

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
				
				if (!ReflectMethod ()) {
#if NET_2_0
					// (It is somewhat hacky) If we don't
					// add input/output Messages, update
					// portType/input/@message and
					// porttype/output/@message.
					Message dupIn = Parent.MappedMessagesIn [method.MethodInfo];
					ServiceDescription.Messages.Remove (inputMessage);
					inOp.Message = new XmlQualifiedName (dupIn.Name, ServiceDescription.TargetNamespace);
					Message dupOut = Parent.MappedMessagesOut [method.MethodInfo];
					ServiceDescription.Messages.Remove (outputMessage);
					outOp.Message = new XmlQualifiedName (dupOut.Name, ServiceDescription.TargetNamespace);
#endif
				}

				foreach (SoapExtensionReflector reflector in extensionReflectors)
				{
					reflector.ReflectionContext = this;
					reflector.ReflectMethod ();
				}
			}
			
			EndClass ();
			
			if (portType.Operations.Count > 0)
			{
				desc.Bindings.Add (binding);
				desc.PortTypes.Add (portType);
				return true;
			}
			else
				return false;
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

#if NET_2_0
		[MonoNotSupported("Not Implemented")]
		protected virtual void ReflectDescription () 
		{
			throw new NotImplementedException ();
		}
#endif

		#endregion
	}
}
