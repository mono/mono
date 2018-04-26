//
// ServiceEndpointTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://novell.com
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
using System.Text;
using NUnit.Framework;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Description
{
	[TestFixture]
	public class ServiceEndpointTest
	{
		static ContractDescription contract1 = ContractDescription.GetContract (typeof (Foo));

		[Test]
		public void NullArguments ()
		{
			new ServiceEndpoint (contract1, null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullArguments2 ()
		{
			new ServiceEndpoint (null, null, null);
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void ListenUri ()
		{
			Uri uri = new Uri ("http://localhost:" + NetworkHelpers.FindFreePort ());
			var se = new ServiceEndpoint (contract1, null, new EndpointAddress (uri));
			Assert.AreEqual (uri, se.ListenUri, "#1");
		}

		#region contracts

		[ServiceContract]
		interface Foo
		{
			[OperationContract]
			string Echo (string input);
		}

		#endregion
	}
}
