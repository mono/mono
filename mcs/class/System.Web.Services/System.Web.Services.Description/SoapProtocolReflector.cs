// 
// System.Web.Services.Description.SoapProtocolReflector.cs
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
using System.Xml.Schema;
using System.Xml;

namespace System.Web.Services.Description {

	internal class SoapProtocolReflector : ProtocolReflector 
	{
		#region Fields

		internal const string EncodingNamespace = "http://schemas.xmlsoap.org/soap/encoding/";
		SoapBinding soapBinding;

		#endregion // Fields

		#region Constructors

		public SoapProtocolReflector ()
		{
		}
		
		#endregion // Constructors

		#region Properties

		public override string ProtocolName {
			get { return "Soap"; }
		}

		#endregion // Properties

		#region Methods

		protected override void BeginClass ()
		{
			SoapBinding sb = new SoapBinding ();
			sb.Transport = SoapBinding.HttpTransport;
			sb.Style = ((SoapTypeStubInfo)TypeInfo).SoapBindingStyle;
			Binding.Extensions.Add (sb);

			SoapAddressBinding abind = new SoapAddressBinding ();
			abind.Location = ServiceUrl;
			Port.Extensions.Add (abind);
		}

		protected override void EndClass ()
		{
		}

		protected override bool ReflectMethod ()
		{
			SoapOperationBinding sob = new SoapOperationBinding();
			SoapMethodStubInfo method = (SoapMethodStubInfo) MethodStubInfo;
			
			sob.SoapAction = method.Action;
			sob.Style = method.SoapBindingStyle;
			OperationBinding.Extensions.Add (sob);
			
			ImportMessage (method.InputMembersMapping, InputMessage);
			ImportMessage (method.OutputMembersMapping, OutputMessage);
				
			AddOperationMsgBindings (method, OperationBinding.Input);
			AddOperationMsgBindings (method, OperationBinding.Output);

			foreach (HeaderInfo hf in method.Headers)
			{
				Message msg = new Message ();
				msg.Name = Operation.Name + hf.HeaderType.Name;
				MessagePart part = new MessagePart ();
				part.Name = hf.HeaderType.Name;
				msg.Parts.Add (part);
				ServiceDescription.Messages.Add (msg);

				SoapHeaderBinding hb = new SoapHeaderBinding ();
				hb.Message = new XmlQualifiedName (msg.Name, ServiceDescription.TargetNamespace);
				hb.Part = part.Name;
				hb.Use = method.Use;
				
				if (method.Use == SoapBindingUse.Literal)
				{
					// MS.NET reflects header classes in a weird way. The root element
					// name is the CLR class name unless it is specified in an XmlRootAttribute.
					// The usual is to use the xml type name by default, but not in this case.
				
					XmlRootAttribute root;
					XmlAttributes ats = new XmlAttributes (hf.HeaderType);
					if (ats.XmlRoot != null) root = ats.XmlRoot;
					else root = new XmlRootAttribute (hf.HeaderType.Name);
					
					if (root.Namespace == null) root.Namespace = TypeInfo.LogicalType.GetWebServiceLiteralNamespace (ServiceDescription.TargetNamespace);
					if (root.ElementName == null) root.ElementName = hf.HeaderType.Name;
					
					XmlTypeMapping mapping = ReflectionImporter.ImportTypeMapping (hf.HeaderType, root);
					part.Element = new XmlQualifiedName (mapping.ElementName, mapping.Namespace);
					SchemaExporter.ExportTypeMapping (mapping);
				}
				else
				{
					XmlTypeMapping mapping = SoapReflectionImporter.ImportTypeMapping (hf.HeaderType, TypeInfo.LogicalType.GetWebServiceEncodedNamespace (ServiceDescription.TargetNamespace));
					part.Type = new XmlQualifiedName (mapping.ElementName, mapping.Namespace);
					SoapSchemaExporter.ExportTypeMapping (mapping);
					hb.Encoding = EncodingNamespace;
				}

				if ((hf.Direction & SoapHeaderDirection.Out) != 0)
					OperationBinding.Output.Extensions.Add (hb);
				if ((hf.Direction & SoapHeaderDirection.In) != 0)
					OperationBinding.Input.Extensions.Add (hb);
			}
			
			return true;
		}

		void AddOperationMsgBindings (SoapMethodStubInfo method, MessageBinding msg)
		{
			SoapBodyBinding sbbo = new SoapBodyBinding();
			msg.Extensions.Add (sbbo);
			sbbo.Use = method.Use;
			if (method.Use == SoapBindingUse.Encoded)
			{
				sbbo.Namespace = ServiceDescription.TargetNamespace;
				sbbo.Encoding = EncodingNamespace;
			}
		}
		
		void ImportMessage (XmlMembersMapping members, Message msg)
		{
			SoapMethodStubInfo method = (SoapMethodStubInfo) MethodStubInfo;
			bool needsEnclosingElement = (method.ParameterStyle == SoapParameterStyle.Wrapped && 
											method.SoapBindingStyle == SoapBindingStyle.Document);

			if (needsEnclosingElement)
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
						if (namesp == "") namesp = members[n].Namespace;
						part.Type = new XmlQualifiedName (members[n].TypeName, namesp);
					}
					msg.Parts.Add (part);
				}
			}
			
			
			if (method.Use == SoapBindingUse.Literal)
				SchemaExporter.ExportMembersMapping (members);
			else
				SoapSchemaExporter.ExportMembersMapping (members, needsEnclosingElement);
		}

		protected override string ReflectMethodBinding ()
		{
			return ((SoapMethodStubInfo)MethodStubInfo).Binding;
		}

		#endregion
	}
}
