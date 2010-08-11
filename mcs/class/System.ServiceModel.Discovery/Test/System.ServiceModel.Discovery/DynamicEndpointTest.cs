//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.ServiceModel.Dispatcher;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Discovery
{
	[TestFixture]
	public class DynamicEndpointTest
	{
		[Test]
		public void DefaultValues ()
		{
			var cd = ContractDescription.GetContract (typeof (ITestService));
			var binding = new BasicHttpBinding ();
			var de = new DynamicEndpoint (cd, binding);
			Assert.AreEqual (DiscoveryClientBindingElement.DiscoveryEndpointAddress, de.Address, "#1");
			var fc = de.FindCriteria;
			Assert.IsNotNull (fc, "#2");
			Assert.AreEqual (1, fc.ContractTypeNames.Count, "#2-2");
			Assert.AreEqual (0, fc.Scopes.Count, "#2-3");
			Assert.AreEqual (0, fc.Extensions.Count, "#2-4");
			Assert.IsNotNull (de.DiscoveryEndpointProvider, "#3");
		}
	}
}
