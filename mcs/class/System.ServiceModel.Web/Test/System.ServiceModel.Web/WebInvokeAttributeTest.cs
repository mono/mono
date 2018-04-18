//
// WebInvokeAttributeTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using NUnit.Framework;

using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.ServiceModel.Description
{
	[TestFixture]
	public class WebInvokeAttributeTest
	{
		[Test]
		public void IOperationBehaviorMethods ()
		{
			IOperationBehavior oper = new WebInvokeAttribute ();
			var pl = new BindingParameterCollection ();
			var od = ContractDescription.GetContract (typeof (TestService)).Operations [0];
			oper.AddBindingParameters (od, pl);
			Assert.AreEqual (0, pl.Count, "#1");

			// yeah it really does nothing.
			oper.AddBindingParameters (null, null);

			oper.ApplyClientBehavior (od, null);
			oper.ApplyDispatchBehavior (od, null);
			oper.Validate (od);
		}

		[Test]
		public void RejectTwoParametersWhenNotWrapped ()
		{
			var factory = new WebChannelFactory<IBogusService1> (new WebHttpBinding (), new Uri ("http://localhost:37564"));

#if MOBILE
			factory.Endpoint.Behaviors.Add (new WebHttpBehavior ());
#endif

			Assert.Throws<InvalidOperationException> (() => factory.CreateChannel ());
		}

		[ServiceContract]
		public interface TestService
		{
			[OperationContract]
			string TestMethod (string input);
		}

		[ServiceContract]
		public interface IBogusService1
		{
			[OperationContract]
			[WebInvoke]
			string Join (string s1, string s2);
		}
	}
}
