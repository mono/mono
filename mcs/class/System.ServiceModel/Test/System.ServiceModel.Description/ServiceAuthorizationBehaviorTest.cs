//
// ServiceAuthorizationBehaviorTest.cs
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
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Description
{
	[TestFixture]
	public class ServiceAuthorizationBehaviorTest
	{
		[Test]
		public void DefaultValues ()
		{
			var b = new ServiceAuthorizationBehavior ();
			Assert.IsNull (b.ExternalAuthorizationPolicies, "#1-1");
			Assert.IsFalse (b.ImpersonateCallerForAllOperations, "#1-2");
			Assert.AreEqual (PrincipalPermissionMode.UseWindowsGroups, b.PrincipalPermissionMode, "#1-3");

			ServiceHost host = new ServiceHost (typeof (TestService));
			b = host.Description.Behaviors.Find<ServiceAuthorizationBehavior> ();
			Assert.IsNull (b.ExternalAuthorizationPolicies, "#2-1");
			Assert.IsFalse (b.ImpersonateCallerForAllOperations, "#2-2");
			Assert.AreEqual (PrincipalPermissionMode.UseWindowsGroups, b.PrincipalPermissionMode, "#2-3");
		}

		[Test]
		public void Validate ()
		{
			var b = new ServiceAuthorizationBehavior ();
			IServiceBehavior sb = b;
			sb.Validate (new ServiceDescription (),
			new ServiceHost (typeof (object)));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Category ("NotWorking")]
		public void ImpersonateCallerWithNoValidOperation ()
		{
			ServiceHost host = new ServiceHost (typeof (TestService));
			var b = host.Description.Behaviors.Find<ServiceAuthorizationBehavior> ();
			b.ImpersonateCallerForAllOperations = true;
			b.PrincipalPermissionMode = PrincipalPermissionMode.None;

			host.AddServiceEndpoint (typeof (TestService), new BasicHttpBinding (), new Uri ("http://localhost:" + NetworkHelpers.FindFreePort ()));

			host.Open ();
		}

		[Test]
		public void ApplyBehavior ()
		{
			ServiceHost host = new ServiceHost (typeof (TestService2));
			var b = host.Description.Behaviors.Find<ServiceAuthorizationBehavior> ();
			b.ImpersonateCallerForAllOperations = false;
			b.PrincipalPermissionMode = PrincipalPermissionMode.None;

			host.AddServiceEndpoint (typeof (TestService2), new BasicHttpBinding (), new Uri ("http://localhost:" + NetworkHelpers.FindFreePort ()));

			host.Open ();
			var ed = ((ChannelDispatcher) host.ChannelDispatchers [0]).Endpoints [0];
			var db = ed.DispatchRuntime;
			host.Close ();

			Assert.IsFalse (db.ImpersonateCallerForAllOperations, "#1");
			Assert.AreEqual (PrincipalPermissionMode.None,
				db.PrincipalPermissionMode, "#2");
		}

		[ServiceContract]
		public class TestService
		{
			[OperationContract]
			public void Foo () {}
		}

		[ServiceContract]
		public class TestService2
		{
			[OperationContract]
			[OperationBehavior (Impersonation = ImpersonationOption.Allowed)]
			public void Foo () {}
		}
	}
}
#endif
