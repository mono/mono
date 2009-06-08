//
// CommunicationObjectTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Channels
{
	class ExtCommObj : CommunicationObject
	{
		public new bool Aborted, Opened, Closed, OnClosedCalled;

		public ExtCommObj () : base ()
		{
		}

		protected override TimeSpan DefaultCloseTimeout {
			get { return TimeSpan.Zero; }
		}
		protected override TimeSpan DefaultOpenTimeout {
			get { return TimeSpan.Zero; }
		}

		public new bool IsDisposed {
			get { return base.IsDisposed; }
		}

		public void XFault ()
		{
			Fault ();
		}

		protected override void OnAbort ()
		{
			if (Aborted)
				throw new Exception ("Already aborted");
			Aborted = true;
		}

		protected override IAsyncResult OnBeginClose (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected override IAsyncResult OnBeginOpen (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		protected override void OnClose (TimeSpan timeout)
		{
			if (Closed)
				throw new Exception ("Already closed");
			Closed = true;
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			if (Opened)
				throw new Exception ("Already opened");
			Opened = true;
		}

		protected override void OnClosed ()
		{
			if (OnClosedCalled)
				throw new Exception ("OnClosed() already called");
			OnClosedCalled = true;
			base.OnClosed ();
		}
	}

	class ExtCommObj2 : ExtCommObj
	{
		public bool OnClosedCalled;

		// It does not call base -> Abort() detects it as an error.
		protected override void OnClosed ()
		{
			if (OnClosedCalled)
				throw new Exception ("OnClosed() already called");
			OnClosedCalled = true;
		}
	}

	[TestFixture]
	public class CommunicationObjectTest
	{
		[Test]
		public void OpenClose ()
		{
			ExtCommObj obj = new ExtCommObj ();
			Assert.AreEqual (CommunicationState.Created, obj.State, "#1");
			obj.Open ();
			Assert.AreEqual (CommunicationState.Opened, obj.State, "#2");
			Assert.IsTrue (obj.Opened, "#2-2");
			obj.Close ();
			Assert.AreEqual (CommunicationState.Closed, obj.State, "#3");
			Assert.IsTrue (obj.Closed, "#3-2");
			Assert.AreEqual (true, obj.IsDisposed, "#4");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void OpenOpenFails ()
		{
			ExtCommObj obj = new ExtCommObj ();
			obj.Open ();
			obj.Open ();
		}

		[Test]
		public void CloseAtInitialState ()
		{
			ExtCommObj obj = new ExtCommObj ();
			obj.Close ();
			Assert.IsTrue (obj.Aborted, "#1"); // OnAbort() is called.
			Assert.IsFalse (obj.Closed, "#2"); // OnClose() is *not* called.
			Assert.IsTrue (obj.OnClosedCalled, "#3");
		}

		[Test]
		public void CloseAtInitialStateAsync ()
		{
			ExtCommObj obj = new ExtCommObj ();
			obj.EndClose (obj.BeginClose (null, null)); // does not call OnBeginClose() / OnEndClose().
			Assert.IsTrue (obj.Aborted, "#1");
			Assert.IsFalse (obj.Closed, "#2");
			Assert.IsTrue (obj.OnClosedCalled, "#3");
		}

		[Test]
		public void CloseAtOpenedState ()
		{
			ExtCommObj obj = new ExtCommObj ();
			obj.Open ();
			obj.Close (); // Aborted() is *not* called.
			Assert.IsFalse (obj.Aborted, "#1");
			Assert.IsTrue (obj.Closed, "#2");
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void OpenClosedItemFails ()
		{
			ExtCommObj obj = new ExtCommObj ();
			obj.Open ();
			obj.Close ();
			obj.Open ();
		}

		[Test]
		public void Fault ()
		{
			ExtCommObj obj = new ExtCommObj ();
			obj.XFault ();

			obj = new ExtCommObj ();
			obj.Open ();
			obj.XFault ();
			Assert.AreEqual (CommunicationState.Faulted, obj.State, "#1");
			Assert.AreEqual (false, obj.IsDisposed, "#2");
		}

		[Test]
		[ExpectedException (typeof (CommunicationObjectFaultedException))]
		public void OpenFaulted ()
		{
			ExtCommObj obj = new ExtCommObj ();
			obj.XFault ();
			obj.Open ();
		}

		[Test]
		[ExpectedException (typeof (CommunicationObjectFaultedException))]
		public void CloseFaulted ()
		{
			ExtCommObj obj = new ExtCommObj ();
			obj.Open ();
			obj.XFault ();
			obj.Close ();
		}

		[Test]
		public void AbortFaulted ()
		{
			ExtCommObj obj = new ExtCommObj ();
			obj.Open ();
			obj.XFault ();
			Assert.AreEqual (CommunicationState.Faulted, obj.State, "#1");
			obj.Abort (); // does not raise an error
			Assert.AreEqual (CommunicationState.Closed, obj.State, "#2");
			Assert.IsTrue (obj.Aborted, "#3");
			Assert.IsFalse (obj.Closed, "#4");
			obj.Abort (); // does not raise an error!
		}

		[Test]
		public void AbortCreated ()
		{
			ExtCommObj obj = new ExtCommObj ();
			obj.Abort ();
			Assert.IsTrue (obj.Aborted, "#1"); // OnAbort() is called.
			Assert.IsFalse (obj.Closed, "#2"); // OnClose() is *not* called.
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void OnClosedImplementedWithoutCallingBase ()
		{
			ExtCommObj obj = new ExtCommObj2 ();
			obj.Close ();
		}
	}
}
