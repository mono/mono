// AssemblyInstallerTest.cs
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
using System.Configuration.Install;
using System.Reflection;
using System.IO;
using System.Collections;

namespace MonoTests.System.Configuration.Install
{

[TestFixture]
public class AssemblyInstallerTest {
	private AssemblyInstaller ins;
	private string[] args;
	private Assembly testAssembly;
	private const string testAssemblyPath = "Test/System.Configuration.Install/InstallerAssembly.dll";
	StringWriter log;

	[SetUp]
	public void SetUp ()
	{
		testAssembly = Assembly.LoadFrom (testAssemblyPath);
		args = new string [] { "/Option1", "--Option2", "-Option3=val", "LogFile=", "LogToConsole" };
		ins = new AssemblyInstaller (testAssemblyPath, args);
		ins.UseNewContext = false;
		log = new StringWriter ();
		log.NewLine = "\n";
		Console.SetOut (log);
	}

	// Testing constructor with no arguments
	[Test]
	public void TestCtor01 ()
	{
		AssemblyInstaller i = new AssemblyInstaller ();
		Assert.AreEqual (null, i.Path, "#AINSaa01");
		Assert.AreEqual (null, i.Assembly, "#AINSaa02");
	}

	// Testing Assembly property and Path property
	// I tried to write another test function that assigns Assembly propery
	// and checks Path property value.  But the problem I encountered was that
	// the assembly path is something from temporary directory.  So I could not
	// do that.
	[Test]
	public void TestAssemblyAndPath ()
	{
		AssemblyInstaller i = new AssemblyInstaller ();
		i.Path = testAssemblyPath;
		Assert.AreEqual (testAssembly, i.Assembly, "#AINSab01");
	}

	// Test CommandLine property
	[Test]
	public void TestCommandLine ()
	{
		Assert.AreEqual (args, ins.CommandLine, "#AINSac01");
	}

	// Test HelpText property
	[Test]
	public void TestHelpText ()
	{
		Assert.IsTrue (ins.HelpText.IndexOf ("This is help text from the installer.") != -1, "#AINSad01");
	}

	// Test Installers.Count propery
	[Test]
	public void TestInstallersCount ()
	{
		string t = ins.HelpText;	// calling this would update Installers collection
		Assert.AreEqual (1, ins.Installers.Count, "#AINSae01");
	}

	// Test CheckIfInstallable method with non-existing assembly
	[Test]
	[ExpectedException (typeof (Exception))]
	[Category ("NotDotNet")]
	// It should throw Exception, as per documentation.
	// But it throws FileNotFoundExceptionin Microsoft implementation
	public void TestCheckIfInstallable01 ()
	{
		AssemblyInstaller.CheckIfInstallable("NON-EXIST.dll");
	}

	// Test CheckIfInstallable method with non-installable assembly
	[Test]
	[ExpectedException (typeof (Exception))]
	[Category ("NotDotNet")]
	// It should throw Exception, as per documentation.
	// But it throws InvalidOperationException in Microsoft implementation
	public void TestCheckIfInstallable02 ()
	{
		AssemblyInstaller.CheckIfInstallable ("../lib/default/System.Configuration.Install.dll");
	}

	// Test CheckIfInstallable method with installable assembly
	[Test]
	public void TestCheckIfInstallable03 ()
	{
		AssemblyInstaller.CheckIfInstallable (testAssemblyPath);
	}

	// Test Install method
	[Test]
	public void TestInstall ()
	{
		ins.Install (new Hashtable ());
		string output = log.ToString ();
		Assert.IsTrue (output.IndexOf (">>InstallerAssembly.Install called") != -1, "#AINSaf01");
		Assert.AreEqual (-1, output.IndexOf (">>This should not be called"), "#AINSaf02");
	}

	// Test Commit method
	[Test]
	public void TestCommit ()
	{
		Hashtable ht = new Hashtable ();
		ins.Install (ht);
		ins.Commit (ht);
		string output = log.ToString ();
		Assert.IsTrue (output.IndexOf (">>InstallerAssembly.Install called") != -1, "#AINSag01");
		Assert.IsTrue (output.IndexOf (">>InstallerAssembly.Commit called") != -1, "#AINSag02");
	}

	// Test Rollback method
	[Test]
	public void TestRollback ()
	{
		Hashtable ht = new Hashtable ();
		ins.Install (ht);
		ins.Rollback (ht);
		string output = log.ToString ();
		Assert.IsTrue (output.IndexOf (">>InstallerAssembly.Install called") != -1, "#AINSah01");
		Assert.IsTrue (output.IndexOf (">>InstallerAssembly.Rollback called") != -1, "#AINSah02");
	}

	// Test Uninstall method
	[Test]
	public void TestUninstall ()
	{
		Hashtable ht = new Hashtable ();
		ins.Install (ht);
		ins.Uninstall (ht);
		string output = log.ToString ();
		Assert.IsTrue (output.IndexOf (">>InstallerAssembly.Uninstall called") != -1, "#AINSai01");
	}
}

}
