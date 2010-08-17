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
	public class EndpointDiscoveryMetadataTest
	{
		[Test]
		public void DefaultValues ()
		{
			var edm = new EndpointDiscoveryMetadata ();
			Assert.IsNotNull (edm.Address, "#1");
			Assert.AreEqual (EndpointAddress.AnonymousUri, edm.Address.Uri, "#1-2");
			Assert.AreEqual (0, edm.Version, "#2");
			Assert.IsNotNull (edm.ContractTypeNames, "#3");
			Assert.IsNotNull (edm.Scopes, "#4");
			Assert.IsNotNull (edm.ListenUris, "#5");
			Assert.IsNotNull (edm.Extensions, "#6");
		}
	}
}
