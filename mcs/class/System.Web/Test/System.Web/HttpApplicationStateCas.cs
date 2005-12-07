//
// HttpApplicationStateCas.cs 
//	- CAS unit tests for System.Web.HttpApplicationStateCas
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
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Web;

namespace MonoCasTests.System.Web {

	[TestFixture]
	[Category ("CAS")]
	public class HttpApplicationStateCas : AspNetHostingMinimal {

		private HttpContext context;
		private HttpApplicationState appstate;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// running at fulltrust
			context = new HttpContext (null);
			appstate = context.Application;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Properties_Deny_Unrestricted ()
		{
			Assert.IsNotNull (appstate.AllKeys, "AllKeys");
			Assert.IsNotNull (appstate.Contents, "Contents");
			Assert.IsNotNull (appstate.Count, "Count");
			appstate["mono"] = "monkey";
			Assert.AreEqual ("monkey", appstate["mono"], "this[string]");
			Assert.AreEqual ("monkey", appstate[0], "this[int]");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Methods_Deny_Unrestricted ()
		{
			appstate.Add (String.Empty, String.Empty);
			Assert.IsNotNull (appstate.Get (String.Empty), "Get(string)");
			Assert.IsNotNull (appstate.Get (0), "Get(int)");
			Assert.IsNotNull (appstate.GetKey (0), "GetKey(int)");
			appstate.Remove (String.Empty);
			appstate.RemoveAll ();
			appstate.Set (String.Empty, String.Empty);
			appstate.RemoveAt (0);
			appstate.Lock ();
			appstate.UnLock ();
			appstate.Clear ();
		}

		// LinkDemand

		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			// there are no public ctor so we're taking a method that we know isn't protected
			// (by a Demand) and call it thru reflection so any linkdemand (on the class) will
			// be promoted to a Demand
			MethodInfo mi = this.Type.GetProperty ("AllKeys").GetGetMethod ();
			return mi.Invoke (appstate, null);
		}

		public override Type Type {
			get { return typeof (HttpApplicationState); }
		}
	}
}
