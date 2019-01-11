//
// ServiceModelSectionGroupTest.cs
//
// Author:
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
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Text;
using System.Xml;

using NUnit.Framework;

using ConfigurationType = System.Configuration.Configuration;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Configuration
{
	[TestFixture]
	public class ServiceModelSectionGroupTest
	{
		ServiceModelSectionGroup GetConfig (string file)
		{
			// FIXME: this should work.
			//ConfigurationType c = ConfigurationManager.OpenExeConfiguration (file);
			//return ServiceModelSectionGroup.GetSectionGroup (c);

			return (ServiceModelSectionGroup) ConfigurationManager.OpenExeConfiguration (file).GetSectionGroup ("system.serviceModel");
		}

		[Test]
		public void GetSectionGroup ()
		{
			ServiceModelSectionGroup g = GetConfig (TestResourceHelper.GetFullPathOfResource ("Test/config/test1"));
			Assert.IsNotNull (g.Bindings, "bindings");
			Assert.IsNotNull (g.Client, "client");
			Assert.IsNotNull (g.Services, "services");
			Assert.IsNotNull (g.Client.Endpoints, "client/endpoint*");
		}

		[Test]
		[Category ("NotWorking")]
		[Ignore ("fails under .NET; I never bothered to fix the test")]
		public void BindingCollections () {
			ServiceModelSectionGroup g = GetConfig (TestResourceHelper.GetFullPathOfResource ("Test/config/test1.config"));
			List<BindingCollectionElement> coll = g.Bindings.BindingCollections;
			Assert.AreEqual (20, coll.Count, "Count");
		}

		[Test]
		public void Endpoints ()
		{
			ServiceModelSectionGroup g = GetConfig (TestResourceHelper.GetFullPathOfResource ("Test/config/test1"));
			ChannelEndpointElementCollection col = g.Client.Endpoints;
			Assert.AreEqual (1, col.Count, "initial count");
			ChannelEndpointElement e = col [0];
			Assert.AreEqual (String.Empty, e.Name, "0.Name");
			Assert.AreEqual ("IFoo", e.Contract, "0.Contract");
			Assert.AreEqual ("basicHttpBinding", e.Binding, "0.Binding");
			col.Add (new ChannelEndpointElement ());
			Assert.AreEqual (2, col.Count, "after Add()");
		}
	}
}
#endif
