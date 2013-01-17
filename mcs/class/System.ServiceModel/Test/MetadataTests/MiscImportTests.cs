//
// Test_Misc.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2012 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Net;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.SyntaxHelpers;

using WS = System.Web.Services.Description;

namespace MonoTests.System.ServiceModel.MetadataTests {

	public class MiscImportTests {

		[Test]
		public void BasicHttpBinding_ImportBinding ()
		{
			var label = new TestLabel ("BasicHttpBinding_ImportBinding");
			
			var doc = TestContext.LoadMetadata ("BasicHttp");
			var sd = (WS.ServiceDescription)doc.MetadataSections [0].Metadata;
			var wsdlBinding = sd.Bindings [0];
			
			var importer = new WsdlImporter (doc);
			
			Assert.That (sd.Bindings, Is.Not.Null, label.Get ());
			Assert.That (sd.Bindings.Count, Is.EqualTo (1), label.Get ());
			
			var binding = importer.ImportBinding (wsdlBinding);
			BindingTestAssertions.CheckImportErrors (importer, label);
			Assert.That (binding, Is.Not.Null, label.Get ());
		}
		
		[Test]
		public void BasicHttpBinding_ImportEndpoint ()
		{
			var label = new TestLabel ("BasicHttpBinding_ImportEndpoint");
			
			var doc = TestContext.LoadMetadata ("BasicHttp");
			var sd = (WS.ServiceDescription)doc.MetadataSections [0].Metadata;
			
			label.EnterScope ("wsdl");
			Assert.That (sd.Services, Is.Not.Null, label.Get ());
			Assert.That (sd.Services.Count, Is.EqualTo (1), label.Get ());
			
			var service = sd.Services [0];
			Assert.That (service.Ports, Is.Not.Null, label.Get ());
			Assert.That (service.Ports.Count, Is.EqualTo (1), label.Get ());
			label.LeaveScope ();
			
			var importer = new WsdlImporter (doc);
			
			var port = importer.ImportEndpoint (service.Ports [0]);
			BindingTestAssertions.CheckImportErrors (importer, label);
			Assert.That (port, Is.Not.Null, label.Get ());
		}
		
		[Test]
		public void BasicHttpBinding_Error ()
		{
			var label = new TestLabel ("BasicHttpBinding_Error");
			
			var doc = TestContext.LoadMetadata ("http-error.xml");
			var sd = (WS.ServiceDescription)doc.MetadataSections [0].Metadata;
			var wsdlBinding = sd.Bindings [0];
			
			var importer = new WsdlImporter (doc);
			
			label.EnterScope ("all");
			
			var bindings = importer.ImportAllBindings ();
			Assert.That (bindings, Is.Not.Null, label.Get ());
			Assert.That (bindings.Count, Is.EqualTo (0), label.Get ());
			
			label.EnterScope ("errors");
			Assert.That (importer.Errors, Is.Not.Null, label.Get ());
			Assert.That (importer.Errors.Count, Is.EqualTo (1), label.Get ());
			
			var error = importer.Errors [0];
			Assert.That (error.IsWarning, Is.False, label.Get ());
			label.LeaveScope ();
			label.LeaveScope ();
			
			label.EnterScope ("single");
			
			try {
				importer.ImportBinding (wsdlBinding);
				Assert.Fail (label.Get ());
			} catch {
				;
			}
			
			Assert.That (importer.Errors.Count, Is.EqualTo (1), label.Get ());
			
			label.LeaveScope ();
			
			label.EnterScope ("single-first");
			
			var importer2 = new WsdlImporter (doc);
			
			try {
				importer2.ImportBinding (wsdlBinding);
				Assert.Fail (label.Get ());
			} catch {
				;
			}
			
			Assert.That (importer2.Errors.Count, Is.EqualTo (1), label.Get ());
			
			try {
				importer2.ImportBinding (wsdlBinding);
				Assert.Fail (label.Get ());
			} catch {
				;
			}
			
			var bindings2 = importer.ImportAllBindings ();
			Assert.That (bindings2, Is.Not.Null, label.Get ());
			Assert.That (bindings2.Count, Is.EqualTo (0), label.Get ());
			
			label.LeaveScope ();
		}
		
		[Test]
		public void BasicHttpBinding_Error2 ()
		{
			var label = new TestLabel ("BasicHttpBinding_Error2");
			
			var doc = TestContext.LoadMetadata ("http-error.xml");
			var sd = (WS.ServiceDescription)doc.MetadataSections [0].Metadata;
			
			label.EnterScope ("wsdl");
			Assert.That (sd.Services, Is.Not.Null, label.Get ());
			Assert.That (sd.Services.Count, Is.EqualTo (1), label.Get ());
			
			var service = sd.Services [0];
			Assert.That (service.Ports, Is.Not.Null, label.Get ());
			Assert.That (service.Ports.Count, Is.EqualTo (1), label.Get ());
			label.LeaveScope ();
			
			var importer = new WsdlImporter (doc);
			
			label.EnterScope ("all");
			
			var endpoints = importer.ImportAllEndpoints ();
			Assert.That (endpoints, Is.Not.Null, label.Get ());
			Assert.That (endpoints.Count, Is.EqualTo (0), label.Get ());
			
			label.EnterScope ("errors");
			Assert.That (importer.Errors, Is.Not.Null, label.Get ());
			Assert.That (importer.Errors.Count, Is.EqualTo (2), label.Get ());
			
			Assert.That (importer.Errors [0].IsWarning, Is.False, label.Get ());
			Assert.That (importer.Errors [1].IsWarning, Is.False, label.Get ());
			label.LeaveScope ();
			label.LeaveScope ();
			
			label.EnterScope ("single");
			
			try {
				importer.ImportEndpoint (service.Ports [0]);
				Assert.Fail (label.Get ());
			} catch {
				;
			}
			
			Assert.That (importer.Errors.Count, Is.EqualTo (2), label.Get ());
			
			label.LeaveScope ();
			
			label.EnterScope ("single-first");
			
			var importer2 = new WsdlImporter (doc);
			
			try {
				importer2.ImportEndpoint (service.Ports [0]);
				Assert.Fail (label.Get ());
			} catch {
				;
			}
			
			Assert.That (importer2.Errors.Count, Is.EqualTo (2), label.Get ());
			
			try {
				importer2.ImportEndpoint (service.Ports [0]);
				Assert.Fail (label.Get ());
			} catch {
				;
			}
			
			var endpoints2 = importer.ImportAllEndpoints ();
			Assert.That (endpoints2, Is.Not.Null, label.Get ());
			Assert.That (endpoints2.Count, Is.EqualTo (0), label.Get ());
			
			label.LeaveScope ();
		}
		
		[Test]
		public void BasicHttpBinding_ImportEndpoints ()
		{
			var label = new TestLabel ("BasicHttpBinding_ImportEndpoints");
			
			var doc = TestContext.LoadMetadata ("BasicHttp");
			var sd = (WS.ServiceDescription)doc.MetadataSections [0].Metadata;
			
			label.EnterScope ("wsdl");
			Assert.That (sd.Bindings, Is.Not.Null, label.Get ());
			Assert.That (sd.Bindings.Count, Is.EqualTo (1), label.Get ());
			var binding = sd.Bindings [0];
			
			Assert.That (sd.Services, Is.Not.Null, label.Get ());
			Assert.That (sd.Services.Count, Is.EqualTo (1), label.Get ());
			var service = sd.Services [0];
			
			Assert.That (service.Ports, Is.Not.Null, label.Get ());
			Assert.That (service.Ports.Count, Is.EqualTo (1), label.Get ());
			var port = service.Ports [0];
			
			Assert.That (sd.PortTypes, Is.Not.Null, label.Get ());
			Assert.That (sd.PortTypes.Count, Is.EqualTo (1), label.Get ());
			var portType = sd.PortTypes [0];
			
			label.LeaveScope ();
			
			var importer = new WsdlImporter (doc);
			
			label.EnterScope ("by-service");
			var byService = importer.ImportEndpoints (service);
			BindingTestAssertions.CheckImportErrors (importer, label);
			Assert.That (byService, Is.Not.Null, label.Get ());
			Assert.That (byService.Count, Is.EqualTo (1), label.Get ());
			label.LeaveScope ();
			
			label.EnterScope ("by-binding");
			var byBinding = importer.ImportEndpoints (binding);
			BindingTestAssertions.CheckImportErrors (importer, label);
			Assert.That (byBinding, Is.Not.Null, label.Get ());
			Assert.That (byBinding.Count, Is.EqualTo (1), label.Get ());
			label.LeaveScope ();
			
			label.EnterScope ("by-port-type");
			var byPortType = importer.ImportEndpoints (portType);
			BindingTestAssertions.CheckImportErrors (importer, label);
			Assert.That (byPortType, Is.Not.Null, label.Get ());
			Assert.That (byPortType.Count, Is.EqualTo (1), label.Get ());
			label.LeaveScope ();
		}

	}
}

