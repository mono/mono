//
// MetadataElementTest.cs
//
// Author:
//	Igor Zelmanovich <igorz@mainsoft.com>
//
// Copyright (C) 2008 Mainsoft, Inc.  http://www.mainsoft.com
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
using NUnit.Framework;
using System.ServiceModel.Configuration;
using System.Configuration;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Configuration
{
	[TestFixture]
	public class MetadataElementTest
	{
		ServiceModelSectionGroup OpenConfig (string name) {
			return (ServiceModelSectionGroup) ConfigurationManager.OpenExeConfiguration (TestResourceHelper.GetFullPathOfResource ("Test/config/" + name)).GetSectionGroup ("system.serviceModel");
		}

		[Test]
		public void PolicyImporters () {
			ServiceModelSectionGroup config = OpenConfig ("client.metadata");
			PolicyImporterElementCollection col = config.Client.Metadata.PolicyImporters;

			Assert.AreEqual (2, col.Count, "Count");

			PolicyImporterElement item = col ["PolicyImporterType1"];
			if (item == null)
				Assert.Fail ("PolicyImporterType1 not exists");

			Assert.AreEqual ("PolicyImporterType1", item.Type, "PolicyImporterType1.Type");

			item = col ["PolicyImporterType2"];
			if (item == null)
				Assert.Fail ("PolicyImporterType2 not exists");

			Assert.AreEqual ("PolicyImporterType2", item.Type, "PolicyImporterType2.Type");
		}

		[Test]
		public void WsdlImporters () {
			ServiceModelSectionGroup config = OpenConfig ("client.metadata");
			WsdlImporterElementCollection col = config.Client.Metadata.WsdlImporters;

			Assert.AreEqual (2, col.Count, "Count");

			WsdlImporterElement item = col ["WSDLImporter1"];
			if (item == null)
				Assert.Fail ("WSDLImporter1 not exists");

			Assert.AreEqual ("WSDLImporter1", item.Type, "WSDLImporter1.Type");

			item = col ["WSDLImporter2"];
			if (item == null)
				Assert.Fail ("WSDLImporter2 not exists");

			Assert.AreEqual ("WSDLImporter2", item.Type, "WSDLImporter2.Type");
		}

		[Test]
		public void PolicyImporters_DefaultConfiguration () {
			ServiceModelSectionGroup config = OpenConfig ("empty");
			PolicyImporterElementCollection col = config.Client.Metadata.PolicyImporters;

			Type [] types = new Type [] {
				typeof(CompositeDuplexBindingElementImporter),
				typeof(MessageEncodingBindingElementImporter),
				typeof(OneWayBindingElementImporter),
				typeof(PrivacyNoticeBindingElementImporter),
				typeof(ReliableSessionBindingElementImporter),
				typeof(SecurityBindingElementImporter),
				typeof(TransactionFlowBindingElementImporter),
				typeof(TransportBindingElementImporter),
				typeof(UseManagedPresentationBindingElementImporter)
			};
			foreach (Type type in types) {
				PolicyImporterElement item = col [type.AssemblyQualifiedName];
				if (item == null)
					Assert.Fail (type.Name + " not exists");

				Assert.AreEqual (type.AssemblyQualifiedName, item.Type, type.Name);
			}
		}

		[Test]
		public void WsdlImporters_DefaultConfiguration () {
			ServiceModelSectionGroup config = OpenConfig ("empty");
			WsdlImporterElementCollection col = config.Client.Metadata.WsdlImporters;

			Type [] types = new Type [] { 
				typeof(MessageEncodingBindingElementImporter),
				typeof(StandardBindingImporter),
				typeof(TransportBindingElementImporter),
				typeof(DataContractSerializerMessageContractImporter),
				typeof(XmlSerializerMessageContractImporter)
			};
			foreach (Type type in types) {
				WsdlImporterElement item = col [type.AssemblyQualifiedName];
				if (item == null)
					Assert.Fail (type.Name + " not exists");

				Assert.AreEqual (type.AssemblyQualifiedName, item.Type, type.Name);
			}
		}

	}
}
#endif
