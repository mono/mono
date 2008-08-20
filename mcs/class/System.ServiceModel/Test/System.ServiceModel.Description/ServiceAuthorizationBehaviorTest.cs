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
#if USE_DEPRECATED
// This class is not deprecated, but most of members and depending classes
// have changed, so most of the code is not reusable.

using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Description
{
	[TestFixture]
	public class ServiceAuthorizationBehaviorTest
	{
		[Test]
		public void DefaultValues ()
		{
			ServiceAuthorizationBehavior b =
				new ServiceAuthorizationBehavior ();
			Assert.IsNull (b.AuthorizationDomain, "#1");
			Assert.IsFalse (b.ImpersonateCallerForAllServiceOperations,
				"#2");
			Assert.IsNull (b.OperationRequirement, "#3");
			Assert.AreEqual (PrincipalPermissionMode.UseWindowsGroups,
				b.PrincipalPermissionMode, "#4");
		}

		[Test]
		public void ApplyBehavior ()
		{
			ServiceHost host = new ServiceHost (typeof (object));
			EndpointDispatcher d = new EndpointDispatcher (host, "a", "");
			DispatchRuntime db = d.Behavior;
			EndpointDispatcher d2 = new EndpointDispatcher (host, "a", "");
			DispatchRuntime db2 = d2.Behavior;

			ServiceAuthorizationBehavior b =
				new ServiceAuthorizationBehavior ();
			b.ImpersonateCallerForAllServiceOperations = true;
			b.OperationRequirement = new MyRequirement ();
			b.PrincipalPermissionMode = PrincipalPermissionMode.None;

			Collection<DispatchRuntime> c =
				new Collection<DispatchRuntime> (
					new DispatchRuntime [] {db, db2});
			Collection<BindingParameterCollection> pc =
				new Collection<BindingParameterCollection> (
					new BindingParameterCollection [] {
						new BindingParameterCollection ()});
			((IServiceBehavior) b).ApplyBehavior (null, host, c, pc);

			Assert.IsNull (db.AuthorizationDomain, "#1-1");
			Assert.IsTrue (db.ImpersonateCallerForAllServiceOperations, "#1-2");
			Assert.AreEqual (typeof (MyRequirement),
				db.OperationRequirement.GetType (), "#1-3");
			Assert.AreEqual (PrincipalPermissionMode.None,
				db.PrincipalPermissionMode, "#1-4");
			Assert.IsNull (db2.AuthorizationDomain, "#2-1");
			Assert.IsTrue (db2.ImpersonateCallerForAllServiceOperations, "#2-2");
			/*
			Assert.AreEqual (typeof (MyRequirement),
				db2.OperationRequirement.GetType (), "#2-3");
			Assert.AreEqual (PrincipalPermissionMode.None,
				db2.PrincipalPermissionMode, "#2-4");
			*/
		}

		/*
		class MyRequirement : OperationRequirement
		{
			public override bool AccessCheck (OperationContext ctx)
			{
				return true;
			}
		}
		*/
	}
}
#endif
