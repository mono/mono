// 
// System.Web.Services.Description.ServiceDescriptionReflector.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;
using System.Xml.Serialization;
using System.Xml;
using System.Web.Services.Protocols;

namespace System.Web.Services.Description {
	public class ServiceDescriptionReflector {

		#region Fields

		Types types;
		ServiceDescriptionCollection serviceDescriptions;
		XmlSchemaExporter schemaExporter;

		#endregion // Fields

		#region Constructors
	
		public ServiceDescriptionReflector ()
		{
			types = new Types ();
			serviceDescriptions = new ServiceDescriptionCollection ();
			schemaExporter = new XmlSchemaExporter (types.Schemas);
		}
		
		#endregion // Constructors

		#region Properties

		public XmlSchemas Schemas {
			get { return types.Schemas; }
		}

		public ServiceDescriptionCollection ServiceDescriptions {
			get { return serviceDescriptions; }
		}

	
		#endregion // Properties

		#region Methods

		[MonoTODO]
		public void Reflect (Type type, string url)
		{
			ServiceDescription desc = new ServiceDescription ();
			serviceDescriptions.Add (desc);
			
			TypeStubInfo typeInfo = TypeStubManager.GetTypeStub (type);
			desc.TargetNamespace = typeInfo.WebServiceNamespace;
			desc.Name = typeInfo.WebServiceName;
			
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
		
		Service ImportService (ServiceDescription desc, TypeStubInfo typeInfo, string url)
		{
			Service service = new Service ();
//			service.Documentation = wsa.Description;
			service.Name = typeInfo.WebServiceName;

			desc.Services.Add (service);
			
			foreach (BindingInfo binfo in typeInfo.Bindings)
				ImportBinding (desc, service, typeInfo, url, binfo);

			return service;
		}
		
		void ImportBinding (ServiceDescription desc, Service service, TypeStubInfo typeInfo, string url, BindingInfo binfo)
		{
			Port port = new Port ();
			port.Name = binfo.Name;
			port.Binding = new XmlQualifiedName (binfo.Name, binfo.Namespace);
			service.Ports.Add (port);

			SoapAddressBinding abind = new SoapAddressBinding ();
			abind.Location = url;
			port.Extensions.Add (abind);
			
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
			Binding binding = new Binding ();
			binding.Name = binfo.Name;
			binding.Type = new XmlQualifiedName (binfo.Name, binfo.Namespace);
			desc.Bindings.Add (binding);
			
			SoapBinding sb = new SoapBinding ();
			sb.Transport = SoapBinding.HttpTransport;
			sb.Style = typeInfo.SoapBindingStyle;
			binding.Extensions.Add (sb);
			
			PortType ptype = new PortType ();
			ptype.Name = binding.Name;
			desc.PortTypes.Add (ptype);

			foreach (MethodStubInfo method in typeInfo.Methods)
			{
				if (method.Binding != binding.Name) continue;
				
				Operation oper = ImportOperation (desc, method);
				ptype.Operations.Add (oper);

				OperationBinding operb = ImportOperationBinding (desc, method);
				binding.Operations.Add (operb);
			}
		}
		
		Operation ImportOperation (ServiceDescription desc, MethodStubInfo method)
		{
			Operation oper = new Operation ();
			oper.Name = method.Name;
			
			OperationInput inOp = new OperationInput ();
			inOp.Message = ImportMessage (desc, oper.Name + "In", method.InputMembersMapping);
			oper.Messages.Add (inOp);
			
			OperationOutput outOp = new OperationOutput ();
			outOp.Message = ImportMessage (desc, oper.Name + "Out", method.OutputMembersMapping);
			oper.Messages.Add (outOp);
			
			return oper;
		}
		
		OperationBinding ImportOperationBinding (ServiceDescription desc, MethodStubInfo method)
		{
			OperationBinding oper = new OperationBinding ();
			oper.Name = method.Name;
			
			SoapOperationBinding sob = new SoapOperationBinding();
			sob.SoapAction = method.Action;
			sob.Style = method.SoapBindingStyle;
			oper.Extensions.Add (sob);
			
			InputBinding inOp = new InputBinding ();
			SoapBodyBinding sbbi = new SoapBodyBinding();
			sbbi.Use = method.Use;
			inOp.Extensions.Add (sbbi);
			oper.Input = inOp;
			
			OutputBinding outOp = new OutputBinding ();
			SoapBodyBinding sbbo = new SoapBodyBinding();
			sbbo.Use = method.Use;
			outOp.Extensions.Add (sbbo);
			oper.Output = outOp;
			
			return oper;
		}
		
		XmlQualifiedName ImportMessage (ServiceDescription desc, string name, XmlMembersMapping members)
		{
			Message msg = new Message ();
			msg.Name = name;
			
			MessagePart part = new MessagePart ();
			part.Name = "parameters";
			part.Element = new XmlQualifiedName (members.ElementName, members.Namespace);
			msg.Parts.Add (part);
			
			desc.Messages.Add (msg);
			schemaExporter.ExportMembersMapping (members);
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
		
		#endregion
	}
}
