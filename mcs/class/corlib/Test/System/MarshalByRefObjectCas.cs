//
// MarshalByRefObjectCas.cs - CAS unit tests for System.MarshalByRefObject
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;

using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace MonoCasTests.System {

	[TestFixture]
	[Category ("CAS")]
	public class MarshalByRefObjectCas {

		private MarshalByRefObject mbro;

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");

			// we do this in SetUp because we want the security 
			// stack to be "normal/empty" so each unit test can 
			// mess with it as it wishes
			mbro = (MarshalByRefObject) AppDomain.CurrentDomain;
		}

		// we use reflection to call the AppDomain (who inherits from MarshalByRefObject)
		// as it's methods are protected by LinkDemand (which will be converted into full
		// demand, i.e. a stack walk) when reflection is used (i.e. it gets testable).

		[Test]
		[SecurityPermission (SecurityAction.Deny, Infrastructure = true)]
		[ExpectedException (typeof (SecurityException))]
		public void CreateObjRef ()
		{
			MethodInfo mi = typeof (AppDomain).GetMethod ("CreateObjRef");
			Assert.IsNotNull (mi.Invoke (mbro, new object [1] { typeof (int) }), "CreateObjRef");
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, Infrastructure = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetLifetimeService ()
		{

			MethodInfo mi = typeof (AppDomain).GetMethod ("GetLifetimeService");
			Assert.IsNotNull (mi.Invoke (mbro, null), "GetLifetimeService");
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, Infrastructure = true)]
		public void AppDomain_InitializeLifetimeService ()
		{
			// special case - AppDomain doesn't call base.InitializeLifetimeService
			// so there is no link, nor the stack required, to initiate the Demand
			MethodInfo mi = typeof (AppDomain).GetMethod ("InitializeLifetimeService");
			Assert.IsNull (mi.Invoke (mbro, null), "InitializeLifetimeService");
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, Infrastructure = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Stream_InitializeLifetimeService ()
		{
			MethodInfo mi = typeof (Stream).GetMethod ("InitializeLifetimeService");
			Assert.IsNull (mi.Invoke (Stream.Null, null), "InitializeLifetimeService");
		}
	}
}
