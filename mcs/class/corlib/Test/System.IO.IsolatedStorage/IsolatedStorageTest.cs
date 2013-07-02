//
// IsolatedStorageTest.cs - Unit Tests for abstract IsolatedStorage class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell Inc. (http://www.novell.com)
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

using System;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.IO.IsolatedStorage;

using NUnit.Framework;

namespace MonoTests.System.IO.IsolatedStorageTest {

	// note: IsolatedStorage is abstract so we create a
	// non-abstract class to test it

	// naming a class with the same name as a namespace is a BAD idea
	public class NonAbstractIsolatedStorage : global::System.IO.IsolatedStorage.IsolatedStorage
	{
		public NonAbstractIsolatedStorage ()
		{
			// no InitStore here
		}

		public NonAbstractIsolatedStorage (IsolatedStorageScope scope, Type domain, Type assembly)
		{
			InitStore (scope, domain, assembly);
		}

		public NonAbstractIsolatedStorage(IsolatedStorageScope scope, Type application)
		{
			InitStore (scope, application);
		}

		protected override IsolatedStoragePermission GetPermission (PermissionSet ps)
		{
			throw new NotImplementedException();
		}

		public override void Remove ()
		{
			throw new NotImplementedException();
		}

		public char PublicSeparatorExternal {
			get { return base.SeparatorExternal; }
		}

		public char PublicSeparatorInternal {
			get { return base.SeparatorInternal; }
		}
	}

	[TestFixture]
	public class IsolatedStorageTest {

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsolatedStorage_Bad_Null ()
		{
			IsolatedStorageScope bad = (IsolatedStorageScope)Int32.MinValue;
			NonAbstractIsolatedStorage nais = new NonAbstractIsolatedStorage (bad, null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsolatedStorage_None_Null ()
		{
			NonAbstractIsolatedStorage nais = new NonAbstractIsolatedStorage (IsolatedStorageScope.None, null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsolatedStorage_None ()
		{
			Type t = typeof (object);
			NonAbstractIsolatedStorage nais = new NonAbstractIsolatedStorage (IsolatedStorageScope.None, t, t);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsolatedStorage_User ()
		{
			NonAbstractIsolatedStorage nais = new NonAbstractIsolatedStorage (IsolatedStorageScope.User, null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsolatedStorage_Domain ()
		{
			NonAbstractIsolatedStorage nais = new NonAbstractIsolatedStorage (IsolatedStorageScope.Domain, null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsolatedStorage_Assembly ()
		{
			NonAbstractIsolatedStorage nais = new NonAbstractIsolatedStorage (IsolatedStorageScope.Assembly, null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsolatedStorage_Roaming ()
		{
			NonAbstractIsolatedStorage nais = new NonAbstractIsolatedStorage (IsolatedStorageScope.Roaming, null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsolatedStorage_Machine ()
		{
			NonAbstractIsolatedStorage nais = new NonAbstractIsolatedStorage (IsolatedStorageScope.Machine, null, null);
		}

		[Test]
#if !MOBILE
		[ExpectedException (typeof (IsolatedStorageException))]
#endif
		public void IsolatedStorage_Application ()
		{
			NonAbstractIsolatedStorage nais = new NonAbstractIsolatedStorage (IsolatedStorageScope.Application, null);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void IsolatedStorage_AssemblyUser ()
		{
			NonAbstractIsolatedStorage nais = new NonAbstractIsolatedStorage (IsolatedStorageScope.Assembly | IsolatedStorageScope.User, null, null);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void IsolatedStorage_AssemblyUserDomain ()
		{
			NonAbstractIsolatedStorage nais = new NonAbstractIsolatedStorage (IsolatedStorageScope.Assembly | IsolatedStorageScope.User | IsolatedStorageScope.Domain, null, null);
		}

		[Test]
		public void IsolatedStorage ()
		{
			NonAbstractIsolatedStorage nais = new NonAbstractIsolatedStorage ();
			Assert.AreEqual (IsolatedStorageScope.None, nais.Scope, "Scope");
			Assert.AreEqual (Path.DirectorySeparatorChar, nais.PublicSeparatorExternal, "SeparatorExternal");
			Assert.AreEqual ('.', nais.PublicSeparatorInternal, "SeparatorInternal");
		}
#if NET_2_0
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void IsolatedStorage_ApplicationIdentity ()
		{
			NonAbstractIsolatedStorage nais = new NonAbstractIsolatedStorage ();
			object o = nais.ApplicationIdentity;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void IsolatedStorage_AssemblyIdentity ()
		{
			NonAbstractIsolatedStorage nais = new NonAbstractIsolatedStorage ();
			object o = nais.AssemblyIdentity;
		}
#else
		[Test]
		public void IsolatedStorage_AssemblyIdentity ()
		{
			NonAbstractIsolatedStorage nais = new NonAbstractIsolatedStorage ();
			Assert.IsNull (nais.AssemblyIdentity, "AssemblyIdentity");
		}
#endif

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void IsolatedStorage_CurrentSize ()
		{
			NonAbstractIsolatedStorage nais = new NonAbstractIsolatedStorage ();
			ulong ul = nais.CurrentSize;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void IsolatedStorage_DomainIdentity ()
		{
			NonAbstractIsolatedStorage nais = new NonAbstractIsolatedStorage ();
			object o = nais.DomainIdentity;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void IsolatedStorage_MaximumSize ()
		{
			NonAbstractIsolatedStorage nais = new NonAbstractIsolatedStorage ();
			ulong ul = nais.MaximumSize;
		}
		
		[Test]
		public void MultiLevel ()
		{
			// see bug #4101
			IsolatedStorageFile isf;
#if MOBILE
			isf = IsolatedStorageFile.GetUserStoreForApplication ();
#else
			isf = IsolatedStorageFile.GetStore (IsolatedStorageScope.User |  IsolatedStorageScope.Assembly | IsolatedStorageScope.Domain,
	   					typeof (global::System.Security.Policy.Url), typeof (global::System.Security.Policy.Url));
#endif

			try {
				isf.CreateDirectory ("dir1");
				string [] dirs = isf.GetDirectoryNames ("*");
				Assert.AreEqual (1, dirs.Length, "1a");
				Assert.AreEqual ("dir1", dirs [0], "1b");
	
				isf.CreateDirectory ("dir1/test");
				dirs = isf.GetDirectoryNames ("dir1/*");
				Assert.AreEqual (1, dirs.Length, "2a");
				Assert.AreEqual ("test", dirs [0], "2b");
	
				isf.CreateDirectory ("dir1/test/test2a");
				isf.CreateDirectory ("dir1/test/test2b");
				dirs = isf.GetDirectoryNames ("dir1/test/*");
				Assert.AreEqual (2, dirs.Length, "3a");
				Assert.AreEqual ("test2a", dirs [0], "3b");
				Assert.AreEqual ("test2b", dirs [1], "3c");
			} finally {
				isf.DeleteDirectory ("dir1/test/test2a");
				isf.DeleteDirectory ("dir1/test/test2b");
				isf.DeleteDirectory ("dir1/test");
				isf.DeleteDirectory ("dir1");
			}
		}

#if NET_4_0
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void IsolatedStorage_UsedSize ()
		{
			NonAbstractIsolatedStorage nais = new NonAbstractIsolatedStorage ();
			Console.WriteLine (nais.UsedSize);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void IsolatedStorage_AvailableFreeSpace ()
		{
			NonAbstractIsolatedStorage nais = new NonAbstractIsolatedStorage ();
			Console.WriteLine (nais.AvailableFreeSpace);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void IsolatedStorage_Quota ()
		{
			NonAbstractIsolatedStorage nais = new NonAbstractIsolatedStorage ();
			Console.WriteLine (nais.Quota);
		}

		[Test]
		public void IsolatedStorage_IncreaseQuotaTo ()
		{
			NonAbstractIsolatedStorage nais = new NonAbstractIsolatedStorage ();
			Assert.AreEqual (false, nais.IncreaseQuotaTo (-10), "#A0");
			Assert.AreEqual (false, nais.IncreaseQuotaTo (0), "#A1");
			Assert.AreEqual (false, nais.IncreaseQuotaTo (100), "#A2");
		}
#endif
	}
}
