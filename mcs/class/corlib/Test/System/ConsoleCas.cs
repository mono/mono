//
// ConsoleCas.cs - CAS unit tests for System.Console
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
using System.Security;
using System.Security.Permissions;

namespace MonoCasTests.System {

	[TestFixture]
	[Category ("CAS")]
	public class ConsoleCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		// Partial Trust Tests - i.e. call "normal" unit with reduced privileges

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void PartialTrust_DenyUnrestricted_Success ()
		{
			MonoTests.System.ConsoleTest ct = new MonoTests.System.ConsoleTest ();
			// call most unit tests from ConsoleTest
			ct.TestError ();
			ct.TestIn ();
			ct.TestOut ();
			ct.TestOpenStandardError ();
			ct.TestOpenStandardInput ();
			ct.TestOpenStandardOutput ();
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		public void PartialTrust_PermitOnly_UnmanagedCode ()
		{
			MonoTests.System.ConsoleTest ct = new MonoTests.System.ConsoleTest ();
			// call unit tests from ConsoleTest that requires UnmanagedCode to work
			ct.TestSetError ();
			ct.TestSetIn ();
			ct.TestSetOut ();
			ct.TestRead ();
			ct.TestReadLine ();
			ct.TestWrite ();
			ct.TestWriteLine ();
		}

		// Console is using restricted stuff (like opening FileStream 
		// with handles) but isn't restricted itself - so we must 
		// assert the required permission in Console

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void OpenStandardError ()
		{
			Stream s = Console.OpenStandardError ();
			Assert.IsNotNull (s, "1");
			s = Console.OpenStandardError (1024);
			Assert.IsNotNull (s, "2");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void OpenStandardInput ()
		{
			Stream s = Console.OpenStandardInput ();
			Assert.IsNotNull (s, "1");
			s = Console.OpenStandardInput (1024);
			Assert.IsNotNull (s, "2");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void OpenStandardOutput ()
		{
			Stream s = Console.OpenStandardOutput ();
			Assert.IsNotNull (s, "1");
			s = Console.OpenStandardOutput (1024);
			Assert.IsNotNull (s, "2");
		}

		// Unmanaged code is required to call Console.Set* methods

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void SetError ()
		{
			Console.SetError (new StreamWriter (Console.OpenStandardError ()));
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void SetIn ()
		{
			Console.SetIn (new StreamReader (Console.OpenStandardInput ()));
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void SetOut ()
		{
			Console.SetOut (new StreamWriter (Console.OpenStandardOutput ()));
		}
	}
}
