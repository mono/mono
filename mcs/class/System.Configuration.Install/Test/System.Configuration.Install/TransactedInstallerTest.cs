// TransactedInstallerTest.cs
//   NUnit Test Cases for System.Configuration.Install.TransactedInstaller class
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
public class TransactedInstallerTest {
	private Installer ins;
	private MyInstaller sub1, sub2, sub3;
	private CallInfo BfInstEvt, AfInstEvt;
	private CallInfo CommittingEvt, CommittedEvt;
	private CallInfo BfRbackEvt, AfRbackEvt;
	private CallInfo BfUninsEvt, AfUninsEvt;
	private Hashtable state;

	[SetUp]
	public void SetUp ()
	{
		ins = new TransactedInstaller ();
		state = new Hashtable ();
		sub1 = new SucceedInstaller ();
		sub2 = new FailureInstaller ();
		sub3 = new SucceedInstaller ();

		BfInstEvt = new CallInfo ();
		AfInstEvt = new CallInfo ();
		CommittingEvt = new CallInfo ();
		CommittedEvt = new CallInfo ();
		BfRbackEvt = new CallInfo ();
		AfRbackEvt = new CallInfo ();
		BfUninsEvt = new CallInfo ();
		AfUninsEvt = new CallInfo ();

		ins.Installers.Add (sub1);
		string [] cmdLine = new string [] { "/logToConsole=false" };
		ins.Context = new InstallContext ("", cmdLine);	// no log file

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
	[Category ("NotDotNet")]
	// As per documentation, this should throw ArgumentException,
	// but Microsoft implementation throws InvalidOperationException
	public void TestInstall01 ()
	{
		ins.Install (null);
	}

	// Testing Install method -- successful installation
	[Test]
	public void TestInstall02 ()
	{
		ins.Installers.Add (sub3);
		ins.Install (state);

		Assert.IsTrue (sub1.ciInstall.methodCalled, "#TRINaa01");
		Assert.IsTrue (sub1.ciCommit.methodCalled, "#TRINaa02");
		Assert.IsFalse (sub1.ciRollback.methodCalled, "#TRINaa03");
		Assert.IsFalse (sub1.ciUninstall.methodCalled, "#TRINaa04");

		Assert.IsTrue (sub3.ciInstall.methodCalled, "#TRINaa05");
		Assert.IsTrue (sub3.ciCommit.methodCalled, "#TRINaa06");
		Assert.IsFalse (sub3.ciRollback.methodCalled, "#TRINaa07");
		Assert.IsFalse (sub3.ciUninstall.methodCalled, "#TRINaa08");

		Assert.IsTrue (sub1.ciInstall.timeOfCall > BfInstEvt.timeOfCall,
				"#TRINaa09");
		Assert.IsTrue (sub3.ciInstall.timeOfCall > BfInstEvt.timeOfCall,
				"#TRINaa10");
		Assert.IsTrue (sub1.ciInstall.timeOfCall < AfInstEvt.timeOfCall,
				"#TRINaa11");
		Assert.IsTrue (sub3.ciInstall.timeOfCall < AfInstEvt.timeOfCall,
				"#TRINaa12");
		
		Assert.IsTrue (sub1.ciCommit.timeOfCall > CommittingEvt.timeOfCall,
				"#TRINaa13");
		Assert.IsTrue (sub3.ciCommit.timeOfCall > CommittingEvt.timeOfCall,
				"#TRINaa14");
		Assert.IsTrue (sub1.ciCommit.timeOfCall < CommittedEvt.timeOfCall,
				"#TRINaa15");
		Assert.IsTrue (sub3.ciCommit.timeOfCall < CommittedEvt.timeOfCall,
				"#TRINaa16");
	}

	// Testing Install method -- unsuccessful installation
	[Test]
	[ExpectedException (typeof (Exception))]
	[Category ("NotDotNet")]
	// As per documentation, this should throw Exception,
	// but Microsoft implementation throws InvalidOperationException
	public void TestInstall03 ()
	{
		ins.Installers.Add (sub2);
		ins.Install (state);
	}

	// Testing Install method -- unsuccessful installation
	// In this test case, we are testing rollback functionality
	[Test]
	public void TestInstall04 ()
	{
		ins.Installers.Add (sub2);
		try {
			ins.Install (state);
		} catch (Exception) {
			// Ignore this exception
		}

		Assert.IsTrue (sub1.ciInstall.methodCalled, "#TRINab01");
		Assert.IsFalse (sub1.ciCommit.methodCalled, "#TRINab02");
		Assert.IsTrue (sub1.ciRollback.methodCalled, "#TRINab03");
		Assert.IsFalse (sub1.ciUninstall.methodCalled, "#TRINab04");

		Assert.IsTrue (sub2.ciInstall.methodCalled, "#TRINab05");
		Assert.IsFalse (sub2.ciCommit.methodCalled, "#TRINab06");
		Assert.IsTrue (sub2.ciRollback.methodCalled, "#TRINab07");
		Assert.IsFalse (sub2.ciUninstall.methodCalled, "#TRINab08");

		Assert.IsTrue (sub1.ciInstall.timeOfCall > BfInstEvt.timeOfCall,
				"#TRINab09");
		Assert.IsTrue (sub2.ciInstall.timeOfCall > BfInstEvt.timeOfCall,
				"#TRINab10");

		Assert.IsTrue (sub1.ciRollback.timeOfCall > BfRbackEvt.timeOfCall,
				"#TRINab13");
		Assert.IsTrue (sub2.ciRollback.timeOfCall > BfRbackEvt.timeOfCall,
				"#TRINab14");
		Assert.IsTrue (sub1.ciRollback.timeOfCall < AfRbackEvt.timeOfCall,
				"#TRINab15");
		Assert.IsTrue (sub2.ciRollback.timeOfCall < AfRbackEvt.timeOfCall,
				"#TRINab16");
	}

	// Testing Install method with null argument
	[Test]
	public void TestUninstall01 ()
	{
		ins.Installers.Add (sub3);
		ins.Install (state);
		ins.Uninstall (null);

		Assert.IsTrue (sub1.ciInstall.methodCalled, "#TRINac01");
		Assert.IsTrue (sub1.ciCommit.methodCalled, "#TRINac02");
		Assert.IsFalse (sub1.ciRollback.methodCalled, "#TRINac03");
		Assert.IsTrue (sub1.ciUninstall.methodCalled, "#TRINac04");

		Assert.IsTrue (sub3.ciInstall.methodCalled, "#TRINac05");
		Assert.IsTrue (sub3.ciCommit.methodCalled, "#TRINac06");
		Assert.IsFalse (sub3.ciRollback.methodCalled, "#TRINac07");
		Assert.IsTrue (sub3.ciUninstall.methodCalled, "#TRINac08");

		Assert.IsTrue (sub1.ciUninstall.timeOfCall > BfUninsEvt.timeOfCall,
				"#TRINac09");
		Assert.IsTrue (sub3.ciUninstall.timeOfCall > BfUninsEvt.timeOfCall,
				"#TRINac10");
		Assert.IsTrue (sub1.ciUninstall.timeOfCall < AfUninsEvt.timeOfCall,
				"#TRINac11");
		Assert.IsTrue (sub3.ciUninstall.timeOfCall < AfUninsEvt.timeOfCall,
				"#TRINac12");

		Assert.IsTrue (sub1.ciCommit.timeOfCall > CommittingEvt.timeOfCall,
				"#TRINac13");
		Assert.IsTrue (sub3.ciCommit.timeOfCall > CommittingEvt.timeOfCall,
				"#TRINac14");
		Assert.IsTrue (sub1.ciCommit.timeOfCall < CommittedEvt.timeOfCall,
				"#TRINac15");
		Assert.IsTrue (sub3.ciCommit.timeOfCall < CommittedEvt.timeOfCall,
				"#TRINac16");
	}

	// Testing Install method with corrupt argument
	[Test]
	[ExpectedException (typeof (ArgumentException))]
	[Category ("NotDotNet")]
	// As per documentation, this should throw ArgumentException,
	// but Microsoft implementation throws NullReferenceException
	public void TestUninstall02 ()
	{
		ins.Installers.Add (sub3);
		ins.Install (state);
		state.Clear ();
		ins.Uninstall (state);
	}

	// Testing Install method with null argument
	[Test]
	public void TestUninstall03 ()
	{
		ins.Installers.Add (sub3);
		ins.Install (state);
		ins.Uninstall (state);

		Assert.IsTrue (sub1.ciInstall.methodCalled, "#TRINad01");
		Assert.IsTrue (sub1.ciCommit.methodCalled, "#TRINad02");
		Assert.IsFalse (sub1.ciRollback.methodCalled, "#TRINad03");
		Assert.IsTrue (sub1.ciUninstall.methodCalled, "#TRINad04");

		Assert.IsTrue (sub3.ciInstall.methodCalled, "#TRINad05");
		Assert.IsTrue (sub3.ciCommit.methodCalled, "#TRINad06");
		Assert.IsFalse (sub3.ciRollback.methodCalled, "#TRINad07");
		Assert.IsTrue (sub3.ciUninstall.methodCalled, "#TRINad08");

		Assert.IsTrue (sub1.ciUninstall.timeOfCall > BfUninsEvt.timeOfCall,
				"#TRINad09");
		Assert.IsTrue (sub3.ciUninstall.timeOfCall > BfUninsEvt.timeOfCall,
				"#TRINad10");
		Assert.IsTrue (sub1.ciUninstall.timeOfCall < AfUninsEvt.timeOfCall,
				"#TRINad11");
		Assert.IsTrue (sub3.ciUninstall.timeOfCall < AfUninsEvt.timeOfCall,
				"#TRINad12");

		Assert.IsTrue (sub1.ciCommit.timeOfCall > CommittingEvt.timeOfCall,
				"#TRINad13");
		Assert.IsTrue (sub3.ciCommit.timeOfCall > CommittingEvt.timeOfCall,
				"#TRINad14");
		Assert.IsTrue (sub1.ciCommit.timeOfCall < CommittedEvt.timeOfCall,
				"#TRINad15");
		Assert.IsTrue (sub3.ciCommit.timeOfCall < CommittedEvt.timeOfCall,
				"#TRINad16");
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

	private class SucceedInstaller : MyInstaller {
		// Nothing to override
	}

	private class FailureInstaller : MyInstaller {
		public override void Install (IDictionary state)
		{
			base.Install (state);
			throw new Exception("I always fail.");
		}

		public override void Uninstall (IDictionary state)
		{
			base.Uninstall (state);
			throw new Exception("I always fail.");
		}
	}
}

}
