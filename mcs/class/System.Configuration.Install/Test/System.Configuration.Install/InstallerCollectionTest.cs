// InstallerCollectionTest.cs
//   NUnit Test Cases for System.Configuration.Install.InstallContext class
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
using System.Collections.Specialized;

namespace MonoTests.System.Configuration.Install
{

[TestFixture]
public class InstallerCollectionTest {
	private TransactedInstaller ti;
	private InstallerCollection ic;

	[SetUp]
	public void SetUp ()
	{
		Installer[] ins;

		ins = new AssemblyInstaller[3];
		ins[0] = new AssemblyInstaller ();
		ins[1] = new AssemblyInstaller ();
		ins[2] = new AssemblyInstaller ();

		ti = new TransactedInstaller ();
		ic = ti.Installers;
		ic.AddRange (ins);
	}

	// Testing Add method
	// In addition, it tests these members also:
	// 		Count
	// 		Item (int) // indexer
	// 		Contains (Installer)
	// 		IndexOf (Installer)
	[Test]
	public void TestAdd ()
	{
		Installer[] ins;
		InstallerCollection lic;

		lic = new TransactedInstaller ().Installers;
		ins = new AssemblyInstaller[3];
		ins[0] = new AssemblyInstaller ();
		ins[1] = new AssemblyInstaller ();
		ins[2] = new AssemblyInstaller ();

		lic.Add (ins[0]);
		lic.Add (ins[1]);

		Assert.AreEqual (2, lic.Count, "#ICOLaa01");
		Assert.AreEqual (ins[0], lic[0], "#ICOLaa02");
		Assert.AreEqual (ins[1], lic[1], "#ICOLaa03");
		Assert.IsTrue (lic.Contains (ins[0]), "#ICOLaa04");
		Assert.IsTrue (lic.Contains (ins[1]), "#ICOLaa05");
		Assert.IsFalse (lic.Contains (ins[2]), "#ICOLaa06");
		Assert.AreEqual (0, lic.IndexOf (ins[0]), "#ICOLaa07");
		Assert.AreEqual (1, lic.IndexOf (ins[1]), "#ICOLaa08");
		Assert.AreEqual (-1, lic.IndexOf (ins[2]), "#ICOLaa08");

		lic.Add (ins[2]);
		Assert.AreEqual (3, lic.Count, "#ICOLaa09");
		Assert.AreEqual (ins[0], lic[0], "#ICOLaa10");
		Assert.AreEqual (ins[1], lic[1], "#ICOLaa11");
		Assert.AreEqual (ins[2], lic[2], "#ICOLaa12");
		Assert.IsTrue (lic.Contains (ins[0]), "#ICOLaa13");
		Assert.IsTrue (lic.Contains (ins[1]), "#ICOLaa14");
		Assert.IsTrue (lic.Contains (ins[2]), "#ICOLaa15");
		Assert.AreEqual (0, lic.IndexOf (ins[0]), "#ICOLaa16");
		Assert.AreEqual (1, lic.IndexOf (ins[1]), "#ICOLaa17");
		Assert.AreEqual (2, lic.IndexOf (ins[2]), "#ICOLaa18");
	}

	// Testing AddRange method
	[Test]
	public void TestAddRange01 ()
	{
		Installer[] ins;
		InstallerCollection lic;

		lic = new TransactedInstaller ().Installers;
		ins = new AssemblyInstaller[3];
		ins[0] = new AssemblyInstaller ();
		ins[1] = new AssemblyInstaller ();
		ins[2] = new AssemblyInstaller ();

		lic.AddRange (ins);

		Assert.AreEqual (3, lic.Count, "#ICOLab01");
		Assert.AreEqual (ins[0], lic[0], "#ICOLab02");
		Assert.AreEqual (ins[1], lic[1], "#ICOLab03");
		Assert.AreEqual (ins[2], lic[2], "#ICOLab04");
		Assert.IsTrue (lic.Contains (ins[0]), "#ICOLab05");
		Assert.IsTrue (lic.Contains (ins[1]), "#ICOLab06");
		Assert.IsTrue (lic.Contains (ins[2]), "#ICOLab07");
		Assert.AreEqual (0, lic.IndexOf (ins[0]), "#ICOLab08");
		Assert.AreEqual (1, lic.IndexOf (ins[1]), "#ICOLab09");
		Assert.AreEqual (2, lic.IndexOf (ins[2]), "#ICOLab10");
	}

	// Testing Clear method
	[Test]
	public void TestClear ()
	{
		Assert.AreEqual (3, ic.Count, "#ICOLac01");
		ic.Clear ();
		Assert.AreEqual (0, ic.Count, "#ICOLac02");
	}

	// Testing CopyTo method
	[Test]
	public void TestCopyTo ()
	{
		Installer[] insa = new Installer[ti.Installers.Count];
		ic.CopyTo (insa, 0);
		Assert.AreEqual (ic[0], insa[0], "#ICOLad01");
		Assert.AreEqual (ic[1], insa[1], "#ICOLad02");
		Assert.AreEqual (ic[2], insa[2], "#ICOLad03");
	}

	// Testing Insert method
	[Test]
	public void TestInsert ()
	{
		Installer[] old = new Installer[ti.Installers.Count];
		ic.CopyTo (old, 0);
		Installer insert = new AssemblyInstaller ();
		ic.Insert (1, insert);
		Assert.AreEqual (old[0], ic[0], "#ICOLae01");
		Assert.AreEqual (insert, ic[1], "#ICOLae02");
		Assert.AreEqual (old[1], ic[2], "#ICOLae03");
		Assert.AreEqual (old[2], ic[3], "#ICOLae04");
	}

	// Testing Remove method
	[Test]
	public void Remove ()
	{
		Installer[] old = new Installer[ti.Installers.Count];
		ic.CopyTo (old, 0);
		ic.Remove (old[1]);
		Assert.AreEqual (old[0], ic[0], "#ICOLaf01");
		Assert.AreEqual (old[2], ic[1], "#ICOLaf02");
		Assert.AreEqual (2, ic.Count, "#ICOLaf03");
	}

	// Testing RemoveAt method -- normal execution
	[Test]
	public void RemoveAt01 ()
	{
		Installer[] old = new Installer[ti.Installers.Count];
		ic.CopyTo (old, 0);
		ic.RemoveAt (1);
		Assert.AreEqual (old[0], ic[0], "#ICOLag01");
		Assert.AreEqual (old[2], ic[1], "#ICOLag02");
		Assert.AreEqual (2, ic.Count, "#ICOLag03");
	}

	// Testing RemoveAt method -- exception case
	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void RemoveAt02 ()
	{
		ic.RemoveAt (-1);
	}

	// Testing RemoveAt method -- exception case
	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void RemoveAt03 ()
	{
		ic.RemoveAt (3);
	}
}

}
