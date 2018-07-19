//
// System.Runtime.InteropServices.SafeHandle Test Cases
//
// Authors:
// 	Miguel de Icaza (miguel@novell.com)
//
// Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
//
using NUnit.Framework;
using System;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32.SafeHandles;

namespace MonoTests.System.Runtime.InteropServices
{
	[TestFixture]
	public class SafeHandleTest 
	{
		//
		// This mimics SafeFileHandle, but does not actually own a handle
		// We use this to test ownership and dispose exceptions.
		//
		public class FakeSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
		{
			public bool released = false;
			public bool disposed = false;
			
			public FakeSafeHandle (): base (true)
			{
			}
			
			public FakeSafeHandle (bool ownership) : base (ownership)
			{
			}

			public void ChangeHandle (IntPtr hnd)
			{
				this.handle = hnd;
			}

			protected override bool ReleaseHandle ()
			{
				released = true;
				return true;
			}

			protected override void Dispose (bool manual)
			{
				disposed = true;
				base.Dispose (manual);
			}
		}

		[Test]
		public void SimpleDispose ()
		{
			FakeSafeHandle sf = new FakeSafeHandle ();
			sf.Dispose ();
		}

		[Test]
		public void BadDispose1 ()
		{
			FakeSafeHandle sf = new FakeSafeHandle ();

			sf.DangerousRelease ();

			try {
				sf.DangerousRelease ();
				Assert.Fail ("#1");
			} catch (ObjectDisposedException) {
			}

			GC.SuppressFinalize (sf);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void BadDispose2 ()
		{
			FakeSafeHandle sf = new FakeSafeHandle ();

			sf.Close ();
			sf.DangerousRelease ();
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void BadDispose3 ()
		{
			FakeSafeHandle sf = new FakeSafeHandle ();

			sf.Dispose ();
			sf.DangerousRelease ();
		}

		[Test]
		public void MultipleDisposes ()
		{
			FakeSafeHandle sf = new FakeSafeHandle ();

			sf.Dispose ();
			sf.Dispose ();
			sf.Dispose ();
		}

		[Test]
		public void CloseWillDispose ()
		{
			FakeSafeHandle sf = new FakeSafeHandle ();

			sf.Close ();
			Assert.IsTrue (sf.disposed, "disposed");
		}

		[Test]
		public void GoodDispose ()
		{
			int dummyHandle = 0xDEAD;
			FakeSafeHandle sf = new FakeSafeHandle ();
			sf.ChangeHandle (new IntPtr (dummyHandle));
			Assert.AreEqual ((int)sf.DangerousGetHandle(), dummyHandle, "handle");

			sf.DangerousRelease ();

			try {
				sf.Close ();
				Assert.Fail ("#1");
			} catch (ObjectDisposedException) {
			}

			try {
				sf.Dispose ();
				Assert.Fail ("#2");
			} catch (ObjectDisposedException) {
			}

			//In Ms.Net SafeHandle does not change the value of the handle after being SetInvalid or Disposed.
			Assert.AreEqual ((int)sf.DangerousGetHandle(), dummyHandle, "handle");
			//Handle was closed properly.
			Assert.IsTrue (sf.released, "released");
			Assert.IsTrue (sf.IsClosed, "closed");
			//Handle value is not changed, so the value itself is still valid (not 0 or -1)
			Assert.IsFalse (sf.IsInvalid, "invalid");

			GC.SuppressFinalize (sf);
		}

		[Test]
		public void SetHandleAsInvalid ()
		{
			int dummyHandle = 0xDEAD;
			FakeSafeHandle sf = new FakeSafeHandle ();

			sf.ChangeHandle (new IntPtr (dummyHandle));
			Assert.AreEqual ((int)sf.DangerousGetHandle(), dummyHandle, "handle");

			sf.SetHandleAsInvalid();

			//In Ms.Net SafeHandle does not change the value of the handle after being SetInvalid or Disposed.
			Assert.AreEqual ((int)sf.DangerousGetHandle(), dummyHandle, "handle");
			//Released == false since handle was not released, Set Invalid was called before it could be released.
			Assert.IsFalse (sf.released, "released");
			//IsClosed == true since handle is pointing to a disposed or invalid object.
			Assert.IsTrue (sf.IsClosed, "closed");
			//Handle value is not changed, so the value itself is still valid (not 0 or -1)
			Assert.IsFalse (sf.IsInvalid, "invalid");
		}

		[Test]
		public void SetInvalidDispose ()
		{
			int dummyHandle = 0xDEAD;
			FakeSafeHandle sf = new FakeSafeHandle (true);

			sf.ChangeHandle (new IntPtr (dummyHandle));
			Assert.AreEqual ((int)sf.DangerousGetHandle(), dummyHandle, "handle");

			sf.SetHandleAsInvalid();
			sf.Dispose ();

			//In Ms.Net SafeHandle does not change the value of the handle after being SetInvalid or Disposed.
			Assert.AreEqual ((int)sf.DangerousGetHandle(), dummyHandle, "handle");
			//Released == false since handle was not released, Set Invalid was called before it could be released.
			Assert.IsFalse (sf.released, "released");
			//IsClosed == true since handle is pointing to a disposed or invalid object.
			Assert.IsTrue (sf.IsClosed, "closed");
			//Handle value is not changed, so the value itself is still valid (not 0 or -1)
			Assert.IsFalse (sf.IsInvalid, "invalid");
		}

		[Test]
		public void SetInvalidRelease1 ()
		{
			FakeSafeHandle sf = new FakeSafeHandle (true);

			bool success = false;
			sf.DangerousAddRef(ref success);
			Assert.IsTrue (success, "dar");

			sf.SetHandleAsInvalid();

			Assert.IsFalse (sf.released, "released");
			Assert.IsTrue (sf.IsClosed, "closed");

			//Allow remaining refs to be released after SetHandleAsInvalid
			sf.DangerousRelease ();
			sf.DangerousRelease ();

			Assert.IsFalse (sf.released, "released");
			Assert.IsTrue (sf.IsClosed, "closed");
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void SetInvalidRelease2 ()
		{
			FakeSafeHandle sf = new FakeSafeHandle (true);

			bool success = false;
			sf.DangerousAddRef(ref success);
			Assert.IsTrue (success, "dar");

			sf.SetHandleAsInvalid();
			sf.DangerousRelease ();
			sf.DangerousRelease ();

			//This release need to throw ObjectDisposedException.
			//No more ref to release.
			sf.DangerousRelease ();
		}

		[Test]
		public void ReleaseAfterDispose1 ()
		{
			int dummyHandle = 0xDEAD;
			FakeSafeHandle sf = new FakeSafeHandle (true);
			sf.ChangeHandle (new IntPtr (dummyHandle));
			Assert.AreEqual ((int)sf.DangerousGetHandle(), dummyHandle, "handle");

			bool success = false;
			sf.DangerousAddRef(ref success);
			Assert.IsTrue (success, "dar");

			sf.Dispose ();
			//Still one ref left.
			Assert.IsFalse (sf.released, "released");
			Assert.IsFalse (sf.IsClosed, "closed");

			sf.DangerousRelease ();
			//In Ms.Net SafeHandle does not change the value of the handle after being SetInvalid or Disposed.
			Assert.AreEqual ((int)sf.DangerousGetHandle(), dummyHandle, "handle");
			//Handle was closed properly.
			Assert.IsTrue (sf.released, "released");
			Assert.IsTrue (sf.IsClosed, "closed");
			//Handle value is not changed, so the value itself is still valid (not 0 or -1)
			Assert.IsFalse (sf.IsInvalid, "invalid");
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void ReleaseAfterDispose2 ()
		{
			FakeSafeHandle sf = new FakeSafeHandle (true);

			bool success = false;
			sf.DangerousAddRef(ref success);
			Assert.IsTrue (success, "dar");

			sf.Dispose ();

			sf.DangerousRelease ();

			//Second release need to throw ObjectDisposedException.
			//No more ref to release.
			sf.DangerousRelease ();
		}

		[Test]
		public void NoReleaseUnowned ()
		{
			FakeSafeHandle sf = new FakeSafeHandle (false);

			sf.Close ();
			Assert.IsFalse (sf.released, "r1");
			Assert.IsTrue (sf.IsClosed, "c1");

			sf = new FakeSafeHandle (false);
			sf.DangerousRelease ();
			Assert.IsFalse (sf.released, "r2");
			Assert.IsTrue (sf.IsClosed, "c2");

			sf = new FakeSafeHandle (false);
			((IDisposable) sf).Dispose ();
			Assert.IsFalse (sf.released, "r3");
			Assert.IsTrue (sf.IsClosed, "c3");
		}

		//
		// This test does a DangerousAddRef on a new instance
		// of a custom user Safe Handle, and it just happens
		// that the default value for the handle is an invalid
		// handle.
		//
		// .NET does not throw an exception in this case, so
		// we should not either
		//
		[Test]
		public void DangerousAddRefOnNewInstance ()
		{
			FakeSafeHandle sf = new FakeSafeHandle ();
			sf.ChangeHandle (IntPtr.Zero);
			Assert.IsTrue (sf.IsInvalid, "invalid");

			bool success = false;
			sf.DangerousAddRef (ref success);
			Assert.IsTrue (success, "daroni");
		}
	}
}

