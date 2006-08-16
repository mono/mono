//
// EventLogTest.cs -
// NUnit Test Cases for System.Diagnostics.EventLog
//
// Author:
//	Gert Driesen <driesen@users.sourceforge.net>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
// TODO: 
// - Clear
// - Close (?)
// - WriteEvent (check written event)
// - WriteEvent (2.0 only)
// - CreateEventSource2
// - CreateEventSource3 (2.0 only)
// - Entries (reading eventlog entries, unix only for now)
// - Exists (2 overloads)
// - LogNameFromSourceName
//

using System;
using System.Diagnostics;

using Microsoft.Win32;

using NUnit.Framework;

namespace MonoTests.System.Diagnostics
{
	[TestFixture]
	[Ignore ("Enable after unixregistry bug has been tracked and fixed.")]
	public class EventLogTest
	{
		[Test]
		public void Constructor1 ()
		{
			EventLog eventLog = new EventLog ();
			Assert.IsFalse (eventLog.EnableRaisingEvents, "#1");
			Assert.IsNotNull (eventLog.Entries, "#2");
			try {
				eventLog.Entries.GetEnumerator ().MoveNext ();
				Assert.Fail ("#3a");
			} catch (ArgumentException ex) {
				// Log property is not set (zero-length string)
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#3b");
				Assert.IsNotNull (ex.Message, "#3c");
				Assert.IsNull (ex.InnerException, "#3d");
			}
			Assert.IsNotNull (eventLog.Log, "#4");
			Assert.AreEqual (string.Empty, eventLog.Log, "#5");
#if NET_2_0
			try {
				string displayName = eventLog.LogDisplayName;
				Assert.Fail ("#6a: " + displayName);
			} catch (InvalidOperationException ex) {
				// Event log names must consist of printable characters and
				// cannot contain \, *, ?, or spaces
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#6b");
				Assert.IsNotNull (ex.Message, "#6c");
				Assert.IsNull (ex.InnerException, "#6d");
			}
#else
			Assert.IsNotNull (eventLog.LogDisplayName, "#6a");
			Assert.AreEqual (string.Empty, eventLog.LogDisplayName, "#6b");
#endif
			Assert.IsNotNull (eventLog.MachineName, "#7");
			Assert.AreEqual (".", eventLog.MachineName, "#8");
			Assert.IsNotNull (eventLog.Source, "#9");
			Assert.AreEqual (string.Empty, eventLog.Source, "#10");
			eventLog.Close ();
		}

		[Test]
		public void Constructor2 ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monotemplog", "."))
				Assert.Ignore ("Event log 'monotemplog' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monotemplog", ".");
			try {
				using (EventLog eventLog = new EventLog ("monotemplog")) {
					Assert.IsFalse (eventLog.EnableRaisingEvents, "#B1");
					Assert.IsNotNull (eventLog.Entries, "#B2");
					// MSBUG: Assert.AreEqual (0, eventLog.Entries.Count, "#B3");
					Assert.IsNotNull (eventLog.Log, "#B4");
					Assert.AreEqual ("monotemplog", eventLog.Log, "#B5");
					Assert.AreEqual ("monotemplog", eventLog.LogDisplayName, "#B6");
					Assert.IsNotNull (eventLog.MachineName, "#B7");
					Assert.AreEqual (".", eventLog.MachineName, "#B8");
					Assert.IsNotNull (eventLog.Source, "#B9");
					Assert.AreEqual (string.Empty, eventLog.Source, "#B10");
				}
			} finally {
				EventLog.Delete ("monotemplog");
			}
		}

		[Test]
		public void Constructor2_Log_DoesNotExist ()
		{
			if (EventLog.Exists ("monotemplog", "."))
				Assert.Ignore ("Event log 'monotemplog' should not exist.");

			EventLog eventLog = new EventLog ("monotemplog");
			Assert.IsFalse (eventLog.EnableRaisingEvents, "#B1");
			Assert.IsNotNull (eventLog.Entries, "#B2");
			try {
				eventLog.Entries.GetEnumerator ().MoveNext ();
				Assert.Fail ("#B3a");
			} catch (InvalidOperationException ex) {
				// The event log 'monotemplog' on computer '.' does not exist
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B3b");
				Assert.IsNotNull (ex.Message, "#B3c");
				Assert.IsTrue (ex.Message.IndexOf ("'monotemplog'") != -1, "#B3d");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#B3e");
				Assert.IsNull (ex.InnerException, "#B3f");
			}
			Assert.IsNotNull (eventLog.Log, "#B4");
			Assert.AreEqual ("monotemplog", eventLog.Log, "#B5");
			try {
				string displayName = eventLog.LogDisplayName;
				Assert.Fail ("#B6a: " + displayName);
			} catch (InvalidOperationException ex) {
				// Cannot find Log monotemplog on computer .
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B6b");
				Assert.IsNotNull (ex.Message, "#B6c");
				Assert.IsTrue (ex.Message.IndexOf ("monotemplog") != -1, "#B6d");
				Assert.IsTrue (ex.Message.IndexOf (".") != -1, "#B6e");
				Assert.IsNull (ex.InnerException, "#B6f");
			}
			Assert.IsNotNull (eventLog.MachineName, "#B7");
			Assert.AreEqual (".", eventLog.MachineName, "#B8");
			Assert.IsNotNull (eventLog.Source, "#B9");
			Assert.AreEqual (string.Empty, eventLog.Source, "#B10");
			eventLog.Close ();
		}

		[Test]
		public void Constructor2_Log_Empty ()
		{
			EventLog eventLog = new EventLog (string.Empty);
			Assert.IsFalse (eventLog.EnableRaisingEvents, "#A1");
			Assert.IsNotNull (eventLog.Entries, "#A2");
			try {
				eventLog.Entries.GetEnumerator ().MoveNext ();
				Assert.Fail ("#A3a");
			} catch (ArgumentException ex) {
				// Log property is not set (zero-length string)
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A3b");
				Assert.IsNotNull (ex.Message, "#A3c");
				Assert.IsNull (ex.InnerException, "#A3d");
			}
			Assert.IsNotNull (eventLog.Log, "#A4");
			Assert.AreEqual (string.Empty, eventLog.Log, "#A5");
#if NET_2_0
			try {
				string displayName = eventLog.LogDisplayName;
				Assert.Fail ("#A6a: " + displayName);
			} catch (InvalidOperationException ex) {
				// Event log names must consist of printable characters and
				// cannot contain \, *, ?, or spaces
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A6b");
				Assert.IsNotNull (ex.Message, "#A6c");
				Assert.IsNull (ex.InnerException, "#A6d");
			}
#else
			Assert.IsNotNull (eventLog.LogDisplayName, "#A6a");
			Assert.AreEqual (string.Empty, eventLog.LogDisplayName, "#A6b");
#endif
			Assert.IsNotNull (eventLog.MachineName, "#A7");
			Assert.AreEqual (".", eventLog.MachineName, "#A8");
			Assert.IsNotNull (eventLog.Source, "#A9");
			Assert.AreEqual (string.Empty, eventLog.Source, "#A10");
			eventLog.Close ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor2_Log_Null ()
		{
			new EventLog (null);
		}

		[Test]
		public void Constructor3 ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monotemplog", "."))
				Assert.Ignore ("Event log 'monotemplog' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monotemplog", ".");
			try {
				using (EventLog eventLog = new EventLog ("monotemplog", ".")) {
					Assert.IsFalse (eventLog.EnableRaisingEvents, "#B1");
					Assert.IsNotNull (eventLog.Entries, "#B2");
					// MSBUG: Assert.AreEqual (0, eventLog.Entries.Count, "#B3");
					Assert.IsNotNull (eventLog.Log, "#B4");
					Assert.AreEqual ("monotemplog", eventLog.Log, "#B5");
					Assert.AreEqual ("monotemplog", eventLog.LogDisplayName, "#B6");
					Assert.IsNotNull (eventLog.MachineName, "#B7");
					Assert.AreEqual (".", eventLog.MachineName, "#B8");
					Assert.IsNotNull (eventLog.Source, "#B9");
					Assert.AreEqual (string.Empty, eventLog.Source, "#B10");
				}
			} finally {
				EventLog.Delete ("monotemplog");
			}
		}

		[Test]
		public void Constructor3_Log_DoesNotExist ()
		{
			if (EventLog.Exists ("monotemplog", "."))
				Assert.Ignore ("Event log 'monotemplog' should not exist.");

			EventLog eventLog = new EventLog ("monotemplog", ".");
			Assert.IsFalse (eventLog.EnableRaisingEvents, "#B1");
			Assert.IsNotNull (eventLog.Entries, "#B2");
			try {
				eventLog.Entries.GetEnumerator ().MoveNext ();
				Assert.Fail ("#B3a");
			} catch (InvalidOperationException ex) {
				// The event log 'monotemplog' on computer '.' does not exist
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B3b");
				Assert.IsNotNull (ex.Message, "#B3c");
				Assert.IsTrue (ex.Message.IndexOf ("'monotemplog'") != -1, "#B3d");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#B3e");
				Assert.IsNull (ex.InnerException, "#B3f");
			}
			Assert.IsNotNull (eventLog.Log, "#B4");
			Assert.AreEqual ("monotemplog", eventLog.Log, "#B5");
			try {
				string displayName = eventLog.LogDisplayName;
				Assert.Fail ("#B6a: " + displayName);
			} catch (InvalidOperationException ex) {
				// Cannot find Log monotemplog on computer .
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B6b");
				Assert.IsNotNull (ex.Message, "#B6c");
				Assert.IsTrue (ex.Message.IndexOf ("monotemplog") != -1, "#B6d");
				Assert.IsTrue (ex.Message.IndexOf (".") != -1, "#B6e");
				Assert.IsNull (ex.InnerException, "#B6f");
			}
			Assert.IsNotNull (eventLog.MachineName, "#B7");
			Assert.AreEqual (".", eventLog.MachineName, "#B8");
			Assert.IsNotNull (eventLog.Source, "#B9");
			Assert.AreEqual (string.Empty, eventLog.Source, "#B10");
			eventLog.Close ();
		}

		[Test]
		public void Constructor3_Log_Empty ()
		{
			EventLog eventLog = new EventLog (string.Empty, ".");
			Assert.IsFalse (eventLog.EnableRaisingEvents, "#A1");
			Assert.IsNotNull (eventLog.Entries, "#A2");
			try {
				eventLog.Entries.GetEnumerator ().MoveNext ();
				Assert.Fail ("#A3a");
			} catch (ArgumentException ex) {
				// Log property is not set (zero-length string)
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A3b");
				Assert.IsNotNull (ex.Message, "#A3c");
				Assert.IsNull (ex.InnerException, "#A3d");
			}
			Assert.IsNotNull (eventLog.Log, "#A4");
			Assert.AreEqual (string.Empty, eventLog.Log, "#A5");
#if NET_2_0
			try {
				string displayName = eventLog.LogDisplayName;
				Assert.Fail ("#A6a: " + displayName);
			} catch (InvalidOperationException ex) {
				// Event log names must consist of printable characters and
				// cannot contain \, *, ?, or spaces
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A6b");
				Assert.IsNotNull (ex.Message, "#A6c");
				Assert.IsNull (ex.InnerException, "#A6d");
			}
#else
			Assert.IsNotNull (eventLog.LogDisplayName, "#A6a");
			Assert.AreEqual (string.Empty, eventLog.LogDisplayName, "#A6b");
#endif
			Assert.IsNotNull (eventLog.MachineName, "#A7");
			Assert.AreEqual (".", eventLog.MachineName, "#A8");
			Assert.IsNotNull (eventLog.Source, "#A9");
			Assert.AreEqual (string.Empty, eventLog.Source, "#A10");
			eventLog.Close ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor3_Log_Null ()
		{
			new EventLog (null, ".");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Invalid value '' for parameter 'machineName'
		public void Constructor3_MachineName_Empty ()
		{
			new EventLog ("monotemplog", string.Empty);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Invalid value '' for parameter 'machineName'
		public void Constructor3_MachineName_Null ()
		{
			new EventLog ("monotemplog", null);
		}

		[Test]
		public void Constructor4 ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monotemplog", "."))
				Assert.Ignore ("Event log 'monotemplog' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monotemplog", ".");
			try {
				using (EventLog eventLog = new EventLog ("monotemplog", ".", "monotempsource")) {
					Assert.IsFalse (eventLog.EnableRaisingEvents, "#A1");
					Assert.IsNotNull (eventLog.Entries, "#A2");
					// MSBUG: Assert.AreEqual (0, eventLog.Entries.Count, "#A3");
					Assert.IsNotNull (eventLog.Log, "#A4");
					Assert.AreEqual ("monotemplog", eventLog.Log, "#A5");
					Assert.AreEqual ("monotemplog", eventLog.LogDisplayName, "#A6");
					Assert.IsNotNull (eventLog.MachineName, "#A7");
					Assert.AreEqual (".", eventLog.MachineName, "#A8");
					Assert.IsNotNull (eventLog.Source, "#A9");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A10");
				}
			} finally {
				EventLog.Delete ("monotemplog");
			}
		}

		[Test]
		public void Constructor4_Log_DoesNotExist ()
		{
			if (EventLog.Exists ("monotemplog", "."))
				Assert.Ignore ("Event log 'monotemplog' should not exist.");

			EventLog eventLog = new EventLog ("monotemplog", ".", "monotempsource");
			Assert.IsFalse (eventLog.EnableRaisingEvents, "#B1");
			Assert.IsNotNull (eventLog.Entries, "#B2");
			try {
				eventLog.Entries.GetEnumerator ().MoveNext ();
				Assert.Fail ("#B3a");
			} catch (InvalidOperationException ex) {
				// The event log 'monotemplog' on computer '.' does not exist
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B3b");
				Assert.IsNotNull (ex.Message, "#B3c");
				Assert.IsTrue (ex.Message.IndexOf ("'monotemplog'") != -1, "#B3d");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#B3e");
				Assert.IsNull (ex.InnerException, "#B3f");
			}
			Assert.IsNotNull (eventLog.Log, "#B4");
			Assert.AreEqual ("monotemplog", eventLog.Log, "#B5");
			try {
				string displayName = eventLog.LogDisplayName;
				Assert.Fail ("#B6a: " + displayName);
			} catch (InvalidOperationException ex) {
				// Cannot find Log monotemplog on computer .
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B6b");
				Assert.IsNotNull (ex.Message, "#B6c");
				Assert.IsTrue (ex.Message.IndexOf ("monotemplog") != -1, "#B6d");
				Assert.IsTrue (ex.Message.IndexOf (".") != -1, "#B6e");
				Assert.IsNull (ex.InnerException, "#B6f");
			}
			Assert.IsNotNull (eventLog.MachineName, "#B7");
			Assert.AreEqual (".", eventLog.MachineName, "#B8");
			Assert.IsNotNull (eventLog.Source, "#B9");
			Assert.AreEqual ("monotempsource", eventLog.Source, "#B10");
			eventLog.Close ();
		}

		[Test]
		public void Constructor4_Log_Empty ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			bool applicationLogExists = EventLog.Exists ("Application");
			try {
				EventLog eventLog = new EventLog (string.Empty, ".", "monotempsource");
				Assert.IsFalse (eventLog.EnableRaisingEvents, "#A1");
				Assert.IsNotNull (eventLog.Entries, "#A2");
				try {
					eventLog.Entries.GetEnumerator ().MoveNext ();
					Assert.Fail ("#A3a");
				} catch (ArgumentException ex) {
					// Log property is not set (zero-length string)
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A3b");
					Assert.IsNotNull (ex.Message, "#A3c");
					Assert.IsNull (ex.InnerException, "#A3d");
				}
				Assert.IsNotNull (eventLog.Log, "#A4");
				Assert.AreEqual (string.Empty, eventLog.Log, "#A5");
#if NET_2_0
				try {
					string displayName = eventLog.LogDisplayName;
					Assert.Fail ("#A6a: " + displayName);
				} catch (InvalidOperationException ex) {
					// Event log names must consist of printable characters and
					// cannot contain \, *, ?, or spaces
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A6b");
					Assert.IsNotNull (ex.Message, "#A6c");
					Assert.IsNull (ex.InnerException, "#A6d");
				}
#else
				Assert.IsNotNull (eventLog.LogDisplayName, "#A6a");
				Assert.AreEqual (string.Empty, eventLog.LogDisplayName, "#A6b");
#endif
				Assert.IsNotNull (eventLog.MachineName, "#A7");
				Assert.AreEqual (".", eventLog.MachineName, "#A8");
				Assert.IsNotNull (eventLog.Source, "#A9");
				Assert.AreEqual ("monotempsource", eventLog.Source, "#A10");
				eventLog.Close ();
			} finally {
				if (!applicationLogExists) {
					if (EventLog.Exists ("Application"))
						EventLog.Delete ("Application");
				} else {
					if (EventLog.SourceExists ("monotempsource", "."))
						EventLog.DeleteEventSource ("monotempsource", ".");
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor4_Log_Null ()
		{
			new EventLog (null, ".", "monotempsource");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Invalid value '' for parameter 'machineName'
		public void Constructor4_MachineName_Empty ()
		{
			new EventLog ("monotemplog", string.Empty, "monotempsource");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Invalid value '' for parameter 'machineName'
		public void Constructor4_MachineName_Null ()
		{
			new EventLog ("monotemplog", null, "monotempsource");
		}

		[Test]
		public void Constructor4_Source_DoesNotExist ()
		{
			if (EventLog.SourceExists ("monoothersource", "."))
				Assert.Ignore ("Event log source 'monoothersource' should not exist.");

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monotemplog", "."))
				Assert.Ignore ("Event log 'monotemplog' should not exist.");

			EventLog.CreateEventSource ("monoothersource", "monotemplog", ".");
			try {
				using (EventLog eventLog = new EventLog ("monotemplog", ".", "monotempsource")) {
					Assert.IsFalse (eventLog.EnableRaisingEvents, "#A1");
					Assert.IsNotNull (eventLog.Entries, "#A2");
					// MSBUG: Assert.AreEqual (0, eventLog.Entries.Count, "#A3");
					Assert.IsNotNull (eventLog.Log, "#A4");
					Assert.AreEqual ("monotemplog", eventLog.Log, "#A5");
					Assert.AreEqual ("monotemplog", eventLog.LogDisplayName, "#A6");
					Assert.IsNotNull (eventLog.MachineName, "#A7");
					Assert.AreEqual (".", eventLog.MachineName, "#A8");
					Assert.IsNotNull (eventLog.Source, "#A9");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A10");
					Assert.IsTrue (EventLog.Exists ("monotemplog"), "#A11");
					Assert.IsTrue (EventLog.SourceExists ("monoothersource"), "#A12");
					Assert.IsFalse (EventLog.SourceExists ("monotempsource"), "#A13");
				}
			} finally {
				if (EventLog.Exists ("monotemplog"))
					EventLog.Delete ("monotemplog");
			}
		}

		[Test]
		public void Constructor4_Source_Empty ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monotemplog", "."))
				Assert.Ignore ("Event log 'monotemplog' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monotemplog", ".");
			try {
				using (EventLog eventLog = new EventLog ("monotemplog", ".", string.Empty)) {
					Assert.IsFalse (eventLog.EnableRaisingEvents, "#A1");
					Assert.IsNotNull (eventLog.Entries, "#A2");
					// MSBUG: Assert.AreEqual (0, eventLog.Entries.Count, "#A3");
					Assert.IsNotNull (eventLog.Log, "#A4");
					Assert.AreEqual ("monotemplog", eventLog.Log, "#A5");
					Assert.AreEqual ("monotemplog", eventLog.LogDisplayName, "#A6");
					Assert.IsNotNull (eventLog.MachineName, "#A7");
					Assert.AreEqual (".", eventLog.MachineName, "#A8");
					Assert.IsNotNull (eventLog.Source, "#A9");
					Assert.AreEqual (string.Empty, eventLog.Source, "#A10");
					Assert.IsTrue (EventLog.Exists ("monotemplog"), "#A11");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A12");
				}
			} finally {
				if (EventLog.Exists ("monotemplog"))
					EventLog.Delete ("monotemplog");
			}
		}

		[Test]
		public void Constructor4_Source_Null ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monotemplog", "."))
				Assert.Ignore ("Event log 'monotemplog' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monotemplog", ".");
			try {
				using (EventLog eventLog = new EventLog ("monotemplog", ".", null)) {
					Assert.IsFalse (eventLog.EnableRaisingEvents, "#A1");
					Assert.IsNotNull (eventLog.Entries, "#A2");
					// MSBUG: Assert.AreEqual (0, eventLog.Entries.Count, "#A3");
					Assert.IsNotNull (eventLog.Log, "#A4");
					Assert.AreEqual ("monotemplog", eventLog.Log, "#A5");
					Assert.AreEqual ("monotemplog", eventLog.LogDisplayName, "#A6");
					Assert.IsNotNull (eventLog.MachineName, "#A7");
					Assert.AreEqual (".", eventLog.MachineName, "#A8");
					Assert.IsNull (eventLog.Source, "#A9");
					Assert.IsTrue (EventLog.Exists ("monotemplog"), "#A10");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A11");
				}
			} finally {
				if (EventLog.Exists ("monotemplog"))
					EventLog.Delete ("monotemplog");
			}
		}

		[Test]
		public void CreateEventSource1 ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monotemplog", "."))
				Assert.Ignore ("Event log 'monotemplog' should not exist.");

			try {
				EventLog.CreateEventSource ("monotempsource", "monotemplog");
				Assert.IsTrue (EventLog.Exists ("monotemplog", "."), "#A1");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource", "."), "#A2");

				using (EventLog eventLog = new EventLog ("monotemplog", ".", "monotempsource")) {
					Assert.IsFalse (eventLog.EnableRaisingEvents, "#B1");
					Assert.IsNotNull (eventLog.Entries, "#B2");
					// MSBUG: Assert.AreEqual (0, eventLog.Entries.Count, "#B3");
					Assert.IsNotNull (eventLog.Log, "#B4");
					Assert.AreEqual ("monotemplog", eventLog.Log, "#B5");
					Assert.IsNotNull (eventLog.LogDisplayName, "#B6");
					Assert.AreEqual ("monotemplog", eventLog.LogDisplayName, "#B7");
					Assert.IsNotNull (eventLog.MachineName, "#B8");
					Assert.AreEqual (".", eventLog.MachineName, "#B9");
					Assert.IsNotNull (eventLog.Source, "#B10");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#B11");
				}
			} finally {
				if (EventLog.Exists ("monotemplog", "."))
					EventLog.Delete ("monotemplog", ".");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Must specify value for source
		public void CreateEventSource1_Source_Empty ()
		{
			EventLog.CreateEventSource (string.Empty, "monotemplog");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Must specify value for source
		public void CreateEventSource1_Source_Null ()
		{
			EventLog.CreateEventSource (null, "monotemplog");
		}

		[Test]
		public void CreateEventSource1_Log_Empty ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			bool logExists = EventLog.Exists ("Application", ".");
			try {
				EventLog.CreateEventSource ("monotempsource", string.Empty);
				string logName = EventLog.LogNameFromSourceName ("monotempsource", ".");
				Assert.IsNotNull (logName, "#1");
				Assert.AreEqual ("application", logName.ToLower (), "#2");
			} finally {
				if (!logExists) {
					if (EventLog.Exists ("Application", ".")) {
						EventLog.Delete ("Application", ".");
					}
				} else {
					if (EventLog.SourceExists ("monotempsource", "."))
						EventLog.DeleteEventSource ("monotempsource", ".");
				}
			}
		}

		[Test]
		public void CreateEventSource1_Log_Null ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			bool logExists = EventLog.Exists ("Application", ".");
			try {
				EventLog.CreateEventSource ("monotempsource", null);
				string logName = EventLog.LogNameFromSourceName ("monotempsource", ".");
				Assert.IsNotNull (logName, "#1");
				Assert.AreEqual ("application", logName.ToLower (), "#2");
			} finally {
				if (!logExists) {
					if (EventLog.Exists ("Application"))
						EventLog.Delete ("Application");
				} else {
					if (EventLog.SourceExists ("monotempsource", "."))
						EventLog.DeleteEventSource ("monotempsource", ".");
				}
			}
		}

		[Test]
		public void Delete1 ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monotemplog", "."))
				Assert.Ignore ("Event log 'monotemplog' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monotemplog", ".");
			Assert.IsTrue (EventLog.Exists ("monotemplog", "."), "#1");
			EventLog.Delete ("monotemplog");
			Assert.IsFalse (EventLog.Exists ("monotemplog", "."), "#2");
			Assert.IsFalse (EventLog.Exists ("monotempsource", "."), "#3");
		}

		[Test]
		public void Delete1_Log_DoesNotExist ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monotemplog", "."))
				Assert.Ignore ("Event log 'monotemplog' should not exist.");

			try {
				EventLog.Delete ("monotemplog");
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("monotemplog") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf (".") != -1, "#5");
				Assert.IsNull (ex.InnerException, "#6");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Log to delete was not specified
		public void Delete1_Log_Empty ()
		{
			EventLog.Delete (string.Empty);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Log to delete was not specified
		public void Delete1_Log_Null ()
		{
			EventLog.Delete (null);
		}

		[Test]
		public void Delete2 ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monotemplog", "."))
				Assert.Ignore ("Event log 'monotemplog' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monotemplog", ".");
			Assert.IsTrue (EventLog.Exists ("monotemplog", "."), "#1");
			EventLog.Delete ("monotemplog", ".");
			Assert.IsFalse (EventLog.Exists ("monotemplog", "."), "#2");
			Assert.IsFalse (EventLog.Exists ("monotempsource", "."), "#3");
		}

		[Test]
		public void Delete2_Log_DoesNotExist ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monotemplog", "."))
				Assert.Ignore ("Event log 'monotemplog' should not exist.");

			try {
				EventLog.Delete ("monotemplog", ".");
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("monotemplog") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf (".") != -1, "#5");
				Assert.IsNull (ex.InnerException, "#6");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Log to delete was not specified
		public void Delete2_Log_Empty ()
		{
			EventLog.Delete (string.Empty, ".");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Log to delete was not specified
		public void Delete2_Log_Null ()
		{
			EventLog.Delete (null, ".");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Invalid format for argument machineName
		public void Delete2_MachineName_Empty ()
		{
			EventLog.Delete ("monotemplog", string.Empty);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Invalid format for argument machineName
		public void Delete2_MachineName_Null ()
		{
			EventLog.Delete ("monotemplog", null);
		}

		[Test]
		public void DeleteEventSource1 ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			bool monotemplogExists = EventLog.Exists ("monotemplog", ".");
			try {
				EventLog.CreateEventSource ("monotempsource", "monotemplog", ".");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource", "."), "#1");
				EventLog.DeleteEventSource ("monotempsource");
				Assert.IsFalse (EventLog.SourceExists ("monotempsource", "."), "#2");
			} finally {
				if (!monotemplogExists) {
					EventLog.Delete ("monotemplog");
				}
			}
		}

		[Test]
		public void DeleteEventSource1_Source_DoesNotExist ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			try {
				EventLog.DeleteEventSource ("monotempsource");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("'monotempsource'") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#5");
				Assert.IsNull (ex.InnerException, "#6");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // The source '' is not registered on machine '.', ...
		public void DeleteEventSource1_Source_Empty ()
		{
			EventLog.DeleteEventSource (string.Empty);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // The source '' is not registered on machine '.', ...
		public void DeleteEventSource1_Source_Null ()
		{
			EventLog.DeleteEventSource (null);
		}

		[Test]
		public void DeleteEventSource2 ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			bool monotemplogExists = EventLog.Exists ("monotemplog", ".");
			try {
				EventLog.CreateEventSource ("monotempsource", "monotemplog");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#1");
				EventLog.DeleteEventSource ("monotempsource", ".");
				Assert.IsFalse (EventLog.SourceExists ("monotempsource"), "#2");
			} finally {
				if (!monotemplogExists) {
					EventLog.Delete ("monotemplog");
				}
			}
		}

		[Test]
		public void DeleteEventSource2_Source_DoesNotExist ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			try {
				EventLog.DeleteEventSource ("monotempsource", ".");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("'monotempsource'") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#5");
				Assert.IsNull (ex.InnerException, "#6");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // The source '' is not registered on machine '.', ...
		public void DeleteEventSource2_Source_Empty ()
		{
			EventLog.DeleteEventSource (string.Empty, ".");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // The source '' is not registered on machine '.', ...
		public void DeleteEventSource2_Source_Null ()
		{
			EventLog.DeleteEventSource (null, ".");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Invalid value '' for parameter 'machineName'
		public void DeleteEventSource2_MachineName_Empty ()
		{
			EventLog.DeleteEventSource ("monotempsource", string.Empty);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Invalid value '' for parameter 'machineName'
		public void DeleteEventSource2_MachineName_Null ()
		{
			EventLog.DeleteEventSource ("monotempsource", null);
		}

		[Test]
		public void Log ()
		{
			EventLog eventLog = new EventLog ();
			eventLog.Log = string.Empty;
			Assert.AreEqual (string.Empty, eventLog.Log, "#1");
			Assert.AreEqual (string.Empty, eventLog.Source, "#2");
			eventLog.Log = "monotemplog";
			Assert.AreEqual ("monotemplog", eventLog.Log, "#3");
			Assert.AreEqual (string.Empty, eventLog.Source, "#4");
			eventLog.Log = string.Empty;
			Assert.AreEqual (string.Empty, eventLog.Log, "#5");
			Assert.AreEqual (string.Empty, eventLog.Source, "#6");
			eventLog.Close ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Log_Null ()
		{
			EventLog eventLog = new EventLog ();
			eventLog.Log = null;
		}

		[Test]
		public void Source ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monotemplog", "."))
				Assert.Ignore ("Event log 'monotemplog' should not exist.");

			using (EventLog eventLog = new EventLog ()) {
				eventLog.Source = null;
				Assert.AreEqual (string.Empty, eventLog.Source, "#A1");
				Assert.AreEqual (string.Empty, eventLog.Log, "#A2");
				eventLog.Source = "monotempsource";
				Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
				Assert.AreEqual (string.Empty, eventLog.Log, "#A4");
				eventLog.Source = null;
				Assert.AreEqual (string.Empty, eventLog.Source, "#A5");
				Assert.AreEqual (string.Empty, eventLog.Log, "#A6");

				EventLog.CreateEventSource ("monotempsource", "monotemplog");
				try {
					Assert.AreEqual (string.Empty, eventLog.Source, "#B1");
					Assert.AreEqual (string.Empty, eventLog.Log, "#B2");
					eventLog.Source = "monotempsource";
					Assert.AreEqual ("monotempsource", eventLog.Source, "#B3");
					Assert.AreEqual ("monotemplog", eventLog.Log, "#B4");
					eventLog.Log = string.Empty;
					Assert.AreEqual ("monotempsource", eventLog.Source, "#B5");
					Assert.AreEqual ("monotemplog", eventLog.Log, "#B6");
					eventLog.Source = null;
					Assert.AreEqual (string.Empty, eventLog.Source, "#B7");
					Assert.AreEqual ("monotemplog", eventLog.Log, "#B8");
					eventLog.Log = string.Empty;
					Assert.AreEqual (string.Empty, eventLog.Source, "#B9");
					Assert.AreEqual (string.Empty, eventLog.Log, "#B10");
				} finally {
					EventLog.Delete ("monotemplog");
				}
			}
		}

		[Test]
		[Category ("NotWorking")] // bug #79059
		public void SourceExists1 ()
		{
			using (RegistryKey logKey = FindLogKeyByName ("monotempsource")) {
				if (logKey != null)
					Assert.Ignore ("Event log 'monotempsource' should not exist.");
			}

			using (RegistryKey logKey = FindLogKeyByName ("monotemplog")) {
				if (logKey != null)
					Assert.Ignore ("Event log 'monotemplog' should not exist.");
			}

			using (RegistryKey sourceKey = FindSourceKeyByName ("monotempsource")) {
				if (sourceKey != null)
					Assert.Ignore ("Event log source 'monotempsource' should not exist.");
			}

			Assert.IsFalse (EventLog.SourceExists ("monotempsource"), "#1");

			using (RegistryKey eventLogKey = EventLogKey) {
				RegistryKey logKey = eventLogKey.CreateSubKey ("monotempsource");
				try {
					// make sure we do not mistake a log for a source
					Assert.IsFalse (EventLog.SourceExists ("monotempsource"), "#2");
				} finally {
					if (logKey != null) {
						logKey.Close ();
						eventLogKey.DeleteSubKeyTree ("monotempsource");
					}
				}

				logKey = eventLogKey.CreateSubKey ("monotemplog");
				try {
					RegistryKey sourceKey = null;
					try {
						// create temporary source key
						sourceKey = logKey.CreateSubKey ("monotempsource");
						Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#3");
					} finally {
						if (sourceKey != null) {
							sourceKey.Close ();
							logKey.DeleteSubKeyTree ("monotempsource");
						}
					}
				} finally {
					if (logKey != null) {
						logKey.Close ();
						eventLogKey.DeleteSubKeyTree ("monotemplog");
					}
				}
			}
		}

		[Test]
		public void SourceExists1_Source_Empty ()
		{
			Assert.IsFalse (EventLog.SourceExists (string.Empty));
		}

		[Test]
		public void SourceExists1_Source_Null ()
		{
			Assert.IsFalse (EventLog.SourceExists (null));
		}

		[Test]
		[Category ("NotWorking")] // bug #79059
		public void SourceExists2 ()
		{
			using (RegistryKey logKey = FindLogKeyByName ("monotempsource")) {
				if (logKey != null)
					Assert.Ignore ("Event log 'monotempsource' should not exist.");
			}

			using (RegistryKey logKey = FindLogKeyByName ("monotemplog")) {
				if (logKey != null)
					Assert.Ignore ("Event log 'monotemplog' should not exist.");
			}

			using (RegistryKey sourceKey = FindSourceKeyByName ("monotempsource")) {
				if (sourceKey != null)
					Assert.Ignore ("Event log source 'monotempsource' should not exist.");
			}

			Assert.IsFalse (EventLog.SourceExists ("monotempsource", "."), "#1");

			using (RegistryKey eventLogKey = EventLogKey) {
				RegistryKey logKey = eventLogKey.CreateSubKey ("monotempsource");
				try {
					// make sure we do not mistake a log for a source
					Assert.IsFalse (EventLog.SourceExists ("monotempsource", "."), "#2");
				} finally {
					if (logKey != null) {
						logKey.Close ();
						eventLogKey.DeleteSubKeyTree ("monotempsource");
					}
				}

				logKey = eventLogKey.CreateSubKey ("monotemplog");
				try {
					RegistryKey sourceKey = null;
					try {
						// create temporary source key
						sourceKey = logKey.CreateSubKey ("monotempsource");
						Assert.IsTrue (EventLog.SourceExists ("monotempsource", "."), "#3");
					} finally {
						if (sourceKey != null) {
							sourceKey.Close ();
							logKey.DeleteSubKeyTree ("monotempsource");
						}
					}
				} finally {
					if (logKey != null) {
						logKey.Close ();
						eventLogKey.DeleteSubKeyTree ("monotemplog");
					}
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Invalid value  for parameter machineName
		public void SourceExists2_MachineName_Empty ()
		{
			EventLog.SourceExists ("monotempsource", string.Empty);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Invalid value  for parameter machineName
		public void SourceExists2_MachineName_Null ()
		{
			EventLog.SourceExists ("monotempsource", null);
		}

		[Test]
		public void SourceExists2_Source_Empty ()
		{
			Assert.IsFalse (EventLog.SourceExists (string.Empty, "."));
		}

		[Test]
		public void SourceExists2_Source_Null ()
		{
			Assert.IsFalse (EventLog.SourceExists (null, "."));
		}

		[Test]
		public void WriteEntry1 ()
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				Assert.Ignore ("Win32 implementation does not yet support reading from event log");

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monotemplog", "."))
				Assert.Ignore ("Event log 'monotemplog' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monotemplog");
			try {
				using (EventLog eventLog = new EventLog ("monotemplog", ".", "monotempsource")) {
					eventLog.WriteEntry ("WriteEntry1");
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#1");
					Assert.AreEqual ("monotemplog", eventLog.Log.ToLower (), "#2"); // unix registry API performs ToLower of keys
					Assert.AreEqual ("monotempsource", eventLog.Source, "#3");
					Assert.IsTrue (EventLog.Exists ("monotemplog"), "#4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#5");
					Assert.AreEqual ("monotemplog", EventLog.LogNameFromSourceName ("monotempsource", ".").ToLower (), "#6");

					EventLogEntry entry = eventLog.Entries[eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#7");
					Assert.IsNotNull (entry.Category, "#8");
					Assert.AreEqual ("(0)", entry.Category, "#9");
					Assert.AreEqual (0, entry.CategoryNumber, "#10");
					Assert.IsNotNull (entry.Data, "#11");
					Assert.AreEqual (0, entry.Data.Length, "#12");
					Assert.AreEqual (EventLogEntryType.Information, entry.EntryType, "#13");
					Assert.AreEqual (0, entry.EventID, "#14");
#if false && NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#15");
#endif
					Assert.IsNotNull (entry.MachineName, "#16");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#17");
					Assert.IsNotNull (entry.ReplacementStrings, "#18");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#19");
					Assert.AreEqual ("WriteEntry1", entry.ReplacementStrings[0], "#20");
					Assert.IsNotNull (entry.Source, "#21");
					Assert.AreEqual ("monotempsource", entry.Source, "#22");
					Assert.IsNull (entry.UserName, "#23");
				}
			} finally {
				if (EventLog.Exists ("monotemplog"))
					EventLog.Delete ("monotemplog");
			}
		}

		[Test]
		public void WriteEntry1_Log_Empty ()
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				Assert.Ignore ("Win32 implementation does not yet support reading from event log");

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			bool applicationLogExists = EventLog.Exists ("Application", ".");

			// specified source does not exist, so use Application log
			try {
				using (EventLog eventLog = new EventLog (string.Empty, ".", "monotempsource")) {
					eventLog.WriteEntry ("WriteEntry1_Log_Empty");
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#1");
					Assert.AreEqual ("application", eventLog.Log.ToLower (), "#2"); // unix registry API performs ToLower of keys
					Assert.AreEqual ("monotempsource", eventLog.Source, "#3");
					Assert.IsTrue (EventLog.Exists ("Application"), "#4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#5");
					Assert.AreEqual ("application", EventLog.LogNameFromSourceName ("monotempsource", ".").ToLower (), "#6"); // unix registry API performs ToLower of keys
				}
			} finally {
				if (!applicationLogExists) {
					if (EventLog.Exists ("Application"))
						EventLog.Delete ("Application");
				} else {
					if (EventLog.SourceExists ("monotempsource", "."))
						EventLog.DeleteEventSource ("monotempsource", ".");
				}
			}
		}

		[Test]
		public void WriteEntry1_Source_DoesNotExist ()
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				Assert.Ignore ("Win32 implementation does not yet support reading from event log");

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.SourceExists ("monoothersource", "."))
				Assert.Ignore ("Event log source 'monoothersource' should not exist.");

			if (EventLog.Exists ("monotemplog", "."))
				Assert.Ignore ("Event log 'monotemplog' should not exist.");

			EventLog.CreateEventSource ("monoothersource", "monotemplog");
			try {
				using (EventLog eventLog = new EventLog ("monotemplog", ".", "monotempsource")) {
					eventLog.WriteEntry ("WriteEntry1");
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#1");
					Assert.AreEqual ("monotemplog", eventLog.Log.ToLower (), "#2"); // unix registry API performs ToLower of keys
					Assert.AreEqual ("monotempsource", eventLog.Source, "#3");
					Assert.IsTrue (EventLog.Exists ("monotemplog"), "#4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#5");
					Assert.AreEqual ("monotemplog", EventLog.LogNameFromSourceName ("monotempsource", ".").ToLower (), "#6");

					EventLogEntry entry = eventLog.Entries[eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#7");
					Assert.IsNotNull (entry.Category, "#8");
					Assert.AreEqual ("(0)", entry.Category, "#9");
					Assert.AreEqual (0, entry.CategoryNumber, "#10");
					Assert.IsNotNull (entry.Data, "#11");
					Assert.AreEqual (0, entry.Data.Length, "#12");
					Assert.AreEqual (EventLogEntryType.Information, entry.EntryType, "#13");
					Assert.AreEqual (0, entry.EventID, "#14");
#if false && NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#15");
#endif
					Assert.IsNotNull (entry.MachineName, "#16");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#17");
					Assert.IsNotNull (entry.ReplacementStrings, "#18");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#19");
					Assert.AreEqual ("WriteEntry1", entry.ReplacementStrings[0], "#20");
					Assert.IsNotNull (entry.Source, "#21");
					Assert.AreEqual ("monotempsource", entry.Source, "#22");
					Assert.IsNull (entry.UserName, "#23");
				}
			} finally {
				if (EventLog.Exists ("monotemplog"))
					EventLog.Delete ("monotemplog");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Source property was not set before writing to the event log
		public void WriteEntry1_Source_Empty ()
		{
			EventLog eventLog = new EventLog ("monotemplog");
			eventLog.WriteEntry ("test");
		}

		private static RegistryKey EventLogKey {
			get {
				return Registry.LocalMachine.OpenSubKey (@"SYSTEM\CurrentControlSet\Services\EventLog", true);
			}
		}

		private static RegistryKey FindLogKeyByName (string logName)
		{
			RegistryKey eventLogKey = null;
			try {
				eventLogKey = EventLogKey;
				if (eventLogKey == null)
					Assert.Fail ("Event log key does not exist");

				RegistryKey logKey = eventLogKey.OpenSubKey (logName, true);
				if (logKey != null)
					return logKey;
				return null;
			} finally {
				if (eventLogKey != null)
					eventLogKey.Close ();
			}
		}

		private static RegistryKey FindSourceKeyByName (string source) {
			RegistryKey eventLogKey = null;
			try {
				eventLogKey = EventLogKey;
				if (eventLogKey == null)
					Assert.Fail ("Event log key does not exist");

				string[] subKeys = eventLogKey.GetSubKeyNames ();
				for (int i = 0; i < subKeys.Length; i++) {
					using (RegistryKey logKey = eventLogKey.OpenSubKey (subKeys[i], true)) {
						if (logKey != null) {
							RegistryKey sourceKey = logKey.OpenSubKey (source, true);
							if (sourceKey != null)
								return sourceKey;
						}
					}
				}
				return null;
			} finally {
				if (eventLogKey != null)
					eventLogKey.Close ();
			}
		}
	}
}
