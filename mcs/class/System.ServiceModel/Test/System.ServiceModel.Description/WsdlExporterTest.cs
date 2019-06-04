//
// WsdlExporterTest.cs
//
// Author:
//	Ankit Jain <JAnkit@novell.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using NUnit.Framework;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Web.Services;

using WSServiceDescription = System.Web.Services.Description.ServiceDescription;
using WSMessage = System.Web.Services.Description.Message;
using WSBinding = System.Web.Services.Description.Binding;
using QName = System.Xml.XmlQualifiedName;

using SMMessage = System.ServiceModel.Channels.Message;

using System.Xml;
using System.Web.Services.Description;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Reflection;

namespace MonoTests.System.ServiceModel.Description
{
	[TestFixture]
	public class WsdlExporterTest
	{
		[Test]
		[Category ("NotWorking")]
		public void Ctor1 ()
		{
			WsdlExporter we = new WsdlExporter ();

			Assert.IsNotNull (we.GetGeneratedMetadata ());
			Assert.IsNotNull (we.GeneratedWsdlDocuments, "#c1");
			Assert.AreEqual (0, we.GeneratedWsdlDocuments.Count, "#c2");

			Assert.IsNotNull (we.GeneratedXmlSchemas, "#c3");
			Assert.AreEqual (0, we.GeneratedXmlSchemas.Count, "#c4");
		}

		[Test]
		public void ExportEndpointTest ()
		{
			WsdlExporter we = new WsdlExporter ();

			ServiceEndpoint se = new ServiceEndpoint (ContractDescription.GetContract (typeof (IEchoService)));
			se.Binding = new BasicHttpBinding ();
			se.Address = new EndpointAddress ("http://localhost:8080");
			//TEST Invalid name: 5se.Name = "Service#1";
			//se.Name = "Service0";
			//se.ListenUri = new Uri ("http://localhost:8080/svc");

			we.ExportEndpoint (se);

			MetadataSet ms = we.GetGeneratedMetadata ();
			Assert.AreEqual (6, ms.MetadataSections.Count);
			CheckContract_IEchoService (ms, "#eet01");

			WSServiceDescription sd = GetServiceDescription (ms, "http://tempuri.org/", "ExportEndpointTest");
			CheckServicePort (GetService (sd, "service", "ExportEndpointTest"),
				"BasicHttpBinding_IEchoService", new XmlQualifiedName ("BasicHttpBinding_IEchoService", "http://tempuri.org/"),
				"http://localhost:8080/", "#eet02");

			CheckBasicHttpBinding (sd, "BasicHttpBinding_IEchoService", new XmlQualifiedName ("IEchoService", "http://myns/echo"),
				"Echo", "http://myns/echo/IEchoService/Echo", true, true, "#eet03");
		}

		[Test]
		public void ExportEndpointTest2 ()
		{
			WsdlExporter we = new WsdlExporter ();

			ServiceEndpoint se = new ServiceEndpoint (ContractDescription.GetContract (typeof (IEchoService2)));
			se.Binding = new BasicHttpBinding ();
			se.Address = new EndpointAddress ("http://localhost:8080");
			we.ExportEndpoint (se);

			MetadataSet ms = we.GetGeneratedMetadata ();
			Assert.AreEqual (5, ms.MetadataSections.Count);

			WSServiceDescription sd = ms.MetadataSections [0].Metadata as WSServiceDescription;
			CheckContract_IEchoService2 (ms, "#eet20");
			CheckServicePort (GetService (GetServiceDescription (ms, "http://tempuri.org/", "#eet21"), "service", "ExportEndpointTest"),
				"BasicHttpBinding_ThisIsEchoService", new XmlQualifiedName ("BasicHttpBinding_ThisIsEchoService", "http://tempuri.org/"),
				"http://localhost:8080/", "#eet22");

			CheckBasicHttpBinding (sd, "BasicHttpBinding_ThisIsEchoService",
				new XmlQualifiedName ("ThisIsEchoService", "http://tempuri.org/"),
				"Echo", "http://tempuri.org/ThisIsEchoService/Echo", true, true, "#eet03");

			//FIXME: CheckXmlSchema
		}

		[Test]
		public void ExportEndpointTest3 ()
		{
			WsdlExporter we = new WsdlExporter ();
			/*ContractDescription contract =*/ ContractDescription.GetContract (typeof (IEchoService2));

			ServiceEndpoint se = new ServiceEndpoint (ContractDescription.GetContract (typeof (IEchoService2)));
			se.Binding = new BasicHttpBinding ();
			se.Address = new EndpointAddress ("http://localhost:8080");
			we.ExportEndpoint (se);

			se = new ServiceEndpoint (ContractDescription.GetContract (typeof (IEchoService)));
			se.Binding = new BasicHttpBinding ();
			se.Address = new EndpointAddress ("http://somehost");
			we.ExportEndpoint (se);

			MetadataSet ms = we.GetGeneratedMetadata ();
			Assert.AreEqual (7, ms.MetadataSections.Count);

			Service svc = GetService (
					GetServiceDescription (ms, "http://tempuri.org/", "ExportEndpointTest"),
					"service", "ExportEndpointTest");

			CheckContract_IEchoService (ms, "#eet31");
			CheckServicePort (svc, "BasicHttpBinding_IEchoService",
				new XmlQualifiedName ("BasicHttpBinding_IEchoService", "http://tempuri.org/"),
				"http://somehost/", "#eet32");

			CheckContract_IEchoService2 (ms, "#eet33");
			CheckServicePort (svc, "BasicHttpBinding_ThisIsEchoService",
				new XmlQualifiedName ("BasicHttpBinding_ThisIsEchoService", "http://tempuri.org/"),
				"http://localhost:8080/", "#eet34");


			WSServiceDescription sd = ms.MetadataSections [0].Metadata as WSServiceDescription;
			CheckBasicHttpBinding (sd, "BasicHttpBinding_IEchoService", new XmlQualifiedName ("IEchoService", "http://myns/echo"),
				"Echo", "http://myns/echo/IEchoService/Echo", true, true, "#eet35");

			CheckBasicHttpBinding (sd, "BasicHttpBinding_ThisIsEchoService", new XmlQualifiedName ("ThisIsEchoService", "http://tempuri.org/"),
				"Echo", "http://tempuri.org/ThisIsEchoService/Echo", true, true, "#eet36");


			//FIXME: CheckXmlSchema
		}

		[Test]
		public void ExportContractInvalid1 ()
		{
			WsdlExporter we = new WsdlExporter ();

			we.ExportContract (ContractDescription.GetContract (typeof (IEchoService2)));
		        //Duplicate contract QNames not allowed
			ExportContractExpectException (we, ContractDescription.GetContract (typeof (IEchoService2)),
				typeof (ArgumentException), "ExportContractInvalid1");
		}

		[Test]
		public void ExportContractInvalid2 ()
		{
			WsdlExporter we = new WsdlExporter ();

			we.ExportContract (ContractDescription.GetContract (typeof (IEchoService2)));
			//Invalid as IEchoService3.Echo is http://tempuri.org/Echo message which has already been exported
			//Even though, the service name is different
			ExportContractExpectException (we, ContractDescription.GetContract (typeof (IEchoService3)),
				typeof (InvalidOperationException), "ExportContractInvalid2");
		}

		[Test]
		public void ExportContract1 ()
		{
			WsdlExporter we = new WsdlExporter ();
			we.ExportContract (ContractDescription.GetContract (typeof (IEchoService)));

			MetadataSet ms = we.GetGeneratedMetadata ();
			Assert.AreEqual (5, ms.MetadataSections.Count);

			CheckContract_IEchoService (ms, "ExportContract1");
		}

		[Test]
		public void ExportContract2 ()
		{
			WsdlExporter we = new WsdlExporter ();
			we.ExportContract (ContractDescription.GetContract (typeof (IFoo1)));

			MetadataSet ms = we.GetGeneratedMetadata ();
			Assert.AreEqual (5, ms.MetadataSections.Count);
		}

		[Test]
		public void ExportContract2a ()
		{
			WsdlExporter we = new WsdlExporter ();
			we.ExportContract (ContractDescription.GetContract (typeof (IFoo1)));
			//IFoo1a.Op1 is the same operations as IFoo1.Op1, so cant be exported
			//the message element for both is the same
			//(Compared by names not signature)
			ExportContractExpectException (we, ContractDescription.GetContract (typeof (IFoo1a)),
				typeof (InvalidOperationException), "ExportContract2a");
		}

		[Test]
		[Category ("NotWorking")]
		// System.Xml.Schema.XmlSchemaException : XmlSchema error: Named item
		// http://schemas.datacontract.org/2004/07/System.Reflection:ArrayOfTypeInfo was already contained
		// in the schema object table. Consider setting MONO_STRICT_MS_COMPLIANT to 'yes' to mimic
		// MS implementation. Related schema item SourceUri: , Line 0, Position 0.
		public void ExportMessageContract ()
		{
			WsdlExporter we = new WsdlExporter ();
			ContractDescription cd = ContractDescription.GetContract (typeof (IFoo2));
			we.ExportContract (cd);
		}

		[Test]
		[Category ("NotWorking")]
		//FIXME: One check not working, BeginGetResult
		public void ExportMexContract ()
		{
			WsdlExporter we = new WsdlExporter ();
			ContractDescription cd = ContractDescription.GetContract (typeof (IMetadataExchange));
			we.ExportContract (cd);

			MetadataSet ms = we.GetGeneratedMetadata ();

			WSServiceDescription sd = GetServiceDescription (ms, "http://schemas.microsoft.com/2006/04/mex", "ExportMexContract");

			CheckMessage (sd, "IMetadataExchange_Get_InputMessage", "request", "http://schemas.microsoft.com/Message:MessageBody", true, "#exc0");
			CheckMessage (sd, "IMetadataExchange_Get_OutputMessage", "GetResult", "http://schemas.microsoft.com/Message:MessageBody", true, "#exc1");

			//PortType
			PortType port_type = sd.PortTypes ["IMetadataExchange"];
			Assert.IsNotNull (port_type, "#exc2, PortType named IMetadataExchange not found.");

			Assert.AreEqual (1, port_type.Operations.Count, "#exc3");
			Operation op = port_type.Operations [0];
			Assert.AreEqual ("Get", op.Name, "#exc4");

			Assert.AreEqual (2, op.Messages.Count, "#exc5");
			CheckOperationMessage (op.Messages [0], "http://schemas.microsoft.com/2006/04/mex:IMetadataExchange_Get_InputMessage", 
				typeof (OperationInput), "http://schemas.xmlsoap.org/ws/2004/09/transfer/Get");
			
			CheckOperationMessage (op.Messages [1], "http://schemas.microsoft.com/2006/04/mex:IMetadataExchange_Get_OutputMessage", 
				typeof (OperationOutput), "http://schemas.xmlsoap.org/ws/2004/09/transfer/GetResponse");

			CheckSpecialMessage (ms, "#exc6");

			Assert.AreEqual (1, we.GeneratedWsdlDocuments.Count, "GeneratedWsdlDocuments.Count");
			Assert.AreEqual (1, we.GeneratedXmlSchemas.Count, "GeneratedXmlSchemas.Count");
		}

		[Test]
		public void ExportBar1Contract ()
		{
			WsdlExporter we = new WsdlExporter ();
			ContractDescription cd = ContractDescription.GetContract (typeof (Bar1));
			we.ExportContract (cd);
		}

		//Helper methods

		//Checks the ComplexType emitted for CLR type Message
		void CheckSpecialMessage (MetadataSet ms, string label)
		{
			//Check ComplexType MessageBody
			XmlSchema xs = GetXmlSchema (ms, "http://schemas.microsoft.com/Message", label + " #csm0");
			foreach (XmlSchemaObject o in xs.SchemaTypes.Values) {
				XmlSchemaComplexType complex_type = o as XmlSchemaComplexType;
				if (complex_type == null)
					continue;
				if (complex_type.Name != "MessageBody")
					continue;

				//MessageBody
				Assert.IsNotNull (complex_type.Particle, label + " #cms1");
				Assert.AreEqual (typeof (XmlSchemaSequence), complex_type.Particle.GetType (), label + " #cms2");
				XmlSchemaSequence seq = (XmlSchemaSequence) complex_type.Particle;
			
				Assert.AreEqual (1, seq.Items.Count, label + " #cms3");
				Assert.AreEqual (typeof (XmlSchemaAny), seq.Items [0].GetType (), label + " #cms4");
				XmlSchemaAny any = (XmlSchemaAny) seq.Items [0];

				Assert.AreEqual ("##any", any.Namespace, label + " #cms5");
				Assert.AreEqual (0, any.MinOccurs, label + " #cms6");
				Assert.AreEqual ("unbounded", any.MaxOccursString, label + " #cms6");
			}
		}

		//somebody fix this name!
		void ExportContractExpectException (WsdlExporter we, ContractDescription cd, Type exception_type, string msg)
		{
			try {
				we.ExportContract (cd);
			} catch (Exception e) {
				if (e.GetType () == exception_type)
					return;
				Assert.Fail (String.Format ("[{0}] Expected {1}, but got : {2}", msg, exception_type, e));
			}

			Assert.Fail (String.Format ("[{0}] Expected {1}", msg, exception_type));
		}

		WSServiceDescription GetServiceDescription (MetadataSet ms, string ns, string msg)
		{
			foreach (MetadataSection section in ms.MetadataSections) {
				WSServiceDescription sd = section.Metadata as WSServiceDescription;
				if (sd == null)
					continue;
				if (sd.TargetNamespace == ns) {
					/*Assert.AreEqual ("http://schemas.xmlsoap.org/wsdl/", section.Dialect, msg + " Dialect");
					Assert.AreEqual (id, section.Identifier, msg + "Identifier");
					Assert.AreEqual (0, section.Attributes.Count, "#cw4");*/

					return sd;
				}
			}

			Assert.Fail (String.Format ("[{0}] ServiceDescription for ns : {1} not found.", msg, ns));
			return null;
		}

		XmlSchema GetXmlSchema (MetadataSet ms, string ns, string msg)
		{
			foreach (MetadataSection section in ms.MetadataSections) {
				XmlSchema xs = section.Metadata as XmlSchema;
				if (xs == null)
					continue;
				if (xs.TargetNamespace == ns) {
					/*Assert.AreEqual ("http://schemas.xmlsoap.org/wxsl/", section.Dialect, msg + " Dialect");
					Assert.AreEqual (id, section.Identifier, msg + "Identifier");
					Assert.AreEqual (0, section.Attributes.Count, "#cw4");*/

					return xs;
				}
			}

			Assert.Fail (String.Format ("[{0}] XmlSchema for tns : {1} not found.", msg, ns));
			return null;
		}
		Service GetService (WSServiceDescription sd, string name, string label)
		{
			Service ret = sd.Services [name];
			if (ret == null)
				Assert.Fail (String.Format ("[{0}] Service named '{1}' not found.", label, name));
			return ret;
		}

		WSBinding GetBinding (WSServiceDescription sd, string name, string label)
		{
			WSBinding ret = sd.Bindings [name];
			if (ret == null)
				Assert.Fail (String.Format ("[{0}] Binding named '{1}' not found.", label, name));
			return ret;
		}

		OperationBinding GetOperationBinding (WSBinding b, string name, string label)
		{
			foreach (OperationBinding op in b.Operations)
				if (op.Name == name)
					return op;

			Assert.Fail (String.Format ("[{0}] OperationBinding named '{1}' not found.", label, name));
			return null;
		}

		void CheckBasicHttpBinding (WSServiceDescription wsd, string binding_name, XmlQualifiedName binding_type,
			string operation_name, string action, bool has_input, bool has_output, string label)
		{
			WSBinding b = GetBinding (wsd, binding_name, label);
			OperationBinding op = GetOperationBinding (b, operation_name, label + " CheckBasicHttpBinding");

			Assert.AreEqual (binding_type, b.Type, label + " #cbh0");

			if (has_input) {
				InputBinding inb = op.Input;
				Assert.IsNotNull (inb, label + " #cbh1");
				Assert.AreEqual (1, inb.Extensions.Count, label + " #cbh2");

				Assert.AreEqual (typeof (SoapBodyBinding), inb.Extensions [0].GetType (), label + " #cbh3");
				SoapBodyBinding soap_binding = (SoapBodyBinding) inb.Extensions [0];
				Assert.AreEqual (SoapBindingUse.Literal, soap_binding.Use, label + " #cbh4");

				if (action != null) {
					Assert.AreEqual (1, op.Extensions.Count, label + " #chb5");
					Assert.AreEqual (typeof (SoapOperationBinding), op.Extensions [0].GetType (), label + " #cbh6");
					SoapOperationBinding sopb = (SoapOperationBinding) op.Extensions [0];
					Assert.AreEqual (action, sopb.SoapAction, label + " #cbh7");
				}
			}

			if (has_output) {
				OutputBinding outb = op.Output;
				Assert.IsNotNull (outb, label + " #cbh10");
				Assert.AreEqual (1, outb.Extensions.Count, label + " #cbh11");

				Assert.AreEqual (typeof (SoapBodyBinding), outb.Extensions [0].GetType (), label + " #cbh12");
				SoapBodyBinding soap_binding = (SoapBodyBinding) outb.Extensions [0];
				Assert.AreEqual (SoapBindingUse.Literal, soap_binding.Use, label + " #cbh13");
			}

			Assert.AreEqual (1, b.Extensions.Count, label + " #cbh20");
			Assert.AreEqual (typeof (SoapBinding), b.Extensions [0].GetType (), label + " #cbh21");
			SoapBinding sb = (SoapBinding) b.Extensions [0];
			Assert.AreEqual (SoapBinding.HttpTransport, sb.Transport, label + " #cbh22");
		}

		void CheckServicePort (Service svc, string port_name, XmlQualifiedName binding_name, string address, string label)
		{
			Port port = svc.Ports [port_name];
			Assert.IsNotNull (port, label + " #csp0");
			Assert.AreEqual (port.Binding, binding_name, label + " #csp1");

			Assert.AreEqual (1, port.Extensions.Count, label + " #csp2");
			Assert.AreEqual (typeof (SoapAddressBinding), port.Extensions [0].GetType (), label + " #csp3");
			SoapAddressBinding sab = (SoapAddressBinding) port.Extensions [0];
			Assert.AreEqual (address, sab.Location, label + " #csp3");
		}

		void CheckContract_IEchoService2 (MetadataSet ms, string label)
		{
			WSServiceDescription wsd = GetServiceDescription (ms, "http://tempuri.org/", label + "#a1");
			Assert.AreEqual (3, wsd.Messages.Count, "#cw5");

			Assert.IsNotNull (wsd.Messages [0]);
			// WSMessage m = wsd.Messages [0];
			CheckMessage (wsd, "ThisIsEchoService_Echo_InputMessage", "http://tempuri.org/:Echo");
			CheckMessage (wsd, "ThisIsEchoService_Echo_OutputMessage", "http://tempuri.org/:EchoResponse");

			CheckMessage (wsd, "ThisIsEchoService_DoubleIt_InputMessage", "http://tempuri.org/:DoubleIt");

			//PortTypes
			Assert.AreEqual (1, wsd.PortTypes.Count, "#cw6");
			PortType port = wsd.PortTypes [0];
			Assert.AreEqual ("ThisIsEchoService", port.Name, "#cw7");

			//Operations
			Assert.AreEqual (2, port.Operations.Count, "#cw8");
			//Operations [0]
			Operation op = port.Operations [0];
			Assert.AreEqual ("Echo", op.Name, "#co1");
			Assert.AreEqual (0, op.Extensions.Count, "#co2");
			Assert.IsNull (op.ParameterOrder, "#co3");
			Assert.AreEqual ("", op.ParameterOrderString, "#co4");
			Assert.AreEqual (0, op.Faults.Count, "#co5");

			//OperationMessages
			Assert.AreEqual (2, op.Messages.Count, "#co6");
			Assert.AreEqual (OperationFlow.RequestResponse, op.Messages.Flow, "#co7");

			CheckOperationMessage (op.Messages [0], "http://tempuri.org/:ThisIsEchoService_Echo_InputMessage",
				typeof (OperationInput), "http://tempuri.org/ThisIsEchoService/Echo");
			CheckOperationMessage (op.Messages [1], "http://tempuri.org/:ThisIsEchoService_Echo_OutputMessage",
				typeof (OperationOutput), "http://tempuri.org/ThisIsEchoService/EchoResponse");

			op = port.Operations [1];
			Assert.AreEqual ("DoubleIt", op.Name, "#co8");
			Assert.AreEqual (0, op.Extensions.Count, "#co9");
			Assert.IsNull (op.ParameterOrder, "#co10");
			Assert.AreEqual ("", op.ParameterOrderString, "#co11");
			Assert.AreEqual (0, op.Faults.Count, "#co12");

			//OperationMessages
			Assert.AreEqual (1, op.Messages.Count, "#co13");
			Assert.AreEqual (OperationFlow.OneWay, op.Messages.Flow, "#co14");

			CheckOperationMessage (op.Messages [0], "http://tempuri.org/:ThisIsEchoService_DoubleIt_InputMessage",
				typeof (OperationInput), "http://tempuri.org/ThisIsEchoService/DoubleIt");

			/* FIXME: Assert.AreEqual (1, wsd.Types.Schemas.Count, "#co20");
			XmlSchema xs = wsd.Types.Schemas [0];

			Assert.AreEqual (4, xs.Includes.Count);
			//FIXME: Check the imports.. */
		}

		void CheckContract_IEchoService (MetadataSet ms, string label)
		{
			WSServiceDescription wsd = GetServiceDescription (ms, "http://myns/echo", label + "#a0");
			Assert.AreEqual (4, wsd.Messages.Count, "#cw5");

			Assert.IsNotNull (wsd.Messages [0]);
			//WSMessage m = wsd.Messages [0];

			CheckMessage (wsd, "IEchoService_Echo_InputMessage", "http://myns/echo:Echo");
			CheckMessage (wsd, "IEchoService_Echo_OutputMessage", "http://myns/echo:EchoResponse");

			CheckMessage (wsd, "IEchoService_DoubleIt_InputMessage", "http://myns/echo:DoubleIt");
			CheckMessage (wsd, "IEchoService_DoubleIt_OutputMessage", "http://myns/echo:DoubleItResponse");

			//PortTypes
			Assert.AreEqual (1, wsd.PortTypes.Count, "#cw6");
			PortType port = wsd.PortTypes [0];
			Assert.AreEqual ("IEchoService", port.Name, "#cw7");

			//Operations
			Assert.AreEqual (2, port.Operations.Count, "#cw8");
			//Operations [0]
			Operation op = port.Operations [0];
			Assert.AreEqual ("Echo", op.Name, "#co1");
			Assert.AreEqual (0, op.Extensions.Count, "#co2");
			Assert.IsNull (op.ParameterOrder, "#co3");
			Assert.AreEqual ("", op.ParameterOrderString, "#co4");
			Assert.AreEqual (0, op.Faults.Count, "#co5");

			//OperationMessages
			Assert.AreEqual (2, op.Messages.Count, "#co6");
			Assert.AreEqual (OperationFlow.RequestResponse, op.Messages.Flow, "#co7");

			CheckOperationMessage (op.Messages [0], "http://myns/echo:IEchoService_Echo_InputMessage", typeof (OperationInput), "http://myns/echo/IEchoService/Echo");
			CheckOperationMessage (op.Messages [1], "http://myns/echo:IEchoService_Echo_OutputMessage", typeof (OperationOutput), "http://myns/echo/IEchoService/EchoResponse");

			op = port.Operations [1];
			Assert.AreEqual ("DoubleIt", op.Name, "#co8");
			Assert.AreEqual (0, op.Extensions.Count, "#co9");
			Assert.IsNull (op.ParameterOrder, "#co10");
			Assert.AreEqual ("", op.ParameterOrderString, "#co11");
			Assert.AreEqual (0, op.Faults.Count, "#co12");

			//OperationMessages
			Assert.AreEqual (2, op.Messages.Count, "#co13");
			Assert.AreEqual (OperationFlow.RequestResponse, op.Messages.Flow, "#co14");

			CheckOperationMessage (op.Messages [0], "http://myns/echo:IEchoService_DoubleIt_InputMessage", typeof (OperationInput), "http://myns/echo/IEchoService/DoubleIt");
			CheckOperationMessage (op.Messages [1], "http://myns/echo:IEchoService_DoubleIt_OutputMessage", typeof (OperationOutput), "http://myns/echo/IEchoService/DoubleItResponse");

			/* FIXME: Assert.AreEqual (1, wsd.Types.Schemas.Count, "#co20");
			XmlSchema xs = wsd.Types.Schemas [0];

			Assert.AreEqual (4, xs.Includes.Count);
			//FIXME: Check the imports.. */
		}

		void CheckOperationMessage (OperationMessage opmsg, string msg, Type type, string action)
		{
			Assert.AreEqual (type, opmsg.GetType (), "#com1");
			Assert.AreEqual (msg, opmsg.Message.ToString (), "#com2");
			Assert.AreEqual (0, opmsg.Extensions.Count, "#com3");
			Assert.AreEqual (1, opmsg.ExtensibleAttributes.Length, "#com4");
			Assert.IsNull (opmsg.Name, "#com5");

			XmlAttribute attr = opmsg.ExtensibleAttributes [0];
			Assert.AreEqual ("Action", attr.LocalName, "#ca1");
			Assert.AreEqual ("http://www.w3.org/2006/05/addressing/wsdl", attr.NamespaceURI, "#ca2");
			Assert.AreEqual (action, attr.Value, "#ca3");
		}

		void CheckMessage (WSServiceDescription sd, string msg_name, string part_type)
		{
			CheckMessage (sd, msg_name, "parameters", part_type, false, "");
		}

		void CheckMessage (WSServiceDescription sd, string msg_name, string part_name, string part_type, bool is_type, string label)
		{
			WSMessage m = sd.Messages [msg_name];
			Assert.IsNotNull (m, label + " : Message named " + msg_name + " not found.");

			Assert.AreEqual (msg_name, m.Name, label + " #cm1");
			Assert.AreEqual (0, m.Extensions.Count, label + " #cm2");

			Assert.IsNull (m.ExtensibleAttributes, label + " #cm3a");
			Assert.AreEqual (1, m.Parts.Count, label + " #cm3");

			Assert.AreEqual (part_name, m.Parts [0].Name, label + " #cm9");

			if (is_type) {
				Assert.AreEqual ("", m.Parts [0].Element.ToString (), label + " #cm4");
				Assert.AreEqual (part_type, m.Parts [0].Type.ToString (), label + " #cm4a");
			} else {
				Assert.AreEqual ("", m.Parts [0].Type.ToString (), label + " #cm5");
				Assert.AreEqual (part_type, m.Parts [0].Element.ToString (), label + " #cm5a");
			}

			Assert.IsNull (m.Parts [0].ExtensibleAttributes, label + " #cm6");
			Assert.AreEqual (0, m.Parts [0].Extensions.Count, label + " #cm7");
		}

		void WriteTo (WsdlExporter we, string name)
		{
			using (XmlTextWriter xw = new XmlTextWriter (name, Encoding.UTF8)) {
				xw.Formatting = Formatting.Indented;
				we.GetGeneratedMetadata ().WriteTo (xw);
			}
		}

		[Test]
		public void ExportEndpointTest5 () {
			WsdlExporter we = new WsdlExporter ();

			ServiceEndpoint se = new ServiceEndpoint (ContractDescription.GetContract (typeof (IEchoService)));
			se.Binding = new BasicHttpBinding ();
			se.Address = new EndpointAddress ("http://localhost:8080");

			we.ExportEndpoint (se);

			MetadataSet ms = we.GetGeneratedMetadata ();
			Assert.AreEqual (6, ms.MetadataSections.Count);
			WSServiceDescription wsd = GetServiceDescription (ms, "http://tempuri.org/", "ExportEndpointTest5#1");

			SoapBinding soapBinding = (SoapBinding) wsd.Bindings [0].Extensions [0];
			Assert.AreEqual (SoapBindingStyle.Document, soapBinding.Style, "soapBinding.Style");
			Assert.AreEqual (SoapBinding.HttpTransport, soapBinding.Transport, "soapBinding.Transport");
		}

	}

	[DataContract]
	public class dc
	{
		[DataMember]
		string foo;

		/*  [DataMember]
			dc me;

			[DataMember]
			some_enum en;

			[DataMember]
			NotReferenced nr;*/

		[DataMember]
		public FooNS.bar bb;
	}

	[ServiceContract (Namespace = "http://myns/echo")]
	public interface IEchoService
	{

		[OperationContract]
		string Echo (string msg, int num, dc d);

		[OperationContract]
		void DoubleIt (int it, string prefix);

		/*[OperationContract]
		void foo ();*/

	}

	[ServiceContract (Name = "ThisIsEchoService")]
	public interface IEchoService2
	{

		[OperationContract]
		string Echo (string msg, int num, dc d);

		[OperationContract (IsOneWay = true)]
		void DoubleIt (int it, string prefix);

		/*[OperationContract]
		void foo ();*/
	}

	[ServiceContract (Name = "AnotherService")]
	public interface IEchoService3
	{
		[OperationContract]
		string Echo (string msg, int num, dc d);

		[OperationContract (IsOneWay = true)]
		void DoubleIt (int it, string prefix);

		/*[OperationContract]
		void foo ();*/
	}

	[ServiceContract]
	public class Bar1
	{
		[OperationContract]
		public void Foo (SMMessage msg, string s)
		{
		}
	}

	[ServiceContract]
	public interface IFoo1
	{
		//Same DataContract used in different operations

		[OperationContract]
		void Op1 (dc d); 

		[OperationContract]
		void Op2 (dc d); 
	}

	//Used to check whether both IFoo1 & IFoo1a
	//can be exported
	//Operations are not Service scoped
	[ServiceContract]
	public interface IFoo1a
	{
		[OperationContract]
		void Op1 (string s);
	}

	[ServiceContract]
	public interface IFoo2
	{
		// FIXME: it does not pass yet
		[OperationContract]
		OregoMessage Nanoda (OregoMessage msg);

		// FIXME: it does not pass yet
		[OperationContract]
		Mona NewMona (Mona source);
	}

	[MessageContract]
	public class OregoMessage
	{
		[MessageBodyMember]
		public string Neutral;

		[MessageBodyMember]
		public Assembly Huh;

		[MessageBodyMember] // it should be ignored ...
		public string Setter { set { } }

		public string NonMember;
	}

	[DataContract]
	public class Mona
	{
		[DataMember]
		public string OmaeMona;

		[DataMember]
		public string OreMona;
	}

}
namespace FooNS
{
	[DataContract]
	public class bar
	{
		[DataMember]
		public string foo;
	}
}

#endif


