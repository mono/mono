//
// ElapsedEventArgsCas.cs - CAS unit tests for System.Timers.ElapsedEventArgs
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
using System.Timers;
using ST = System.Threading;

namespace MonoCasTests.System.Timers {

	[TestFixture]
	[Category ("CAS")]
	public class ElapsedEventArgsCas {

		Timer t;
		ElapsedEventArgs eea;

		private void SetElapsedEventArgs (object sender, ElapsedEventArgs e)
		{
			eea = e;
		}

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// fulltrust
			t = new Timer (1);
			t.Elapsed += new ElapsedEventHandler (SetElapsedEventArgs);
			t.Enabled = true;
			ST.Thread.Sleep (100);
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Deny_Unrestricted ()
		{
			Assert.IsNotNull (eea, "ElapsedEventArgs");
			DateTime dt = eea.SignalTime;
			DateTime now = DateTime.Now;
			Assert.IsTrue (dt > now.AddSeconds (-2), ">");
			Assert.IsTrue (dt < now.AddSeconds (2), "<");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			MethodInfo mi = typeof (ElapsedEventArgs).GetProperty ("SignalTime").GetGetMethod ();
			Assert.IsNotNull (mi, "SignalTime");
			Assert.IsNotNull (mi.Invoke (eea, null), "invoke");
		}
	}
}

