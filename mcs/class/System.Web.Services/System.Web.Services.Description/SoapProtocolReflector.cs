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
using System.Xml;

namespace System.Web.Services.Description {

	internal class SoapProtocolReflector : ProtocolReflector 
	{
		#region Fields

		const string EncodingNamespace = "http://schemas.xmlsoap.org/soap/encoding/";
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
			sb.Style = TypeInfo.SoapBindingStyle;
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
			sob.SoapAction = MethodStubInfo.Action;
			sob.Style = MethodStubInfo.SoapBindingStyle;
			OperationBinding.Extensions.Add (sob);
			
			AddOperationMsgBindings (OperationBinding.Input);
			AddOperationMsgBindings (OperationBinding.Output);

			foreach (HeaderInfo hf in MethodStubInfo.Headers)
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
				hb.Use = MethodStubInfo.Use;
				
				if (MethodStubInfo.Use == SoapBindingUse.Literal)
				{
					XmlTypeMapping mapping = ReflectionImporter.ImportTypeMapping (hf.HeaderType, ServiceDescription.TargetNamespace);
					part.Element = new XmlQualifiedName (mapping.ElementName, mapping.Namespace);
					SchemaExporter.ExportTypeMapping (mapping);
				}
				else
				{
					XmlTypeMapping mapping = SoapReflectionImporter.ImportTypeMapping (hf.HeaderType, ServiceDescription.TargetNamespace);
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

		void AddOperationMsgBindings (MessageBinding msg)
		{
			SoapBodyBinding sbbo = new SoapBodyBinding();
			msg.Extensions.Add (sbbo);
			sbbo.Use = MethodStubInfo.Use;
			if (MethodStubInfo.Use == SoapBindingUse.Encoded)
			{
				sbbo.Namespace = ServiceDescription.TargetNamespace;
				sbbo.Encoding = EncodingNamespace;
			}
		}
		
		protected override string ReflectMethodBinding ()
		{
			return MethodStubInfo.Binding;
		}

		#endregion
	}
}
