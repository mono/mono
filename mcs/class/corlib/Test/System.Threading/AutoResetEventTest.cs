//
// AutoResetEventTest.cs - NUnit test cases for System.Threading.AutoResetEvent
//
// Author:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Gert Driesen <gert.driesen@telenet.be>
//
// Copyright 2005 Novell, Inc (http://www.novell.com)
// Copyright 2007 Gert Driesen
//

using NUnit.Framework;
using System;
using System.Threading;
#if NET_2_0
using Microsoft.Win32.SafeHandles;
#endif

namespace MonoTests.System.Threading
{
	[TestFixture]
	public class AutoResetEventTest
	{
		[Test]
		public void MultipleSet ()
		{
			AutoResetEvent evt = new AutoResetEvent (true);
			Assert.IsTrue (evt.WaitOne (1000, false), "#1");
			evt.Set ();
			evt.Set ();
			Assert.IsTrue (evt.WaitOne (1000, false), "#2");
			Assert.IsFalse (evt.WaitOne (1000, false), "#3");
		}

#if NET_2_0
		[Test] // bug #81529
		public void SafeWaitHandle ()
		{
			AutoResetEvent are1 = new AutoResetEvent (false);
			AutoResetEvent are2 = new AutoResetEvent (false);
			SafeWaitHandle swh1 = are1.SafeWaitHandle;
			SafeWaitHandle swh2 = are2.SafeWaitHandle;
			are1.SafeWaitHandle = are2.SafeWaitHandle;
			Assert.AreSame (are1.SafeWaitHandle, are2.SafeWaitHandle, "#1");
			Assert.AreEqual (are1.Handle, are2.Handle, "#2");
			Assert.IsFalse (are1.SafeWaitHandle.IsInvalid, "#3");
			Assert.IsFalse (are1.SafeWaitHandle.IsClosed, "#4");
			Assert.IsFalse (swh1.IsClosed, "#5");
			Assert.IsFalse (swh1.IsInvalid, "#6");
			swh1.Dispose ();
			are1.Close ();
		}

		[Test] // bug #81529
		public void SafeWaitHandle_Null ()
		{
			AutoResetEvent are1 = new AutoResetEvent (false);
			SafeWaitHandle swh1 = are1.SafeWaitHandle;
			are1.SafeWaitHandle = null;
			Assert.IsNotNull (are1.SafeWaitHandle, "#1");
			Assert.AreEqual (-1, (int) are1.Handle, "#2");
			Assert.IsTrue (are1.SafeWaitHandle.IsInvalid, "#3");
			Assert.IsFalse (are1.SafeWaitHandle.IsClosed, "#4");
			Assert.IsFalse (swh1.IsClosed, "#5");
			Assert.IsFalse (swh1.IsInvalid, "#6");
		}

		[Test] // bug #81529
		public void Handle_Valid ()
		{
			AutoResetEvent are1 = new AutoResetEvent (false);
			SafeWaitHandle swh1 = are1.SafeWaitHandle;
			Assert.IsFalse (swh1.IsClosed, "#1");
			Assert.IsFalse (swh1.IsInvalid, "#2");
			IntPtr dummyHandle = (IntPtr) 2;
			are1.Handle = dummyHandle;
			Assert.AreEqual (are1.Handle, dummyHandle, "#3");
			Assert.IsFalse (swh1.IsClosed, "#4");
			Assert.IsFalse (swh1.IsClosed, "#5");
			Assert.IsFalse (swh1.IsInvalid, "#6");
			Assert.IsFalse (are1.SafeWaitHandle.IsClosed, "#7");
			Assert.IsFalse (are1.SafeWaitHandle.IsInvalid, "#8");
			are1.Close ();
			swh1.Dispose ();
		}

		[Test] // bug #81529
		public void Handle_Invalid ()
		{
			AutoResetEvent are1 = new AutoResetEvent (false);
			SafeWaitHandle swh1 = are1.SafeWaitHandle;
			are1.Handle = (IntPtr) (-1);
			Assert.IsTrue (swh1 != are1.SafeWaitHandle, "#1");
			Assert.IsFalse (swh1.IsClosed, "#2");
			Assert.IsFalse (swh1.IsInvalid, "#3");
			Assert.IsFalse (are1.SafeWaitHandle.IsClosed, "#4");
			Assert.IsTrue (are1.SafeWaitHandle.IsInvalid, "#5");
			are1.Close ();
			swh1.Dispose ();
		}

		[Test] // bug #81529
		public void Handle_ZeroPtr ()
		{
			AutoResetEvent are1 = new AutoResetEvent (false);
			SafeWaitHandle swh1 = are1.SafeWaitHandle;
			are1.Handle = IntPtr.Zero;
			Assert.IsTrue (swh1 != are1.SafeWaitHandle, "#1");
			Assert.IsFalse (swh1.IsClosed, "#2");
			Assert.IsFalse (swh1.IsInvalid, "#3");
			Assert.IsFalse (are1.SafeWaitHandle.IsClosed, "#4");
			Assert.IsTrue (are1.SafeWaitHandle.IsInvalid, "#5");
			are1.Close ();
			swh1.Dispose ();
		}
#endif
	}
}
