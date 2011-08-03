//
// Unit tests for System.ServiceModel.CommunicationObject
//
// Contact:
//   Moonlight List (moonlight-list@lists.ximian.com)
//
// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
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

// the file 'NUnitMoonHelper.cs' makes the Moon's unit test compiles and runs on NUnit
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Moonlight.UnitTesting;

namespace MoonTest.ServiceModel {

	[TestClass]
	public class CommunicationObjectSyncTest {

		class CommunicationObjectPoker : CommunicationObject {

			public CommunicationObjectPoker ()
			{
			}

			public CommunicationObjectPoker (object o)
				: base (o)
			{
			}

			public bool DefaultCloseTimeoutCalled { get; set; }
			public bool DefaultOpenTimeoutCalled { get; set; }

			public bool OnBeginCloseCalled { get; set; }
			public bool OnCloseCalled { get; set; }
			public bool OnEndCloseCalled { get; set; }

			public bool OnBeginOpenCalled { get; set; }
			public bool OnOpenCalled { get; set; }
			public bool OnEndOpenCalled { get; set; }

			public bool OnAbortCalled { get; set; }
			public CommunicationState OnAbortState { get; set; }

			public bool Disposed {
				get { return IsDisposed; }
			}

			protected override TimeSpan DefaultCloseTimeout	{
				get {
					DefaultCloseTimeoutCalled = true;
					return TimeSpan.Zero; 
				}
			}

			protected override TimeSpan DefaultOpenTimeout {
				get {
					DefaultOpenTimeoutCalled = true;
					return TimeSpan.Zero;
				}
			}

			protected override void OnAbort ()
			{
				OnAbortCalled = true;
				Assert.AreEqual (OnAbortState, State, "OnAbort/State");
				Assert.IsFalse (Disposed, "OnAbort/IsDisposed");
			}

			protected override IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, object state)
			{
				OnBeginCloseCalled = true;
				return null;
			}

			protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
			{
				OnBeginOpenCalled = true;
				return null;
			}

			protected override void OnClose (TimeSpan timeout)
			{
				OnCloseCalled = true;
			}

			protected override void OnEndClose (IAsyncResult result)
			{
				OnEndCloseCalled = true;
			}

			protected override void OnEndOpen (IAsyncResult result)
			{
				OnEndOpenCalled = true;
			}

			protected override void OnOpen (TimeSpan timeout)
			{
				OnOpenCalled = true;
			}

			public void _Fault ()
			{
				Fault ();
				Assert.AreEqual (CommunicationState.Faulted, State, "Fault/State");
				Assert.IsFalse (Disposed, "Fault/IsDisposed");
			}

			public void _FaultNoAssert ()
			{
				Fault ();
			}

			public object _ThisLock {
				get { return ThisLock; }
			}

			public void _ThrowIfDisposed ()
			{
				ThrowIfDisposed ();
			}

			public void _ThrowIfDisposedOrImmutable ()
			{
				ThrowIfDisposedOrImmutable ();
			}

			public void _ThrowIfDisposedOrNotOpen ()
			{
				ThrowIfDisposedOrNotOpen ();
			}
		}

		[TestMethod]
		public void Constructor ()
		{
			CommunicationObjectPoker co = new CommunicationObjectPoker ();
			Assert.AreEqual (typeof (object), co._ThisLock.GetType (), "ThisLock/default");

			co = new CommunicationObjectPoker (null);
			Assert.IsNull (co._ThisLock, "ThisLock/null");

			co = new CommunicationObjectPoker (String.Empty);
			Assert.AreSame (String.Empty, co._ThisLock, "ThisLock/weak");
		}

		[TestMethod]
#if NET_2_1
		[MoonlightBug]
#else
		[NUnit.Framework.Ignore]
#endif
		public void Create_Abort ()
		{
			int closing = 0;
			int closed = 0;

			CommunicationObjectPoker co = new CommunicationObjectPoker ();
			co.Closing += delegate (object sender, EventArgs e) {
				closing++;
				Assert.AreEqual (CommunicationState.Closing, co.State, "Closing/State");
				Assert.AreSame (co, sender, "Closing/sender");
				Assert.AreSame (EventArgs.Empty, e, "Closing/e");

				Assert.IsFalse (co.Disposed, "Closing/Disposed");
				Assert.Throws<CommunicationObjectAbortedException> (delegate {
					co._ThrowIfDisposed ();
				}, "Closing/ThrowIfDisposed");
				Assert.Throws<CommunicationObjectAbortedException> (delegate {
					co._ThrowIfDisposedOrImmutable ();
				}, "Closing/ThrowIfDisposedOrImmutable");
				Assert.Throws<CommunicationObjectAbortedException> (delegate {
					co._ThrowIfDisposedOrNotOpen ();
				}, "Closing/ThrowIfDisposedOrNotOpen");
			};
			co.Closed += delegate (object sender, EventArgs e) {
				closed++;
				Assert.AreEqual (CommunicationState.Closed, co.State, "Closed/State");
				Assert.AreSame (co, sender, "Closed/sender");
				Assert.AreSame (EventArgs.Empty, e, "Closed/e");

				Assert.IsTrue (co.Disposed, "Closed/Disposed");
				Assert.Throws<CommunicationObjectAbortedException> (delegate {
					co._ThrowIfDisposed ();
				}, "Closed/ThrowIfDisposed");
				Assert.Throws<CommunicationObjectAbortedException> (delegate {
					co._ThrowIfDisposedOrImmutable ();
				}, "Closed/ThrowIfDisposedOrImmutable");
				Assert.Throws<CommunicationObjectAbortedException> (delegate {
					co._ThrowIfDisposedOrNotOpen ();
				}, "Closed/ThrowIfDisposedOrNotOpen");
			};
			Assert.AreEqual (CommunicationState.Created, co.State, "State/before");

			co.OnAbortState = CommunicationState.Closing;
			co.Abort ();
			Assert.AreEqual (1, closing, "closing");
			Assert.AreEqual (1, closed, "closed");

			Assert.AreEqual (CommunicationState.Closed, co.State, "State/after");
			Assert.IsTrue (co.Disposed, "IsDisposed");

			Assert.IsFalse (co.DefaultCloseTimeoutCalled, "DefaultCloseTimeoutCalled");
			Assert.IsFalse (co.DefaultOpenTimeoutCalled, "DefaultOpenTimeoutCalled");
			Assert.IsTrue (co.OnAbortCalled, "OnAbortCalled");

			Assert.IsFalse (co.OnBeginCloseCalled, "OnBeginCloseCalled");
			Assert.IsFalse (co.OnCloseCalled, "OnCloseCalled");
			Assert.IsFalse (co.OnEndCloseCalled, "OnEndCloseCalled");

			Assert.IsFalse (co.OnBeginOpenCalled, "OnBeginOpenCalled");
			Assert.IsFalse (co.OnOpenCalled, "OnOpenCalled");
			Assert.IsFalse (co.OnEndOpenCalled, "OnEndCloseCalled");
		}

		[TestMethod]
#if NET_2_1
		[MoonlightBug]
#else
		[NUnit.Framework.Ignore]
#endif
		public void Create_Close ()
		{
			int opening = 0;
			int opened = 0;
			int closing = 0;
			int closed = 0;

			CommunicationObjectPoker co = new CommunicationObjectPoker ();
			co.Opening += delegate (object sender, EventArgs e) {
				opening++;
				Assert.AreEqual (CommunicationState.Opening, co.State, "Opening/State");
				Assert.AreSame (co, sender, "Opening/sender");
				Assert.AreSame (EventArgs.Empty, e, "Opening/e");
			};
			co.Opened += delegate (object sender, EventArgs e) {
				opened++;
				Assert.AreEqual (CommunicationState.Opened, co.State, "Opened/State");
				Assert.AreSame (co, sender, "Opened/sender");
				Assert.AreSame (EventArgs.Empty, e, "Opened/e");
			};
			co.Closing += delegate (object sender, EventArgs e) {
				closing++;
				Assert.AreEqual (CommunicationState.Closing, co.State, "Closing/State");
				Assert.AreSame (co, sender, "Closing/sender");
				Assert.AreSame (EventArgs.Empty, e, "Closing/e");

				Assert.IsFalse (co.Disposed, "Closing/Disposed");
				// note: IsDisposed is false but we still throw! 
				// but this match MSDN docs about ThrowIfDisposed
				Assert.Throws<ObjectDisposedException> (delegate {
					co._ThrowIfDisposed ();
				}, "Closing/ThrowIfDisposed");
				Assert.Throws<ObjectDisposedException> (delegate {
					co._ThrowIfDisposedOrImmutable ();
				}, "Closing/ThrowIfDisposedOrImmutable");
				Assert.Throws<ObjectDisposedException> (delegate {
					co._ThrowIfDisposedOrNotOpen ();
				}, "Closing/ThrowIfDisposedOrNotOpen");
			};
			co.Closed += delegate (object sender, EventArgs e) {
				closed++;
				Assert.AreEqual (CommunicationState.Closed, co.State, "Closed/State");
				Assert.AreSame (co, sender, "Closed/sender");
				Assert.AreSame (EventArgs.Empty, e, "Closed/e");

				Assert.IsTrue (co.Disposed, "Closed/Disposed");
				Assert.Throws<ObjectDisposedException> (delegate {
					co._ThrowIfDisposed ();
				}, "Closed/ThrowIfDisposed");
				Assert.Throws<ObjectDisposedException> (delegate {
					co._ThrowIfDisposedOrImmutable ();
				}, "Closed/ThrowIfDisposedOrImmutable");
				Assert.Throws<ObjectDisposedException> (delegate {
					co._ThrowIfDisposedOrNotOpen ();
				}, "Closed/ThrowIfDisposedOrNotOpen");
			};
			Assert.AreEqual (CommunicationState.Created, co.State, "State/before");

			co.OnAbortState = CommunicationState.Closing;
			// note: since this is not a "direct" abort then ObjectDisposedException
			co.Close ();
			Assert.AreEqual (0, opening, "opening");
			Assert.AreEqual (0, opened, "opened");
			Assert.AreEqual (1, closing, "closing");
			Assert.AreEqual (1, closed, "closed");

			Assert.AreEqual (CommunicationState.Closed, co.State, "State/after");
			Assert.IsTrue (co.Disposed, "IsDisposed");

			Assert.IsTrue (co.DefaultCloseTimeoutCalled, "DefaultCloseTimeoutCalled");
			Assert.IsFalse (co.DefaultOpenTimeoutCalled, "DefaultOpenTimeoutCalled");
			Assert.IsTrue (co.OnAbortCalled, "OnAbortCalled");

			Assert.IsFalse (co.OnBeginCloseCalled, "OnBeginCloseCalled");
			Assert.IsFalse (co.OnCloseCalled, "OnCloseCalled");
			Assert.IsFalse (co.OnEndCloseCalled, "OnEndCloseCalled");

			Assert.IsFalse (co.OnBeginOpenCalled, "OnBeginOpenCalled");
			Assert.IsFalse (co.OnOpenCalled, "OnOpenCalled");
			Assert.IsFalse (co.OnEndOpenCalled, "OnEndCloseCalled");

			// 2nd time, no events raised
			co.Close ();
			Assert.AreEqual (0, opening, "opening-b");
			Assert.AreEqual (0, opened, "opened-b");
			Assert.AreEqual (1, closing, "closing-c");
			Assert.AreEqual (1, closed, "closed-c");

			Assert.Throws<ObjectDisposedException> (delegate {
				co.Open ();
			}, "Open");
			Assert.AreEqual (0, opening, "opening-c");
			Assert.AreEqual (0, opened, "opened-c");
			Assert.AreEqual (1, closing, "closing-c");
			Assert.AreEqual (1, closed, "closed-c");
		}

		[TestMethod]
#if NET_2_1
		[MoonlightBug]
#else
		[NUnit.Framework.Ignore]
#endif
		public void Create_Close_Fault ()
		{
			bool faulted = false;

			CommunicationObjectPoker co = new CommunicationObjectPoker ();
			co.Faulted += delegate (object sender, EventArgs e) {
				faulted = true; // won't be hit
			};
			co.Open ();
			co.Close (); // real Close, not an implicit Abort since Open was called
			co._FaultNoAssert (); // don't check State since it won't be Faulted
			Assert.AreEqual (CommunicationState.Closed, co.State, "State/Fault");
			Assert.IsFalse (faulted, "Faulted");
		}

		[TestMethod]
#if NET_2_1
		[MoonlightBug]
#else
		[NUnit.Framework.Ignore]
#endif
		public void Create_Open_Close_Abort ()
		{
			int opening = 0;
			int opened = 0;
			int closing = 0;
			int closed = 0;

			CommunicationObjectPoker co = new CommunicationObjectPoker ();
			co.Opening += delegate (object sender, EventArgs e) {
				opening++;
				Assert.AreEqual (CommunicationState.Opening, co.State, "Opening/State");
				Assert.AreSame (co, sender, "Opening/sender");
				Assert.AreSame (EventArgs.Empty, e, "Opening/e");
			};
			co.Opened += delegate (object sender, EventArgs e) {
				opened++;
				Assert.AreEqual (CommunicationState.Opened, co.State, "Opened/State");
				Assert.AreSame (co, sender, "Opened/sender");
				Assert.AreSame (EventArgs.Empty, e, "Opened/e");
			};
			co.Closing += delegate (object sender, EventArgs e) {
				closing++;
				Assert.AreEqual (CommunicationState.Closing, co.State, "Closing/State");
				Assert.AreSame (co, sender, "Closing/sender");
				Assert.AreSame (EventArgs.Empty, e, "Closing/e");
			};
			co.Closed += delegate (object sender, EventArgs e) {
				closed++;
				Assert.AreEqual (CommunicationState.Closed, co.State, "Closed/State");
				Assert.AreSame (co, sender, "Closed/sender");
				Assert.AreSame (EventArgs.Empty, e, "Closed/e");
			};
			Assert.AreEqual (CommunicationState.Created, co.State, "State/before");

			co.Open ();
			Assert.AreEqual (1, opening, "opening");
			Assert.AreEqual (1, opened, "opened");
			Assert.AreEqual (0, closing, "closing");
			Assert.AreEqual (0, closed, "closed");

			Assert.AreEqual (CommunicationState.Opened, co.State, "State/after/open");
			Assert.IsFalse (co.Disposed, "IsDisposed/open");

			Assert.IsFalse (co.DefaultCloseTimeoutCalled, "DefaultCloseTimeoutCalled/open");
			Assert.IsTrue (co.DefaultOpenTimeoutCalled, "DefaultOpenTimeoutCalled/open");
			Assert.IsFalse (co.OnAbortCalled, "OnAbortCalled/open");

			Assert.IsFalse (co.OnBeginCloseCalled, "OnBeginCloseCalled/open");
			Assert.IsFalse (co.OnCloseCalled, "OnCloseCalled/open");
			Assert.IsFalse (co.OnEndCloseCalled, "OnEndCloseCalled/open");

			Assert.IsFalse (co.OnBeginOpenCalled, "OnBeginOpenCalled/open");
			Assert.IsTrue (co.OnOpenCalled, "OnOpenCalled/open");
			Assert.IsFalse (co.OnEndOpenCalled, "OnEndCloseCalled/open");

			co.Close ();
			Assert.AreEqual (1, opening, "opening-b");
			Assert.AreEqual (1, opened, "opened-b");
			Assert.AreEqual (1, closing, "closing-b");
			Assert.AreEqual (1, closed, "closed-b");

			Assert.AreEqual (CommunicationState.Closed, co.State, "State/close");
			Assert.IsTrue (co.Disposed, "IsDisposed/close");

			Assert.IsTrue (co.DefaultCloseTimeoutCalled, "DefaultCloseTimeoutCalled/close");
			Assert.IsTrue (co.DefaultOpenTimeoutCalled, "DefaultOpenTimeoutCalled/close");
			Assert.IsFalse (co.OnAbortCalled, "OnAbortCalled/close");

			Assert.IsFalse (co.OnBeginCloseCalled, "OnBeginCloseCalled/close");
			Assert.IsTrue (co.OnCloseCalled, "OnCloseCalled/close");
			Assert.IsFalse (co.OnEndCloseCalled, "OnEndCloseCalled/close");

			Assert.IsFalse (co.OnBeginOpenCalled, "OnBeginOpenCalled/close");
			Assert.IsTrue (co.OnOpenCalled, "OnOpenCalled/close");
			Assert.IsFalse (co.OnEndOpenCalled, "OnEndCloseCalled/close");

			co.Abort ();
			Assert.AreEqual (1, opening, "opening-c");
			Assert.AreEqual (1, opened, "opened-c");
			Assert.AreEqual (1, closing, "closing-c");
			Assert.AreEqual (1, closed, "closed-c");

			Assert.IsFalse (co.OnAbortCalled, "OnAbortCalled/abort");
			Assert.AreEqual (CommunicationState.Closed, co.State, "State/abort");
		}

		[TestMethod]
#if NET_2_1
		[MoonlightBug]
#else
		[NUnit.Framework.Ignore]
#endif
		public void Create_Fault_Abort ()
		{
			int opening = 0;
			int opened = 0;
			int closing = 0;
			int closed = 0;

			CommunicationObjectPoker co = new CommunicationObjectPoker ();
			co.Opening += delegate (object sender, EventArgs e) {
				opening++;
				Assert.AreEqual (CommunicationState.Opening, co.State, "Opening/State");
				Assert.AreSame (co, sender, "Opening/sender");
				Assert.AreSame (EventArgs.Empty, e, "Opening/e");
			};
			co.Opened += delegate (object sender, EventArgs e) {
				opened++;
				Assert.AreEqual (CommunicationState.Opened, co.State, "Opened/State");
				Assert.AreSame (co, sender, "Opened/sender");
				Assert.AreSame (EventArgs.Empty, e, "Opened/e");
			};
			co.Closing += delegate (object sender, EventArgs e) {
				closing++;
				Assert.AreEqual (CommunicationState.Closing, co.State, "Closing/State");
				Assert.AreSame (co, sender, "Closing/sender");
				Assert.AreSame (EventArgs.Empty, e, "Closing/e");

				Assert.IsFalse (co.Disposed, "Closing/Disposed");
				// note: IsDisposed is false but we still throw! 
				// but this match MSDN docs about ThrowIfDisposed
				Assert.Throws<CommunicationObjectAbortedException> (delegate {
					co._ThrowIfDisposed ();
				}, "Closing/ThrowIfDisposed");
				Assert.Throws<CommunicationObjectAbortedException> (delegate {
					co._ThrowIfDisposedOrImmutable ();
				}, "Closing/ThrowIfDisposedOrImmutable");
				Assert.Throws<CommunicationObjectAbortedException> (delegate {
					co._ThrowIfDisposedOrNotOpen ();
				}, "Closing/ThrowIfDisposedOrNotOpen");
			};
			co.Closed += delegate (object sender, EventArgs e) {
				closed++;
				Assert.AreEqual (CommunicationState.Closed, co.State, "Closed/State");
				Assert.AreSame (co, sender, "Closed/sender");
				Assert.AreSame (EventArgs.Empty, e, "Closed/e");

				Assert.IsTrue (co.Disposed, "Closed/Disposed");
				Assert.Throws<CommunicationObjectAbortedException> (delegate {
					co._ThrowIfDisposed ();
				}, "Closed/ThrowIfDisposed");
				Assert.Throws<CommunicationObjectAbortedException> (delegate {
					co._ThrowIfDisposedOrImmutable ();
				}, "Closed/ThrowIfDisposedOrImmutable");
				Assert.Throws<CommunicationObjectAbortedException> (delegate {
					co._ThrowIfDisposedOrNotOpen ();
				}, "Closed/ThrowIfDisposedOrNotOpen");
			};
			Assert.AreEqual (CommunicationState.Created, co.State, "State/before");

			co.OnAbortState = CommunicationState.Closing;
			co._Fault ();
			Assert.AreEqual (0, opening, "opening");
			Assert.AreEqual (0, opened, "opened");
			Assert.AreEqual (0, closing, "closing");
			Assert.AreEqual (0, closed, "closed");

			Assert.AreEqual (CommunicationState.Faulted, co.State, "State/after");
			Assert.IsFalse (co.Disposed, "IsDisposed");

			Assert.IsFalse (co.DefaultCloseTimeoutCalled, "DefaultCloseTimeoutCalled");
			Assert.IsFalse (co.DefaultOpenTimeoutCalled, "DefaultOpenTimeoutCalled");
			Assert.IsFalse (co.OnAbortCalled, "OnAbortCalled");

			Assert.IsFalse (co.OnBeginCloseCalled, "OnBeginCloseCalled");
			Assert.IsFalse (co.OnCloseCalled, "OnCloseCalled");
			Assert.IsFalse (co.OnEndCloseCalled, "OnEndCloseCalled");

			Assert.IsFalse (co.OnBeginOpenCalled, "OnBeginOpenCalled");
			Assert.IsFalse (co.OnOpenCalled, "OnOpenCalled");
			Assert.IsFalse (co.OnEndOpenCalled, "OnEndCloseCalled");

			co.Abort ();
			Assert.AreEqual (0, opening, "opening-b");
			Assert.AreEqual (0, opened, "opened-b");
			Assert.AreEqual (1, closing, "closing-c");
			Assert.AreEqual (1, closed, "closed-c");
		}

		[TestMethod]
#if NET_2_1
		[MoonlightBug]
#else
		[NUnit.Framework.Ignore]
#endif
		public void Create_Fault_Open_Close ()
		{
			int opening = 0;
			int opened = 0;
			int closing = 0;
			int closed = 0;
			int faulted = 0;

			CommunicationObjectPoker co = new CommunicationObjectPoker ();
			co.Faulted += delegate (object sender, EventArgs e) {
				faulted++;
				Assert.AreEqual (CommunicationState.Faulted, co.State, "State/Faulted");
				Assert.AreSame (co, sender, "sender");
				Assert.AreSame (EventArgs.Empty, e, "e");
			};
			co.Opening += delegate (object sender, EventArgs e) {
				opening++;
				Assert.AreEqual (CommunicationState.Opening, co.State, "Opening/State");
				Assert.AreSame (co, sender, "Opening/sender");
				Assert.AreSame (EventArgs.Empty, e, "Opening/e");
			};
			co.Opened += delegate (object sender, EventArgs e) {
				opened++;
				Assert.AreEqual (CommunicationState.Opened, co.State, "Opened/State");
				Assert.AreSame (co, sender, "Opened/sender");
				Assert.AreSame (EventArgs.Empty, e, "Opened/e");
			};
			co.Closing += delegate (object sender, EventArgs e) {
				closing++;
				Assert.AreEqual (CommunicationState.Closing, co.State, "Closing/State");
				Assert.AreSame (co, sender, "Closing/sender");
				Assert.AreSame (EventArgs.Empty, e, "Closing/e");
			};
			co.Closed += delegate (object sender, EventArgs e) {
				closed++;
				Assert.AreEqual (CommunicationState.Closed, co.State, "Closed/State");
				Assert.AreSame (co, sender, "Closed/sender");
				Assert.AreSame (EventArgs.Empty, e, "Closed/e");
			};

			Assert.AreEqual (CommunicationState.Created, co.State, "State/before");

			co._Fault ();
			Assert.AreEqual (0, opening, "opening");
			Assert.AreEqual (0, opened, "opened");
			Assert.AreEqual (0, closing, "closing");
			Assert.AreEqual (0, closed, "closed");
			Assert.AreEqual (1, faulted, "faulted");

			Assert.AreEqual (CommunicationState.Faulted, co.State, "State/after");
			Assert.IsFalse (co.Disposed, "IsDisposed");

			// 2nd fault does not throw a CommunicationObjectFaultedException
			// nor does it raise Faulted again
			co._Fault ();
			Assert.AreEqual (1, faulted, "faulted(same)");

			Assert.IsFalse (co.DefaultCloseTimeoutCalled, "DefaultCloseTimeoutCalled");
			Assert.IsFalse (co.DefaultOpenTimeoutCalled, "DefaultOpenTimeoutCalled");
			Assert.IsFalse (co.OnAbortCalled, "OnAbortCalled");

			Assert.IsFalse (co.OnBeginCloseCalled, "OnBeginCloseCalled");
			Assert.IsFalse (co.OnCloseCalled, "OnCloseCalled");
			Assert.IsFalse (co.OnEndCloseCalled, "OnEndCloseCalled");

			Assert.IsFalse (co.OnBeginOpenCalled, "OnBeginOpenCalled");
			Assert.IsFalse (co.OnOpenCalled, "OnOpenCalled");
			Assert.IsFalse (co.OnEndOpenCalled, "OnEndCloseCalled");

			Assert.Throws<CommunicationObjectFaultedException> (delegate {
				co.Open ();
			}, "Open");
			Assert.AreEqual (0, opening, "opening-b");
			Assert.AreEqual (0, opened, "opened-b");
			Assert.AreEqual (CommunicationState.Faulted, co.State, "State/Open");

			// calling Close on an Faulted instance will call OnAbort (not OnClose)
			co.OnAbortState = CommunicationState.Closing;
			Assert.IsFalse (co.OnAbortCalled, "OnAbortCalled/before");
			Assert.Throws<CommunicationObjectFaultedException> (delegate {
				co.Close ();
			}, "Close");
			Assert.IsTrue (co.OnAbortCalled, "OnAbortCalled/after");
			Assert.IsFalse (co.OnCloseCalled, "OnCloseCalled/after");
			Assert.AreEqual (1, closing, "closing-c");
			Assert.AreEqual (1, closed, "closed-c");
			Assert.AreEqual (CommunicationState.Closed, co.State, "State/Close");
		}

		[TestMethod]
#if NET_2_1
		[MoonlightBug]
#else
		[NUnit.Framework.Ignore]
#endif
		public void Create_Fault_Open_Abort_Close ()
		{
			int opening = 0;
			int opened = 0;
			int closing = 0;
			int closed = 0;
			int faulted = 0;

			CommunicationObjectPoker co = new CommunicationObjectPoker ();
			co.Faulted += delegate (object sender, EventArgs e) {
				faulted++;
				Assert.AreEqual (CommunicationState.Faulted, co.State, "State/Faulted");
				Assert.AreSame (co, sender, "sender");
				Assert.AreSame (EventArgs.Empty, e, "e");
			};
			co.Opening += delegate (object sender, EventArgs e) {
				opening++;
				Assert.AreEqual (CommunicationState.Opening, co.State, "Opening/State");
				Assert.AreSame (co, sender, "Opening/sender");
				Assert.AreSame (EventArgs.Empty, e, "Opening/e");
			};
			co.Opened += delegate (object sender, EventArgs e) {
				opened++;
				Assert.AreEqual (CommunicationState.Opened, co.State, "Opened/State");
				Assert.AreSame (co, sender, "Opened/sender");
				Assert.AreSame (EventArgs.Empty, e, "Opened/e");
			};
			co.Closing += delegate (object sender, EventArgs e) {
				closing++;
				Assert.AreEqual (CommunicationState.Closing, co.State, "Closing/State");
				Assert.AreSame (co, sender, "Closing/sender");
				Assert.AreSame (EventArgs.Empty, e, "Closing/e");
			};
			co.Closed += delegate (object sender, EventArgs e) {
				closed++;
				Assert.AreEqual (CommunicationState.Closed, co.State, "Closed/State");
				Assert.AreSame (co, sender, "Closed/sender");
				Assert.AreSame (EventArgs.Empty, e, "Closed/e");
			};

			Assert.AreEqual (CommunicationState.Created, co.State, "State/before");

			co._Fault ();
			Assert.AreEqual (0, opening, "opening");
			Assert.AreEqual (0, opened, "opened");
			Assert.AreEqual (0, closing, "closing");
			Assert.AreEqual (0, closed, "closed");
			Assert.AreEqual (1, faulted, "faulted");

			Assert.AreEqual (CommunicationState.Faulted, co.State, "State/after");
			Assert.IsFalse (co.Disposed, "IsDisposed");

			// 2nd fault does not throw a CommunicationObjectFaultedException
			// nor does it raise Faulted again
			co._Fault ();
			Assert.AreEqual (1, faulted, "faulted(same)");

			Assert.IsFalse (co.DefaultCloseTimeoutCalled, "DefaultCloseTimeoutCalled");
			Assert.IsFalse (co.DefaultOpenTimeoutCalled, "DefaultOpenTimeoutCalled");
			Assert.IsFalse (co.OnAbortCalled, "OnAbortCalled");

			Assert.IsFalse (co.OnBeginCloseCalled, "OnBeginCloseCalled");
			Assert.IsFalse (co.OnCloseCalled, "OnCloseCalled");
			Assert.IsFalse (co.OnEndCloseCalled, "OnEndCloseCalled");

			Assert.IsFalse (co.OnBeginOpenCalled, "OnBeginOpenCalled");
			Assert.IsFalse (co.OnOpenCalled, "OnOpenCalled");
			Assert.IsFalse (co.OnEndOpenCalled, "OnEndCloseCalled");

			Assert.Throws<CommunicationObjectFaultedException> (delegate {
				co.Open ();
			}, "Open");
			Assert.AreEqual (0, opening, "opening-b");
			Assert.AreEqual (0, opened, "opened-b");

			co.OnAbortState = CommunicationState.Closing;
			// Abort does not throw a CommunicationObjectFaultedException
			co.Abort ();
			Assert.AreEqual (1, closing, "closing-b");
			Assert.AreEqual (1, closed, "closed-b");

			co.Close ();
			Assert.AreEqual (1, closing, "closing-c");
			Assert.AreEqual (1, closed, "closed-c");
		}

		[TestMethod]
#if NET_2_1
		[MoonlightBug]
#else
		[NUnit.Framework.Ignore]
#endif
		public void Create_Open_Open ()
		{
			int opening = 0;
			int opened = 0;

			CommunicationObjectPoker co = new CommunicationObjectPoker ();
			co.Opening += delegate (object sender, EventArgs e) {
				opening++;
				Assert.AreEqual (CommunicationState.Opening, co.State, "Opening/State");
				Assert.AreSame (co, sender, "Opening/sender");
				Assert.AreSame (EventArgs.Empty, e, "Opening/e");
			};
			co.Opened += delegate (object sender, EventArgs e) {
				opened++;
				Assert.AreEqual (CommunicationState.Opened, co.State, "Opened/State");
				Assert.AreSame (co, sender, "Opened/sender");
				Assert.AreSame (EventArgs.Empty, e, "Opened/e");
			};

			co.Open ();
			Assert.AreEqual (1, opening, "opening");
			Assert.AreEqual (1, opened, "opened");
			Assert.AreEqual (CommunicationState.Opened, co.State, "State/after/open");
			Assert.IsFalse (co.Disposed, "IsDisposed/open");

			Assert.Throws<InvalidOperationException> (delegate {
				co.Open ();
			}, "Open/2");
			Assert.AreEqual (1, opening, "opening-b");
			Assert.AreEqual (1, opened, "openedg-b");
			Assert.AreEqual (CommunicationState.Opened, co.State, "State/after/openg-b");
			Assert.IsFalse (co.Disposed, "IsDisposed/openg-b");
		}


		// http://msdn.microsoft.com/en-us/library/ms789041.aspx
		// ThrowIfDisposed throws an exception if the state is Closing, Closed or Faulted.

		[TestMethod]
#if NET_2_1
		[MoonlightBug]
#else
		[NUnit.Framework.Ignore]
#endif
		public void ThrowIfDisposed_Open_Close ()
		{
			bool opening = false;
			bool opened = false;
			bool closing = false;
			bool closed = false;
			bool faulted = false;

			CommunicationObjectPoker co = new CommunicationObjectPoker ();

			co.Opening += delegate (object sender, EventArgs e) {
				opening = true;
				Assert.AreEqual (CommunicationState.Opening, co.State, "Opening/State");
				co._ThrowIfDisposed ();
			};
			co.Opened += delegate (object sender, EventArgs e) {
				opened = true;
				Assert.AreEqual (CommunicationState.Opened, co.State, "Opened/State");
				co._ThrowIfDisposed ();
			};
			co.Closing += delegate (object sender, EventArgs e) {
				closing = true;
				Assert.AreEqual (CommunicationState.Closing, co.State, "Closing/State");
				Assert.Throws<ObjectDisposedException> (delegate {
					co._ThrowIfDisposed ();
				}, "ThrowIfDisposed/Closing");
			};
			co.Closed += delegate (object sender, EventArgs e) {
				closed = true;
				Assert.AreEqual (CommunicationState.Closed, co.State, "Closed/State");
				Assert.Throws<ObjectDisposedException> (delegate {
					co._ThrowIfDisposed ();
				}, "ThrowIfDisposed/Closed");
			};
			co.Faulted += delegate (object sender, EventArgs e) {
				faulted = true;
				Assert.AreEqual (CommunicationState.Faulted, co.State, "Faulted/State");
				Assert.Throws<CommunicationObjectFaultedException> (delegate {
					co._ThrowIfDisposed ();
				}, "ThrowIfDisposed/Faulted");
			};

			Assert.AreEqual (CommunicationState.Created, co.State, "Created");
			co._ThrowIfDisposed ();

			co.Open ();

			co.Close ();

			co._FaultNoAssert ();

			// ensure all states were tested
			Assert.IsTrue (opening, "opening");
			Assert.IsTrue (opened, "opened");
			Assert.IsTrue (closing, "closing");
			Assert.IsTrue (closed, "closing");
			Assert.IsFalse (faulted, "faulted");
		}


		[TestMethod]
#if NET_2_1
		[MoonlightBug]
#else
		[NUnit.Framework.Ignore]
#endif
		public void ThrowIfDisposed_Fault_Abort ()
		{
			bool opening = false;
			bool opened = false;
			bool closing = false;
			bool closed = false;
			bool faulted = false;

			CommunicationObjectPoker co = new CommunicationObjectPoker ();

			co.Opening += delegate (object sender, EventArgs e) {
				opening = true;
				Assert.AreEqual (CommunicationState.Opening, co.State, "Opening/State");
				co._ThrowIfDisposed ();
			};
			co.Opened += delegate (object sender, EventArgs e) {
				opened = true;
				Assert.AreEqual (CommunicationState.Opened, co.State, "Opened/State");
				co._ThrowIfDisposed ();
			};
			co.Closing += delegate (object sender, EventArgs e) {
				closing = true;
				Assert.AreEqual (CommunicationState.Closing, co.State, "Closing/State");
				Assert.Throws<CommunicationObjectAbortedException> (delegate {
					co._ThrowIfDisposed ();
				}, "ThrowIfDisposed/Closing");
			};
			co.Closed += delegate (object sender, EventArgs e) {
				closed = true;
				Assert.AreEqual (CommunicationState.Closed, co.State, "Closed/State");
				Assert.Throws<CommunicationObjectAbortedException> (delegate {
					co._ThrowIfDisposed ();
				}, "ThrowIfDisposed/Closed");
			};
			co.Faulted += delegate (object sender, EventArgs e) {
				faulted = true;
				Assert.AreEqual (CommunicationState.Faulted, co.State, "Faulted/State");
				Assert.Throws<CommunicationObjectFaultedException> (delegate {
					co._ThrowIfDisposed ();
				}, "ThrowIfDisposed/Faulted");
			};

			Assert.AreEqual (CommunicationState.Created, co.State, "Created");
			co._ThrowIfDisposed ();

			co._FaultNoAssert ();

			co.OnAbortState = CommunicationState.Closing;
			co.Abort (); 

			// ensure all states were tested
			Assert.IsFalse (opening, "opening");
			Assert.IsFalse (opened, "opened");
			Assert.IsTrue (closing, "closing");
			Assert.IsTrue (closed, "closing");
			Assert.IsTrue (faulted, "faulted");
		}

		// http://msdn.microsoft.com/en-us/library/ms789041.aspx
		// ThrowIfDisposedOrImmutable throws an exception if the state is not Created.

		[TestMethod]
#if NET_2_1
		[MoonlightBug]
#else
		[NUnit.Framework.Ignore]
#endif
		public void ThrowIfDisposedOrImmutable_Open_Close ()
		{
			bool opening = false;
			bool opened = false;
			bool closing = false;
			bool closed = false;
			bool faulted = false;

			CommunicationObjectPoker co = new CommunicationObjectPoker ();

			co.Opening += delegate (object sender, EventArgs e) {
				opening = true;
				Assert.AreEqual (CommunicationState.Opening, co.State, "Opening/State");
				Assert.Throws<InvalidOperationException> (delegate {
					co._ThrowIfDisposedOrImmutable ();
				}, "ThrowIfDisposedOrImmutable/Opening");
			};
			co.Opened += delegate (object sender, EventArgs e) {
				opened = true;
				Assert.AreEqual (CommunicationState.Opened, co.State, "Opened/State");
				Assert.Throws<InvalidOperationException> (delegate {
					co._ThrowIfDisposedOrImmutable ();
				}, "ThrowIfDisposedOrImmutable/Opened");
			};
			co.Closing += delegate (object sender, EventArgs e) {
				closing = true;
				Assert.AreEqual (CommunicationState.Closing, co.State, "Closing/State");
				Assert.Throws<ObjectDisposedException> (delegate {
					co._ThrowIfDisposedOrImmutable ();
				}, "ThrowIfDisposedOrImmutable/Closing");
			};
			co.Closed += delegate (object sender, EventArgs e) {
				closed = true;
				Assert.AreEqual (CommunicationState.Closed, co.State, "Closed/State");
				Assert.Throws<ObjectDisposedException> (delegate {
					co._ThrowIfDisposedOrImmutable ();
				}, "ThrowIfDisposedOrImmutable/Closed");
			};
			co.Faulted += delegate (object sender, EventArgs e) {
				faulted = true;
				Assert.AreEqual (CommunicationState.Faulted, co.State, "Faulted/State");
				Assert.Throws<CommunicationObjectFaultedException> (delegate {
					co._ThrowIfDisposedOrImmutable ();
				}, "ThrowIfDisposedOrImmutable/Faulted");
			};

			Assert.AreEqual (CommunicationState.Created, co.State, "Created");
			co._ThrowIfDisposedOrImmutable ();

			co.Open ();

			co.Close ();

			co._FaultNoAssert ();

			// ensure all states were tested
			Assert.IsTrue (opening, "opening");
			Assert.IsTrue (opened, "opened");
			Assert.IsTrue (closing, "closing");
			Assert.IsTrue (closed, "closing");
			Assert.IsFalse (faulted, "faulted");
		}

		[TestMethod]
#if NET_2_1
		[MoonlightBug]
#else
		[NUnit.Framework.Ignore]
#endif
		public void ThrowIfDisposedOrImmutable_Fault_Abort ()
		{
			bool opening = false;
			bool opened = false;
			bool closing = false;
			bool closed = false;
			bool faulted = false;

			CommunicationObjectPoker co = new CommunicationObjectPoker ();

			co.Opening += delegate (object sender, EventArgs e) {
				opening = true;
				Assert.AreEqual (CommunicationState.Opening, co.State, "Opening/State");
				Assert.Throws<ObjectDisposedException> (delegate {
					co._ThrowIfDisposedOrImmutable ();
				}, "ThrowIfDisposedOrImmutable/Opening");
			};
			co.Opened += delegate (object sender, EventArgs e) {
				opened = true;
				Assert.AreEqual (CommunicationState.Opened, co.State, "Opened/State");
				Assert.Throws<ObjectDisposedException> (delegate {
					co._ThrowIfDisposedOrImmutable ();
				}, "ThrowIfDisposedOrImmutable/Opened");
			};
			co.Closing += delegate (object sender, EventArgs e) {
				closing = true;
				Assert.AreEqual (CommunicationState.Closing, co.State, "Closing/State");
				Assert.Throws<CommunicationObjectAbortedException> (delegate {
					co._ThrowIfDisposedOrImmutable ();
				}, "ThrowIfDisposedOrImmutable/Closing");
			};
			co.Closed += delegate (object sender, EventArgs e) {
				closed = true;
				Assert.AreEqual (CommunicationState.Closed, co.State, "Closed/State");
				Assert.Throws<CommunicationObjectAbortedException> (delegate {
					co._ThrowIfDisposedOrImmutable ();
				}, "ThrowIfDisposedOrImmutable/Closed");
			};
			co.Faulted += delegate (object sender, EventArgs e) {
				faulted = true;
				Assert.AreEqual (CommunicationState.Faulted, co.State, "Faulted/State");
				Assert.Throws<CommunicationObjectFaultedException> (delegate {
					co._ThrowIfDisposedOrImmutable ();
				}, "ThrowIfDisposedOrImmutable/Faulted");
			};

			Assert.AreEqual (CommunicationState.Created, co.State, "Created");
			co._ThrowIfDisposedOrImmutable ();

			co._FaultNoAssert ();

			co.OnAbortState = CommunicationState.Closing;
			co.Abort ();

			// ensure all states were tested
			Assert.IsFalse (opening, "opening");
			Assert.IsFalse (opened, "opened");
			Assert.IsTrue (closing, "closing");
			Assert.IsTrue (closed, "closing");
			Assert.IsTrue (faulted, "faulted");
		}

		// http://msdn.microsoft.com/en-us/library/ms789041.aspx
		// ThrowIfDisposedOrNotOpen throws an exception if the state is not Opened. 

		[TestMethod]
#if NET_2_1
		[MoonlightBug]
#else
		[NUnit.Framework.Ignore]
#endif
		public void ThrowIfDisposedOrNotOpen_Open_Close ()
		{
			bool opening = false;
			bool opened = false;
			bool closing = false;
			bool closed = false;
			bool faulted = false;

			CommunicationObjectPoker co = new CommunicationObjectPoker ();

			co.Opening += delegate (object sender, EventArgs e) {
				opening = true;
				Assert.AreEqual (CommunicationState.Opening, co.State, "Opening/State");
				Assert.Throws<InvalidOperationException> (delegate {
					co._ThrowIfDisposedOrNotOpen ();
				}, "ThrowIfDisposedOrNotOpen/Opening");
			};
			co.Opened += delegate (object sender, EventArgs e) {
				opened = true;
				Assert.AreEqual (CommunicationState.Opened, co.State, "Opened/State");
				co._ThrowIfDisposedOrNotOpen ();
			};
			co.Closing += delegate (object sender, EventArgs e) {
				closing = true;
				Assert.AreEqual (CommunicationState.Closing, co.State, "Closing/State");
				Assert.Throws<ObjectDisposedException> (delegate {
					co._ThrowIfDisposedOrNotOpen ();
				}, "ThrowIfDisposedOrNotOpen/Closing");
			};
			co.Closed += delegate (object sender, EventArgs e) {
				closed = true;
				Assert.AreEqual (CommunicationState.Closed, co.State, "Closed/State");
				Assert.Throws<ObjectDisposedException> (delegate {
					co._ThrowIfDisposedOrNotOpen ();
				}, "ThrowIfDisposedOrNotOpen/Closed");
			};
			co.Faulted += delegate (object sender, EventArgs e) {
				faulted = true;
				Assert.AreEqual (CommunicationState.Faulted, co.State, "Faulted/State");
				Assert.Throws<CommunicationObjectFaultedException> (delegate {
					co._ThrowIfDisposedOrNotOpen ();
				}, "ThrowIfDisposedOrNotOpen/Faulted");
			};

			Assert.AreEqual (CommunicationState.Created, co.State, "Created");
			Assert.Throws<InvalidOperationException> (delegate {
				co._ThrowIfDisposedOrNotOpen ();
			}, "ThrowIfDisposedOrNotOpen/Created");

			co.Open ();

			co.Close ();

			co._FaultNoAssert ();

			// ensure all states were tested
			Assert.IsTrue (opening, "opening");
			Assert.IsTrue (opened, "opened");
			Assert.IsTrue (closing, "closing");
			Assert.IsTrue (closed, "closing");
			Assert.IsFalse (faulted, "faulted");
		}

		[TestMethod]
#if NET_2_1
		[MoonlightBug]
#else
		[NUnit.Framework.Ignore]
#endif
		public void ThrowIfDisposedOrNotOpen_Fault_Abort ()
		{
			bool opening = false;
			bool opened = false;
			bool closing = false;
			bool closed = false;
			bool faulted = false;

			CommunicationObjectPoker co = new CommunicationObjectPoker ();

			co.Opening += delegate (object sender, EventArgs e) {
				opening = true;
				Assert.AreEqual (CommunicationState.Opening, co.State, "Opening/State");
				Assert.Throws<ObjectDisposedException> (delegate {
					co._ThrowIfDisposedOrNotOpen ();
				}, "ThrowIfDisposedOrNotOpen/Opening");
			};
			co.Opened += delegate (object sender, EventArgs e) {
				opened = true;
				Assert.AreEqual (CommunicationState.Opened, co.State, "Opened/State");
				co._ThrowIfDisposedOrNotOpen ();
			};
			co.Closing += delegate (object sender, EventArgs e) {
				closing = true;
				Assert.AreEqual (CommunicationState.Closing, co.State, "Closing/State");
				Assert.Throws<CommunicationObjectAbortedException> (delegate {
					co._ThrowIfDisposedOrNotOpen ();
				}, "ThrowIfDisposedOrNotOpen/Closing");
			};
			co.Closed += delegate (object sender, EventArgs e) {
				closed = true;
				Assert.AreEqual (CommunicationState.Closed, co.State, "Closed/State");
				Assert.Throws<CommunicationObjectAbortedException> (delegate {
					co._ThrowIfDisposedOrNotOpen ();
				}, "ThrowIfDisposedOrNotOpen/Closed");
			};
			co.Faulted += delegate (object sender, EventArgs e) {
				faulted = true;
				Assert.AreEqual (CommunicationState.Faulted, co.State, "Faulted/State");
				Assert.Throws<CommunicationObjectFaultedException> (delegate {
					co._ThrowIfDisposedOrNotOpen ();
				}, "ThrowIfDisposedOrNotOpen/Faulted");
			};

			Assert.AreEqual (CommunicationState.Created, co.State, "Created");
			Assert.Throws<InvalidOperationException> (delegate {
				co._ThrowIfDisposedOrNotOpen ();
			}, "ThrowIfDisposedOrNotOpen/Created");

			co._FaultNoAssert ();

			co.OnAbortState = CommunicationState.Closing;
			co.Abort ();

			// ensure all states were tested
			Assert.IsFalse (opening, "opening");
			Assert.IsFalse (opened, "opened");
			Assert.IsTrue (closing, "closing");
			Assert.IsTrue (closed, "closing");
			Assert.IsTrue (faulted, "faulted");
		}

		class NoFaultCommunicationObject : CommunicationObjectPoker {

			protected override void OnFaulted ()
			{
				Assert.AreEqual (CommunicationState.Faulted, State, "OnFaulted/State");
				// base not called - Faulted won't be raised
			}
		}

		[TestMethod]
#if NET_2_1
		[MoonlightBug]
#else
		[NUnit.Framework.Ignore]
#endif
		public void NoOnFault ()
		{
			bool faulted = false;
			NoFaultCommunicationObject co = new NoFaultCommunicationObject ();
			co.Faulted += delegate (object sender, EventArgs e) {
				faulted = true;
			};
			co._FaultNoAssert ();
			Assert.AreEqual (CommunicationState.Faulted, co.State, "State");
			Assert.IsFalse (faulted, "faulted");
		}

		class FaultCommunicationObject : CommunicationObjectPoker {

			protected override void OnFaulted ()
			{
				base.OnFaulted ();
				throw new NotFiniteNumberException ();
			}
		}

		[TestMethod]
#if NET_2_1
		[MoonlightBug]
#else
		[NUnit.Framework.Ignore]
#endif
		public void OnFaultThrowing ()
		{
			FaultCommunicationObject co = new FaultCommunicationObject ();
			Assert.Throws<NotFiniteNumberException> (delegate {
				co._FaultNoAssert ();
			}, "Fault");
			// OnFault is not called more than one time
			co._FaultNoAssert ();
		}

		[TestMethod]
#if NET_2_1
		[MoonlightBug]
#else
		[NUnit.Framework.Ignore]
#endif
		public void AbortWhileAborting ()
		{
			int closing = 0;
			int closed = 0;
			CommunicationObjectPoker co = new CommunicationObjectPoker ();
			co.Closing += delegate (object sender, EventArgs e) {
				closing++;
				co.OnAbortState = CommunicationState.Closing;
				co.Abort ();
			};
			co.Closed += delegate (object sender, EventArgs e) {
				closed++;
				co.Abort ();
			};
			co.OnAbortState = CommunicationState.Created;
			// Abort will call Closing, which can call Abort...
			co.Abort ();

			Assert.AreEqual (1, closing, "closing");
			Assert.AreEqual (1, closed, "closed");
		}

		[TestMethod]
#if NET_2_1
		[MoonlightBug]
#else
		[NUnit.Framework.Ignore]
#endif
		public void AbortWhileClosing ()
		{
			int closing = 0;
			int closed = 0;
			CommunicationObjectPoker co = new CommunicationObjectPoker ();
			co.Closing += delegate (object sender, EventArgs e) {
				closing++;
				co.OnAbortState = CommunicationState.Closing;
				// ensure we're not repeating OnClosing
				co.Abort ();
			};
			co.Closed += delegate (object sender, EventArgs e) {
				closed++;
				co.Abort ();
			};
			co.Close ();

			Assert.AreEqual (1, closing, "closing");
			Assert.AreEqual (1, closed, "closed");
		}

		class NoAbortCommunicationObject : CommunicationObjectPoker {

			protected override void OnAbort ()
			{
				Assert.AreEqual (CommunicationState.Closing, State, "OnAbort/State");
				// base not called
			}
		}

		[TestMethod]
#if NET_2_1
		[MoonlightBug]
#else
		[NUnit.Framework.Ignore]
#endif
		public void NoOnAbort ()
		{
			bool closing = false;
			bool closed = false;
			NoAbortCommunicationObject co = new NoAbortCommunicationObject ();
			co.Closing += delegate (object sender, EventArgs e) {
				closing = true;
			};
			co.Closed += delegate (object sender, EventArgs e) {
				closed = true;
			};
			co.Abort ();
			Assert.AreEqual (CommunicationState.Closed, co.State, "State");
			Assert.IsTrue (closing, "closing");
			Assert.IsTrue (closed, "closed");
		}
	}
}

