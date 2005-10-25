//
// CompilerParametersCas.cs 
//	- CAS unit tests for System.CodeDom.Compiler.CompilerParameters
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
using System.CodeDom.Compiler;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

namespace MonoCasTests.System.CodeDom.Compiler {

	[TestFixture]
	[Category ("CAS")]
	public class CompilerParametersCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor0_Deny_Unrestricted ()
		{
			CompilerParameters cp = new CompilerParameters ();
			Assert.IsNull (cp.CompilerOptions, "CompilerOptions");
			cp.CompilerOptions = "-debug";
			Assert.IsNull (cp.Evidence, "Evidence");
			Assert.IsFalse (cp.GenerateExecutable, "GenerateExecutable");
			cp.GenerateExecutable = true;
			Assert.IsFalse (cp.GenerateInMemory, "GenerateInMemory");
			cp.GenerateInMemory = true;
			Assert.IsFalse (cp.IncludeDebugInformation, "IncludeDebugInformation");
			cp.IncludeDebugInformation = true;
			Assert.IsNull (cp.MainClass, "MainClass");
			cp.MainClass = "Program";
			Assert.IsNull (cp.OutputAssembly, "OutputAssembly");
			cp.OutputAssembly = "mono.dll";
			Assert.AreEqual (0, cp.ReferencedAssemblies.Count, "ReferencedAssemblies");
			Assert.AreEqual (0, cp.TempFiles.Count, "TempFiles");
			cp.TempFiles = new TempFileCollection ();
			Assert.AreEqual (IntPtr.Zero, cp.UserToken, "UserToken");
			cp.UserToken = (IntPtr) 1;
			Assert.AreEqual (-1, cp.WarningLevel, "WarningLevel");
			cp.WarningLevel = 0;
			Assert.IsNull (cp.Win32Resource, "Win32Resource");
			cp.Win32Resource = "*";
#if NET_2_0
			Assert.AreEqual (0, cp.EmbeddedResources.Count, "EmbeddedResources");
			Assert.AreEqual (0, cp.LinkedResources.Count, "LinkedResources");
#endif
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor1_Deny_Unrestricted ()
		{
			CompilerParameters cp = new CompilerParameters (new string[1] { "mono.exe" });
			Assert.IsNull (cp.CompilerOptions, "CompilerOptions");
			cp.CompilerOptions = "-debug";
			Assert.IsNull (cp.Evidence, "Evidence");
			Assert.IsFalse (cp.GenerateExecutable, "GenerateExecutable");
			cp.GenerateExecutable = true;
			Assert.IsFalse (cp.GenerateInMemory, "GenerateInMemory");
			cp.GenerateInMemory = true;
			Assert.IsFalse (cp.IncludeDebugInformation, "IncludeDebugInformation");
			cp.IncludeDebugInformation = true;
			Assert.IsNull (cp.MainClass, "MainClass");
			cp.MainClass = "Program";
			Assert.IsNull (cp.OutputAssembly, "OutputAssembly");
			cp.OutputAssembly = "mono.dll";
			Assert.AreEqual (1, cp.ReferencedAssemblies.Count, "ReferencedAssemblies");
			Assert.AreEqual (0, cp.TempFiles.Count, "TempFiles");
			cp.TempFiles = new TempFileCollection ();
			Assert.AreEqual (IntPtr.Zero, cp.UserToken, "UserToken");
			cp.UserToken = (IntPtr) 1;
			Assert.AreEqual (-1, cp.WarningLevel, "WarningLevel");
			cp.WarningLevel = 0;
			Assert.IsNull (cp.Win32Resource, "Win32Resource");
			cp.Win32Resource = "*";
#if NET_2_0
			Assert.AreEqual (0, cp.EmbeddedResources.Count, "EmbeddedResources");
			Assert.AreEqual (0, cp.LinkedResources.Count, "LinkedResources");
#endif
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor2_Deny_Unrestricted ()
		{
			CompilerParameters cp = new CompilerParameters (new string[1] { "mono.exe" }, "mono.dll");
			Assert.IsNull (cp.CompilerOptions, "CompilerOptions");
			cp.CompilerOptions = "-debug";
			Assert.IsNull (cp.Evidence, "Evidence");
			Assert.IsFalse (cp.GenerateExecutable, "GenerateExecutable");
			cp.GenerateExecutable = true;
			Assert.IsFalse (cp.GenerateInMemory, "GenerateInMemory");
			cp.GenerateInMemory = true;
			Assert.IsFalse (cp.IncludeDebugInformation, "IncludeDebugInformation");
			cp.IncludeDebugInformation = true;
			Assert.IsNull (cp.MainClass, "MainClass");
			cp.MainClass = "Program";
			Assert.AreEqual ("mono.dll", cp.OutputAssembly, "OutputAssembly");
			cp.OutputAssembly = null;
			Assert.AreEqual (1, cp.ReferencedAssemblies.Count, "ReferencedAssemblies");
			Assert.AreEqual (0, cp.TempFiles.Count, "TempFiles");
			cp.TempFiles = new TempFileCollection ();
			Assert.AreEqual (IntPtr.Zero, cp.UserToken, "UserToken");
			cp.UserToken = (IntPtr) 1;
			Assert.AreEqual (-1, cp.WarningLevel, "WarningLevel");
			cp.WarningLevel = 0;
			Assert.IsNull (cp.Win32Resource, "Win32Resource");
			cp.Win32Resource = "*";
#if NET_2_0
			Assert.AreEqual (0, cp.EmbeddedResources.Count, "EmbeddedResources");
			Assert.AreEqual (0, cp.LinkedResources.Count, "LinkedResources");
#endif
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor3_Deny_Unrestricted ()
		{
			CompilerParameters cp = new CompilerParameters (new string[1] { "mono.exe" }, "mono.dll", true);
			Assert.IsNull (cp.CompilerOptions, "CompilerOptions");
			cp.CompilerOptions = "-debug";
			Assert.IsNull (cp.Evidence, "Evidence");
			Assert.IsFalse (cp.GenerateExecutable, "GenerateExecutable");
			cp.GenerateExecutable = true;
			Assert.IsFalse (cp.GenerateInMemory, "GenerateInMemory");
			cp.GenerateInMemory = true;
			Assert.IsTrue (cp.IncludeDebugInformation, "IncludeDebugInformation");
			cp.IncludeDebugInformation = false;
			Assert.IsNull (cp.MainClass, "MainClass");
			cp.MainClass = "Program";
			Assert.AreEqual ("mono.dll", cp.OutputAssembly, "OutputAssembly");
			cp.OutputAssembly = null;
			Assert.AreEqual (1, cp.ReferencedAssemblies.Count, "ReferencedAssemblies");
			Assert.AreEqual (0, cp.TempFiles.Count, "TempFiles");
			cp.TempFiles = new TempFileCollection ();
			Assert.AreEqual (IntPtr.Zero, cp.UserToken, "UserToken");
			cp.UserToken = (IntPtr) 1;
			Assert.AreEqual (-1, cp.WarningLevel, "WarningLevel");
			cp.WarningLevel = 0;
			Assert.IsNull (cp.Win32Resource, "Win32Resource");
			cp.Win32Resource = "*";
#if NET_2_0
			Assert.AreEqual (0, cp.EmbeddedResources.Count, "EmbeddedResources");
			Assert.AreEqual (0, cp.LinkedResources.Count, "LinkedResources");
#endif
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlEvidence = true)]
		public void Evidence_PermitOnly_Unrestricted ()
		{
			CompilerParameters cp = new CompilerParameters ();
			cp.Evidence = new Evidence ();
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlEvidence = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Evidence_Deny_Unrestricted ()
		{
			CompilerParameters cp = new CompilerParameters ();
			cp.Evidence = new Evidence ();
		}

		[Test]
		public void LinkDemand_No_Restriction ()
		{
			ConstructorInfo ci = typeof (CompilerParameters).GetConstructor (new Type[0]);
			Assert.IsNotNull (ci, "default .ctor()");
			Assert.IsNotNull (ci.Invoke (null), "invoke");
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "Mono")]
		[ExpectedException (typeof (SecurityException))]
		public void LinkDemand_Deny_Anything ()
		{
			// denying anything results in a non unrestricted permission set
			ConstructorInfo ci = typeof (CompilerParameters).GetConstructor (new Type[0]);
			Assert.IsNotNull (ci, "default .ctor()");
			Assert.IsNotNull (ci.Invoke (null), "invoke");
		}
	}
}
