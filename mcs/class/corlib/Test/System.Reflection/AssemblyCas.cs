//
// AssemblyCas.cs - CAS unit tests for System.Reflection.Assembly
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
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace MonoCasTests.System.Reflection {

	[TestFixture]
	[Category ("CAS")]
	public class AssemblyCas {

		private MonoTests.System.Reflection.AssemblyTest at;
		private Assembly corlib;
		private Assembly corlib_test;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			at = new MonoTests.System.Reflection.AssemblyTest ();
			corlib = typeof (int).Assembly;
			corlib_test = Assembly.GetExecutingAssembly ();
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		// Partial Trust Tests - i.e. call "normal" unit with reduced privileges

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void PartialTrust_Deny_Unrestricted ()
		{
			at.CreateInstance ();
			at.CreateInvalidInstance ();
			at.GetAssembly ();
			at.GetReferencedAssemblies ();

			Assert.IsNotNull (Assembly.GetCallingAssembly (), "GetCallingAssembly");
			Assembly.GetEntryAssembly (); // null for MS, non-null with Mono

			Assert.IsTrue (corlib.GetCustomAttributes (true).Length > 0, "GetCustomAttribute");
			Assert.IsTrue (corlib.GetExportedTypes ().Length > 0, "GetExportedTypes");
			Assert.IsTrue (corlib.GetLoadedModules (true).Length > 0, "GetLoadedModules(true)");
			Assert.IsNotNull (corlib.ToString (), "ToString");

			Module[] ms = corlib.GetModules (true);
			Assert.IsTrue (ms.Length > 0, "GetModules(true)");
			// can't use ms [0].Name as this requires PathDiscovery 
			// but ToString return the same value without the check
			Assert.IsNotNull (corlib.GetModule (ms [0].ToString ()), "GetModule");

			corlib.GetManifestResourceNames ();

			Assembly corlib_test = Assembly.GetExecutingAssembly ();
			Assert.AreEqual (corlib_test.GetCustomAttributes (true).Length, 
				corlib_test.GetCustomAttributes (false).Length, "GetCustomAttribute true==false");
			Assert.AreEqual (corlib_test.GetLoadedModules ().Length,
				corlib_test.GetLoadedModules (false).Length, "GetLoadedModules()==(false)");
			Assert.AreEqual (corlib_test.GetModules ().Length,
				corlib_test.GetModules (false).Length, "GetModules()==(false)");

			Assert.IsTrue (corlib_test.GetReferencedAssemblies ().Length > 0, "GetReferencedAssemblies");
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlEvidence = true)]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void PartialTrust_PermitOnly_ControlEvidenceFileIOPermission ()
		{
			at.Corlib_test ();
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlEvidence = true)]
		[ExpectedException (typeof (SecurityException))]
		public void PartialTrust_Deny_ControlEvidence ()
		{
			Assert.IsNotNull (corlib_test.Evidence, "Evidence");
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void CodeBase_Deny_FileIOPermission ()
		{
			Assert.IsNotNull (corlib_test.CodeBase, "CodeBase");
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void EscapedCodeBase_Deny_FileIOPermission ()
		{
			Assert.IsNotNull (corlib_test.EscapedCodeBase, "EscapedCodeBase");
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Location_Deny_FileIOPermission ()
		{
			Assert.IsNotNull (corlib_test.Location, "Location");
		}

		[Test]
		public void GetFile_PermitOnly_FileIOPermission ()
		{
			FileStream[] fss = corlib.GetFiles (false);
			if (fss.Length > 0) {
				foreach (FileStream fs in fss) {
					GetFile_PermitOnly (fs.Name);
				}
			}
		}

		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		private void GetFile_PermitOnly (string filename)
		{
			corlib.GetFile (filename);
		}

		[Test]
		public void GetFile_Deny_FileIOPermission ()
		{
			FileStream[] fss = corlib.GetFiles (false);
			if (fss.Length > 0) {
				foreach (FileStream fs in fss) {
					GetFile_Deny (fs.Name);
				}
			}
			// note: we already know the name
		}

		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		private void GetFile_Deny (string filename)
		{
			corlib.GetFile (filename);
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		public void GetFile_Unexisting_Deny ()
		{
			corlib.GetFile ("TOTO");
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void GetFiles_PermitOnly_FileIOPermission ()
		{
			at.GetFiles_False ();
			at.GetFiles_True ();
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		public void GetFilesFalse_Deny_FileIOPermission ()
		{
			try {
				FileStream[] fss = corlib.GetFiles (false);
				if (fss.Length != 0)
					Assert.Fail ("Expected SecurityException");
			}
			catch (SecurityException) {
				// so there was at least one (like on MS runtime)
			}
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		public void GetFilesTrue_Deny_FileIOPermission ()
		{
			try {
				FileStream[] fss = corlib.GetFiles (true);
				if (fss.Length != 0)
					Assert.Fail ("Expected SecurityException");
			}
			catch (SecurityException) {
				// so there was at least one (like on MS runtime)
			}
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetName_Deny_FileIOPermission ()
		{
			corlib.GetName ();
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void GetName_PermitOnly_FileIOPermission ()
		{
			corlib.GetName ();
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		public void LoadWithPartialName_Deny_FileIOPermission ()
		{
			// FileIOPermission isn't (always) required for LoadWithPartialName
			// e.g. in this case both assemblies are already loaded in memory
			at.LoadWithPartialName ();
		}
#if !NET_2_0
		// that one is unclear (undocumented) and doesn't happen in 2.0
		// will not be implemented in Mono unless if find out why...
		[Category ("NotWorking")]
		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void LoadWithPartialName_Deny_SecurityPermission ()
		{
			at.LoadWithPartialName ();
		}
#endif
		// we use reflection to call Assembly as some methods and events are protected 
		// by LinkDemand (which will be converted into full demand, i.e. a stack walk)
		// when reflection is used (i.e. it gets testable).

		[Test]
		[SecurityPermission (SecurityAction.Deny, SerializationFormatter = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetObjectData ()
		{
			SerializationInfo info = null;
			StreamingContext context = new StreamingContext (StreamingContextStates.All);
			Assembly a = Assembly.GetExecutingAssembly ();
			MethodInfo mi = typeof (Assembly).GetMethod ("GetObjectData");
			mi.Invoke (a, new object [2] { info, context });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void AddModuleResolve ()
		{
			Assembly a = Assembly.GetExecutingAssembly ();
			MethodInfo mi = typeof (Assembly).GetMethod ("add_ModuleResolve");
			mi.Invoke (a, new object [1] { null });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void RemoveModuleResolve ()
		{
			Assembly a = Assembly.GetExecutingAssembly ();
			MethodInfo mi = typeof (Assembly).GetMethod ("remove_ModuleResolve");
			mi.Invoke (a, new object [1] { null });
		}
	}
}
