//
// TimerCas.cs - CAS unit tests for System.Timers.Timer
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
using System.ComponentModel;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Timers;

namespace MonoCasTests.System.Timers {

	class TestSyncInvoke: ISynchronizeInvoke {

		public IAsyncResult BeginInvoke (Delegate method, object[] args)
		{
			return null;
		}

		public object EndInvoke (IAsyncResult result)
		{
			return null;
		}

		public object Invoke (Delegate method, object[] args)
		{
			return EndInvoke (BeginInvoke (method, args));
		}

		public bool InvokeRequired {
			get { return true; }
		}
	}

	[TestFixture]
	[NUnit.Framework.Category ("CAS")]
	public class TimerCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		private void Callback (object sender, ElapsedEventArgs e)
		{
		}

		private void CommonTests (Timer t)
		{
			Assert.IsTrue (t.AutoReset, "AutoReset");
			t.AutoReset = false;
			Assert.IsFalse (t.Enabled, "Enabled");
			t.Enabled = true;
			Assert.IsNull (t.Site, "Site");
			t.Site = null;
			Assert.IsNull (t.SynchronizingObject, "SynchronizingObject");
			t.SynchronizingObject = new TestSyncInvoke ();

			t.Elapsed += new ElapsedEventHandler (Callback);
			t.Elapsed -= new ElapsedEventHandler (Callback);

			t.BeginInit ();
			t.EndInit ();
			t.Start ();
			t.Stop ();
			t.Close ();

			(t as IDisposable).Dispose ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor_Deny_Unrestricted ()
		{
			Timer t = new Timer ();
			Assert.AreEqual (100.0, t.Interval, "Interval");
			t.Interval = 200.0;
			CommonTests (t);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void ConstructorDouble_Deny_Unrestricted ()
		{
			Timer t = new Timer (200.0);
			Assert.AreEqual (200.0, t.Interval, "Interval");
			t.Interval = 100.0;
			CommonTests (t);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			ConstructorInfo ci = typeof (Timer).GetConstructor (new Type [0]);
			Assert.IsNotNull (ci, "default .ctor");
			Assert.IsNotNull (ci.Invoke (null), "invoke");
		}
	}
}
