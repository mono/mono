// InstallerTest.cs
//   NUnit Test Cases for System.Configuration.Install.Installer class
//
// Author:
//    Muthu Kannan (t.manki@gmail.com)
//
// (C) 2005 Novell, Inc.  http://www.novell.com/
// 

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
using System.Collections;
using System.Configuration.Install;
using System.Threading;

namespace MonoTests.System.Configuration.Install
{

[TestFixture]
public class InstallerTest {
	private Installer ins;
	private MyInstaller sub1, sub2;
	private CallInfo BfInstEvt, AfInstEvt;
	private CallInfo CommittingEvt, CommittedEvt;
	private CallInfo BfRbackEvt, AfRbackEvt;
	private CallInfo BfUninsEvt, AfUninsEvt;
	private Hashtable state;

	[SetUp]
	public void SetUp ()
	{
		ins = new Installer ();
		state = new Hashtable ();
		sub1 = new MyInstaller ();
		sub2 = new MyInstaller ();

		BfInstEvt = new CallInfo ();
		AfInstEvt = new CallInfo ();
		CommittingEvt = new CallInfo ();
		CommittedEvt = new CallInfo ();
		BfRbackEvt = new CallInfo ();
		AfRbackEvt = new CallInfo ();
		BfUninsEvt = new CallInfo ();
		AfUninsEvt = new CallInfo ();;

		ins.Installers.Add (sub1);
		ins.Installers.Add (sub2);

		ins.BeforeInstall += new InstallEventHandler (onBeforeInstall);
		ins.AfterInstall += new InstallEventHandler (onAfterInstall);
		ins.Committing += new InstallEventHandler (onCommitting);
		ins.Committed += new InstallEventHandler (onCommitted);
		ins.BeforeRollback += new InstallEventHandler (onBeforeRollback);
		ins.AfterRollback += new InstallEventHandler (onAfterRollback);
		ins.BeforeUninstall += new InstallEventHandler (onBeforeUninstall);
		ins.AfterUninstall += new InstallEventHandler (onAfterUninstall);
	}

	// Testing Install method with invalid argument
	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void TestInstall01 ()
	{
		ins.Install (null);
	}

	// Testing Install method
	[Test]
	public void TestInstall02 ()
	{
		ins.Install (state);

		Assert.IsTrue (sub1.ciInstall.methodCalled, "#INSTaa01");
		Assert.IsFalse (sub1.ciCommit.methodCalled, "#INSTaa02");
		Assert.IsFalse (sub1.ciRollback.methodCalled, "#INSTaa03");
		Assert.IsFalse (sub1.ciUninstall.methodCalled, "#INSTaa04");

		Assert.IsTrue (sub2.ciInstall.methodCalled, "#INSTaa05");
		Assert.IsFalse (sub2.ciCommit.methodCalled, "#INSTaa06");
		Assert.IsFalse (sub2.ciRollback.methodCalled, "#INSTaa07");
		Assert.IsFalse (sub2.ciUninstall.methodCalled, "#INSTaa08");

		Assert.IsTrue (sub1.ciInstall.timeOfCall > BfInstEvt.timeOfCall,
				"#INSTaa09");
		Assert.IsTrue (sub2.ciInstall.timeOfCall > BfInstEvt.timeOfCall,
				"#INSTaa10");
		Assert.IsTrue (sub1.ciInstall.timeOfCall < AfInstEvt.timeOfCall,
				"#INSTaa11");
		Assert.IsTrue (sub2.ciInstall.timeOfCall < AfInstEvt.timeOfCall,
				"#INSTaa12");
	}

	// Testing Commit method with null argument
	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void TestCommit01 ()
	{
		ins.Commit (null);
	}

	// Testing Commit method with empty hashtable argument
	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void TestCommit02 ()
	{
		ins.Commit (state);
	}

	// Testing Commit with proper arguments
	[Test]
	public void TestCommit03 ()
	{
		TestInstall02 ();	// Call Install method so that
					// state hash table is prepared
		ins.Commit (state);

		/*
		Assert.IsTrue (sub1.ciInstall.methodCalled, "#INSTab01");
		Assert.IsTrue (sub1.ciCommit.methodCalled, "#INSTab02");
		Assert.IsFalse (sub1.ciRollback.methodCalled, "#INSTab03");
		Assert.IsFalse (sub1.ciUninstall.methodCalled, "#INSTab04");

		Assert.IsTrue (sub2.ciInstall.methodCalled, "#INSTab05");
		Assert.IsTrue (sub2.ciCommit.methodCalled, "#INSTab06");
		Assert.IsFalse (sub2.ciRollback.methodCalled, "#INSTab07");
		Assert.IsFalse (sub2.ciUninstall.methodCalled, "#INSTab08");

		Assert.IsTrue (sub1.ciCommit.timeOfCall > CommittingEvt.timeOfCall,
				"#INSTab09");
		Assert.IsTrue (sub2.ciCommit.timeOfCall > CommittingEvt.timeOfCall,
				"#INSTab10");
		Assert.IsTrue (sub1.ciCommit.timeOfCall < CommittedEvt.timeOfCall,
				"#INSTab11");
		Assert.IsTrue (sub2.ciCommit.timeOfCall < CommittedEvt.timeOfCall,
				"#INSTab12");
				*/
	}

	// Testing Rollback method with null argument
	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void TestRollback01 ()
	{
		ins.Rollback (null);
	}

	// Testing Rollback method with empty hashtable argument
	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void TestRollback02 ()
	{
		ins.Rollback (state);
	}

	// Testing Rollback with proper arguments
	[Test]
	public void TestRollback03 ()
	{
		TestInstall02 ();	// Call Install method so that
					// state hash table is prepared
		ins.Rollback (state);

		Assert.IsTrue (sub1.ciInstall.methodCalled, "#INSTac01");
		Assert.IsFalse (sub1.ciCommit.methodCalled, "#INSTac02");
		Assert.IsTrue (sub1.ciRollback.methodCalled, "#INSTac03");
		Assert.IsFalse (sub1.ciUninstall.methodCalled, "#INSTac04");

		Assert.IsTrue (sub2.ciInstall.methodCalled, "#INSTac05");
		Assert.IsFalse (sub2.ciCommit.methodCalled, "#INSTac06");
		Assert.IsTrue (sub2.ciRollback.methodCalled, "#INSTac07");
		Assert.IsFalse (sub2.ciUninstall.methodCalled, "#INSTac08");

		Assert.IsTrue (sub1.ciRollback.timeOfCall > BfRbackEvt.timeOfCall,
				"#INSTac09");
		Assert.IsTrue (sub2.ciRollback.timeOfCall > BfRbackEvt.timeOfCall,
				"#INSTac10");
		Assert.IsTrue (sub1.ciRollback.timeOfCall < AfRbackEvt.timeOfCall,
				"#INSTac11");
		Assert.IsTrue (sub2.ciRollback.timeOfCall < AfRbackEvt.timeOfCall,
				"#INSTac12");
	}

	// Testing Uninstall method with proper argument
	[Test]
	public void TestUninstall01 ()
	{
		TestInstall02 ();	// Call Install method so that
					// state hash table is prepared
		ins.Uninstall (state);

		Assert.IsTrue (sub1.ciInstall.methodCalled, "#INSTad01");
		Assert.IsFalse (sub1.ciCommit.methodCalled, "#INSTad02");
		Assert.IsFalse (sub1.ciRollback.methodCalled, "#INSTad03");
		Assert.IsTrue (sub1.ciUninstall.methodCalled, "#INSTad04");

		Assert.IsTrue (sub2.ciInstall.methodCalled, "#INSTad05");
		Assert.IsFalse (sub2.ciCommit.methodCalled, "#INSTad06");
		Assert.IsFalse (sub2.ciRollback.methodCalled, "#INSTad07");
		Assert.IsTrue (sub2.ciUninstall.methodCalled, "#INSTad08");

		Assert.IsTrue (sub1.ciUninstall.timeOfCall > BfUninsEvt.timeOfCall,
				"#INSTad09");
		Assert.IsTrue (sub2.ciUninstall.timeOfCall > BfUninsEvt.timeOfCall,
				"#INSTad10");
		Assert.IsTrue (sub1.ciUninstall.timeOfCall < AfUninsEvt.timeOfCall,
				"#INSTad11");
		Assert.IsTrue (sub2.ciUninstall.timeOfCall < AfUninsEvt.timeOfCall,
				"#INSTad12");
	}

	// Testing Parent property
	[Test]
	public void TestParent01 ()
	{
		Assert.AreEqual (ins, sub1.Parent, "#INSTae01");
		Assert.AreEqual (ins, sub2.Parent, "#INSTae02");
	}

	[Test]
	[ExpectedException (typeof (InvalidOperationException))]
	public void TestParent02 ()
	{
		ins.Parent = sub1;
	}

	[Test]
	[ExpectedException (typeof (InvalidOperationException))]
	public void TestParent03 ()
	{
		Installer ins1 = new Installer ();
		sub1.Installers.Add (ins1);
		ins.Parent = ins1;
	}

	[Test]
	[ExpectedException (typeof (InvalidOperationException))]
	public void TestParent04 ()
	{
		Installer ins1 = new Installer ();
		Installer ins2 = new Installer ();
		ins1.Installers.Add (ins2);
		sub1.Installers.Add (ins1);
		ins.Parent = ins1;
	}

	private void onBeforeInstall (object sender, InstallEventArgs e)
	{
		BfInstEvt.SetCalled ();
	}

	private void onAfterInstall (object sender, InstallEventArgs e)
	{
		AfInstEvt.SetCalled ();
	}

	private void onCommitting (object sender, InstallEventArgs e)
	{
		CommittingEvt.SetCalled ();
	}

	private void onCommitted (object sender, InstallEventArgs e)
	{
		CommittedEvt.SetCalled ();
	}

	private void onBeforeRollback (object sender, InstallEventArgs e)
	{
		BfRbackEvt.SetCalled ();
	}

	private void onAfterRollback (object sender, InstallEventArgs e)
	{
		AfRbackEvt.SetCalled ();
	}

	private void onBeforeUninstall (object sender, InstallEventArgs e)
	{
		BfUninsEvt.SetCalled ();
	}

	private void onAfterUninstall (object sender, InstallEventArgs e)
	{
		AfUninsEvt.SetCalled ();
	}

	private struct CallInfo
	{
		public bool methodCalled;
		public DateTime timeOfCall;
		private const int SLEEP_TIME = 60;	// waiting time in ms

		public void SetCalled ()
		{
			methodCalled = true;
			// Wait for some time so that time comparison is effective
			Thread.Sleep (SLEEP_TIME);
			timeOfCall = DateTime.Now;
		}
	}

	// This is a custom installer base class
	private class MyInstaller : Installer {
		public CallInfo ciInstall;
		public CallInfo ciCommit;
		public CallInfo ciRollback;
		public CallInfo ciUninstall;

		public MyInstaller ()
		{
			ciInstall = new CallInfo ();
			ciCommit = new CallInfo ();
			ciRollback = new CallInfo ();
			ciUninstall = new CallInfo ();
		}

		public override void Install (IDictionary state)
		{
			base.Install (state);
			ciInstall.SetCalled ();
		}

		public override void Commit (IDictionary state)
		{
			base.Commit (state);
			ciCommit.SetCalled ();
		}

		public override void Rollback (IDictionary state)
		{
			base.Rollback (state);
			ciRollback.SetCalled ();
		}

		public override void Uninstall (IDictionary state)
		{
			base.Uninstall (state);
			ciUninstall.SetCalled ();
		}
	}
}

}
