//
// ServiceEndpointTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.Net.Security;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class ServiceEndpointTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CtorNullContract ()
		{
			new ServiceEndpoint (null, new BasicHttpBinding (),
				new EndpointAddress ("http://localhost"));
		}

		[Test]
		// null Binding is allowed, dunno how it should be handled tho
		public void CtorNullBinding ()
		{
			ServiceEndpoint ep = new ServiceEndpoint (
				ContractDescription.GetContract (typeof (Foo)),
				null,
				new EndpointAddress ("http://localhost"));
			Assert.IsNull (ep.Binding, "#1");
		}

		[Test]
		// null endpoint is allowed.
		public void CtorNullEndpoint ()
		{
			new ServiceEndpoint (
				ContractDescription.GetContract (typeof (Foo)),
				new BasicHttpBinding (),
				null);
		}

		[Test]
		public void DefaultValues ()
		{
			ServiceEndpoint ep = new ServiceEndpoint (
				ContractDescription.GetContract (typeof (Foo)),
				new BasicHttpBinding (),
				new EndpointAddress ("http://localhost"));
			Assert.IsNotNull (ep.Behaviors, "#1");
			Assert.AreEqual (0, ep.Behaviors.Count, "#2");
		}


		[ServiceContract]
		class Foo
		{
			[OperationContract]
			public void SayWhat () { }
		}
	}
}
