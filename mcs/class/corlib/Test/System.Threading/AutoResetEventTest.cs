//
// AutoResetEventTest.cs - NUnit test cases for System.Threading.AutoResetEvent
//
// Author:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//      Gert Driesen <gert.driesen@telenet.be>
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

namespace MonoTests.System.Threading {

	[TestFixture]
	public class AutoResetEventTest : Assertion {
		[Test]
		public void MultipleSet ()
		{
			AutoResetEvent evt = new AutoResetEvent (true);
			Assertion.AssertEquals ("#01", true, evt.WaitOne (1000, false));
			evt.Set ();
			evt.Set ();
			Assertion.AssertEquals ("#02", true, evt.WaitOne (1000, false));
			Assertion.AssertEquals ("#03", false, evt.WaitOne (1000, false));
		}
	}

#if NET_2_0
	[TestFixture]
	public class AutoResetEvent_SafeHandles : Assertion {

		//
		// Verifies that the safe SafeWaitHandle is used even when Handle is set
		//
		[Test]
		public void SafeWaitHandleIdentity ()
		{
			AutoResetEvent are1 = new AutoResetEvent (false);
			AutoResetEvent are2 = new AutoResetEvent (false);
			SafeWaitHandle swh1 = are1.SafeWaitHandle;
			Assertion.AssertEquals ("#A1:", false, swh1.IsClosed);
			Assertion.AssertEquals ("#A2:", false, swh1.IsInvalid);
			IntPtr dummyHandle = (IntPtr) 2;
			are1.Handle = dummyHandle;
			Assertion.AssertEquals ("#A3:", true, (are1.Handle == dummyHandle));
			Assertion.AssertEquals ("#A4:", false, swh1.IsClosed);
			Assertion.AssertEquals ("#A5:", false, swh1.IsClosed);
			Assertion.AssertEquals ("#A6:", false, swh1.IsInvalid);
			Assertion.AssertEquals ("#A7:", false, are1.SafeWaitHandle.IsClosed);
			Assertion.AssertEquals ("#A8:", false, are1.SafeWaitHandle.IsInvalid);
			are1.Close ();
			are2.Close ();
			swh1.Dispose ();
		}

		[Test]
	        public void Test2 ()
	        {
	                AutoResetEvent are1 = new AutoResetEvent (false);
	                are1.SafeWaitHandle = null;
	                Assertion.AssertEquals ("#B1:", true, (are1.SafeWaitHandle != null));
	                Assertion.AssertEquals ("#B2:", true, (((int) are1.Handle) == -1));
	                Assertion.AssertEquals ("#B3:", true, are1.SafeWaitHandle.IsInvalid);
	                Assertion.AssertEquals ("#B4:", false, are1.SafeWaitHandle.IsClosed);
	        }

		[Test]
	        public void Test3 ()
	        {
	                AutoResetEvent are1 = new AutoResetEvent (false);
	                AutoResetEvent are2 = new AutoResetEvent (false);
	                SafeWaitHandle swh1 = are1.SafeWaitHandle;
	                SafeWaitHandle swh2 = are2.SafeWaitHandle;
			Assertion.AssertEquals ("#C1:", true,  (swh1 != swh2));
			Assertion.AssertEquals ("#C2:", true, (are1.SafeWaitHandle == swh1));
	                are1.Handle = are2.Handle;
	                Assertion.AssertEquals ("#C3:", true, (are1.SafeWaitHandle != swh1));
	                Assertion.AssertEquals ("#C4:", false, swh1.IsClosed);
	                Assertion.AssertEquals ("#C5:", false, swh1.IsInvalid);
	                swh1.Dispose ();
	                are1.Close ();
	                are2.Close ();
	        }

		[Test]
	        public void Test4 ()
	        {
	                AutoResetEvent are1 = new AutoResetEvent (false);
	                SafeWaitHandle swh1 = are1.SafeWaitHandle;
	                are1.Handle = (IntPtr) (-1);
	                Assertion.AssertEquals ("#D1:" , true, swh1 != are1.SafeWaitHandle);
	                Assertion.AssertEquals ("#D2:" , false, are1.SafeWaitHandle.IsClosed);
	                Assertion.AssertEquals ("#D3:" , true, are1.SafeWaitHandle.IsInvalid);
	                are1.Close ();
	                swh1.Dispose ();
	        }

		[Test]
	        public void Test5 ()
	        {
	                AutoResetEvent are1 = new AutoResetEvent (false);
	                AutoResetEvent are2 = new AutoResetEvent (false);
	
	                SafeWaitHandle swh1 = are1.SafeWaitHandle;
	                Assertion.AssertEquals ("#E1:", false, swh1.IsClosed);
			Assertion.AssertEquals ("#E2:", false, swh1.IsInvalid);
	                are1.Handle = IntPtr.Zero;
			Assertion.AssertEquals ("#E3:", false, swh1.IsClosed);
	                Assertion.AssertEquals ("#E4:", false, swh1.IsInvalid);
	                Assertion.AssertEquals ("#E5:", false, are1.SafeWaitHandle.IsClosed);
	                Assertion.AssertEquals ("#E6:", true, are1.SafeWaitHandle.IsInvalid);
	                are1.Close ();
	                are2.Close ();
	                swh1.Dispose ();
	        }
	}
#endif
}

