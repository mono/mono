//
// WsdlImporterTest.cs
//
// Author:
//	Ankit Jain <JAnkit@novell.com>
//	Atsushi Enomoto <atsushi@ximian.com>
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Web.Services;
using System.Web.Services.Description;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Xml;
using System.Xml.Schema;
using Microsoft.CSharp;
using NUnit.Framework;

using WSServiceDescription = System.Web.Services.Description.ServiceDescription;
using SMBinding = System.ServiceModel.Channels.Binding;

namespace MonoTests.System.ServiceModel.Description
{
	[TestFixture]
	public class WsdlImporterTest
	{
		MetadataSet ms = null;
		WsdlImporter wi = null;
		XmlReader xtr = null;

		void NoExtensionsSetup ()
		{
			XmlReaderSettings xs = new XmlReaderSettings ();
			xs.IgnoreWhitespace = true;
			xtr = XmlTextReader.Create ("Test/System.ServiceModel.Description/dump.xml", xs);

			xtr.Read ();

			//FIXME: skipping Headers
			while (xtr.LocalName != "Body") {
				if (!xtr.Read ())
					return;
			}

			//Move to <Metadata ..
			xtr.Read ();
			ms = MetadataSet.ReadFrom (xtr);

			//MyWsdlImportExtension mw = new MyWsdlImportExtension ();
			List<IWsdlImportExtension> list = new List<IWsdlImportExtension> ();
			//list.Add (mw);
			list.Add (new DataContractSerializerMessageContractImporter ());

			/*list.Add (new MessageEncodingBindingElementImporter ());
			list.Add (new TransportBindingElementImporter ());
			list.Add (new StandardBindingImporter ());*/

			wi = new WsdlImporter (ms, null, list);
		}

		[TearDown]
		public void TearDown ()
		{
			if (xtr != null)
				xtr.Close ();
		}

		[Test]
		public void CtorTest ()
		{
			NoExtensionsSetup ();
			Assert.AreEqual (2, wi.WsdlDocuments.Count, "#CT1");
			Assert.AreEqual (3, wi.XmlSchemas.Count, "#CT2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CtorNullTest1 ()
		{
			new WsdlImporter (null);
		}

		private void CheckDefaultPolicyImportExtensions (KeyedByTypeCollection<IPolicyImportExtension> list)
		{
			Assert.IsNotNull (list, "#CN1");
			// 9 in 3.0, 10 in 3.5
			// Assert.AreEqual (10, list.Count, "#CN2");

			Assert.AreEqual (typeof (PrivacyNoticeBindingElementImporter), list [0].GetType (), "#CN3");
			Assert.AreEqual (typeof (UseManagedPresentationBindingElementImporter), list [1].GetType (), "#CN4");
			Assert.AreEqual (typeof (TransactionFlowBindingElementImporter), list [2].GetType (), "#CN5");
			Assert.AreEqual (typeof (ReliableSessionBindingElementImporter), list [3].GetType (), "#CN6");
			Assert.AreEqual (typeof (SecurityBindingElementImporter), list [4].GetType (), "#CN7");
			Assert.AreEqual (typeof (CompositeDuplexBindingElementImporter), list [5].GetType (), "#CN8");
			Assert.AreEqual (typeof (OneWayBindingElementImporter), list [6].GetType (), "#CN9");
			Assert.AreEqual (typeof (MessageEncodingBindingElementImporter), list [7].GetType (), "#CN10");
			Assert.AreEqual (typeof (TransportBindingElementImporter), list [8].GetType (), "#CN11");
			// It is in System.WorkflowServices.dll (3.5)
			//Assert.AreEqual (typeof (ContextBindingElementImporter), list [9].GetType (), "#CN12");
		}

		private void CheckDefaultWsdlImportExtensions (KeyedByTypeCollection<IWsdlImportExtension> list)
		{
			Assert.IsNotNull (list, "#CN20");
			// 5 in 3.0, 6 in 3.5
			// Assert.AreEqual (6, list.Count, "#CN21");

			Assert.AreEqual (typeof (DataContractSerializerMessageContractImporter), list [0].GetType (), "#CN22");
			Assert.AreEqual (typeof (XmlSerializerMessageContractImporter), list [1].GetType (), "#CN23");
			Assert.AreEqual (typeof (MessageEncodingBindingElementImporter), list [2].GetType (), "#CN24");
			Assert.AreEqual (typeof (TransportBindingElementImporter), list [3].GetType (), "#CN25");
			Assert.AreEqual (typeof (StandardBindingImporter), list [4].GetType (), "#CN26");
			// It is in System.WorkflowServices.dll (3.5)
			//Assert.AreEqual (typeof (ContextBindingElementImporter), list [5].GetType (), "#CN27");
		}

		[Test]
		[Category ("NotWorking")]
		public void CtorNullTest2 ()
		{
			WsdlImporter wi = new WsdlImporter (new MetadataSet (), null, new List<IWsdlImportExtension> ());

			Assert.IsNotNull (wi.WsdlImportExtensions, "#CN12");
			Assert.AreEqual (0, wi.WsdlImportExtensions.Count, "#CN13");

			/* FIXME: Not all importers are implemented yet */
			CheckDefaultPolicyImportExtensions (wi.PolicyImportExtensions);
		}

		[Test]
		[Category ("NotWorking")]
		public void CtorNullTest3 ()
		{
			WsdlImporter wi = new WsdlImporter (new MetadataSet (), new List<IPolicyImportExtension> (), null);

			Assert.IsNotNull (wi.PolicyImportExtensions, "#CN18");
			Assert.AreEqual (0, wi.PolicyImportExtensions.Count, "#CN19");

			/* FIXME: Not all importers are implemented yet */
			CheckDefaultWsdlImportExtensions (wi.WsdlImportExtensions);
		}

		[Test]
		[Category ("NotWorking")]
		public void CtorNullTest4 ()
		{
			WsdlImporter wi = new WsdlImporter (new MetadataSet (), null, null);
			/* FIXME: Not all importers are implemented yet */
			CheckDefaultPolicyImportExtensions (wi.PolicyImportExtensions);
			CheckDefaultWsdlImportExtensions (wi.WsdlImportExtensions);
		}

		public void CtorNullTest5 ()
		{
			WsdlImporter wi = new WsdlImporter (new MetadataSet ());
			CheckDefaultWsdlImportExtensions (wi.WsdlImportExtensions);
			CheckDefaultPolicyImportExtensions (wi.PolicyImportExtensions);
		}

		[Test]
		public void ExtensionsTest ()
		{
			XmlReaderSettings xs = new XmlReaderSettings ();
			xs.IgnoreWhitespace = true;
			xtr = XmlTextReader.Create ("Test/System.ServiceModel.Description/dump.xml", xs);

			xtr.Read ();

			//FIXME: skipping Headers
			while (xtr.LocalName != "Body") {
				if (!xtr.Read ())
					return;
			}

			//Move to <Metadata ..
			xtr.Read ();
			ms = MetadataSet.ReadFrom (xtr);

			Assert.AreEqual (typeof (WSServiceDescription), ms.MetadataSections [0].Metadata.GetType (), "#ET1");
			WSServiceDescription wsd = ms.MetadataSections [1].Metadata as WSServiceDescription;

			//ServiceDescription
			Assert.IsNotNull (wsd.Extensions, "#ET2");
			Assert.AreEqual (0, wsd.Extensions.Count, "#ET3");

			//Binding [0]
			Assert.IsNotNull (wsd.Bindings [0].Extensions, "#ET4");
			Assert.AreEqual (1, wsd.Bindings [0].Extensions.Count, "#ET5");
			Assert.AreEqual (typeof (SoapBinding), wsd.Bindings [0].Extensions [0].GetType (), "#ET6");

			//Binding [0].Operations [0]
			Assert.IsNotNull (wsd.Bindings [0].Operations [0].Extensions, "#ET7");
			Assert.AreEqual (1, wsd.Bindings [0].Operations [0].Extensions.Count, "#ET8");
			Assert.AreEqual (typeof (SoapOperationBinding), wsd.Bindings [0].Operations [0].Extensions [0].GetType (), "#ET9");

			//Binding [0].Operations [1]
			Assert.IsNotNull (wsd.Bindings [0].Operations [1].Extensions, "#ET10");
			Assert.AreEqual (1, wsd.Bindings [0].Operations [1].Extensions.Count, "#ET11");
			Assert.AreEqual (typeof (SoapOperationBinding), wsd.Bindings [0].Operations [1].Extensions [0].GetType (), "#ET12");

			//Service.Port
			Assert.IsNotNull (wsd.Services [0].Ports [0].Extensions, "#ET13");
			Assert.AreEqual (1, wsd.Services [0].Ports [0].Extensions.Count, "#ET14");
			Assert.AreEqual (typeof (SoapAddressBinding), wsd.Services [0].Ports [0].Extensions [0].GetType (), "#ET15");
		}

		void CheckXmlElement (object o, string name)
		{
			Assert.AreEqual (typeof (XmlElement), o.GetType ());
			Assert.AreEqual (name, ((XmlElement) o).Name);
		}

		[Test]
		public void BindingsTest ()
		{
			NoExtensionsSetup ();
			IEnumerable<SMBinding> bindings = wi.ImportAllBindings ();

			int count = 0;
			foreach (SMBinding b in bindings) {
				Assert.AreEqual (typeof (CustomBinding), b.GetType (), "#B1");
				Assert.AreEqual ("BasicHttpBinding_IEchoService", b.Name, "#B2");
				Assert.AreEqual ("http://tempuri.org/", b.Namespace, "#B3");
				Assert.AreEqual ("", b.Scheme, "#B4");

				//FIXME: Test BindingElements

				count++;
			}
			Assert.AreEqual (1, count);
		}

		[Test]
		[Category ("NotWorking")]
		public void ContractsTest ()
		{
			NoExtensionsSetup ();
			Collection<ContractDescription> cds = wi.ImportAllContracts ();

			Assert.AreEqual (1, cds.Count);
			CheckContractDescription (cds [0]);

		}

		//Used only for contract from test.xml
		private void CheckContractDescription (ContractDescription cd)
		{
			Assert.AreEqual ("IEchoService", cd.Name, "#CD1");
			Assert.AreEqual ("http://myns/echo", cd.Namespace, "#CD2");
			Assert.AreEqual (0, cd.Behaviors.Count, "#CD3");

			Assert.IsNull (cd.CallbackContractType, "#CD4");
			Assert.IsNull (cd.ContractType, "#CD5");

			Assert.IsFalse (cd.HasProtectionLevel, "#CD6");
			Assert.AreEqual (SessionMode.Allowed, cd.SessionMode, "#CD7");

			//Operations
			Assert.AreEqual (2, cd.Operations.Count, "#CD8");
			Assert.AreEqual (cd, cd.Operations [1].DeclaringContract, "#CD10");
			CheckOperationDescriptionEcho (cd.Operations [0]);
			CheckOperationDescriptionDouble_it (cd.Operations [1]);

		}

		public void CheckOperationDescriptionEcho (OperationDescription od)
		{
			//OperationDescription od = cd.Operations [0];
			Assert.AreEqual ("Echo", od.Name, "#CD9");

			Assert.IsNull (od.BeginMethod, "#CD11");
			Assert.IsNull (od.EndMethod, "#CD12");

			Assert.AreEqual (0, od.Faults.Count, "#CD13");
			Assert.IsTrue (od.IsInitiating, "#CD14");
			Assert.IsFalse (od.IsOneWay, "#CD15");
			Assert.IsFalse (od.IsTerminating, "#CD16");

			//FIXME: Behaviors

			//OperationDescription.Message
			Assert.AreEqual (2, od.Messages.Count, "#CD17");

			MessageDescription md = od.Messages [0];
			#region MessageDescription 0
			Assert.AreEqual ("http://myns/echo/IEchoService/Echo", md.Action, "#CD18");
			Assert.AreEqual (MessageDirection.Input, md.Direction, "#CD19");
			Assert.AreEqual (0, md.Headers.Count, "#CD20");
			Assert.AreEqual (0, md.Properties.Count, "#CD21");

			//MessageDescription.MessageBodyDescription

			Assert.IsNull (md.Body.ReturnValue, "#CD22");
			Assert.AreEqual ("Echo", md.Body.WrapperName, "#CD23");
			Assert.AreEqual ("http://myns/echo", md.Body.WrapperNamespace, "#CD24");
			#region MessagePartDescription
			//MessagePartDescription 0

			Assert.AreEqual (3, md.Body.Parts.Count, "#CD25");
			MessagePartDescription mpd = md.Body.Parts [0];

			Assert.AreEqual (0, mpd.Index, "#CD26");
			Assert.IsFalse (mpd.Multiple, "#CD27");
			Assert.AreEqual ("msg", mpd.Name, "#CD28");
			Assert.AreEqual ("http://myns/echo", mpd.Namespace, "#CD29");
			Assert.IsNull (mpd.Type, "#CD30");

			//MessagePartDescription 1

			mpd = md.Body.Parts [1];

			Assert.AreEqual (0, mpd.Index, "#CD31");
			Assert.IsFalse (mpd.Multiple, "#CD32");
			Assert.AreEqual ("num", mpd.Name, "#CD33");
			Assert.AreEqual ("http://myns/echo", mpd.Namespace, "#CD34");
			Assert.IsNull (mpd.Type, "#CD35");

			//MessagePartDescription 2

			mpd = md.Body.Parts [2];

			Assert.AreEqual (0, mpd.Index, "#CD31");
			Assert.IsFalse (mpd.Multiple, "#CD32");
			Assert.AreEqual ("d", mpd.Name, "#CD33");
			Assert.AreEqual ("http://myns/echo", mpd.Namespace, "#CD34");
			Assert.IsNull (mpd.Type, "#CD35");

			#endregion
			#endregion

			md = od.Messages [1];

			#region MessageDescription 1

			Assert.AreEqual ("http://myns/echo/IEchoService/EchoResponse", md.Action, "#CD36");
			Assert.AreEqual (MessageDirection.Output, md.Direction, "#CD37");
			Assert.AreEqual (0, md.Headers.Count, "#CD38");
			Assert.AreEqual (0, md.Properties.Count, "#CD39");

			//MessageDescription.MessageBodyDescription

			//Return value
			Assert.IsNotNull (md.Body.ReturnValue, "#CD40");

			Assert.AreEqual ("EchoResponse", md.Body.WrapperName, "#CD44");
			Assert.AreEqual ("http://myns/echo", md.Body.WrapperNamespace, "#CD45");
			Assert.AreEqual (0, md.Body.Parts.Count, "#CD46");

			#endregion

		}

		public void CheckOperationDescriptionDouble_it (OperationDescription od)
		{
			//OperationDescription od = cd.Operations [0];
			Assert.AreEqual ("DoubleIt", od.Name, "#CD9");

			Assert.IsNull (od.BeginMethod, "#CD11");
			Assert.IsNull (od.EndMethod, "#CD12");

			Assert.AreEqual (0, od.Faults.Count, "#CD13");
			Assert.IsTrue (od.IsInitiating, "#CD14");
			Assert.IsFalse (od.IsOneWay, "#CD15");
			Assert.IsFalse (od.IsTerminating, "#CD16");

			//FIXME: Behaviors

			//OperationDescription.Message
			Assert.AreEqual (2, od.Messages.Count, "#CD17");

			MessageDescription md = od.Messages [0];
			#region MessageDescription 0
			Assert.AreEqual ("http://myns/echo/IEchoService/DoubleIt", md.Action, "#CD18");
			Assert.AreEqual (MessageDirection.Input, md.Direction, "#CD19");
			Assert.AreEqual (0, md.Headers.Count, "#CD20");
			Assert.AreEqual (0, md.Properties.Count, "#CD21");

			//MessageDescription.MessageBodyDescription

			Assert.IsNull (md.Body.ReturnValue, "#CD22");
			Assert.AreEqual ("DoubleIt", md.Body.WrapperName, "#CD23");
			Assert.AreEqual ("http://myns/echo", md.Body.WrapperNamespace, "#CD24");
			#region MessagePartDescription
			//MessagePartDescription 0

			//Assert.AreEqual (0, md.Body.Parts.Count, "#CD25");
			MessagePartDescription mpd = md.Body.Parts [0];

			Assert.AreEqual (0, mpd.Index, "#CD26");
			Assert.IsFalse (mpd.Multiple, "#CD27");
			Assert.AreEqual ("it", mpd.Name, "#CD28");
			Assert.AreEqual ("http://myns/echo", mpd.Namespace, "#CD29");
			Assert.IsNull (mpd.Type, "#CD30");

			//MessagePartDescription 1

			mpd = md.Body.Parts [1];

			Assert.AreEqual (0, mpd.Index, "#CD31");
			Assert.IsFalse (mpd.Multiple, "#CD32");
			Assert.AreEqual ("prefix", mpd.Name, "#CD33");
			Assert.AreEqual ("http://myns/echo", mpd.Namespace, "#CD34");
			Assert.IsNull (mpd.Type, "#CD35");

			#endregion
			#endregion

			md = od.Messages [1];

			#region MessageDescription 1

			Assert.AreEqual ("http://myns/echo/IEchoService/DoubleItResponse", md.Action, "#CD36");
			Assert.AreEqual (MessageDirection.Output, md.Direction, "#CD37");
			Assert.AreEqual (0, md.Headers.Count, "#CD38");
			Assert.AreEqual (0, md.Properties.Count, "#CD39");

			//MessageDescription.MessageBodyDescription

			//Return value
			Assert.IsNotNull (md.Body.ReturnValue, "#CD40");

			Assert.AreEqual ("DoubleItResponse", md.Body.WrapperName, "#CD44");
			Assert.AreEqual ("http://myns/echo", md.Body.WrapperNamespace, "#CD45");
			Assert.AreEqual (0, md.Body.Parts.Count, "#CD46");

			#endregion

		}

		[Test]
		[Category ("NotWorking")]
		public void EndpointsTest ()
		{
			NoExtensionsSetup ();
			ServiceEndpointCollection sec = wi.ImportAllEndpoints ();

			Assert.AreEqual (1, sec.Count);

			ServiceEndpoint se = sec [0];

			//.Address
			Assert.IsNull (se.Address, "#ep1");

			Assert.AreEqual (0, se.Behaviors.Count, "#ep6");

			//Binding
			Assert.AreEqual (typeof (CustomBinding), se.Binding.GetType (), "#ep7");

			CustomBinding c = (CustomBinding) se.Binding;

			Assert.AreEqual ("BasicHttpBinding_IEchoService", c.Name, "#ep8");
			Assert.AreEqual ("http://tempuri.org/", c.Namespace, "#ep9");
			Assert.AreEqual ("", c.Scheme, "#ep10");

			Assert.AreEqual (1, c.Elements.Count, "#ep11");
			Assert.AreEqual (typeof (TextMessageEncodingBindingElement), c.Elements [0].GetType (), "#ep12");
			//FIXME: Check c.Elements [0]. properties?

			CheckContractDescription (se.Contract);

			Assert.IsNull (se.ListenUri, "#ep13");
			Assert.AreEqual (ListenUriMode.Explicit, se.ListenUriMode, "#ep14");
		}

		void ExtensionsSetup (List<IWsdlImportExtension> list)
		{
			XmlReaderSettings xs = new XmlReaderSettings ();
			xs.IgnoreWhitespace = true;
			//xtr = XmlTextReader.Create ("Test/System.ServiceModel.Description/test2a.xml", xs);
			xtr = XmlTextReader.Create ("Test/System.ServiceModel.Description/dump.xml", xs);

			xtr.Read ();

			//FIXME: skipping Headers
			while (xtr.LocalName != "Body") {
				if (!xtr.Read ())
					return;
			}

			//Move to <Metadata ..
			xtr.Read ();
			ms = MetadataSet.ReadFrom (xtr);

			/*MyWsdlImportExtension mw = new MyWsdlImportExtension ();
			List<IWsdlImportExtension> list = new List<IWsdlImportExtension> ();
			list.Add (mw);
			list.Add (new DataContractSerializerMessageContractImporter ());

			/*list.Add (new MessageEncodingBindingElementImporter ());
			list.Add (new TransportBindingElementImporter ());
			list.Add (new StandardBindingImporter ());*/

			wi = new WsdlImporter (ms, null, list);
		}


		[Test]
		[Category ("NotWorking")]
		public void EndpointTestWithExtensions ()
		{
			List<IWsdlImportExtension> list = new List<IWsdlImportExtension> ();
			list.Add (new TransportBindingElementImporter ());

			ExtensionsSetup (list);
			ServiceEndpointCollection sec = wi.ImportAllEndpoints ();

			Assert.AreEqual (1, sec.Count, "#epe0");
			ServiceEndpoint se = sec [0];

			Assert.IsNotNull (se.Address, "#epe1");
			Assert.AreEqual (0, se.Address.Headers.Count, "#epe2");
			Assert.IsNull (se.Address.Identity, "#epe3");
			Assert.AreEqual (new Uri ("http://localhost:8080/echo/svc"), se.Address.Uri, "#epe4");

			//Bindings
			Assert.AreEqual (typeof (CustomBinding), se.Binding.GetType (), "#epe5");
			CustomBinding cb = se.Binding as CustomBinding;
			Assert.AreEqual (2, cb.Elements.Count, "#epe6");
			Assert.AreEqual (typeof (TextMessageEncodingBindingElement), cb.Elements [0].GetType (), "#epe7");
			Assert.AreEqual (typeof (HttpTransportBindingElement), cb.Elements [1].GetType (), "#epe8");

			Assert.AreEqual (new Uri ("http://localhost:8080/echo/svc"), se.ListenUri, "#epe9");
			Assert.AreEqual ("http", cb.Scheme, "#epe10");

		}

		MetadataSet GetMetadataSetFromWsdl (string path)
		{
			var ms = new MetadataSet ();
			var sd = WSServiceDescription.Read (XmlReader.Create (path));
			ms.MetadataSections.Add (MetadataSection.CreateFromServiceDescription (sd));
			foreach (XmlSchema xs in sd.Types.Schemas)
				foreach (XmlSchemaImport import in xs.Includes)
					using (var xr = XmlReader.Create (Path.Combine (Path.GetDirectoryName (path), import.SchemaLocation)))
						ms.MetadataSections.Add (MetadataSection.CreateFromSchema (XmlSchema.Read (xr, null)));
			return ms;
		}

		[Test]
		[Ignore ("Until make dist gets fixed I won't enable any of new external-source-dependent tests")]
		public void ImportMethodWithArrayOfint ()
		{
			var ms = GetMetadataSetFromWsdl ("Test/Resources/xml/service1.wsdl");
			var imp = new WsdlImporter (ms);
			var cg = new ServiceContractGenerator ();
			var cd = imp.ImportAllContracts () [0];
			cg.GenerateServiceContractType (cd);
			var sw = new StringWriter ();
			new CSharpCodeProvider ().GenerateCodeFromCompileUnit (
				cg.TargetCompileUnit, sw, null);
			// sort of hacky test
			Assert.IsTrue (sw.ToString ().IndexOf ("int[] GetSearchData") > 0, "#1");
		}

		[Test]
		[Category ("NotWorking")]
		public void ImportXmlTypes ()
		{
			// part of bug #670945
			var mset = new MetadataSet ();
			WSServiceDescription sd = null;

			sd = WSServiceDescription.Read (XmlReader.Create ("670945.wsdl"));
			mset.MetadataSections.Add (new MetadataSection () {
				Dialect = MetadataSection.ServiceDescriptionDialect,
				Metadata = sd });

			var imp = new WsdlImporter (mset);
			var sec = imp.ImportAllContracts ();
			
			// FIXME: examine resulting operations.
		}
	}
}
