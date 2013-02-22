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
// - Close (?)
// - CreateEventSource2
// - CreateEventSource3 (2.0 only)
// - Exists : local file
// - SourceExists : local file
// - GetEventLogs (2 overloads)
// - case-insensitive tests
// - use temp directory for event storage on 2.0 profile
// - WriteEvent tests with large instanceID (and check EventID)
//

#if !MOBILE

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;

using Microsoft.Win32;

using NUnit.Framework;

namespace MonoTests.System.Diagnostics
{
	[TestFixture]
	public class EventLogTest
	{
#if NET_2_0
		private string _originalEventLogImpl;
		private string _eventLogStore;
#endif

		private const string EVENTLOG_TYPE_VAR = "MONO_EVENTLOG_TYPE";

		// IMPORTANT: also update constants in EventLogTest
		private const string LOCAL_FILE_IMPL = "local";
		private const string WIN32_IMPL = "win32";
		private const string NULL_IMPL = "null";

#if NET_2_0 // Environment.SetEnvironmentVariable is only available in 2.0 profile
		[SetUp]
		public void SetUp ()
		{
			if (Win32EventLogEnabled)
				return;

			// determine temp directory for eventlog store
			_eventLogStore = Path.Combine (Path.GetTempPath (),
				Guid.NewGuid ().ToString ());

			// save original eventlog implementation type (if set)
			_originalEventLogImpl = Environment.GetEnvironmentVariable (
				EVENTLOG_TYPE_VAR);

			// use local file implementation
			Environment.SetEnvironmentVariable (EVENTLOG_TYPE_VAR, "local:"
				+ _eventLogStore);
		}

		[TearDown]
		public void TearDown ()
		{
			if (Win32EventLogEnabled)
				return;

			// restore original eventlog implementation type
			Environment.SetEnvironmentVariable (EVENTLOG_TYPE_VAR, 
				_originalEventLogImpl);

			// delete temp directory for eventlog store
			if (Directory.Exists (_eventLogStore))
				Directory.Delete (_eventLogStore, true);
		}
#endif

		[Test]
		public void Clear ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.SourceExists ("monoothersource", "."))
				Assert.Ignore ("Event log source 'monoothersource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp", ".");
			EventLog.CreateEventSource ("monoothersource", "monologtemp", ".");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					// MSBUG: Assert.AreEqual (0, eventLog.Entries.Count, "#A1");
					eventLog.Clear ();
					Assert.AreEqual (0, eventLog.Entries.Count, "#A2");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource", "."), "#A3");
					Assert.IsTrue (EventLog.SourceExists ("monoothersource", "."), "#A4");
					Assert.IsTrue (EventLog.Exists ("monologtemp", "."), "#A5");
					Assert.IsFalse (EventLog.Exists ("monotempsource", "."), "#A6");
					Assert.IsFalse (EventLog.Exists ("monoothersource", "."), "#A7");

					EventLog.WriteEntry ("monotempsource", "Clear1");

					Assert.AreEqual (1, eventLog.Entries.Count, "#B1");
					eventLog.Clear ();
					Assert.AreEqual (0, eventLog.Entries.Count, "#B2");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource", "."), "#B3");
					Assert.IsTrue (EventLog.SourceExists ("monoothersource", "."), "#B4");
					Assert.IsTrue (EventLog.Exists ("monologtemp", "."), "#B5");
					Assert.IsFalse (EventLog.Exists ("monotempsource", "."), "#B6");
					Assert.IsFalse (EventLog.Exists ("monoothersource", "."), "#B7");

					EventLog.WriteEntry ("monotempsource", "Clear2");
					eventLog.Clear ();
					EventLog.WriteEntry ("monotempsource", "Clear3");
					EventLog.WriteEntry ("monoothersource", "Clear4");

					Assert.AreEqual (2, eventLog.Entries.Count, "#C1");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource", "."), "#C2");
					Assert.IsTrue (EventLog.SourceExists ("monoothersource", "."), "#C3");
					Assert.IsTrue (EventLog.Exists ("monologtemp", "."), "#C4");
					Assert.IsFalse (EventLog.Exists ("monotempsource", "."), "#C5");
					Assert.IsFalse (EventLog.Exists ("monoothersource", "."), "#C6");

					EventLogEntry entry = eventLog.Entries [0];
					Assert.IsNotNull (entry, "#D1");
					Assert.IsNotNull (entry.Category, "#D2");
					Assert.AreEqual ("(0)", entry.Category, "#D3");
					Assert.AreEqual (0, entry.CategoryNumber, "#D4");
					Assert.IsNotNull (entry.Data, "#D5");
					Assert.AreEqual (0, entry.Data.Length, "#D6");
					Assert.AreEqual (EventLogEntryType.Information, entry.EntryType, "#D7");
					Assert.AreEqual (0, entry.EventID, "#D8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#D9");
#endif
					Assert.IsNotNull (entry.MachineName, "#D10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#D11");
					Assert.IsNotNull (entry.ReplacementStrings, "#D12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#D13");
					Assert.AreEqual ("Clear3", entry.ReplacementStrings [0], "#D14");
					Assert.IsNotNull (entry.Source, "#D15");
					Assert.AreEqual ("monotempsource", entry.Source, "#D16");
					Assert.IsNull (entry.UserName, "#D17");

					entry = eventLog.Entries [1];
					Assert.IsNotNull (entry, "#E1");
					Assert.IsNotNull (entry.Category, "#E2");
					Assert.AreEqual ("(0)", entry.Category, "#E3");
					Assert.AreEqual (0, entry.CategoryNumber, "#E4");
					Assert.IsNotNull (entry.Data, "#E5");
					Assert.AreEqual (0, entry.Data.Length, "#E6");
					Assert.AreEqual (EventLogEntryType.Information, entry.EntryType, "#E7");
					Assert.AreEqual (0, entry.EventID, "#E8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#E9");
#endif
					Assert.IsNotNull (entry.MachineName, "#E10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#E11");
					Assert.IsNotNull (entry.ReplacementStrings, "#E12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#E13");
					Assert.AreEqual ("Clear4", entry.ReplacementStrings [0], "#E14");
					Assert.IsNotNull (entry.Source, "#E15");
					Assert.AreEqual ("monoothersource", entry.Source, "#E16");
					Assert.IsNull (entry.UserName, "#E17");

					eventLog.Clear ();
					Assert.AreEqual (0, eventLog.Entries.Count, "#F1");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource", "."), "#F2");
					Assert.IsTrue (EventLog.SourceExists ("monoothersource", "."), "#F3");
					Assert.IsTrue (EventLog.Exists ("monologtemp", "."), "#F4");
					Assert.IsFalse (EventLog.Exists ("monotempsource", "."), "#F5");
					Assert.IsFalse (EventLog.Exists ("monoothersource", "."), "#F6");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void Clear_Log_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			using (EventLog eventLog = new EventLog ("monologtemp", ".")) {
				try {
					eventLog.Clear ();
					Assert.Fail ("#1");
				} catch (InvalidOperationException ex) {
					// The event log 'monologtemp' on computer '.' does not exist
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
					Assert.IsNotNull (ex.Message, "#3");
					Assert.IsTrue (ex.Message.IndexOf ("'monologtemp'") != -1, "#4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#5");
					Assert.IsNull (ex.InnerException, "#6");
				}
				Assert.IsFalse (EventLog.Exists ("monologtemp", "."), "#7");
			}
		}

		[Test]
		public void Clear_Log_Empty ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.SourceExists ("monoothersource", "."))
				Assert.Ignore ("Event log source 'monoothersource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp", ".");
			try {
				using (EventLog eventLog = new EventLog (string.Empty, ".")) {
					EventLog.WriteEntry ("monotempsource", "Clear_Log_Empty");

					// both source & log are not set
					try {
						eventLog.Clear ();
						Assert.Fail ("#A1");
					} catch (ArgumentException ex) {
						// Log property value has not been specified.
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
						Assert.IsNotNull (ex.Message, "#A3");
						Assert.IsNull (ex.InnerException, "#A4");
						Assert.IsNull (ex.ParamName, "#A5");
					}
					Assert.AreEqual (string.Empty, eventLog.Log, "#A6");

					// set non-existing source
					eventLog.Source = "monoothersource";

					try {
						eventLog.Clear ();
						Assert.Fail ("#B1");
					} catch (ArgumentException ex) {
						// Log property value has not been specified.
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
						Assert.IsNotNull (ex.Message, "#B3");
						Assert.IsNull (ex.InnerException, "#B4");
						Assert.IsNull (ex.ParamName, "#B5");
					}
					Assert.AreEqual (string.Empty, eventLog.Log, "#B6");

					// set existing source
					eventLog.Source = "monotempsource";

					Assert.IsTrue (eventLog.Entries.Count > 0, "#C1");
					eventLog.Clear ();
					Assert.AreEqual ("monologtemp", eventLog.Log, "#C2");
					Assert.AreEqual (0, eventLog.Entries.Count, "#C3");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void Clear_Source_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.SourceExists ("monoothersource", "."))
				Assert.Ignore ("Event log source 'monoothersource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp", ".");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monoothersource")) {
					EventLog.WriteEntry ("monotempsource", "Clear_Source_DoesNotExist");

					Assert.IsTrue (eventLog.Entries.Count > 0, "#1");
					eventLog.Clear ();
					Assert.AreEqual (0, eventLog.Entries.Count, "#2");
					Assert.IsFalse (EventLog.SourceExists ("monoothersource", "."), "#3");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource", "."), "#4");
					Assert.IsTrue (EventLog.Exists ("monologtemp", "."), "#5");
					Assert.IsFalse (EventLog.Exists ("monotempsource", "."), "#6");
					Assert.IsFalse (EventLog.Exists ("monoothersource", "."), "#7");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void Clear_Source_Empty ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp", ".");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", string.Empty)) {
					EventLog.WriteEntry ("monotempsource", "Clear_Source_Empty");

					Assert.IsTrue (eventLog.Entries.Count > 0, "#1");
					eventLog.Clear ();
					Assert.AreEqual (0, eventLog.Entries.Count, "#2");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource", "."), "#3");
					Assert.IsTrue (EventLog.Exists ("monologtemp", "."), "#4");
					Assert.IsFalse (EventLog.Exists ("monotempsource", "."), "#5");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void Clear_Source_Null ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp", ".");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", null)) {
					EventLog.WriteEntry ("monotempsource", "Clear_Source_Null");

					Assert.IsTrue (eventLog.Entries.Count > 0, "#1");
					eventLog.Clear ();
					Assert.AreEqual (0, eventLog.Entries.Count, "#2");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource", "."), "#3");
					Assert.IsTrue (EventLog.Exists ("monologtemp", "."), "#4");
					Assert.IsFalse (EventLog.Exists ("monotempsource", "."), "#5");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void Constructor1 ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

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
				Assert.IsNull (ex.ParamName, "#3e");
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
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp", ".");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp")) {
					Assert.IsFalse (eventLog.EnableRaisingEvents, "#B1");
					Assert.IsNotNull (eventLog.Entries, "#B2");
					// MSBUG: Assert.AreEqual (0, eventLog.Entries.Count, "#B3");
					Assert.IsNotNull (eventLog.Log, "#B4");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#B5");
					Assert.AreEqual ("monologtemp", eventLog.LogDisplayName, "#B6");
					Assert.IsNotNull (eventLog.MachineName, "#B7");
					Assert.AreEqual (".", eventLog.MachineName, "#B8");
					Assert.IsNotNull (eventLog.Source, "#B9");
					Assert.AreEqual (string.Empty, eventLog.Source, "#B10");
				}
			} finally {
				EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void Constructor2_Log_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog eventLog = new EventLog ("monologtemp");
			Assert.IsFalse (eventLog.EnableRaisingEvents, "#B1");
			Assert.IsNotNull (eventLog.Entries, "#B2");
			try {
				eventLog.Entries.GetEnumerator ().MoveNext ();
				Assert.Fail ("#B3a");
			} catch (InvalidOperationException ex) {
				// The event log 'monologtemp' on computer '.' does not exist
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B3b");
				Assert.IsNotNull (ex.Message, "#B3c");
				Assert.IsTrue (ex.Message.IndexOf ("'monologtemp'") != -1, "#B3d");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#B3e");
				Assert.IsNull (ex.InnerException, "#B3f");
			}
			Assert.IsNotNull (eventLog.Log, "#B4");
			Assert.AreEqual ("monologtemp", eventLog.Log, "#B5");
			try {
				string displayName = eventLog.LogDisplayName;
				Assert.Fail ("#B6a: " + displayName);
			} catch (InvalidOperationException ex) {
				// Cannot find Log monologtemp on computer .
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B6b");
				Assert.IsNotNull (ex.Message, "#B6c");
				Assert.IsTrue (ex.Message.IndexOf ("monologtemp") != -1, "#B6d");
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
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

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
				Assert.IsNull (ex.ParamName, "#A3e");
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
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp", ".");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".")) {
					Assert.IsFalse (eventLog.EnableRaisingEvents, "#B1");
					Assert.IsNotNull (eventLog.Entries, "#B2");
					// MSBUG: Assert.AreEqual (0, eventLog.Entries.Count, "#B3");
					Assert.IsNotNull (eventLog.Log, "#B4");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#B5");
					Assert.AreEqual ("monologtemp", eventLog.LogDisplayName, "#B6");
					Assert.IsNotNull (eventLog.MachineName, "#B7");
					Assert.AreEqual (".", eventLog.MachineName, "#B8");
					Assert.IsNotNull (eventLog.Source, "#B9");
					Assert.AreEqual (string.Empty, eventLog.Source, "#B10");
				}
			} finally {
				EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void Constructor3_Log_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog eventLog = new EventLog ("monologtemp", ".");
			Assert.IsFalse (eventLog.EnableRaisingEvents, "#B1");
			Assert.IsNotNull (eventLog.Entries, "#B2");
			try {
				eventLog.Entries.GetEnumerator ().MoveNext ();
				Assert.Fail ("#B3a");
			} catch (InvalidOperationException ex) {
				// The event log 'monologtemp' on computer '.' does not exist
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B3b");
				Assert.IsNotNull (ex.Message, "#B3c");
				Assert.IsTrue (ex.Message.IndexOf ("'monologtemp'") != -1, "#B3d");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#B3e");
				Assert.IsNull (ex.InnerException, "#B3f");
			}
			Assert.IsNotNull (eventLog.Log, "#B4");
			Assert.AreEqual ("monologtemp", eventLog.Log, "#B5");
			try {
				string displayName = eventLog.LogDisplayName;
				Assert.Fail ("#B6a: " + displayName);
			} catch (InvalidOperationException ex) {
				// Cannot find Log monologtemp on computer .
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B6b");
				Assert.IsNotNull (ex.Message, "#B6c");
				Assert.IsTrue (ex.Message.IndexOf ("monologtemp") != -1, "#B6d");
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
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

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
				Assert.IsNull (ex.ParamName, "#A3e");
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
		public void Constructor3_MachineName_Empty ()
		{
			try {
				new EventLog ("monologtemp", string.Empty);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
#if NET_2_0
				// Invalid value '' for parameter 'machineName'
				Assert.IsTrue (ex.Message.IndexOf ("''") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'machineName'") != -1, "#A5");
#else
				// Invalid value  for parameter MachineName
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("MachineName") != -1, "#A5");
#endif
				Assert.IsNull (ex.InnerException, "#A6");
				Assert.IsNull (ex.ParamName, "#A7");
			}

			try {
				new EventLog ("monologtemp", " \t\n");
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
#if NET_2_0
				// Invalid value ' \t\n' for parameter 'machineName'
				Assert.IsTrue (ex.Message.IndexOf ("' \t\n'") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'machineName'") != -1, "#B5");
#else
				// Invalid value  \t\n for parameter MachineName
				Assert.IsTrue (ex.Message.IndexOf ("  \t\n ") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("MachineName") != -1, "#B5");
#endif
				Assert.IsNull (ex.InnerException, "#B6");
				Assert.IsNull (ex.ParamName, "#B7");
			}
		}

		[Test]
		public void Constructor3_MachineName_Null ()
		{
			try {
				new EventLog ("monologtemp", null);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Invalid value '' for parameter 'machineName'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
#if NET_2_0
				Assert.IsTrue (ex.Message.IndexOf ("''") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'machineName'") != -1, "#A5");
#else
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("MachineName") != -1, "#A5");
#endif
				Assert.IsNull (ex.InnerException, "#A6");
				Assert.IsNull (ex.ParamName, "#A7");
			}
		}

		[Test]
		public void Constructor4 ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp", ".");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					Assert.IsFalse (eventLog.EnableRaisingEvents, "#A1");
					Assert.IsNotNull (eventLog.Entries, "#A2");
					// MSBUG: Assert.AreEqual (0, eventLog.Entries.Count, "#A3");
					Assert.IsNotNull (eventLog.Log, "#A4");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A5");
					Assert.AreEqual ("monologtemp", eventLog.LogDisplayName, "#A6");
					Assert.IsNotNull (eventLog.MachineName, "#A7");
					Assert.AreEqual (".", eventLog.MachineName, "#A8");
					Assert.IsNotNull (eventLog.Source, "#A9");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A10");
				}

				using (EventLog eventLog = new EventLog ("monologtemp", ".", "whatever")) {
					Assert.AreEqual ("monologtemp", eventLog.Log, "#B1");
					Assert.AreEqual ("monologtemp", eventLog.LogDisplayName, "#B2");
				}
			} finally {
				EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void Constructor4_Log_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource");
			Assert.IsFalse (eventLog.EnableRaisingEvents, "#B1");
			Assert.IsNotNull (eventLog.Entries, "#B2");
			try {
				eventLog.Entries.GetEnumerator ().MoveNext ();
				Assert.Fail ("#B3a");
			} catch (InvalidOperationException ex) {
				// The event log 'monologtemp' on computer '.' does not exist
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B3b");
				Assert.IsNotNull (ex.Message, "#B3c");
				Assert.IsTrue (ex.Message.IndexOf ("'monologtemp'") != -1, "#B3d");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#B3e");
				Assert.IsNull (ex.InnerException, "#B3f");
			}
			Assert.IsNotNull (eventLog.Log, "#B4");
			Assert.AreEqual ("monologtemp", eventLog.Log, "#B5");
			try {
				string displayName = eventLog.LogDisplayName;
				Assert.Fail ("#B6a: " + displayName);
			} catch (InvalidOperationException ex) {
				// Cannot find Log monologtemp on computer .
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B6b");
				Assert.IsNotNull (ex.Message, "#B6c");
				Assert.IsTrue (ex.Message.IndexOf ("monologtemp") != -1, "#B6d");
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
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

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
					Assert.IsNull (ex.ParamName, "#A3e");
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
		public void Constructor4_MachineName_Empty ()
		{
			try {
				new EventLog ("monologtemp", string.Empty, "monotempsource");
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
#if NET_2_0
				// Invalid value '' for parameter 'machineName'
				Assert.IsTrue (ex.Message.IndexOf ("''") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'machineName'") != -1, "#A5");
#else
				// Invalid value  for parameter MachineName
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("MachineName") != -1, "#A5");
#endif
				Assert.IsNull (ex.InnerException, "#A6");
				Assert.IsNull (ex.ParamName, "#A7");
			}

			try {
				new EventLog ("monologtemp", " \t\n", "monotempsource");
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
#if NET_2_0
				// Invalid value ' \t\n' for parameter 'machineName'
				Assert.IsTrue (ex.Message.IndexOf ("' \t\n'") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'machineName'") != -1, "#B5");
#else
				// Invalid value  \t\n for parameter MachineName
				Assert.IsTrue (ex.Message.IndexOf ("  \t\n ") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("MachineName") != -1, "#B5");
#endif
				Assert.IsNull (ex.InnerException, "#B6");
				Assert.IsNull (ex.ParamName, "#B7");
			}
		}

		[Test]
		public void Constructor4_MachineName_Null ()
		{
			try {
				new EventLog ("monologtemp", null, "monotempsource");
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Invalid value '' for parameter 'machineName'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
#if NET_2_0
				Assert.IsTrue (ex.Message.IndexOf ("''") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'machineName'") != -1, "#A5");
#else
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("MachineName") != -1, "#A5");
#endif
				Assert.IsNull (ex.InnerException, "#A6");
				Assert.IsNull (ex.ParamName, "#A7");
			}
		}

		[Test]
		public void Constructor4_Source_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monoothersource", "."))
				Assert.Ignore ("Event log source 'monoothersource' should not exist.");

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monoothersource", "monologtemp", ".");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					Assert.IsFalse (eventLog.EnableRaisingEvents, "#A1");
					Assert.IsNotNull (eventLog.Entries, "#A2");
					// MSBUG: Assert.AreEqual (0, eventLog.Entries.Count, "#A3");
					Assert.IsNotNull (eventLog.Log, "#A4");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A5");
					Assert.AreEqual ("monologtemp", eventLog.LogDisplayName, "#A6");
					Assert.IsNotNull (eventLog.MachineName, "#A7");
					Assert.AreEqual (".", eventLog.MachineName, "#A8");
					Assert.IsNotNull (eventLog.Source, "#A9");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A10");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A11");
					Assert.IsTrue (EventLog.SourceExists ("monoothersource"), "#A12");
					Assert.IsFalse (EventLog.SourceExists ("monotempsource"), "#A13");
					Assert.IsFalse (EventLog.Exists ("monotempsource", "."), "#A14");
					Assert.IsFalse (EventLog.Exists ("monoothersource", "."), "#A15");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void Constructor4_Source_Empty ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp", ".");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", string.Empty)) {
					Assert.IsFalse (eventLog.EnableRaisingEvents, "#A1");
					Assert.IsNotNull (eventLog.Entries, "#A2");
					// MSBUG: Assert.AreEqual (0, eventLog.Entries.Count, "#A3");
					Assert.IsNotNull (eventLog.Log, "#A4");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A5");
					Assert.AreEqual ("monologtemp", eventLog.LogDisplayName, "#A6");
					Assert.IsNotNull (eventLog.MachineName, "#A7");
					Assert.AreEqual (".", eventLog.MachineName, "#A8");
					Assert.IsNotNull (eventLog.Source, "#A9");
					Assert.AreEqual (string.Empty, eventLog.Source, "#A10");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A11");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A12");
					Assert.IsFalse (EventLog.Exists ("monotempsource", "."), "#A13");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void Constructor4_Source_Null ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp", ".");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", null)) {
					Assert.IsFalse (eventLog.EnableRaisingEvents, "#A1");
					Assert.IsNotNull (eventLog.Entries, "#A2");
					// MSBUG: Assert.AreEqual (0, eventLog.Entries.Count, "#A3");
					Assert.IsNotNull (eventLog.Log, "#A4");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A5");
					Assert.AreEqual ("monologtemp", eventLog.LogDisplayName, "#A6");
					Assert.IsNotNull (eventLog.MachineName, "#A7");
					Assert.AreEqual (".", eventLog.MachineName, "#A8");
					Assert.IsNull (eventLog.Source, "#A9");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A10");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A11");
					Assert.IsFalse (EventLog.Exists ("monotempsource", "."), "#A12");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void CreateEventSource1 ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.SourceExists ("monologtemp", "."))
				Assert.Ignore ("Event log source 'monologtemp' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			if (EventLog.Exists ("monologother", "."))
				Assert.Ignore ("Event log 'monologother' should not exist.");

			try {
				EventLog.CreateEventSource ("monotempsource", "monologtemp");
				Assert.IsTrue (EventLog.Exists ("monologtemp", "."), "#A1");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource", "."), "#A2");
				Assert.IsTrue (EventLog.SourceExists ("monologtemp", "."), "#A3");
				Assert.IsFalse (EventLog.Exists ("monotempsource", "."), "#A4");

				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					Assert.IsFalse (eventLog.EnableRaisingEvents, "#B1");
					Assert.IsNotNull (eventLog.Entries, "#B2");
					// MSBUG: Assert.AreEqual (0, eventLog.Entries.Count, "#B3");
					Assert.IsNotNull (eventLog.Log, "#B4");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#B5");
					Assert.IsNotNull (eventLog.LogDisplayName, "#B6");
					Assert.AreEqual ("monologtemp", eventLog.LogDisplayName, "#B7");
					Assert.IsNotNull (eventLog.MachineName, "#B8");
					Assert.AreEqual (".", eventLog.MachineName, "#B9");
					Assert.IsNotNull (eventLog.Source, "#B10");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#B11");
				}

				try {
					EventLog.CreateEventSource ("monologtemp", "monologother");
					Assert.Fail ("#C1");
				} catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
					Assert.IsNotNull (ex.Message, "#C3");
					Assert.IsTrue (ex.Message.IndexOf ("monologtemp") != -1, "#C4");
					Assert.IsTrue (ex.Message.IndexOf ("monologother") == -1, "#C5");
					Assert.IsNull (ex.InnerException, "#C6");
					Assert.IsNull (ex.ParamName, "#C7");
				}

				try {
					EventLog.CreateEventSource ("monotempsource", "monologother");
					Assert.Fail ("#D1");
				} catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
					Assert.IsNotNull (ex.Message, "#D3");
					Assert.IsTrue (ex.Message.IndexOf ("monotempsource") != -1, "#D4");
					Assert.IsTrue (ex.Message.IndexOf ("monologother") == -1, "#D5");
					Assert.IsNull (ex.InnerException, "#D6");
					Assert.IsNull (ex.ParamName, "#D7");
				}

				try {
					EventLog.CreateEventSource ("MonoTempSource", "monologother");
					Assert.Fail ("#E1");
				} catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#E2");
					Assert.IsNotNull (ex.Message, "#E3");
					Assert.IsTrue (ex.Message.IndexOf ("MonoTempSource") != -1, "#E4");
					Assert.IsTrue (ex.Message.IndexOf ("monologother") == -1, "#E5");
					Assert.IsNull (ex.InnerException, "#E6");
					Assert.IsNull (ex.ParamName, "#E7");
				}
			} finally {
				if (EventLog.Exists ("monologtemp", "."))
					EventLog.Delete ("monologtemp", ".");

				if (EventLog.Exists ("monologother", "."))
					EventLog.Delete ("monologother", ".");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Must specify value for source
		public void CreateEventSource1_Source_Empty ()
		{
			EventLog.CreateEventSource (string.Empty, "monologtemp");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Must specify value for source
		public void CreateEventSource1_Source_Null ()
		{
			EventLog.CreateEventSource (null, "monologtemp");
		}

		[Test]
		public void CreateEventSource1_Log_Empty ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			bool logExists = EventLog.Exists ("Application", ".");
			try {
				EventLog.CreateEventSource ("monotempsource", string.Empty);
				string logName = EventLog.LogNameFromSourceName ("monotempsource", ".");
				Assert.IsNotNull (logName, "#1");
				Assert.AreEqual ("Application", logName, "#2");
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
		public void CreateEventSource1_Log_ExistsAsSource ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.SourceExists ("monologtemp", "."))
				Assert.Ignore ("Event log source 'monologtemp' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			if (EventLog.Exists ("monologother", "."))
				Assert.Ignore ("Event log 'monologother' should not exist.");

			try {
				EventLog.CreateEventSource ("monologtemp", "monologother", ".");
				Assert.IsTrue (EventLog.Exists ("monologother", "."), "#A1");
				Assert.IsFalse (EventLog.Exists ("monologtemp", "."), "#A2");
				Assert.IsTrue (EventLog.SourceExists ("monologtemp", "."), "#A3");
				Assert.IsTrue (EventLog.SourceExists ("monologother", "."), "#A4");

				try {
					EventLog.CreateEventSource ("monotempsource", "monologtemp", ".");
					Assert.Fail ("#B1");
				} catch (ArgumentException ex) {
					// Log monologtemp has already been registered as a source
					// on the local computer
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
					Assert.IsNotNull (ex.Message, "#B3");
					Assert.IsTrue (ex.Message.IndexOf ("monologtemp") != -1, "#B4");
					Assert.IsNull (ex.InnerException, "#B5");
					Assert.IsNull (ex.ParamName, "#B6");
				}
			} finally {
				if (EventLog.Exists ("monologtemp", "."))
					EventLog.Delete ("monologtemp", ".");

				if (EventLog.Exists ("monologother", "."))
					EventLog.Delete ("monologother", ".");
			}
		}

		[Test]
		public void CreateEventSource1_Log_InvalidCustomerLog ()
		{
			if (EventLogImplType != NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("AppEvent", "."))
				Assert.Ignore ("Event log 'AppEvent' should not exist.");

			try {
				EventLog.CreateEventSource ("monotempsource", "AppEvent");
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// The log name: 'AppEvent' is invalid for customer log creation
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsTrue (ex.Message.IndexOf ("'AppEvent'") != -1, "#A4");
				Assert.IsNull (ex.InnerException, "#A5");
				Assert.IsNull (ex.ParamName, "#A6");
			}

			try {
				EventLog.CreateEventSource ("monotempsource", "appevent");
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// The log name: 'appevent' is invalid for customer log creation
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf ("'appevent'") != -1, "#B4");
				Assert.IsNull (ex.InnerException, "#B5");
				Assert.IsNull (ex.ParamName, "#B6");
			}

			if (EventLog.Exists ("SysEvent", "."))
				Assert.Ignore ("Event log 'SysEvent' should not exist.");

			try {
				EventLog.CreateEventSource ("monotempsource", "SysEvent");
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// The log name: 'SysEvent' is invalid for customer log creation
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.Message, "#C3");
				Assert.IsTrue (ex.Message.IndexOf ("'SysEvent'") != -1, "#C4");
				Assert.IsNull (ex.InnerException, "#C5");
				Assert.IsNull (ex.ParamName, "#C6");
			}

			try {
				EventLog.CreateEventSource ("monotempsource", "sysevent");
				Assert.Fail ("#D1");
			} catch (ArgumentException ex) {
				// The log name: 'sysEvent' is invalid for customer log creation
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
				Assert.IsNotNull (ex.Message, "#D3");
				Assert.IsTrue (ex.Message.IndexOf ("'sysevent'") != -1, "#D4");
				Assert.IsNull (ex.InnerException, "#D5");
				Assert.IsNull (ex.ParamName, "#D6");
			}

			if (EventLog.Exists ("SecEvent", "."))
				Assert.Ignore ("Event log 'SecEvent' should not exist.");

			try {
				EventLog.CreateEventSource ("monotempsource", "SecEvent");
				Assert.Fail ("#E1");
			} catch (ArgumentException ex) {
				// The log name: 'SecEvent' is invalid for customer log creation
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#E2");
				Assert.IsNotNull (ex.Message, "#E3");
				Assert.IsTrue (ex.Message.IndexOf ("'SecEvent'") != -1, "#E4");
				Assert.IsNull (ex.InnerException, "#E5");
				Assert.IsNull (ex.ParamName, "#E6");
			}

			try {
				EventLog.CreateEventSource ("monotempsource", "secevent");
				Assert.Fail ("#F1");
			} catch (ArgumentException ex) {
				// The log name: 'secevent' is invalid for customer log creation
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#F2");
				Assert.IsNotNull (ex.Message, "#F3");
				Assert.IsTrue (ex.Message.IndexOf ("'secevent'") != -1, "#F4");
				Assert.IsNull (ex.InnerException, "#F5");
				Assert.IsNull (ex.ParamName, "#F6");
			}

			if (EventLog.Exists ("AppEventA", "."))
				Assert.Ignore ("Event log 'AppEventA' should not exist.");

			try {
				EventLog.CreateEventSource ("monotempsource", "AppEventA");
				Assert.Fail ("#G1");
			} catch (ArgumentException ex) {
				// The log name: 'AppEventA' is invalid for customer log creation
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#G2");
				Assert.IsNotNull (ex.Message, "#G3");
				Assert.IsTrue (ex.Message.IndexOf ("'AppEventA'") != -1, "#G4");
				Assert.IsNull (ex.InnerException, "#G5");
				Assert.IsNull (ex.ParamName, "#G6");
			}

			if (EventLog.Exists ("SysEventA", "."))
				Assert.Ignore ("Event log 'SysEventA' should not exist.");

			try {
				EventLog.CreateEventSource ("monotempsource", "SysEventA");
				Assert.Fail ("#H1");
			} catch (ArgumentException ex) {
				// The log name: 'SysEventA' is invalid for customer log creation
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#H2");
				Assert.IsNotNull (ex.Message, "#H3");
				Assert.IsTrue (ex.Message.IndexOf ("'SysEventA'") != -1, "#H4");
				Assert.IsNull (ex.InnerException, "#H5");
				Assert.IsNull (ex.ParamName, "#H6");
			}

			if (EventLog.Exists ("SecEventA", "."))
				Assert.Ignore ("Event log 'SecEventA' should not exist.");

			try {
				EventLog.CreateEventSource ("monotempsource", "SecEventA");
				Assert.Fail ("#I1");
			} catch (ArgumentException ex) {
				// The log name: 'SecEventA' is invalid for customer log creation
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#I2");
				Assert.IsNotNull (ex.Message, "#I3");
				Assert.IsTrue (ex.Message.IndexOf ("'SecEventA'") != -1, "#I4");
				Assert.IsNull (ex.InnerException, "#I5");
				Assert.IsNull (ex.ParamName, "#I6");
			}
		}

		[Test]
		public void CreateEventSource1_Log_NotUnique ()
		{
			if (EventLogImplType != NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.SourceExists ("monotestsource", "."))
				Assert.Ignore ("Event log source 'monotestsource' should not exist.");

			if (EventLog.SourceExists ("monoothersource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			if (EventLog.Exists ("monologtest", "."))
				Assert.Ignore ("Event log 'monologtest' should not exist.");

			if (EventLog.Exists ("monologother", "."))
				Assert.Ignore ("Event log 'monologother' should not exist.");

			// the 8th character of the log name differs
			try {
				EventLog.CreateEventSource ("monoothersource", "monologother");
				EventLog.CreateEventSource ("monotempsource", "monologtemp");
			} finally {
				if (EventLog.Exists ("monologother"))
					EventLog.Delete ("monologother");
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}

			// the first 8 characters match
			try {
				EventLog.CreateEventSource ("monotestsource", "monologtest");
				EventLog.CreateEventSource ("monotempsource", "monologtemp");
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Only the first eight characters of a custom log name are
				// significant, and there is already another log on the system
				// using the  first eight characters of the name given.
				// Name given: 'monologtemp', name of existing log: 'monologtest'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf ("'monologtemp'") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'monologtest'") != -1, "#B5");
				Assert.IsNull (ex.InnerException, "#B6");
				Assert.IsNull (ex.ParamName, "#B7");
			} finally {
				if (EventLog.Exists ("monologtest"))
					EventLog.Delete ("monologtest");
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void CreateEventSource1_Log_Null ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			bool logExists = EventLog.Exists ("Application", ".");
			try {
				EventLog.CreateEventSource ("monotempsource", null);
				string logName = EventLog.LogNameFromSourceName ("monotempsource", ".");
				Assert.IsNotNull (logName, "#1");
				Assert.AreEqual ("Application", logName, "#2");
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
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.SourceExists ("monoothersource", "."))
				Assert.Ignore ("Event log source 'monoothersource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			if (EventLog.Exists ("monologother", "."))
				Assert.Ignore ("Event log 'monologother' should not exist.");

			try {
				EventLog.CreateEventSource ("monoothersource", "monologother", ".");
				EventLog.CreateEventSource ("monotempsource", "monologtemp", ".");

				Assert.IsTrue (EventLog.Exists ("monologtemp", "."), "#A1");
				Assert.IsTrue (EventLog.Exists ("monologother", "."), "#A2");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource", "."), "#A3");
				Assert.IsTrue (EventLog.SourceExists ("monoothersource", "."), "#A4");
				Assert.IsFalse (EventLog.Exists ("monotempsource", "."), "#A5");
				Assert.IsFalse (EventLog.Exists ("monoothersource", "."), "#A6");
				EventLog.Delete ("monologtemp");
				Assert.IsFalse (EventLog.Exists ("monologtemp", "."), "#A7");
				Assert.IsTrue (EventLog.Exists ("monologother", "."), "#A8");
				Assert.IsFalse (EventLog.SourceExists ("monotempsource", "."), "#A9");
				Assert.IsTrue (EventLog.SourceExists ("monoothersource", "."), "#A10");
				Assert.IsFalse (EventLog.Exists ("monotempsource", "."), "#A11");
				Assert.IsFalse (EventLog.Exists ("monoothersource", "."), "#A12");

				EventLog.CreateEventSource ("monotempsource", "monologtemp", ".");

				Assert.IsTrue (EventLog.Exists ("monologtemp", "."), "#B1");
				Assert.IsTrue (EventLog.Exists ("monologother", "."), "#B2");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource", "."), "#B3");
				Assert.IsTrue (EventLog.SourceExists ("monoothersource", "."), "#B4");
				Assert.IsFalse (EventLog.Exists ("monotempsource", "."), "#B5");
				Assert.IsFalse (EventLog.Exists ("monoothersource", "."), "#B6");
				EventLog.Delete ("MonoLogTemp");
				Assert.IsFalse (EventLog.Exists ("monologtemp", "."), "#B7");
				Assert.IsTrue (EventLog.Exists ("monologother", "."), "#B8");
				Assert.IsFalse (EventLog.SourceExists ("monotempsource", "."), "#B9");
				Assert.IsTrue (EventLog.SourceExists ("monoothersource", "."), "#B10");
				Assert.IsFalse (EventLog.Exists ("monotempsource", "."), "#B11");
				Assert.IsFalse (EventLog.Exists ("monoothersource", "."), "#B12");
			} finally {
				if (EventLog.Exists ("monologother"))
					EventLog.Delete ("monologother");
			}
		}

		[Test]
		public void Delete1_Log_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			try {
				EventLog.Delete ("monologtemp");
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("monologtemp") != -1, "#4");
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
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.SourceExists ("monoothersource", "."))
				Assert.Ignore ("Event log source 'monoothersource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			if (EventLog.Exists ("monologother", "."))
				Assert.Ignore ("Event log 'monologother' should not exist.");

			try {
				EventLog.CreateEventSource ("monoothersource", "monologother", ".");
				EventLog.CreateEventSource ("monotempsource", "monologtemp", ".");

				Assert.IsTrue (EventLog.Exists ("monologtemp", "."), "#A1");
				Assert.IsTrue (EventLog.Exists ("monologother", "."), "#A2");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource", "."), "#A3");
				Assert.IsTrue (EventLog.SourceExists ("monoothersource", "."), "#A4");
				Assert.IsFalse (EventLog.Exists ("monotempsource", "."), "#A5");
				Assert.IsFalse (EventLog.Exists ("monoothersource", "."), "#A6");
				EventLog.Delete ("monologtemp", ".");
				Assert.IsFalse (EventLog.Exists ("monologtemp", "."), "#A7");
				Assert.IsTrue (EventLog.Exists ("monologother", "."), "#A8");
				Assert.IsFalse (EventLog.SourceExists ("monotempsource", "."), "#A9");
				Assert.IsTrue (EventLog.SourceExists ("monoothersource", "."), "#A10");
				Assert.IsFalse (EventLog.Exists ("monotempsource", "."), "#A11");
				Assert.IsFalse (EventLog.Exists ("monoothersource", "."), "#A12");

				EventLog.CreateEventSource ("monotempsource", "monologtemp", ".");

				Assert.IsTrue (EventLog.Exists ("monologtemp", "."), "#B1");
				Assert.IsTrue (EventLog.Exists ("monologother", "."), "#B2");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource", "."), "#B3");
				Assert.IsTrue (EventLog.SourceExists ("monoothersource", "."), "#B4");
				Assert.IsFalse (EventLog.Exists ("monotempsource", "."), "#B5");
				Assert.IsFalse (EventLog.Exists ("monoothersource", "."), "#B6");
				EventLog.Delete ("MonoLogTemp", ".");
				Assert.IsFalse (EventLog.Exists ("monologtemp", "."), "#B7");
				Assert.IsTrue (EventLog.Exists ("monologother", "."), "#B8");
				Assert.IsFalse (EventLog.SourceExists ("monotempsource", "."), "#B9");
				Assert.IsTrue (EventLog.SourceExists ("monoothersource", "."), "#B10");
				Assert.IsFalse (EventLog.Exists ("monotempsource", "."), "#B11");
				Assert.IsFalse (EventLog.Exists ("monoothersource", "."), "#B12");
			} finally {
				if (EventLog.Exists ("monologother"))
					EventLog.Delete ("monologother");
			}
		}

		[Test]
		public void Delete2_Log_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			try {
				EventLog.Delete ("monologtemp", ".");
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("monologtemp") != -1, "#4");
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
		public void Delete2_MachineName_Empty ()
		{
			try {
				EventLog.Delete ("monologtemp", string.Empty);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// // Invalid format for argument machineName
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsTrue (ex.Message.IndexOf ("machineName") != -1, "#A4");
				Assert.IsNull (ex.InnerException, "#A5");
				Assert.IsNull (ex.ParamName, "#A6");
			}

			try {
				EventLog.Delete ("monologtemp", " \t\n");
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// // Invalid format for argument machineName
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf ("machineName") != -1, "#B4");
				Assert.IsNull (ex.InnerException, "#B5");
				Assert.IsNull (ex.ParamName, "#B6");
			}
		}

		[Test]
		public void Delete2_MachineName_Null ()
		{
			try {
				EventLog.Delete ("monologtemp", null);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// // Invalid format for argument machineName
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("machineName") != -1, "#4");
				Assert.IsNull (ex.InnerException, "#5");
				Assert.IsNull (ex.ParamName, "#6");
			}
		}

		[Test]
		public void DeleteEventSource1 ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.SourceExists ("monoothersource", "."))
				Assert.Ignore ("Event log source 'monoothersource' should not exist.");

			bool monologtempExists = EventLog.Exists ("monologtemp", ".");
			try {
				EventLog.CreateEventSource ("monotempsource", "monologtemp", ".");
				EventLog.CreateEventSource ("monoothersource", "monologtemp", ".");

				Assert.IsTrue (EventLog.SourceExists ("monotempsource", "."), "#A1");
				Assert.IsTrue (EventLog.SourceExists ("monoothersource", "."), "#A2");
				EventLog.DeleteEventSource ("monotempsource");
				Assert.IsFalse (EventLog.SourceExists ("monotempsource", "."), "#A3");
				Assert.IsTrue (EventLog.SourceExists ("monoothersource", "."), "#A4");
				Assert.IsTrue (EventLog.Exists ("monologtemp", "."), "#A5");
				Assert.IsFalse (EventLog.Exists ("monotempsource", "."), "#A6");
				Assert.IsFalse (EventLog.Exists ("monoothersource", "."), "#A7");

				EventLog.CreateEventSource ("monotempsource", "monologtemp", ".");

				Assert.IsTrue (EventLog.SourceExists ("monotempsource", "."), "#B1");
				Assert.IsTrue (EventLog.SourceExists ("monoothersource", "."), "#A2");
				EventLog.DeleteEventSource ("MonoTempSource");
				Assert.IsFalse (EventLog.SourceExists ("monotempsource", "."), "#B3");
				Assert.IsTrue (EventLog.SourceExists ("monoothersource", "."), "#B4");
				Assert.IsTrue (EventLog.Exists ("monologtemp", "."), "#B5");
				Assert.IsFalse (EventLog.Exists ("monotempsource", "."), "#B6");
				Assert.IsFalse (EventLog.Exists ("monoothersource", "."), "#B7");
			} finally {
				if (!monologtempExists) {
					EventLog.Delete ("monologtemp");
				}
			}
		}

		[Test]
		public void DeleteEventSource1_Source_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

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
				Assert.IsNull (ex.ParamName, "#7");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // The source '' is not registered on machine '.', ...
		public void DeleteEventSource1_Source_Empty ()
		{
			if (EventLogImplType == NULL_IMPL)
				// allow test to pass with NULL implementation
				throw new ArgumentException ();

			EventLog.DeleteEventSource (string.Empty);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // The source '' is not registered on machine '.', ...
		public void DeleteEventSource1_Source_Null ()
		{
			if (EventLogImplType == NULL_IMPL)
				// allow test to pass with NULL implementation
				throw new ArgumentException ();

			EventLog.DeleteEventSource (null);
		}

		[Test]
		public void DeleteEventSource2 ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.SourceExists ("monoothersource", "."))
				Assert.Ignore ("Event log source 'monoothersource' should not exist.");

			bool monologtempExists = EventLog.Exists ("monologtemp", ".");
			try {
				EventLog.CreateEventSource ("monotempsource", "monologtemp", ".");
				EventLog.CreateEventSource ("monoothersource", "monologtemp", ".");

				Assert.IsTrue (EventLog.SourceExists ("monotempsource", "."), "#A1");
				Assert.IsTrue (EventLog.SourceExists ("monoothersource", "."), "#A2");
				EventLog.DeleteEventSource ("monotempsource");
				Assert.IsFalse (EventLog.SourceExists ("monotempsource", "."), "#A3");
				Assert.IsTrue (EventLog.SourceExists ("monoothersource", "."), "#A4");
				Assert.IsTrue (EventLog.Exists ("monologtemp", "."), "#A5");
				Assert.IsFalse (EventLog.Exists ("monotempsource", "."), "#A6");
				Assert.IsFalse (EventLog.Exists ("monoothersource", "."), "#A7");

				EventLog.CreateEventSource ("monotempsource", "monologtemp", ".");

				Assert.IsTrue (EventLog.SourceExists ("monotempsource", "."), "#B1");
				Assert.IsTrue (EventLog.SourceExists ("monoothersource", "."), "#A2");
				EventLog.DeleteEventSource ("MonoTempSource");
				Assert.IsFalse (EventLog.SourceExists ("monotempsource", "."), "#B3");
				Assert.IsTrue (EventLog.SourceExists ("monoothersource", "."), "#B4");
				Assert.IsTrue (EventLog.Exists ("monologtemp", "."), "#B5");
				Assert.IsFalse (EventLog.Exists ("monotempsource", "."), "#B6");
				Assert.IsFalse (EventLog.Exists ("monoothersource", "."), "#B7");
			} finally {
				if (!monologtempExists) {
					EventLog.Delete ("monologtemp");
				}
			}
		}

		[Test]
		public void DeleteEventSource2_MachineName_Empty ()
		{
			try {
				EventLog.DeleteEventSource ("monotempsource", string.Empty);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
#if NET_2_0
				// Invalid value '' for parameter 'machineName'
				Assert.IsTrue (ex.Message.IndexOf ("''") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'machineName'") != -1, "#A5");
#else
				// Invalid value  for parameter machineName
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("machineName") != -1, "#A5");
#endif
				Assert.IsNull (ex.InnerException, "#A6");
				Assert.IsNull (ex.ParamName, "#A7");
			}

			try {
				EventLog.DeleteEventSource ("monotempsource", " \t\n");
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
#if NET_2_0
				// Invalid value ' \t\n' for parameter 'machineName'
				Assert.IsTrue (ex.Message.IndexOf ("' \t\n'") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'machineName'") != -1, "#B5");
#else
				// Invalid value  \t\n for parameter machineName
				Assert.IsTrue (ex.Message.IndexOf ("  \t\n ") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("machineName") != -1, "#B5");
#endif
				Assert.IsNull (ex.InnerException, "#B6");
				Assert.IsNull (ex.ParamName, "#B7");
			}
		}

		[Test]
		public void DeleteEventSource2_MachineName_Null ()
		{
			try {
				EventLog.DeleteEventSource ("monotempsource", null);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Invalid value '' for parameter 'machineName'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
#if NET_2_0
				Assert.IsTrue (ex.Message.IndexOf ("''") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'machineName'") != -1, "#A5");
#else
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("machineName") != -1, "#A5");
#endif
				Assert.IsNull (ex.InnerException, "#A6");
				Assert.IsNull (ex.ParamName, "#A7");
			}
		}

		[Test]
		public void DeleteEventSource2_Source_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

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
				Assert.IsNull (ex.ParamName, "#7");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // The source '' is not registered on machine '.', ...
		public void DeleteEventSource2_Source_Empty ()
		{
			if (EventLogImplType == NULL_IMPL)
				// allow test to pass with NULL implementation
				throw new ArgumentException ();

			EventLog.DeleteEventSource (string.Empty, ".");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // The source '' is not registered on machine '.', ...
		public void DeleteEventSource2_Source_Null ()
		{
			if (EventLogImplType == NULL_IMPL)
				// allow test to pass with NULL implementation
				throw new ArgumentException ();

			EventLog.DeleteEventSource (null, ".");
		}

		[Test]
		public void Entries ()
		{
			EventLogEntry entry = null;
			object current = null;

			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp", ".");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", string.Empty)) {
					Assert.IsNotNull (eventLog.Entries, "#A1");
					Assert.AreEqual (0, eventLog.Entries.Count, "#A2");

					IEnumerator enumerator = eventLog.Entries.GetEnumerator ();
					Assert.IsNotNull (enumerator, "#B");

					try {
						current = enumerator.Current;
						Assert.Fail ("#C1: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
						Assert.IsNotNull (ex.Message, "#C3");
						Assert.IsNull (ex.InnerException, "#C4");
					}

					Assert.IsFalse (enumerator.MoveNext (), "#D");

					try {
						current = enumerator.Current;
						Assert.Fail ("#E1: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#E2");
						Assert.IsNotNull (ex.Message, "#E3");
						Assert.IsNull (ex.InnerException, "#E4");
					}

					EventLogEntry [] entries = new EventLogEntry [0];
					eventLog.Entries.CopyTo (entries, 0);

					EventLog.WriteEntry ("monotempsource", "Entries1");

#if NET_2_0
					try {
						current = enumerator.Current;
						Assert.Fail ("#G1: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#G2");
						Assert.IsNotNull (ex.Message, "#G3");
						Assert.IsNull (ex.InnerException, "#G4");
					}
#else
					entry = (EventLogEntry) enumerator.Current;
					Assert.IsNotNull (entry, "#G1");
					Assert.IsNotNull (entry.Source, "#G2");
					Assert.AreEqual ("monotempsource", entry.Source, "#G3");
					Assert.IsNotNull (entry.ReplacementStrings, "#G4");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#G5");
					Assert.AreEqual ("Entries1", entry.ReplacementStrings [0], "#G6");
#endif

					Assert.IsFalse (enumerator.MoveNext (), "#H1");
					Assert.AreEqual (1, eventLog.Entries.Count, "#H2");
					enumerator.Reset ();

					entries = new EventLogEntry [0];
					try {
						eventLog.Entries.CopyTo (entries, 0);
						Assert.Fail ("#I1");
					} catch (ArgumentException ex) {
						// Destination array was not long enough. Check destIndex
						// and length, and the array's lower bounds
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#I2");
						Assert.IsNotNull (ex.Message, "#I3");
						Assert.IsNull (ex.InnerException, "#I4");
#if NET_2_0
						Assert.AreEqual ("", ex.ParamName, "#I5");
#else
						Assert.IsNull (ex.ParamName, "#I5");
#endif
					}

					entries = new EventLogEntry [1];
					eventLog.Entries.CopyTo (entries, 0);

					entry = entries [0];
					Assert.IsNotNull (entry, "#J1");
					Assert.IsNotNull (entry.Source, "#J2");
					Assert.AreEqual ("monotempsource", entry.Source, "#J3");
					Assert.IsNotNull (entry.ReplacementStrings, "#J4");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#J5");
					Assert.AreEqual ("Entries1", entry.ReplacementStrings [0], "#J6");

					Assert.IsTrue (enumerator.MoveNext (), "#K1");
					Assert.IsNotNull (enumerator.Current, "#K2");
					Assert.IsFalse (enumerator.MoveNext (), "#K3");
					enumerator.Reset ();
					Assert.IsTrue (enumerator.MoveNext (), "#K4");
					Assert.IsNotNull (enumerator.Current, "#K5");
					Assert.IsFalse (enumerator.MoveNext (), "#K6");

					EventLog.WriteEntry ("monotempsource", "Entries2");
					EventLog.WriteEntry ("monotempsource", "Entries3");

					Assert.IsTrue (enumerator.MoveNext (), "#L");

					entry = (EventLogEntry) enumerator.Current;
					Assert.IsNotNull (entry, "#M1");
					Assert.IsNotNull (entry.Source, "#M2");
					Assert.AreEqual ("monotempsource", entry.Source, "#M3");
					Assert.IsNotNull (entry.ReplacementStrings, "#M4");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#M5");
					Assert.AreEqual ("Entries3", entry.ReplacementStrings [0], "#M6");

					enumerator.Reset ();
#if NET_2_0
					Assert.IsNotNull (enumerator.Current, "#N1");
#else
					try {
						current = enumerator.Current;
						Assert.Fail ("#N1a: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#N1b");
						Assert.IsNotNull (ex.Message, "#N1c");
						Assert.IsNull (ex.InnerException, "#N1d");
					}
#endif
					Assert.IsTrue (enumerator.MoveNext (), "#N2");
					Assert.IsNotNull (enumerator.Current, "#N3");
					Assert.IsTrue (enumerator.MoveNext (), "#N4");
					Assert.IsNotNull (enumerator.Current, "#N5");
					Assert.IsTrue (enumerator.MoveNext (), "#N6");
					Assert.IsNotNull (enumerator.Current, "#N7");
					Assert.IsFalse (enumerator.MoveNext (), "#N8");
					enumerator.Reset ();

					try {
						current = enumerator.Current;
						Assert.Fail ("#O1: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#O2");
						Assert.IsNotNull (ex.Message, "#O3");
						Assert.IsNull (ex.InnerException, "#O4");
					}

					Assert.IsTrue (enumerator.MoveNext (), "#P1");
					Assert.IsNotNull (enumerator.Current, "#P2");
					eventLog.Clear ();
#if NET_2_0
					Assert.IsNotNull (enumerator.Current, "#P3");
#else
					try {
						current = enumerator.Current;
						Assert.Fail ("#P3a: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#P3b");
						Assert.IsNotNull (ex.Message, "#P3c");
						Assert.IsNull (ex.InnerException, "#P3d");
					}
#endif
					Assert.IsFalse (enumerator.MoveNext (), "#P4");
					Assert.AreEqual (0, eventLog.Entries.Count, "#P5");

					try {
						current = enumerator.Current;
						Assert.Fail ("#Q1: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#Q2");
						Assert.IsNotNull (ex.Message, "#Q3");
						Assert.IsNull (ex.InnerException, "#Q4");
					}

					Assert.IsFalse (enumerator.MoveNext (), "#R1");
					enumerator.Reset ();
					Assert.IsFalse (enumerator.MoveNext (), "#R2");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void Entries_Log_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			using (EventLog eventLog = new EventLog ("monologtemp", ".")) {
				Assert.IsNotNull (eventLog.Entries, "#A1");

				try {
					Assert.Fail ("#B1: " + eventLog.Entries.Count);
				} catch (InvalidOperationException ex) {
					// The event log 'monologtemp' on computer '.' does not exist
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
					Assert.IsNotNull (ex.Message, "#B3");
					Assert.IsTrue (ex.Message.IndexOf ("'monologtemp'") != -1, "#B4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#B5");
					Assert.IsNull (ex.InnerException, "#B6");
				}
				Assert.IsFalse (EventLog.Exists ("monologtemp", "."), "#B7");

				IEnumerator enumerator = eventLog.Entries.GetEnumerator ();
				Assert.IsNotNull (enumerator, "#C");

				try {
					object current = enumerator.Current;
					Assert.Fail ("#D1: " + current);
				} catch (InvalidOperationException ex) {
					// No current EventLog entry available, cursor is located
					// before the first or after the last element of the
					// enumeration.
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
					Assert.IsNotNull (ex.Message, "#D3");
					Assert.IsNull (ex.InnerException, "#D4");
				}

				try {
					enumerator.MoveNext ();
					Assert.Fail ("#E1");
				} catch (InvalidOperationException ex) {
					// The event log 'monologtemp' on computer '.' does not exist
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#E2");
					Assert.IsNotNull (ex.Message, "#E3");
					Assert.IsTrue (ex.Message.IndexOf ("'monologtemp'") != -1, "#E4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#E5");
					Assert.IsNull (ex.InnerException, "#E6");
				}

				try {
					EventLogEntry [] entries = new EventLogEntry [0];
					eventLog.Entries.CopyTo (entries, 0);
					Assert.Fail ("#F1");
				} catch (InvalidOperationException ex) {
					// The event log 'monologtemp' on computer '.' does not exist
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#F2");
					Assert.IsNotNull (ex.Message, "#F3");
					Assert.IsTrue (ex.Message.IndexOf ("'monologtemp'") != -1, "#F4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#F5");
					Assert.IsNull (ex.InnerException, "#F6");
				}

				enumerator.Reset ();
			}
		}

		[Test]
		public void Entries_Log_Empty ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.SourceExists ("monoothersource", "."))
				Assert.Ignore ("Event log source 'monoothersource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp", ".");
			try {
				using (EventLog eventLog = new EventLog (string.Empty, ".")) {
					Assert.IsNotNull (eventLog.Entries, "#A1");

					try {
						Assert.Fail ("#B1: " + eventLog.Entries.Count);
					} catch (ArgumentException ex) {
						// Log property value has not been specified
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
						Assert.IsNotNull (ex.Message, "#B3");
						Assert.IsNull (ex.InnerException, "#B4");
						Assert.IsNull (ex.ParamName, "#B5");
					}

					IEnumerator enumerator = eventLog.Entries.GetEnumerator ();
					Assert.IsNotNull (enumerator, "#C");

					try {
						object current = enumerator.Current;
						Assert.Fail ("#D1: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
						Assert.IsNotNull (ex.Message, "#D3");
						Assert.IsNull (ex.InnerException, "#D4");
					}

					try {
						enumerator.MoveNext ();
						Assert.Fail ("#E1");
					} catch (ArgumentException ex) {
						// Log property value has not been specified
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#E2");
						Assert.IsNotNull (ex.Message, "#E3");
						Assert.IsNull (ex.InnerException, "#E4");
						Assert.IsNull (ex.ParamName, "#E5");
					}

					try {
						EventLogEntry [] entries = new EventLogEntry [0];
						eventLog.Entries.CopyTo (entries, 0);
						Assert.Fail ("#F1");
					} catch (ArgumentException ex) {
						// Log property value has not been specified
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#F2");
						Assert.IsNotNull (ex.Message, "#F3");
						Assert.IsNull (ex.InnerException, "#F4");
						Assert.IsNull (ex.ParamName, "#F5");
					}

					enumerator.Reset ();

					// set non-existing source
					eventLog.Source = "monoothersource";

					try {
						Assert.Fail ("#G1: " + eventLog.Entries.Count);
					} catch (ArgumentException ex) {
						// Log property value has not been specified
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#G2");
						Assert.IsNotNull (ex.Message, "#G3");
						Assert.IsNull (ex.InnerException, "#G4");
						Assert.IsNull (ex.ParamName, "#G5");
					}

					enumerator = eventLog.Entries.GetEnumerator ();
					Assert.IsNotNull (enumerator, "#H");

					try {
						object current = enumerator.Current;
						Assert.Fail ("#I1: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#I2");
						Assert.IsNotNull (ex.Message, "#I3");
						Assert.IsNull (ex.InnerException, "#I4");
					}

					try {
						enumerator.MoveNext ();
						Assert.Fail ("#J1");
					} catch (ArgumentException ex) {
						// Log property value has not been specified
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#J2");
						Assert.IsNotNull (ex.Message, "#J3");
						Assert.IsNull (ex.InnerException, "#J4");
						Assert.IsNull (ex.ParamName, "#J5");
					}

					try {
						EventLogEntry [] entries = new EventLogEntry [0];
						eventLog.Entries.CopyTo (entries, 0);
						Assert.Fail ("#K1");
					} catch (ArgumentException ex) {
						// Log property value has not been specified
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#K2");
						Assert.IsNotNull (ex.Message, "#K3");
						Assert.IsNull (ex.InnerException, "#K4");
						Assert.IsNull (ex.ParamName, "#K5");
					}

					enumerator.Reset ();

					// set existing source
					eventLog.Source = "monotempsource";

					Assert.AreEqual (0, eventLog.Entries.Count, "#L1");
					enumerator = eventLog.Entries.GetEnumerator ();
					Assert.IsNotNull (enumerator, "#L2");

					try {
						object current = enumerator.Current;
						Assert.Fail ("#M1: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#M2");
						Assert.IsNotNull (ex.Message, "#M3");
						Assert.IsNull (ex.InnerException, "#M4");
					}

					Assert.IsFalse (enumerator.MoveNext ());

					try {
						object current = enumerator.Current;
						Assert.Fail ("#N1: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#N2");
						Assert.IsNotNull (ex.Message, "#N3");
						Assert.IsNull (ex.InnerException, "#N4");
					}

					enumerator.Reset ();
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void Entries_Source_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.SourceExists ("monoothersource", "."))
				Assert.Ignore ("Event log source 'monoothersource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp", ".");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monoothersource")) {
					Assert.IsNotNull (eventLog.Entries, "#A1");
					Assert.AreEqual (0, eventLog.Entries.Count, "#A2");
					EventLog.WriteEntry ("monotempsource", "Entries_Source_DoesNotExist1");
					Assert.AreEqual (1, eventLog.Entries.Count, "#A3");

					IEnumerator enumerator = eventLog.Entries.GetEnumerator ();
					Assert.IsNotNull (enumerator, "#B");

					try {
						object current = enumerator.Current;
						Assert.Fail ("#C1: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
						Assert.IsNotNull (ex.Message, "#C3");
						Assert.IsNull (ex.InnerException, "#C4");
					}

					Assert.IsTrue (enumerator.MoveNext (), "#D1");
					Assert.IsNotNull (enumerator.Current, "#D2");
					Assert.IsFalse (enumerator.MoveNext (), "#D3");

					EventLogEntry [] entries = new EventLogEntry [1];
					eventLog.Entries.CopyTo (entries, 0);

					EventLogEntry entry = entries [0];
					Assert.IsNotNull (entry, "#E1");
					Assert.IsNotNull (entry.Source, "#E2");
					Assert.AreEqual ("monotempsource", entry.Source, "#E3");
					Assert.IsNotNull (entry.ReplacementStrings, "#E4");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#E5");
					Assert.AreEqual ("Entries_Source_DoesNotExist1", entry.ReplacementStrings [0], "#E6");

					try {
						object current = enumerator.Current;
						Assert.Fail ("#E1: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#E2");
						Assert.IsNotNull (ex.Message, "#E3");
						Assert.IsNull (ex.InnerException, "#E4");
					}

					try {
						object current = enumerator.Current;
						Assert.Fail ("#F1: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#F2");
						Assert.IsNotNull (ex.Message, "#F3");
						Assert.IsNull (ex.InnerException, "#F4");
					}

					EventLog.WriteEntry ("monotempsource", "Entries_Source_DoesNotExist2");

#if NET_2_0
					try {
						object current = enumerator.Current;
						Assert.Fail ("#G1: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#G2");
						Assert.IsNotNull (ex.Message, "#G3");
						Assert.IsNull (ex.InnerException, "#G4");
					}
#else
					entry = (EventLogEntry) enumerator.Current;
					Assert.IsNotNull (entry, "#G1");
					Assert.IsNotNull (entry.Source, "#G2");
					Assert.AreEqual ("monotempsource", entry.Source, "#G3");
					Assert.IsNotNull (entry.ReplacementStrings, "#G4");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#G5");
					Assert.AreEqual ("Entries_Source_DoesNotExist2", entry.ReplacementStrings [0], "#G6");
#endif

					Assert.IsFalse (enumerator.MoveNext (), "#H1");
					Assert.AreEqual (2, eventLog.Entries.Count, "#H2");

					entries = new EventLogEntry [1];
					try {
						eventLog.Entries.CopyTo (entries, 0);
						Assert.Fail ("#I1");
					} catch (ArgumentException ex) {
						// Destination array was not long enough. Check destIndex
						// and length, and the array's lower bounds
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#I2");
						Assert.IsNotNull (ex.Message, "#I3");
						Assert.IsNull (ex.InnerException, "#I4");
#if NET_2_0
						Assert.AreEqual (string.Empty, ex.ParamName, "#I5");
#else
						Assert.IsNull (ex.ParamName, "#I5");
#endif
					}

					entries = new EventLogEntry [2];
					eventLog.Entries.CopyTo (entries, 0);

					entry = entries [0];
					Assert.IsNotNull (entry, "#J1");
					Assert.IsNotNull (entry.Source, "#J2");
					Assert.AreEqual ("monotempsource", entry.Source, "#J3");
					Assert.IsNotNull (entry.ReplacementStrings, "#J4");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#J5");
					Assert.AreEqual ("Entries_Source_DoesNotExist1", entry.ReplacementStrings [0], "#J6");

					entry = entries [1];
					Assert.IsNotNull (entry, "#K1");
					Assert.IsNotNull (entry.Source, "#K2");
					Assert.AreEqual ("monotempsource", entry.Source, "#K3");
					Assert.IsNotNull (entry.ReplacementStrings, "#K4");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#K5");
					Assert.AreEqual ("Entries_Source_DoesNotExist2", entry.ReplacementStrings [0], "#K6");
	
					Assert.IsFalse (enumerator.MoveNext (), "#L1");
					enumerator.Reset ();
					Assert.IsTrue (enumerator.MoveNext (), "#L2");
					Assert.IsNotNull (enumerator.Current, "#L3");
					Assert.IsTrue (enumerator.MoveNext (), "#L4");
					Assert.IsNotNull (enumerator.Current, "#L5");

					Assert.IsFalse (enumerator.MoveNext (), "#M1");
					enumerator.Reset ();
					Assert.IsTrue (enumerator.MoveNext (), "#M2");
					eventLog.Clear ();
#if NET_2_0
					Assert.IsNotNull (enumerator.Current, "#M3");
#else
					try {
						object current = enumerator.Current;
						Assert.Fail ("#M3a: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#M3b");
						Assert.IsNotNull (ex.Message, "#M3c");
						Assert.IsNull (ex.InnerException, "#M3d");
					}
#endif
					Assert.IsFalse (enumerator.MoveNext (), "#M4");

					try {
						object current = enumerator.Current;
						Assert.Fail ("#N1: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#N2");
						Assert.IsNotNull (ex.Message, "#N3");
						Assert.IsNull (ex.InnerException, "#N4");
					}

					Assert.IsFalse (enumerator.MoveNext (), "#O1");
					enumerator.Reset ();
					Assert.IsFalse (enumerator.MoveNext (), "#O2");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void Entries_Source_Empty ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp", ".");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", string.Empty)) {
					Assert.IsNotNull (eventLog.Entries, "#A1");
					Assert.AreEqual (0, eventLog.Entries.Count, "#A2");
					EventLog.WriteEntry ("monotempsource", "Entries_Source_Empty1");
					Assert.AreEqual (1, eventLog.Entries.Count, "#A3");

					IEnumerator enumerator = eventLog.Entries.GetEnumerator ();
					Assert.IsNotNull (enumerator, "#B");

					try {
						object current = enumerator.Current;
						Assert.Fail ("#C1: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
						Assert.IsNotNull (ex.Message, "#C3");
						Assert.IsNull (ex.InnerException, "#C4");
					}

					Assert.IsTrue (enumerator.MoveNext (), "#D1");
					Assert.IsNotNull (enumerator.Current, "#D2");
					Assert.IsFalse (enumerator.MoveNext (), "#D3");

					EventLogEntry [] entries = new EventLogEntry [1];
					eventLog.Entries.CopyTo (entries, 0);

					EventLogEntry entry = entries [0];
					Assert.IsNotNull (entry, "#E1");
					Assert.IsNotNull (entry.Source, "#E2");
					Assert.AreEqual ("monotempsource", entry.Source, "#E3");
					Assert.IsNotNull (entry.ReplacementStrings, "#E4");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#E5");
					Assert.AreEqual ("Entries_Source_Empty1", entry.ReplacementStrings [0], "#E6");

					try {
						object current = enumerator.Current;
						Assert.Fail ("#E1: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#E2");
						Assert.IsNotNull (ex.Message, "#E3");
						Assert.IsNull (ex.InnerException, "#E4");
					}

					EventLog.WriteEntry ("monotempsource", "Entries_Source_Empty2");

#if NET_2_0
					try {
						object current = enumerator.Current;
						Assert.Fail ("#G1: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#G2");
						Assert.IsNotNull (ex.Message, "#G3");
						Assert.IsNull (ex.InnerException, "#G4");
					}
#else
					entry = (EventLogEntry) enumerator.Current;
					Assert.IsNotNull (entry, "#G1");
					Assert.IsNotNull (entry.Source, "#G2");
					Assert.AreEqual ("monotempsource", entry.Source, "#G3");
					Assert.IsNotNull (entry.ReplacementStrings, "#G4");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#G5");
					Assert.AreEqual ("Entries_Source_Empty2", entry.ReplacementStrings [0], "#G6");
#endif

					Assert.IsFalse (enumerator.MoveNext (), "#H1");
					Assert.AreEqual (2, eventLog.Entries.Count, "#H2");

					entries = new EventLogEntry [1];
					try {
						eventLog.Entries.CopyTo (entries, 0);
						Assert.Fail ("#I1");
					} catch (ArgumentException ex) {
						// Destination array was not long enough. Check destIndex
						// and length, and the array's lower bounds
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#I2");
						Assert.IsNotNull (ex.Message, "#I3");
						Assert.IsNull (ex.InnerException, "#I4");
#if NET_2_0
						Assert.AreEqual ("", ex.ParamName, "#I5");
#else
						Assert.IsNull (ex.ParamName, "#I5");
#endif
					}

					entries = new EventLogEntry [2];
					eventLog.Entries.CopyTo (entries, 0);

					entry = entries [0];
					Assert.IsNotNull (entry, "#J1");
					Assert.IsNotNull (entry.Source, "#J2");
					Assert.AreEqual ("monotempsource", entry.Source, "#J3");
					Assert.IsNotNull (entry.ReplacementStrings, "#J4");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#J5");
					Assert.AreEqual ("Entries_Source_Empty1", entry.ReplacementStrings [0], "#J6");

					entry = entries [1];
					Assert.IsNotNull (entry, "#K1");
					Assert.IsNotNull (entry.Source, "#K2");
					Assert.AreEqual ("monotempsource", entry.Source, "#K3");
					Assert.IsNotNull (entry.ReplacementStrings, "#K4");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#K5");
					Assert.AreEqual ("Entries_Source_Empty2", entry.ReplacementStrings [0], "#K6");

					Assert.IsFalse (enumerator.MoveNext (), "#L1");
					enumerator.Reset ();
					Assert.IsTrue (enumerator.MoveNext (), "#L2");
					Assert.IsNotNull (enumerator.Current, "#L3");
					Assert.IsTrue (enumerator.MoveNext (), "#L4");
					Assert.IsNotNull (enumerator.Current, "#L5");

					Assert.IsFalse (enumerator.MoveNext (), "#M1");
					enumerator.Reset ();
					Assert.IsTrue (enumerator.MoveNext (), "#M2");
					eventLog.Clear ();
#if NET_2_0
					Assert.IsNotNull (enumerator.Current, "#M3");
#else
					try {
						object current = enumerator.Current;
						Assert.Fail ("#M3a: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#M3b");
						Assert.IsNotNull (ex.Message, "#M3c");
						Assert.IsNull (ex.InnerException, "#M3d");
					}
#endif
					Assert.IsFalse (enumerator.MoveNext (), "#M4");

					try {
						object current = enumerator.Current;
						Assert.Fail ("#N1: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#N2");
						Assert.IsNotNull (ex.Message, "#N3");
						Assert.IsNull (ex.InnerException, "#N4");
					}

					Assert.IsFalse (enumerator.MoveNext (), "#O1");
					enumerator.Reset ();
					Assert.IsFalse (enumerator.MoveNext (), "#O2");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void Entries_Source_Null ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp", ".");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", null)) {
					Assert.IsNotNull (eventLog.Entries, "#A1");
					Assert.AreEqual (0, eventLog.Entries.Count, "#A2");
					EventLog.WriteEntry ("monotempsource", "Entries_Source_Null1");
					Assert.AreEqual (1, eventLog.Entries.Count, "#A3");

					IEnumerator enumerator = eventLog.Entries.GetEnumerator ();
					Assert.IsNotNull (enumerator, "#B");

					try {
						object current = enumerator.Current;
						Assert.Fail ("#C1: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
						Assert.IsNotNull (ex.Message, "#C3");
						Assert.IsNull (ex.InnerException, "#C4");
					}

					Assert.IsTrue (enumerator.MoveNext (), "#D1");
					Assert.IsNotNull (enumerator.Current, "#D2");
					Assert.IsFalse (enumerator.MoveNext (), "#D3");

					EventLogEntry [] entries = new EventLogEntry [1];
					eventLog.Entries.CopyTo (entries, 0);

					EventLogEntry entry = entries [0];
					Assert.IsNotNull (entry, "#E1");
					Assert.IsNotNull (entry.Source, "#E2");
					Assert.AreEqual ("monotempsource", entry.Source, "#E3");
					Assert.IsNotNull (entry.ReplacementStrings, "#E4");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#E5");
					Assert.AreEqual ("Entries_Source_Null1", entry.ReplacementStrings [0], "#E6");

					try {
						object current = enumerator.Current;
						Assert.Fail ("#E1: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#E2");
						Assert.IsNotNull (ex.Message, "#E3");
						Assert.IsNull (ex.InnerException, "#E4");
					}

					try {
						object current = enumerator.Current;
						Assert.Fail ("#F1: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#F2");
						Assert.IsNotNull (ex.Message, "#F3");
						Assert.IsNull (ex.InnerException, "#F4");
					}

					EventLog.WriteEntry ("monotempsource", "Entries_Source_Null2");

#if NET_2_0
					try {
						object current = enumerator.Current;
						Assert.Fail ("#G1: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#G2");
						Assert.IsNotNull (ex.Message, "#G3");
						Assert.IsNull (ex.InnerException, "#G4");
					}
#else
					entry = (EventLogEntry) enumerator.Current;
					Assert.IsNotNull (entry, "#G1");
					Assert.IsNotNull (entry.Source, "#G2");
					Assert.AreEqual ("monotempsource", entry.Source, "#G3");
					Assert.IsNotNull (entry.ReplacementStrings, "#G4");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#G5");
					Assert.AreEqual ("Entries_Source_Null2", entry.ReplacementStrings [0], "#G6");
#endif

					Assert.IsFalse (enumerator.MoveNext (), "#H1");
					Assert.AreEqual (2, eventLog.Entries.Count, "#H2");

					entries = new EventLogEntry [1];
					try {
						eventLog.Entries.CopyTo (entries, 0);
						Assert.Fail ("#I1");
					} catch (ArgumentException ex) {
						// Destination array was not long enough. Check destIndex
						// and length, and the array's lower bounds
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#I2");
						Assert.IsNotNull (ex.Message, "#I3");
						Assert.IsNull (ex.InnerException, "#I4");
#if NET_2_0
						Assert.AreEqual ("", ex.ParamName, "#I5");
#else
						Assert.IsNull (ex.ParamName, "#I5");
#endif
					}

					entries = new EventLogEntry [2];
					eventLog.Entries.CopyTo (entries, 0);

					entry = entries [0];
					Assert.IsNotNull (entry, "#J1");
					Assert.IsNotNull (entry.Source, "#J2");
					Assert.AreEqual ("monotempsource", entry.Source, "#J3");
					Assert.IsNotNull (entry.ReplacementStrings, "#J4");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#J5");
					Assert.AreEqual ("Entries_Source_Null1", entry.ReplacementStrings [0], "#J6");

					entry = entries [1];
					Assert.IsNotNull (entry, "#K1");
					Assert.IsNotNull (entry.Source, "#K2");
					Assert.AreEqual ("monotempsource", entry.Source, "#K3");
					Assert.IsNotNull (entry.ReplacementStrings, "#K4");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#K5");
					Assert.AreEqual ("Entries_Source_Null2", entry.ReplacementStrings [0], "#K6");

					Assert.IsFalse (enumerator.MoveNext (), "#L1");
					enumerator.Reset ();
					Assert.IsTrue (enumerator.MoveNext (), "#L2");
					Assert.IsNotNull (enumerator.Current, "#L3");
					Assert.IsTrue (enumerator.MoveNext (), "#L4");
					Assert.IsNotNull (enumerator.Current, "#L5");

					Assert.IsFalse (enumerator.MoveNext (), "#M1");
					enumerator.Reset ();
					Assert.IsTrue (enumerator.MoveNext (), "#M2");
					eventLog.Clear ();
#if NET_2_0
					Assert.IsNotNull (enumerator.Current, "#M3");
#else
					try {
						object current = enumerator.Current;
						Assert.Fail ("#M3a: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#M3b");
						Assert.IsNotNull (ex.Message, "#M3c");
						Assert.IsNull (ex.InnerException, "#M3d");
					}
#endif
					Assert.IsFalse (enumerator.MoveNext (), "#M4");

					try {
						object current = enumerator.Current;
						Assert.Fail ("#N1: " + current);
					} catch (InvalidOperationException ex) {
						// No current EventLog entry available, cursor is located
						// before the first or after the last element of the
						// enumeration
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#N2");
						Assert.IsNotNull (ex.Message, "#N3");
						Assert.IsNull (ex.InnerException, "#N4");
					}

					Assert.IsFalse (enumerator.MoveNext (), "#O1");
					enumerator.Reset ();
					Assert.IsFalse (enumerator.MoveNext (), "#O2");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void Exists1_Win32 ()
		{
			if (EventLogImplType != WIN32_IMPL)
				// test can only pass with win32 implementation
				return;

			using (RegistryKey logKey = FindLogKeyByName ("monotempsource")) {
				if (logKey != null)
					Assert.Ignore ("Event log 'monotempsource' should not exist.");
			}

			using (RegistryKey logKey = FindLogKeyByName ("monologtemp")) {
				if (logKey != null)
					Assert.Ignore ("Event log 'monologtemp' should not exist.");
			}

			using (RegistryKey logKey = FindLogKeyByName ("monologother")) {
				if (logKey != null)
					Assert.Ignore ("Event log 'monologother' should not exist.");
			}

			using (RegistryKey sourceKey = FindSourceKeyByName ("monotempsource")) {
				if (sourceKey != null)
					Assert.Ignore ("Event log source 'monotempsource' should not exist.");
			}

			Assert.IsFalse (EventLog.Exists ("monologtemp"), "#A1");
			Assert.IsFalse (EventLog.Exists ("MonoLogTemp"), "#A2");
			Assert.IsFalse (EventLog.Exists ("monologother"), "#A3");
			Assert.IsFalse (EventLog.Exists ("MonoLogOther"), "#A4");

			using (RegistryKey eventLogKey = EventLogKey) {
				RegistryKey logKey = eventLogKey.CreateSubKey ("monologtemp");
				try {
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#B1");
					Assert.IsTrue (EventLog.Exists ("MonoLogTemp"), "#B2");
					Assert.IsFalse (EventLog.Exists ("monologother"), "#B3");
					Assert.IsFalse (EventLog.Exists ("MonoLogOther"), "#B3");

					Assert.IsFalse (EventLog.Exists ("monotempsource"), "#BXXX");

					using (RegistryKey sourceKey = logKey.CreateSubKey ("monotempsource")) {
					}

					Assert.IsFalse (EventLog.Exists ("monotempsource"), "#C1");
					Assert.IsFalse (EventLog.Exists ("MonoTempSource"), "#C2");
				} finally {
					if (logKey != null) {
						logKey.Close ();
						eventLogKey.DeleteSubKeyTree ("monologtemp");
					}
				}
			}
		}

		[Test]
		public void Exists1_Log_Empty ()
		{
			Assert.IsFalse (EventLog.Exists (string.Empty));
		}

		[Test]
		public void Exists1_Log_Null ()
		{
			Assert.IsFalse (EventLog.Exists (null));
		}

		[Test]
		public void Exists2_Win32 ()
		{
			if (EventLogImplType != WIN32_IMPL)
				// test can only pass with win32 implementation
				return;

			using (RegistryKey logKey = FindLogKeyByName ("monotempsource")) {
				if (logKey != null)
					Assert.Ignore ("Event log 'monotempsource' should not exist.");
			}

			using (RegistryKey logKey = FindLogKeyByName ("monologtemp")) {
				if (logKey != null)
					Assert.Ignore ("Event log 'monologtemp' should not exist.");
			}

			using (RegistryKey logKey = FindLogKeyByName ("monologother")) {
				if (logKey != null)
					Assert.Ignore ("Event log 'monologother' should not exist.");
			}

			using (RegistryKey sourceKey = FindSourceKeyByName ("monotempsource")) {
				if (sourceKey != null)
					Assert.Ignore ("Event log source 'monotempsource' should not exist.");
			}

			Assert.IsFalse (EventLog.Exists ("monologtemp", "."), "#A1");
			Assert.IsFalse (EventLog.Exists ("MonoLogTemp", "."), "#A2");
			Assert.IsFalse (EventLog.Exists ("monologother", "."), "#A3");
			Assert.IsFalse (EventLog.Exists ("MonoLogOther", "."), "#A4");

			using (RegistryKey eventLogKey = EventLogKey) {
				RegistryKey logKey = eventLogKey.CreateSubKey ("monologtemp");
				try {
					Assert.IsTrue (EventLog.Exists ("monologtemp", "."), "#B1");
					Assert.IsTrue (EventLog.Exists ("MonoLogTemp", "."), "#B2");
					Assert.IsFalse (EventLog.Exists ("monologother", "."), "#B3");
					Assert.IsFalse (EventLog.Exists ("MonoLogOther", "."), "#B3");

					using (RegistryKey sourceKey = logKey.CreateSubKey ("monotempsource")) {
					}

					Assert.IsFalse (EventLog.Exists ("monotempsource", "."), "#C1");
					Assert.IsFalse (EventLog.Exists ("MonoTempSource", "."), "#C2");
				} finally {
					if (logKey != null) {
						logKey.Close ();
						eventLogKey.DeleteSubKeyTree ("monologtemp");
					}
				}
			}
		}

		[Test]
		public void Exists2_Log_Empty ()
		{
			Assert.IsFalse (EventLog.Exists (string.Empty, "."));
		}

		[Test]
		public void Exists2_Log_Null ()
		{
			Assert.IsFalse (EventLog.Exists (null, "."));
		}

		[Test]
		public void Exists2_MachineName_Empty ()
		{
			try {
				EventLog.Exists ("monologtemp", string.Empty);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Invalid format for argument machineName
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsTrue (ex.Message.IndexOf ("machineName") != -1, "#A4");
				Assert.IsNull (ex.InnerException, "#A5");
				Assert.IsNull (ex.ParamName, "#A6");
			}

			try {
				EventLog.Exists (string.Empty, string.Empty);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Invalid format for argument machineName
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf ("machineName") != -1, "#B4");
				Assert.IsNull (ex.InnerException, "#B5");
				Assert.IsNull (ex.ParamName, "#B6");
			}

			try {
				EventLog.Exists (null, string.Empty);
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// Invalid format for argument machineName
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.Message, "#C3");
				Assert.IsTrue (ex.Message.IndexOf ("machineName") != -1, "#C4");
				Assert.IsNull (ex.InnerException, "#C5");
				Assert.IsNull (ex.ParamName, "#C6");
			}

			try {
				EventLog.Exists ("monologtemp", " \t\n");
				Assert.Fail ("#D1");
			} catch (ArgumentException ex) {
				// Invalid format for argument machineName
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
				Assert.IsNotNull (ex.Message, "#D3");
				Assert.IsTrue (ex.Message.IndexOf ("machineName") != -1, "#D4");
				Assert.IsNull (ex.InnerException, "#D5");
				Assert.IsNull (ex.ParamName, "#D6");
			}
		}

		[Test]
		public void Exists2_MachineName_Null ()
		{
			try {
				EventLog.Exists ("monologtemp", null);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Invalid format for argument machineName
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsTrue (ex.Message.IndexOf ("machineName") != -1, "#A4");
				Assert.IsNull (ex.InnerException, "#A5");
				Assert.IsNull (ex.ParamName, "#A6");
			}

			try {
				EventLog.Exists (string.Empty, null);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Invalid format for argument machineName
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf ("machineName") != -1, "#B4");
				Assert.IsNull (ex.InnerException, "#B5");
				Assert.IsNull (ex.ParamName, "#B6");
			}

			try {
				EventLog.Exists (null, null);
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// Invalid format for argument machineName
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.Message, "#C3");
				Assert.IsTrue (ex.Message.IndexOf ("machineName") != -1, "#C4");
				Assert.IsNull (ex.InnerException, "#C5");
				Assert.IsNull (ex.ParamName, "#C6");
			}
		}

		[Test]
		public void Log ()
		{
			EventLog eventLog = new EventLog ();
			eventLog.Log = string.Empty;
			Assert.AreEqual (string.Empty, eventLog.Log, "#A1");
			Assert.AreEqual (string.Empty, eventLog.Source, "#A2");
			eventLog.Log = "monologtemp";
			Assert.AreEqual ("monologtemp", eventLog.Log, "#A3");
			Assert.AreEqual (string.Empty, eventLog.Source, "#A4");
			eventLog.Log = string.Empty;
			Assert.AreEqual (string.Empty, eventLog.Log, "#A5");
			Assert.AreEqual (string.Empty, eventLog.Source, "#A6");

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			if (EventLog.Exists ("shouldnotexist", "."))
				Assert.Ignore ("Event log 'shouldnotexist' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp", ".");
			try {
				eventLog.Log = "shouldnotexist";
				eventLog.Source = "monotempsource";
				Assert.AreEqual ("shouldnotexist", eventLog.Log, "#B1");
				eventLog.Log = string.Empty;
				Assert.AreEqual ("monologtemp", eventLog.Log, "#B2");
				eventLog.Source = null;
				Assert.AreEqual ("monologtemp", eventLog.Log, "#B3");
				eventLog.Log = "MONOLOGTEMP";
				Assert.AreEqual ("monologtemp", eventLog.Log, "#B4");
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
				eventLog.Close ();
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Log_Null ()
		{
			EventLog eventLog = new EventLog ();
			eventLog.Log = null;
		}

		[Test]
		public void LogNameFromSourceName ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");


			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				Assert.IsNotNull (EventLog.LogNameFromSourceName ("monotempsource", "."), "#1");
				Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#2");
				Assert.IsNotNull (EventLog.LogNameFromSourceName ("monologtemp", "."), "#3");
				Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monologtemp", "."), "#4");
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void LogNameFromSourceName_MachineName_Empty ()
		{
			try {
				EventLog.LogNameFromSourceName ("monotempsource", string.Empty);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
#if NET_2_0
				// Invalid value '' for parameter 'MachineName'
				Assert.IsTrue (ex.Message.IndexOf ("''") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'MachineName'") != -1, "#A5");
#else
				// Invalid value  for parameter MachineName
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("MachineName") != -1, "#A5");
#endif
				Assert.IsNull (ex.InnerException, "#A6");
				Assert.IsNull (ex.ParamName, "#A7");
			}

			try {
				EventLog.LogNameFromSourceName ("monotempsource", " \t\n");
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
#if NET_2_0
				// Invalid value ' \t\n' for parameter 'MachineName'
				Assert.IsTrue (ex.Message.IndexOf ("' \t\n'") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'MachineName'") != -1, "#B5");
#else
				// Invalid value  \t\n for parameter MachineName
				Assert.IsTrue (ex.Message.IndexOf ("  \t\n ") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("MachineName") != -1, "#B5");
#endif
				Assert.IsNull (ex.InnerException, "#B6");
				Assert.IsNull (ex.ParamName, "#B7");
			}
		}

		[Test]
		public void LogNameFromSourceName_MachineName_Null ()
		{
			try {
				EventLog.LogNameFromSourceName ("monotempsource", null);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Invalid value '' for parameter 'MachineName'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
#if NET_2_0
				Assert.IsTrue (ex.Message.IndexOf ("''") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'MachineName'") != -1, "#A5");
#else
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("MachineName") != -1, "#A5");
#endif
				Assert.IsNull (ex.InnerException, "#A6");
				Assert.IsNull (ex.ParamName, "#A7");
			}
		}

		[Test]
		public void LogNameFromSourceName_Source_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			string logName = EventLog.LogNameFromSourceName ("monotempsource", ".");
			Assert.IsNotNull (logName, "#1");
			Assert.AreEqual (string.Empty, logName, "#2");
		}

		[Test]
		public void LogNameFromSourceName_Source_Empty ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			string logName = EventLog.LogNameFromSourceName (string.Empty, ".");
			Assert.IsNotNull (logName, "#1");
			Assert.AreEqual (string.Empty, logName, "#2");
		}

		[Test]
		public void LogNameFromSourceName_Source_Null ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			string logName = EventLog.LogNameFromSourceName (null, ".");
			Assert.IsNotNull (logName, "#1");
			Assert.AreEqual (string.Empty, logName, "#2");
		}

		[Test]
		public void MachineName_Null ()
		{
			EventLog eventLog = new EventLog ();

			try {
				eventLog.MachineName = null;
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Invalid value  for property MachineName
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf ("MachineName") != -1, "#6");
				Assert.IsNull (ex.ParamName, "#7");
			}
		}

		[Test]
		public void MachineName ()
		{
			string machineName = Environment.MachineName.ToLower ();

			EventLog eventLog = new EventLog ("Application", machineName);
			eventLog.EnableRaisingEvents = true;
			Assert.AreEqual (machineName, eventLog.MachineName, "#1");
			eventLog.MachineName = Environment.MachineName.ToUpper ();
			Assert.AreEqual (machineName, eventLog.MachineName, "#2");
			Assert.IsTrue (eventLog.EnableRaisingEvents, "#3");
			eventLog.MachineName = ".";
			Assert.AreEqual (".", eventLog.MachineName, "#4");
			Assert.IsFalse (eventLog.EnableRaisingEvents, "#5");
		}

		[Test]
		public void MachineName_Empty ()
		{
			EventLog eventLog = new EventLog ();

			try {
				eventLog.MachineName = string.Empty;
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Invalid value  for property MachineName
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf ("MachineName") != -1, "#A6");
				Assert.IsNull (ex.ParamName, "#A7");
			}

			try {
				eventLog.MachineName = " \t\n";
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Invalid value  for property MachineName
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf ("MachineName") != -1, "#B6");
				Assert.IsNull (ex.ParamName, "#B7");
			}
		}

		[Test]
		public void Source ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

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

				EventLog.CreateEventSource ("monotempsource", "monologtemp");
				try {
					Assert.AreEqual (string.Empty, eventLog.Source, "#B1");
					Assert.AreEqual (string.Empty, eventLog.Log, "#B2");
					eventLog.Source = "monotempsource";
					Assert.AreEqual ("monotempsource", eventLog.Source, "#B3");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#B4");
					eventLog.Log = string.Empty;
					Assert.AreEqual ("monotempsource", eventLog.Source, "#B5");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#B6");
					eventLog.Source = null;
					Assert.AreEqual (string.Empty, eventLog.Source, "#B7");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#B8");
					eventLog.Log = string.Empty;
					Assert.AreEqual (string.Empty, eventLog.Source, "#B9");
					Assert.AreEqual (string.Empty, eventLog.Log, "#B10");
				} finally {
					EventLog.Delete ("monologtemp");
				}

				eventLog.Source = "whatever";
				Assert.AreEqual ("whatever", eventLog.Source, "#C1");
				eventLog.Source = "WHATEVER";
				Assert.AreEqual ("whatever", eventLog.Source, "#C2");
			}
		}

		[Test]
		public void SourceExists1_Win32 ()
		{
			if (EventLogImplType != WIN32_IMPL)
				// test can only pass with win32 implementation
				return;

			using (RegistryKey logKey = FindLogKeyByName ("monotempsource")) {
				if (logKey != null)
					Assert.Ignore ("Event log 'monotempsource' should not exist.");
			}

			using (RegistryKey logKey = FindLogKeyByName ("monologtemp")) {
				if (logKey != null)
					Assert.Ignore ("Event log 'monologtemp' should not exist.");
			}

			using (RegistryKey sourceKey = FindSourceKeyByName ("monotempsource")) {
				if (sourceKey != null)
					Assert.Ignore ("Event log source 'monotempsource' should not exist.");
			}

			Assert.IsFalse (EventLog.SourceExists ("monotempsource"), "#A1");
			Assert.IsFalse (EventLog.SourceExists ("MonoTempSource"), "#A2");

			using (RegistryKey eventLogKey = EventLogKey) {
				RegistryKey logKey = eventLogKey.CreateSubKey ("monotempsource");
				try {
					// make sure we do not mistake a log for a source
					Assert.IsFalse (EventLog.SourceExists ("monotempsource"), "#B1");
					Assert.IsFalse (EventLog.SourceExists ("MonoTempSource"), "#B2");
				} finally {
					if (logKey != null) {
						logKey.Close ();
						eventLogKey.DeleteSubKeyTree ("monotempsource");
					}
				}

				logKey = eventLogKey.CreateSubKey ("monologtemp");
				try {
					RegistryKey sourceKey = null;
					try {
						// create temporary source key
						sourceKey = logKey.CreateSubKey ("monotempsource");
						Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#C1");
						Assert.IsTrue (EventLog.SourceExists ("MonoTempSource"), "#C2");
					} finally {
						if (sourceKey != null) {
							sourceKey.Close ();
							logKey.DeleteSubKeyTree ("monotempsource");
						}
					}
				} finally {
					if (logKey != null) {
						logKey.Close ();
						eventLogKey.DeleteSubKeyTree ("monologtemp");
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
		public void SourceExists2_Win32 ()
		{
			if (EventLogImplType != WIN32_IMPL)
				// test can only pass with win32 implementation
				return;

			using (RegistryKey logKey = FindLogKeyByName ("monotempsource")) {
				if (logKey != null)
					Assert.Ignore ("Event log 'monotempsource' should not exist.");
			}

			using (RegistryKey logKey = FindLogKeyByName ("monologtemp")) {
				if (logKey != null)
					Assert.Ignore ("Event log 'monologtemp' should not exist.");
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

				logKey = eventLogKey.CreateSubKey ("monologtemp");
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
						eventLogKey.DeleteSubKeyTree ("monologtemp");
					}
				}
			}
		}

		[Test]
		public void SourceExists2_MachineName_Empty ()
		{
			try {
				EventLog.SourceExists ("monotempsource", string.Empty);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
#if NET_2_0
				// Invalid value '' for parameter 'machineName'
				Assert.IsTrue (ex.Message.IndexOf ("''") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'machineName'") != -1, "#A5");
#else
				// Invalid value '' for parameter machineName
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("machineName") != -1, "#A5");
#endif
				Assert.IsNull (ex.InnerException, "#A6");
				Assert.IsNull (ex.ParamName, "#A7");
			}

			try {
				EventLog.SourceExists ("monotempsource", " \t\n");
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
#if NET_2_0
				// Invalid value ' \t\n' for parameter 'machineName'
				Assert.IsTrue (ex.Message.IndexOf ("' \t\n'") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'machineName'") != -1, "#B5");
#else
				// Invalid value  \t\n for parameter machineName
				Assert.IsTrue (ex.Message.IndexOf ("  \t\n ") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("machineName") != -1, "#B5");
#endif
				Assert.IsNull (ex.InnerException, "#B6");
				Assert.IsNull (ex.ParamName, "#B7");
			}
		}

		[Test]
		public void SourceExists2_MachineName_Null ()
		{
			try {
				EventLog.SourceExists ("monotempsource", null);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
#if NET_2_0
				// Invalid value '' for parameter 'machineName'
				Assert.IsTrue (ex.Message.IndexOf ("''") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("'machineName'") != -1, "#5");
#else
				// Invalid value  for parameter machineName
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("machineName") != -1, "#5");
#endif
				Assert.IsNull (ex.InnerException, "#6");
				Assert.IsNull (ex.ParamName, "#7");
			}
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
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					eventLog.WriteEntry ("WriteEntry1a");

					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");

					EventLogEntry entry = eventLog.Entries[eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(0)", entry.Category, "#B3");
					Assert.AreEqual (0, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.Information, entry.EntryType, "#B7");
					Assert.AreEqual (0, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("WriteEntry1a", entry.ReplacementStrings[0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");

					eventLog.WriteEntry ("WriteEntry1b" + Environment.NewLine + "ok");

					entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#C1");
					Assert.IsNotNull (entry.Category, "#C2");
					Assert.AreEqual ("(0)", entry.Category, "#C3");
					Assert.AreEqual (0, entry.CategoryNumber, "#C4");
					Assert.IsNotNull (entry.Data, "#C5");
					Assert.AreEqual (0, entry.Data.Length, "#C6");
					Assert.AreEqual (EventLogEntryType.Information, entry.EntryType, "#C7");
					Assert.AreEqual (0, entry.EventID, "#C8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#C9");
#endif
					Assert.IsNotNull (entry.MachineName, "#C10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#C11");
					Assert.IsNotNull (entry.ReplacementStrings, "#C12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#C13");
					Assert.AreEqual ("WriteEntry1b" + Environment.NewLine + "ok", entry.ReplacementStrings [0], "#C14");
					Assert.IsNotNull (entry.Source, "#C15");
					Assert.AreEqual ("monotempsource", entry.Source, "#C16");
					Assert.IsNull (entry.UserName, "#C17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry1_Log_Empty ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			bool applicationLogExists = EventLog.Exists ("Application", ".");

			// specified source does not exist, so use Application log
			try {
				using (EventLog eventLog = new EventLog (string.Empty, ".", "monotempsource")) {
					eventLog.WriteEntry ("WriteEntry1_Log_Empty");
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("Application", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("Application"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("Application", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");
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
		public void WriteEntry1_Log_Mismatch ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			if (EventLog.Exists ("monologother", "."))
				Assert.Ignore ("Event log 'monologother' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologother", ".", "monotempsource")) {
					eventLog.WriteEntry ("WriteEntry1_Log_Mismatch");
				}
			} catch (ArgumentException ex) {
				// The source 'monotempsource' is not registered in log
				// 'monologother' (it is registered in log 'monologtemp').
				// The Source and Log properties must be matched, or you may
				// set Log to the empty string, and it will automatically be
				// matched to the Source property
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsTrue (ex.Message.IndexOf ("'monotempsource'") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'monologother'") != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf ("'monologtemp'") != -1, "#A6");
				Assert.IsNull (ex.InnerException, "#A7");
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");

				if (EventLog.Exists ("monologother"))
					EventLog.Delete ("monologother");
			}
		}

		[Test]
		public void WriteEntry1_Message_Empty ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					eventLog.WriteEntry (string.Empty);
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(0)", entry.Category, "#B3");
					Assert.AreEqual (0, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.Information, entry.EntryType, "#B7");
					Assert.AreEqual (0, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry1_Message_Null ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					eventLog.WriteEntry (null);
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(0)", entry.Category, "#B3");
					Assert.AreEqual (0, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.Information, entry.EntryType, "#B7");
					Assert.AreEqual (0, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry1_Source_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.SourceExists ("monoothersource", "."))
				Assert.Ignore ("Event log source 'monoothersource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monoothersource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					eventLog.WriteEntry ("WriteEntry1");
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");

					EventLogEntry entry = eventLog.Entries[eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(0)", entry.Category, "#B3");
					Assert.AreEqual (0, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.Information, entry.EntryType, "#B7");
					Assert.AreEqual (0, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("WriteEntry1", entry.ReplacementStrings[0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Source property was not set before writing to the event log
		public void WriteEntry1_Source_Empty ()
		{
			EventLog eventLog = new EventLog ("monologtemp");
			eventLog.WriteEntry ("test");
		}

		[Test]
		public void WriteEntry2 ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					eventLog.WriteEntry ("WriteEntry2a", EventLogEntryType.Information);

					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(0)", entry.Category, "#B3");
					Assert.AreEqual (0, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.Information, entry.EntryType, "#B7");
					Assert.AreEqual (0, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("WriteEntry2a", entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");

					eventLog.WriteEntry ("WriteEntry2b" + Environment.NewLine + "ok", EventLogEntryType.Error);

					entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#C1");
					Assert.IsNotNull (entry.Category, "#C2");
					Assert.AreEqual ("(0)", entry.Category, "#C3");
					Assert.AreEqual (0, entry.CategoryNumber, "#C4");
					Assert.IsNotNull (entry.Data, "#C5");
					Assert.AreEqual (0, entry.Data.Length, "#C6");
					Assert.AreEqual (EventLogEntryType.Error, entry.EntryType, "#C7");
					Assert.AreEqual (0, entry.EventID, "#C8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#C9");
#endif
					Assert.IsNotNull (entry.MachineName, "#C10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#C11");
					Assert.IsNotNull (entry.ReplacementStrings, "#C12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#C13");
					Assert.AreEqual ("WriteEntry2b" + Environment.NewLine + "ok", entry.ReplacementStrings [0], "#C14");
					Assert.IsNotNull (entry.Source, "#C15");
					Assert.AreEqual ("monotempsource", entry.Source, "#C16");
					Assert.IsNull (entry.UserName, "#C17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry2_Log_Empty ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			bool applicationLogExists = EventLog.Exists ("Application", ".");

			// specified source does not exist, so use Application log
			try {
				using (EventLog eventLog = new EventLog (string.Empty, ".", "monotempsource")) {
					eventLog.WriteEntry ("WriteEntry1_Log_Empty", EventLogEntryType.Error);
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("Application", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("Application"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("Application", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");
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
		public void WriteEntry2_Log_Mismatch ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			if (EventLog.Exists ("monologother", "."))
				Assert.Ignore ("Event log 'monologother' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologother", ".", "monotempsource")) {
					// valid message
					try {
						eventLog.WriteEntry ("WriteEntry2_Log_Mismatch1",
							EventLogEntryType.Error);
						Assert.Fail ("#A1");
					} catch (ArgumentException ex) {
						// The source 'monotempsource' is not registered in log
						// 'monologother' (it is registered in log 'monologtemp').
						// The Source and Log properties must be matched, or you may
						// set Log to the empty string, and it will automatically be
						// matched to the Source property
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
						Assert.IsNotNull (ex.Message, "#A3");
						Assert.IsTrue (ex.Message.IndexOf ("'monotempsource'") != -1, "#A4");
						Assert.IsTrue (ex.Message.IndexOf ("'monologother'") != -1, "#A5");
						Assert.IsTrue (ex.Message.IndexOf ("'monologtemp'") != -1, "#A6");
						Assert.IsNull (ex.InnerException, "#A7");
					}

					// invalid type
					try {
						eventLog.WriteEntry ("WriteEntry2_Log_Mismatch2",
							(EventLogEntryType) 666);
						Assert.Fail ("#B1");
					} catch (InvalidEnumArgumentException) {
					}
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");

				if (EventLog.Exists ("monologother"))
					EventLog.Delete ("monologother");
			}
		}

		[Test]
		public void WriteEntry2_Message_Empty ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					eventLog.WriteEntry (string.Empty, EventLogEntryType.FailureAudit);
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(0)", entry.Category, "#B3");
					Assert.AreEqual (0, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.FailureAudit, entry.EntryType, "#B7");
					Assert.AreEqual (0, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry2_Message_Null ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					eventLog.WriteEntry (null, EventLogEntryType.SuccessAudit);
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(0)", entry.Category, "#B3");
					Assert.AreEqual (0, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.SuccessAudit, entry.EntryType, "#B7");
					Assert.AreEqual (0, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry2_Source_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.SourceExists ("monoothersource", "."))
				Assert.Ignore ("Event log source 'monoothersource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monoothersource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					eventLog.WriteEntry ("WriteEntry1", EventLogEntryType.Warning);
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(0)", entry.Category, "#B3");
					Assert.AreEqual (0, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.Warning, entry.EntryType, "#B7");
					Assert.AreEqual (0, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("WriteEntry1", entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Source property was not set before writing to the event log
		public void WriteEntry2_Source_Empty ()
		{
			EventLog eventLog = new EventLog ("monologtemp");
			eventLog.WriteEntry ("test", EventLogEntryType.Information);
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))] // Enum argument value 666 is not valid for type. type should be a value from EventLogEntryType.
		public void WriteEntry2_Type_NotDefined ()
		{
			EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource");
			eventLog.WriteEntry ("test", (EventLogEntryType) 666);
		}

		[Test]
		public void WriteEntry3 ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					EventLog.WriteEntry ("monotempsource", "WriteEntry3a");

					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(0)", entry.Category, "#B3");
					Assert.AreEqual (0, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.Information, entry.EntryType, "#B7");
					Assert.AreEqual (0, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("WriteEntry3a", entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");

					EventLog.WriteEntry ("monotempsource", "WriteEntry3b" 
						+ Environment.NewLine + "ok");

					entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#C1");
					Assert.IsNotNull (entry.Category, "#C2");
					Assert.AreEqual ("(0)", entry.Category, "#C3");
					Assert.AreEqual (0, entry.CategoryNumber, "#C4");
					Assert.IsNotNull (entry.Data, "#C5");
					Assert.AreEqual (0, entry.Data.Length, "#C6");
					Assert.AreEqual (EventLogEntryType.Information, entry.EntryType, "#C7");
					Assert.AreEqual (0, entry.EventID, "#C8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#C9");
#endif
					Assert.IsNotNull (entry.MachineName, "#C10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#C11");
					Assert.IsNotNull (entry.ReplacementStrings, "#C12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#C13");
					Assert.AreEqual ("WriteEntry3b" + Environment.NewLine + "ok", entry.ReplacementStrings [0], "#C14");
					Assert.IsNotNull (entry.Source, "#C15");
					Assert.AreEqual ("monotempsource", entry.Source, "#C16");
					Assert.IsNull (entry.UserName, "#C17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry3_Message_Empty ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				EventLog.WriteEntry ("monotempsource", string.Empty);

				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#A1");
					Assert.IsNotNull (entry.Category, "#A2");
					Assert.AreEqual ("(0)", entry.Category, "#A3");
					Assert.AreEqual (0, entry.CategoryNumber, "#A4");
					Assert.IsNotNull (entry.Data, "#A5");
					Assert.AreEqual (0, entry.Data.Length, "#A6");
					Assert.AreEqual (EventLogEntryType.Information, entry.EntryType, "#A7");
					Assert.AreEqual (0, entry.EventID, "#A8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#A9");
#endif
					Assert.IsNotNull (entry.MachineName, "#A10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#A11");
					Assert.IsNotNull (entry.ReplacementStrings, "#A12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#A13");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [0], "#A14");
					Assert.IsNotNull (entry.Source, "#A15");
					Assert.AreEqual ("monotempsource", entry.Source, "#A16");
					Assert.IsNull (entry.UserName, "#A17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry3_Message_Null ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				EventLog.WriteEntry ("monotempsource", null);

				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#A1");
					Assert.IsNotNull (entry.Category, "#A2");
					Assert.AreEqual ("(0)", entry.Category, "#A3");
					Assert.AreEqual (0, entry.CategoryNumber, "#A4");
					Assert.IsNotNull (entry.Data, "#A5");
					Assert.AreEqual (0, entry.Data.Length, "#A6");
					Assert.AreEqual (EventLogEntryType.Information, entry.EntryType, "#A7");
					Assert.AreEqual (0, entry.EventID, "#A8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#A9");
#endif
					Assert.IsNotNull (entry.MachineName, "#A10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#A11");
					Assert.IsNotNull (entry.ReplacementStrings, "#A12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#A13");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [0], "#A14");
					Assert.IsNotNull (entry.Source, "#A15");
					Assert.AreEqual ("monotempsource", entry.Source, "#A16");
					Assert.IsNull (entry.UserName, "#A17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry3_Source_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			bool applicationLogExists = EventLog.Exists ("Application");
			try {
				EventLog.WriteEntry ("monotempsource", "test");

				Assert.IsTrue (EventLog.Exists ("Application"), "#A1");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A2");
				Assert.AreEqual ("Application", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A3");

				if (EventLogImplType == WIN32_IMPL)
					// win32 API does not return entries in order for
					// Application log
					return;

				using (EventLog eventLog = new EventLog ("Application", ".", "monotempsource")) {
					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(0)", entry.Category, "#B3");
					Assert.AreEqual (0, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.Information, entry.EntryType, "#B7");
					Assert.AreEqual (0, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("test", entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");
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
		[ExpectedException (typeof (ArgumentException))] // Source property was not set before writing to the event log
		public void WriteEntry3_Source_Empty ()
		{
			EventLog.WriteEntry (string.Empty, "test");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Source property was not set before writing to the event log
		public void WriteEntry3_Source_Null ()
		{
			EventLog.WriteEntry (null, "test");
		}

		[Test]
		public void WriteEntry4 ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					eventLog.WriteEntry ("WriteEntry1", EventLogEntryType.Information, 56);
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(0)", entry.Category, "#B3");
					Assert.AreEqual (0, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.Information, entry.EntryType, "#B7");
					Assert.AreEqual (56, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("WriteEntry1", entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");

					eventLog.WriteEntry ("WriteEntry2", EventLogEntryType.Error, 0);

					entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#C1");
					Assert.IsNotNull (entry.Category, "#C2");
					Assert.AreEqual ("(0)", entry.Category, "#C3");
					Assert.AreEqual (0, entry.CategoryNumber, "#C4");
					Assert.IsNotNull (entry.Data, "#C5");
					Assert.AreEqual (0, entry.Data.Length, "#C6");
					Assert.AreEqual (EventLogEntryType.Error, entry.EntryType, "#C7");
					Assert.AreEqual (0, entry.EventID, "#C8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#C9");
#endif
					Assert.IsNotNull (entry.MachineName, "#C10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#C11");
					Assert.IsNotNull (entry.ReplacementStrings, "#C12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#C13");
					Assert.AreEqual ("WriteEntry2", entry.ReplacementStrings [0], "#C14");
					Assert.IsNotNull (entry.Source, "#C15");
					Assert.AreEqual ("monotempsource", entry.Source, "#C16");
					Assert.IsNull (entry.UserName, "#C17");

					eventLog.WriteEntry ("WriteEntry2", EventLogEntryType.Error, ushort.MaxValue);

					entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#D1");
					Assert.IsNotNull (entry.Category, "#D2");
					Assert.AreEqual ("(0)", entry.Category, "#D3");
					Assert.AreEqual (0, entry.CategoryNumber, "#D4");
					Assert.IsNotNull (entry.Data, "#D5");
					Assert.AreEqual (0, entry.Data.Length, "#D6");
					Assert.AreEqual (EventLogEntryType.Error, entry.EntryType, "#D7");
					Assert.AreEqual (ushort.MaxValue, entry.EventID, "#D8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#D9");
#endif
					Assert.IsNotNull (entry.MachineName, "#D10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#D11");
					Assert.IsNotNull (entry.ReplacementStrings, "#D12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#D13");
					Assert.AreEqual ("WriteEntry2", entry.ReplacementStrings [0], "#D14");
					Assert.IsNotNull (entry.Source, "#D15");
					Assert.AreEqual ("monotempsource", entry.Source, "#D16");
					Assert.IsNull (entry.UserName, "#D17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry4_EventID_Invalid ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource");

			try {
				eventLog.WriteEntry ("test", EventLogEntryType.Information, -1);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Invalid eventID value '-1'. It must be in the range between '0' and '65535'.
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsTrue (ex.Message.IndexOf ("'-1'") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'0'") != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf ("'65535'") != -1, "#A6");
				Assert.IsNull (ex.InnerException, "#A7");
#if NET_2_0
				Assert.IsFalse (EventLog.Exists ("monologtemp"), "#A8");
				Assert.IsFalse (EventLog.SourceExists ("monotempsource"), "#A9");
				Assert.AreEqual (string.Empty, EventLog.LogNameFromSourceName ("monotempsource", "."), "#A10");
#else
				Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A8");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A9");
				Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#B10");
#endif
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}

			try {
				eventLog.WriteEntry ("test", EventLogEntryType.Information, 65536);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Invalid eventID value '65536'. It must be in the range between '0' and '65535'.
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf ("'65536'") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'0'") != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf ("'65535'") != -1, "#B6");
				Assert.IsNull (ex.InnerException, "#B7");
#if NET_2_0
				Assert.IsFalse (EventLog.Exists ("monologtemp"), "#B8");
				Assert.IsFalse (EventLog.SourceExists ("monotempsource"), "#B9");
				Assert.AreEqual (string.Empty, EventLog.LogNameFromSourceName ("monotempsource", "."), "#B10");
#else
				Assert.IsTrue (EventLog.Exists ("monologtemp"), "#B8");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#B9");
				Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#B10");
#endif
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry4_Log_Empty ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			bool applicationLogExists = EventLog.Exists ("Application", ".");

			// specified source does not exist, so use Application log
			try {
				using (EventLog eventLog = new EventLog (string.Empty, ".", "monotempsource")) {
					eventLog.WriteEntry ("WriteEntry1_Log_Empty", EventLogEntryType.Error, 555);
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("Application", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("Application"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("Application", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");
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
		public void WriteEntry4_Log_Mismatch ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			if (EventLog.Exists ("monologother", "."))
				Assert.Ignore ("Event log 'monologother' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologother", ".", "monotempsource")) {
					// valid message
					try {
						eventLog.WriteEntry ("WriteEntry4_Log_Mismatch1",
							EventLogEntryType.Error, 555);
						Assert.Fail ("#A1");
					} catch (ArgumentException ex) {
						// The source 'monotempsource' is not registered in log
						// 'monologother' (it is registered in log 'monologtemp').
						// The Source and Log properties must be matched, or you may
						// set Log to the empty string, and it will automatically be
						// matched to the Source property
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
						Assert.IsNotNull (ex.Message, "#A3");
						Assert.IsTrue (ex.Message.IndexOf ("'monotempsource'") != -1, "#A4");
						Assert.IsTrue (ex.Message.IndexOf ("'monologother'") != -1, "#A5");
						Assert.IsTrue (ex.Message.IndexOf ("'monologtemp'") != -1, "#A6");
						Assert.IsNull (ex.InnerException, "#A7");
					}

					// invalid type
					try {
						eventLog.WriteEntry ("WriteEntry4_Log_Mismatch2",
							(EventLogEntryType) 666, 555);
						Assert.Fail ("#B1");
					} catch (InvalidEnumArgumentException) {
					}

					// invalid eventID
					try {
						eventLog.WriteEntry ("WriteEntry4_Log_Mismatch3",
							EventLogEntryType.Error, -1);
						Assert.Fail ("#C1");
					} catch (ArgumentException ex) {
						// The source 'monotempsource' is not registered in log
						// 'monologother' (it is registered in log 'monologtemp').
						// The Source and Log properties must be matched, or you may
						// set Log to the empty string, and it will automatically be
						// matched to the Source property
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
						Assert.IsNotNull (ex.Message, "#C3");
						Assert.IsFalse (ex.Message.IndexOf ("'monotempsource'") != -1, "#C4");
						Assert.IsFalse (ex.Message.IndexOf ("'monologother'") != -1, "#C5");
						Assert.IsFalse (ex.Message.IndexOf ("'monologtemp'") != -1, "#C6");
						Assert.IsTrue (ex.Message.IndexOf ("'-1'") != -1, "#C7");
						Assert.IsTrue (ex.Message.IndexOf ("'0'") != -1, "#C8");
						Assert.IsTrue (ex.Message.IndexOf ("'65535'") != -1, "#C9");
						Assert.IsNull (ex.InnerException, "#C10");
					}
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");

				if (EventLog.Exists ("monologother"))
					EventLog.Delete ("monologother");
			}
		}

		[Test]
		public void WriteEntry4_Message_Empty ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					eventLog.WriteEntry (string.Empty, EventLogEntryType.FailureAudit, 888);
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(0)", entry.Category, "#B3");
					Assert.AreEqual (0, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.FailureAudit, entry.EntryType, "#B7");
					Assert.AreEqual (888, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry4_Message_Null ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					eventLog.WriteEntry (null, EventLogEntryType.SuccessAudit, 343);
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(0)", entry.Category, "#B3");
					Assert.AreEqual (0, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.SuccessAudit, entry.EntryType, "#B7");
					Assert.AreEqual (343, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry4_Source_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.SourceExists ("monoothersource", "."))
				Assert.Ignore ("Event log source 'monoothersource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monoothersource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					eventLog.WriteEntry ("WriteEntry1", EventLogEntryType.Warning, 2);
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");

					if (EventLogImplType == WIN32_IMPL)
						// win32 API does not return entries in order for
						// Application log
						return;

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(0)", entry.Category, "#B3");
					Assert.AreEqual (0, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.Warning, entry.EntryType, "#B7");
					Assert.AreEqual (2, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("WriteEntry1", entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Source property was not set before writing to the event log
		public void WriteEntry4_Source_Empty ()
		{
			EventLog eventLog = new EventLog ("monologtemp");
			eventLog.WriteEntry ("test", EventLogEntryType.Information, 56);
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))] // Enum argument value 666 is not valid for type. type should be a value from EventLogEntryType.
		public void WriteEntry4_Type_NotDefined ()
		{
			EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource");
			eventLog.WriteEntry ("test", (EventLogEntryType) 666, 44);
		}

		[Test]
		public void WriteEntry5 ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					EventLog.WriteEntry ("monotempsource", "WriteEntry3a",
						EventLogEntryType.Information);

					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(0)", entry.Category, "#B3");
					Assert.AreEqual (0, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.Information, entry.EntryType, "#B7");
					Assert.AreEqual (0, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("WriteEntry3a", entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");

					EventLog.WriteEntry ("monotempsource", "WriteEntry3b"
						+ Environment.NewLine + "ok", EventLogEntryType.Error);

					entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#C1");
					Assert.IsNotNull (entry.Category, "#C2");
					Assert.AreEqual ("(0)", entry.Category, "#C3");
					Assert.AreEqual (0, entry.CategoryNumber, "#C4");
					Assert.IsNotNull (entry.Data, "#C5");
					Assert.AreEqual (0, entry.Data.Length, "#C6");
					Assert.AreEqual (EventLogEntryType.Error, entry.EntryType, "#C7");
					Assert.AreEqual (0, entry.EventID, "#C8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#C9");
#endif
					Assert.IsNotNull (entry.MachineName, "#C10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#C11");
					Assert.IsNotNull (entry.ReplacementStrings, "#C12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#C13");
					Assert.AreEqual ("WriteEntry3b" + Environment.NewLine + "ok", entry.ReplacementStrings [0], "#C14");
					Assert.IsNotNull (entry.Source, "#C15");
					Assert.AreEqual ("monotempsource", entry.Source, "#C16");
					Assert.IsNull (entry.UserName, "#C17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry5_Message_Empty ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				EventLog.WriteEntry ("monotempsource", string.Empty, EventLogEntryType.Error);

				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#A1");
					Assert.IsNotNull (entry.Category, "#A2");
					Assert.AreEqual ("(0)", entry.Category, "#A3");
					Assert.AreEqual (0, entry.CategoryNumber, "#A4");
					Assert.IsNotNull (entry.Data, "#A5");
					Assert.AreEqual (0, entry.Data.Length, "#A6");
					Assert.AreEqual (EventLogEntryType.Error, entry.EntryType, "#A7");
					Assert.AreEqual (0, entry.EventID, "#A8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#A9");
#endif
					Assert.IsNotNull (entry.MachineName, "#A10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#A11");
					Assert.IsNotNull (entry.ReplacementStrings, "#A12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#A13");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [0], "#A14");
					Assert.IsNotNull (entry.Source, "#A15");
					Assert.AreEqual ("monotempsource", entry.Source, "#A16");
					Assert.IsNull (entry.UserName, "#A17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry5_Message_Null ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				EventLog.WriteEntry ("monotempsource", null, EventLogEntryType.FailureAudit);

				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#A1");
					Assert.IsNotNull (entry.Category, "#A2");
					Assert.AreEqual ("(0)", entry.Category, "#A3");
					Assert.AreEqual (0, entry.CategoryNumber, "#A4");
					Assert.IsNotNull (entry.Data, "#A5");
					Assert.AreEqual (0, entry.Data.Length, "#A6");
					Assert.AreEqual (EventLogEntryType.FailureAudit, entry.EntryType, "#A7");
					Assert.AreEqual (0, entry.EventID, "#A8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#A9");
#endif
					Assert.IsNotNull (entry.MachineName, "#A10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#A11");
					Assert.IsNotNull (entry.ReplacementStrings, "#A12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#A13");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [0], "#A14");
					Assert.IsNotNull (entry.Source, "#A15");
					Assert.AreEqual ("monotempsource", entry.Source, "#A16");
					Assert.IsNull (entry.UserName, "#A17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry5_Source_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			bool applicationLogExists = EventLog.Exists ("Application");
			try {
				EventLog.WriteEntry ("monotempsource", "test", EventLogEntryType.SuccessAudit);

				Assert.IsTrue (EventLog.Exists ("Application"), "#A1");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A2");
				Assert.AreEqual ("Application", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A3");

				if (EventLogImplType == WIN32_IMPL)
					// win32 API does not return entries in order for
					// Application log
					return;

				using (EventLog eventLog = new EventLog ("Application", ".", "monotempsource")) {
					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(0)", entry.Category, "#B3");
					Assert.AreEqual (0, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.SuccessAudit, entry.EntryType, "#B7");
					Assert.AreEqual (0, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("test", entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");
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
		[ExpectedException (typeof (ArgumentException))] // Source property was not set before writing to the event log
		public void WriteEntry5_Source_Empty ()
		{
			EventLog.WriteEntry (string.Empty, "test", EventLogEntryType.Warning);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Source property was not set before writing to the event log
		public void WriteEntry5_Source_Null ()
		{
			EventLog.WriteEntry (null, "test", EventLogEntryType.Error);
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))] // Enum argument value 666 is not valid for type. type should be a value from EventLogEntryType.
		public void WriteEntry5_Type_NotDefined ()
		{
			EventLog.WriteEntry ("monotempsource", "test", (EventLogEntryType) 666);
		}

		[Test]
		public void WriteEntry6 ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					eventLog.WriteEntry ("WriteEntry6a", EventLogEntryType.Information, 56, 3);

					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(3)", entry.Category, "#B3");
					Assert.AreEqual (3, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.Information, entry.EntryType, "#B7");
					Assert.AreEqual (56, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("WriteEntry6a", entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");

					eventLog.WriteEntry ("WriteEntry6b" + Environment.NewLine + "ok",
						EventLogEntryType.Error, 0, 0);

					entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#C1");
					Assert.IsNotNull (entry.Category, "#C2");
					Assert.AreEqual ("(0)", entry.Category, "#C3");
					Assert.AreEqual (0, entry.CategoryNumber, "#C4");
					Assert.IsNotNull (entry.Data, "#C5");
					Assert.AreEqual (0, entry.Data.Length, "#C6");
					Assert.AreEqual (EventLogEntryType.Error, entry.EntryType, "#C7");
					Assert.AreEqual (0, entry.EventID, "#C8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#C9");
#endif
					Assert.IsNotNull (entry.MachineName, "#C10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#C11");
					Assert.IsNotNull (entry.ReplacementStrings, "#C12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#C13");
					Assert.AreEqual ("WriteEntry6b" + Environment.NewLine + "ok", entry.ReplacementStrings [0], "#C14");
					Assert.IsNotNull (entry.Source, "#C15");
					Assert.AreEqual ("monotempsource", entry.Source, "#C16");
					Assert.IsNull (entry.UserName, "#C17");

					eventLog.WriteEntry ("WriteEntry6c", EventLogEntryType.Error,
						ushort.MaxValue, short.MaxValue);

					entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#D1");
					Assert.IsNotNull (entry.Category, "#D2");
					Assert.AreEqual ("(32767)", entry.Category, "#D3");
					Assert.AreEqual (short.MaxValue, entry.CategoryNumber, "#D4");
					Assert.IsNotNull (entry.Data, "#D5");
					Assert.AreEqual (0, entry.Data.Length, "#D6");
					Assert.AreEqual (EventLogEntryType.Error, entry.EntryType, "#D7");
					Assert.AreEqual (ushort.MaxValue, entry.EventID, "#D8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#D9");
#endif
					Assert.IsNotNull (entry.MachineName, "#D10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#D11");
					Assert.IsNotNull (entry.ReplacementStrings, "#D12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#D13");
					Assert.AreEqual ("WriteEntry6c", entry.ReplacementStrings [0], "#D14");
					Assert.IsNotNull (entry.Source, "#D15");
					Assert.AreEqual ("monotempsource", entry.Source, "#D16");
					Assert.IsNull (entry.UserName, "#D17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry6_EventID_Invalid ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource");

			try {
				eventLog.WriteEntry ("test", EventLogEntryType.Information, -1, 5);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Invalid eventID value '-1'. It must be in the range between '0' and '65535'.
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsTrue (ex.Message.IndexOf ("'-1'") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'0'") != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf ("'65535'") != -1, "#A6");
				Assert.IsNull (ex.InnerException, "#A7");
#if NET_2_0
				Assert.IsFalse (EventLog.Exists ("monologtemp"), "#A8");
				Assert.IsFalse (EventLog.SourceExists ("monotempsource"), "#A9");
				Assert.AreEqual (string.Empty, EventLog.LogNameFromSourceName ("monotempsource", "."), "#A10");
#else
				Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A8");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A9");
				Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A10");
#endif
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}

			try {
				eventLog.WriteEntry ("test", EventLogEntryType.Information, 65536, 5);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Invalid eventID value '65536'. It must be in the range between '0' and '65535'.
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf ("'65536'") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'0'") != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf ("'65535'") != -1, "#B6");
				Assert.IsNull (ex.InnerException, "#B7");
#if NET_2_0
				Assert.IsFalse (EventLog.Exists ("monologtemp"), "#B8");
				Assert.IsFalse (EventLog.SourceExists ("monotempsource"), "#B9");
				Assert.AreEqual (string.Empty, EventLog.LogNameFromSourceName ("monotempsource", "."), "#B10");
#else
				Assert.IsTrue (EventLog.Exists ("monologtemp"), "#B8");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#B9");
				Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#B10");
#endif
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry6_Log_Empty ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			bool applicationLogExists = EventLog.Exists ("Application", ".");

			// specified source does not exist, so use Application log
			try {
				using (EventLog eventLog = new EventLog (string.Empty, ".", "monotempsource")) {
					eventLog.WriteEntry ("WriteEntry1_Log_Empty", EventLogEntryType.Error, 555, 5);
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("Application", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("Application"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("Application", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");
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
		public void WriteEntry6_Log_Mismatch ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			if (EventLog.Exists ("monologother", "."))
				Assert.Ignore ("Event log 'monologother' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologother", ".", "monotempsource")) {
					// valid message
					try {
						eventLog.WriteEntry ("WriteEntry6_Log_Mismatch1",
							EventLogEntryType.Error, 555, 5);
						Assert.Fail ("#A1");
					} catch (ArgumentException ex) {
						// The source 'monotempsource' is not registered in log
						// 'monologother' (it is registered in log 'monologtemp').
						// The Source and Log properties must be matched, or you may
						// set Log to the empty string, and it will automatically be
						// matched to the Source property
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
						Assert.IsNotNull (ex.Message, "#A3");
						Assert.IsTrue (ex.Message.IndexOf ("'monotempsource'") != -1, "#A4");
						Assert.IsTrue (ex.Message.IndexOf ("'monologother'") != -1, "#A5");
						Assert.IsTrue (ex.Message.IndexOf ("'monologtemp'") != -1, "#A6");
						Assert.IsNull (ex.InnerException, "#A7");
					}

					// invalid type
					try {
						eventLog.WriteEntry ("WriteEntry6_Log_Mismatch2",
							(EventLogEntryType) 666, 555, 5);
						Assert.Fail ("#B1");
					} catch (InvalidEnumArgumentException) {
					}

					// invalid eventID
					try {
						eventLog.WriteEntry ("WriteEntry6_Log_Mismatch3",
							EventLogEntryType.Error, -1, 5);
						Assert.Fail ("#C1");
					} catch (ArgumentException ex) {
						// The source 'monotempsource' is not registered in log
						// 'monologother' (it is registered in log 'monologtemp').
						// The Source and Log properties must be matched, or you may
						// set Log to the empty string, and it will automatically be
						// matched to the Source property
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
						Assert.IsNotNull (ex.Message, "#C3");
						Assert.IsFalse (ex.Message.IndexOf ("'monotempsource'") != -1, "#C4");
						Assert.IsFalse (ex.Message.IndexOf ("'monologother'") != -1, "#C5");
						Assert.IsFalse (ex.Message.IndexOf ("'monologtemp'") != -1, "#C6");
						Assert.IsTrue (ex.Message.IndexOf ("'-1'") != -1, "#C7");
						Assert.IsTrue (ex.Message.IndexOf ("'0'") != -1, "#C8");
						Assert.IsTrue (ex.Message.IndexOf ("'65535'") != -1, "#C9");
						Assert.IsNull (ex.InnerException, "#C10");
					}
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");

				if (EventLog.Exists ("monologother"))
					EventLog.Delete ("monologother");
			}
		}

		[Test]
		public void WriteEntry6_Message_Empty ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					eventLog.WriteEntry (string.Empty, EventLogEntryType.FailureAudit, 888, 6);
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(6)", entry.Category, "#B3");
					Assert.AreEqual (6, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.FailureAudit, entry.EntryType, "#B7");
					Assert.AreEqual (888, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry6_Message_Null ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					eventLog.WriteEntry (null, EventLogEntryType.SuccessAudit, 343, 8);
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(8)", entry.Category, "#B3");
					Assert.AreEqual (8, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.SuccessAudit, entry.EntryType, "#B7");
					Assert.AreEqual (343, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry6_Source_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.SourceExists ("monoothersource", "."))
				Assert.Ignore ("Event log source 'monoothersource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monoothersource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					eventLog.WriteEntry ("WriteEntry1", EventLogEntryType.Warning, 2, 4);
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(4)", entry.Category, "#B3");
					Assert.AreEqual (4, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.Warning, entry.EntryType, "#B7");
					Assert.AreEqual (2, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("WriteEntry1", entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Source property was not set before writing to the event log
		public void WriteEntry6_Source_Empty ()
		{
			EventLog eventLog = new EventLog ("monologtemp");
			eventLog.WriteEntry ("test", EventLogEntryType.Information, 56, 5);
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))] // Enum argument value 666 is not valid for type. type should be a value from EventLogEntryType.
		public void WriteEntry6_Type_NotDefined ()
		{
			EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource");
			eventLog.WriteEntry ("test", (EventLogEntryType) 666, 44, 8);
		}

		[Test]
		public void WriteEntry7 ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					EventLog.WriteEntry ("monotempsource", "WriteEntry7a",
						EventLogEntryType.Information, 54);

					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(0)", entry.Category, "#B3");
					Assert.AreEqual (0, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.Information, entry.EntryType, "#B7");
					Assert.AreEqual (54, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("WriteEntry7a", entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");

					EventLog.WriteEntry ("monotempsource", "WriteEntry7b"
						+ Environment.NewLine + "ok", EventLogEntryType.Error, 0);

					entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#C1");
					Assert.IsNotNull (entry.Category, "#C2");
					Assert.AreEqual ("(0)", entry.Category, "#C3");
					Assert.AreEqual (0, entry.CategoryNumber, "#C4");
					Assert.IsNotNull (entry.Data, "#C5");
					Assert.AreEqual (0, entry.Data.Length, "#C6");
					Assert.AreEqual (EventLogEntryType.Error, entry.EntryType, "#C7");
					Assert.AreEqual (0, entry.EventID, "#C8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#C9");
#endif
					Assert.IsNotNull (entry.MachineName, "#C10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#C11");
					Assert.IsNotNull (entry.ReplacementStrings, "#C12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#C13");
					Assert.AreEqual ("WriteEntry7b" + Environment.NewLine + "ok", entry.ReplacementStrings [0], "#C14");
					Assert.IsNotNull (entry.Source, "#C15");
					Assert.AreEqual ("monotempsource", entry.Source, "#C16");
					Assert.IsNull (entry.UserName, "#C17");

					EventLog.WriteEntry ("monotempsource", "WriteEntry7c"
						+ Environment.NewLine + "ok", EventLogEntryType.Error,
						ushort.MaxValue);

					entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#D1");
					Assert.IsNotNull (entry.Category, "#D2");
					Assert.AreEqual ("(0)", entry.Category, "#D3");
					Assert.AreEqual (0, entry.CategoryNumber, "#D4");
					Assert.IsNotNull (entry.Data, "#D5");
					Assert.AreEqual (0, entry.Data.Length, "#D6");
					Assert.AreEqual (EventLogEntryType.Error, entry.EntryType, "#D7");
					Assert.AreEqual (ushort.MaxValue, entry.EventID, "#D8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#D9");
#endif
					Assert.IsNotNull (entry.MachineName, "#D10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#D11");
					Assert.IsNotNull (entry.ReplacementStrings, "#D12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#D13");
					Assert.AreEqual ("WriteEntry7c" + Environment.NewLine + "ok", entry.ReplacementStrings [0], "#D14");
					Assert.IsNotNull (entry.Source, "#D15");
					Assert.AreEqual ("monotempsource", entry.Source, "#D16");
					Assert.IsNull (entry.UserName, "#D17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry7_EventID_Invalid ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			bool applicationLogExists = EventLog.Exists ("Application", ".");
			try {
				EventLog.WriteEntry ("monotempsource", "test",
					EventLogEntryType.Information, -1);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Invalid eventID value '-1'. It must be in the range between '0' and '65535'.
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsTrue (ex.Message.IndexOf ("'-1'") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'0'") != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf ("'65535'") != -1, "#A6");
				Assert.IsNull (ex.InnerException, "#A7");
#if NET_2_0
				if (!applicationLogExists)
					Assert.IsFalse (EventLog.Exists ("Application"), "#A8");
				Assert.IsFalse (EventLog.SourceExists ("monotempsource"), "#A9");
				Assert.AreEqual (string.Empty, EventLog.LogNameFromSourceName ("monotempsource", "."), "#A10");
#else
				Assert.IsTrue (EventLog.Exists ("Application"), "#A8");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A9");
				Assert.AreEqual ("Application", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A10");
#endif
			} finally {
				if (!applicationLogExists) {
					if (EventLog.Exists ("Application", "."))
						EventLog.Delete ("Application", ".");
				} else {
					if (EventLog.SourceExists ("monotempsource", "."))
						EventLog.DeleteEventSource ("monotempsource", ".");
				}
			}

			try {
				EventLog.WriteEntry ("monotempsource", "test",
					EventLogEntryType.Information, 65536);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Invalid eventID value '65536'. It must be in the range between '0' and '65535'.
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf ("'65536'") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'0'") != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf ("'65535'") != -1, "#B6");
				Assert.IsNull (ex.InnerException, "#B7");
#if NET_2_0
				if (!applicationLogExists)
					Assert.IsFalse (EventLog.Exists ("Application"), "#B8");
				Assert.IsFalse (EventLog.SourceExists ("monotempsource"), "#B9");
				Assert.AreEqual (string.Empty, EventLog.LogNameFromSourceName ("monotempsource", "."), "#B10");
#else
				Assert.IsTrue (EventLog.Exists ("Application"), "#B8");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#B9");
				Assert.AreEqual ("Application", EventLog.LogNameFromSourceName ("monotempsource", "."), "#B10");
#endif
			} finally {
				if (!applicationLogExists) {
					if (EventLog.Exists ("Application", "."))
						EventLog.Delete ("Application", ".");
				} else {
					if (EventLog.SourceExists ("monotempsource", "."))
						EventLog.DeleteEventSource ("monotempsource", ".");
				}
			}
		}

		[Test]
		public void WriteEntry7_Message_Empty ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				EventLog.WriteEntry ("monotempsource", string.Empty,
					EventLogEntryType.Error, 56);

				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#A1");
					Assert.IsNotNull (entry.Category, "#A2");
					Assert.AreEqual ("(0)", entry.Category, "#A3");
					Assert.AreEqual (0, entry.CategoryNumber, "#A4");
					Assert.IsNotNull (entry.Data, "#A5");
					Assert.AreEqual (0, entry.Data.Length, "#A6");
					Assert.AreEqual (EventLogEntryType.Error, entry.EntryType, "#A7");
					Assert.AreEqual (56, entry.EventID, "#A8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#A9");
#endif
					Assert.IsNotNull (entry.MachineName, "#A10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#A11");
					Assert.IsNotNull (entry.ReplacementStrings, "#A12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#A13");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [0], "#A14");
					Assert.IsNotNull (entry.Source, "#A15");
					Assert.AreEqual ("monotempsource", entry.Source, "#A16");
					Assert.IsNull (entry.UserName, "#A17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry7_Message_Null ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				EventLog.WriteEntry ("monotempsource", null,
					EventLogEntryType.FailureAudit, 76);

				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#A1");
					Assert.IsNotNull (entry.Category, "#A2");
					Assert.AreEqual ("(0)", entry.Category, "#A3");
					Assert.AreEqual (0, entry.CategoryNumber, "#A4");
					Assert.IsNotNull (entry.Data, "#A5");
					Assert.AreEqual (0, entry.Data.Length, "#A6");
					Assert.AreEqual (EventLogEntryType.FailureAudit, entry.EntryType, "#A7");
					Assert.AreEqual (76, entry.EventID, "#A8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#A9");
#endif
					Assert.IsNotNull (entry.MachineName, "#A10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#A11");
					Assert.IsNotNull (entry.ReplacementStrings, "#A12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#A13");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [0], "#A14");
					Assert.IsNotNull (entry.Source, "#A15");
					Assert.AreEqual ("monotempsource", entry.Source, "#A16");
					Assert.IsNull (entry.UserName, "#A17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry7_Source_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			bool applicationLogExists = EventLog.Exists ("Application");
			try {
				EventLog.WriteEntry ("monotempsource", "test",
					EventLogEntryType.SuccessAudit, 89);

				Assert.IsTrue (EventLog.Exists ("Application"), "#A1");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A2");
				Assert.AreEqual ("Application", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A3");

				if (EventLogImplType == WIN32_IMPL)
					// win32 API does not return entries in order for
					// Application log
					return;

				using (EventLog eventLog = new EventLog ("Application", ".", "monotempsource")) {
					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(0)", entry.Category, "#B3");
					Assert.AreEqual (0, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.SuccessAudit, entry.EntryType, "#B7");
					Assert.AreEqual (89, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("test", entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");
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
		[ExpectedException (typeof (ArgumentException))] // Source property was not set before writing to the event log
		public void WriteEntry7_Source_Empty ()
		{
			EventLog.WriteEntry (string.Empty, "test", EventLogEntryType.Warning, 5);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Source property was not set before writing to the event log
		public void WriteEntry7_Source_Null ()
		{
			EventLog.WriteEntry (null, "test", EventLogEntryType.Error, 5);
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))] // Enum argument value 666 is not valid for type. type should be a value from EventLogEntryType.
		public void WriteEntry7_Type_NotDefined ()
		{
			EventLog.WriteEntry ("monotempsource", "test", (EventLogEntryType) 666, 4);
		}

		[Test]
		public void WriteEntry8 ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					byte [] data = new byte [] { 56, 55, 23, 24 };

					eventLog.WriteEntry ("WriteEntry8a", EventLogEntryType.Information, 56, 3, data);

					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(3)", entry.Category, "#B3");
					Assert.AreEqual (3, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (data, entry.Data, "#B6");
					Assert.AreEqual (EventLogEntryType.Information, entry.EntryType, "#B7");
					Assert.AreEqual (56, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("WriteEntry8a", entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");

					eventLog.WriteEntry ("WriteEntry8b" + Environment.NewLine + "ok",
						EventLogEntryType.Error, 0, 0, new byte [0]);

					entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#C1");
					Assert.IsNotNull (entry.Category, "#C2");
					Assert.AreEqual ("(0)", entry.Category, "#C3");
					Assert.AreEqual (0, entry.CategoryNumber, "#C4");
					Assert.IsNotNull (entry.Data, "#C5");
					Assert.AreEqual (0, entry.Data.Length, "#C6");
					Assert.AreEqual (EventLogEntryType.Error, entry.EntryType, "#C7");
					Assert.AreEqual (0, entry.EventID, "#C8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#C9");
#endif
					Assert.IsNotNull (entry.MachineName, "#C10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#C11");
					Assert.IsNotNull (entry.ReplacementStrings, "#C12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#C13");
					Assert.AreEqual ("WriteEntry8b" + Environment.NewLine + "ok", entry.ReplacementStrings [0], "#C14");
					Assert.IsNotNull (entry.Source, "#C15");
					Assert.AreEqual ("monotempsource", entry.Source, "#C16");
					Assert.IsNull (entry.UserName, "#C17");

					eventLog.WriteEntry ("WriteEntry8c", EventLogEntryType.Error,
						ushort.MaxValue, short.MaxValue, null);

					entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#D1");
					Assert.IsNotNull (entry.Category, "#D2");
					Assert.AreEqual ("(32767)", entry.Category, "#D3");
					Assert.AreEqual (short.MaxValue, entry.CategoryNumber, "#D4");
					Assert.IsNotNull (entry.Data, "#D5");
					Assert.AreEqual (0, entry.Data.Length, "#D6");
					Assert.AreEqual (EventLogEntryType.Error, entry.EntryType, "#D7");
					Assert.AreEqual (ushort.MaxValue, entry.EventID, "#D8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#D9");
#endif
					Assert.IsNotNull (entry.MachineName, "#D10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#D11");
					Assert.IsNotNull (entry.ReplacementStrings, "#D12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#D13");
					Assert.AreEqual ("WriteEntry8c", entry.ReplacementStrings [0], "#D14");
					Assert.IsNotNull (entry.Source, "#D15");
					Assert.AreEqual ("monotempsource", entry.Source, "#D16");
					Assert.IsNull (entry.UserName, "#D17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry8_EventID_Invalid ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource");

			try {
				eventLog.WriteEntry ("test", EventLogEntryType.Information, -1,
					5, new byte [0]);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Invalid eventID value '-1'. It must be in the range between '0' and '65535'.
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsTrue (ex.Message.IndexOf ("'-1'") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'0'") != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf ("'65535'") != -1, "#A6");
				Assert.IsNull (ex.InnerException, "#A7");
#if NET_2_0
				Assert.IsFalse (EventLog.Exists ("monologtemp"), "#A8");
				Assert.IsFalse (EventLog.SourceExists ("monotempsource"), "#A9");
				Assert.AreEqual (string.Empty, EventLog.LogNameFromSourceName ("monotempsource", "."), "#A10");
#else
				Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A8");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A9");
				Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A10");
#endif
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}

			try {
				eventLog.WriteEntry ("test", EventLogEntryType.Information, 65536,
					5, new byte [0]);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Invalid eventID value '65536'. It must be in the range between '0' and '65535'.
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf ("'65536'") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'0'") != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf ("'65535'") != -1, "#B6");
				Assert.IsNull (ex.InnerException, "#B7");
#if NET_2_0
				Assert.IsFalse (EventLog.Exists ("monologtemp"), "#B8");
				Assert.IsFalse (EventLog.SourceExists ("monotempsource"), "#B9");
				Assert.AreEqual (string.Empty, EventLog.LogNameFromSourceName ("monotempsource", "."), "#B10");
#else
				Assert.IsTrue (EventLog.Exists ("monologtemp"), "#B8");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#B9");
				Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#B10");
#endif
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry8_Log_Empty ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			bool applicationLogExists = EventLog.Exists ("Application", ".");

			// specified source does not exist, so use Application log
			try {
				using (EventLog eventLog = new EventLog (string.Empty, ".", "monotempsource")) {
					eventLog.WriteEntry ("WriteEntry1_Log_Empty", EventLogEntryType.Error, 555, 5, new byte [0]);
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("Application", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("Application"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("Application", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");
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
		public void WriteEntry8_Log_Mismatch ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			if (EventLog.Exists ("monologother", "."))
				Assert.Ignore ("Event log 'monologother' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologother", ".", "monotempsource")) {
					// valid message
					try {
						eventLog.WriteEntry ("WriteEntry8_Log_Mismatch1",
							EventLogEntryType.Error, 555, 5, new byte [0]);
						Assert.Fail ("#A1");
					} catch (ArgumentException ex) {
						// The source 'monotempsource' is not registered in log
						// 'monologother' (it is registered in log 'monologtemp').
						// The Source and Log properties must be matched, or you may
						// set Log to the empty string, and it will automatically be
						// matched to the Source property
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
						Assert.IsNotNull (ex.Message, "#A3");
						Assert.IsTrue (ex.Message.IndexOf ("'monotempsource'") != -1, "#A4");
						Assert.IsTrue (ex.Message.IndexOf ("'monologother'") != -1, "#A5");
						Assert.IsTrue (ex.Message.IndexOf ("'monologtemp'") != -1, "#A6");
						Assert.IsNull (ex.InnerException, "#A7");
					}

					// invalid type
					try {
						eventLog.WriteEntry ("WriteEntry8_Log_Mismatch2",
							(EventLogEntryType) 666, 555, 5, new byte [0]);
						Assert.Fail ("#B1");
					} catch (InvalidEnumArgumentException) {
					}

					// invalid eventID
					try {
						eventLog.WriteEntry ("WriteEntry8_Log_Mismatch3",
							EventLogEntryType.Error, -1, 5, new byte [0]);
						Assert.Fail ("#C1");
					} catch (ArgumentException ex) {
						// The source 'monotempsource' is not registered in log
						// 'monologother' (it is registered in log 'monologtemp').
						// The Source and Log properties must be matched, or you may
						// set Log to the empty string, and it will automatically be
						// matched to the Source property
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
						Assert.IsNotNull (ex.Message, "#C3");
						Assert.IsFalse (ex.Message.IndexOf ("'monotempsource'") != -1, "#C4");
						Assert.IsFalse (ex.Message.IndexOf ("'monologother'") != -1, "#C5");
						Assert.IsFalse (ex.Message.IndexOf ("'monologtemp'") != -1, "#C6");
						Assert.IsTrue (ex.Message.IndexOf ("'-1'") != -1, "#C7");
						Assert.IsTrue (ex.Message.IndexOf ("'0'") != -1, "#C8");
						Assert.IsTrue (ex.Message.IndexOf ("'65535'") != -1, "#C9");
						Assert.IsNull (ex.InnerException, "#C10");
					}
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");

				if (EventLog.Exists ("monologother"))
					EventLog.Delete ("monologother");
			}
		}

		[Test]
		public void WriteEntry8_Message_Empty ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					eventLog.WriteEntry (string.Empty, EventLogEntryType.FailureAudit, 888, 6, new byte [0]);
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(6)", entry.Category, "#B3");
					Assert.AreEqual (6, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.FailureAudit, entry.EntryType, "#B7");
					Assert.AreEqual (888, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry8_Message_Null ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					eventLog.WriteEntry (null, EventLogEntryType.SuccessAudit, 343, 8, new byte [0]);
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(8)", entry.Category, "#B3");
					Assert.AreEqual (8, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.SuccessAudit, entry.EntryType, "#B7");
					Assert.AreEqual (343, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry8_Source_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.SourceExists ("monoothersource", "."))
				Assert.Ignore ("Event log source 'monoothersource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monoothersource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					eventLog.WriteEntry ("WriteEntry1", EventLogEntryType.Warning, 2, 4, new byte [0]);
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(4)", entry.Category, "#B3");
					Assert.AreEqual (4, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.Warning, entry.EntryType, "#B7");
					Assert.AreEqual (2, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("WriteEntry1", entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Source property was not set before writing to the event log
		public void WriteEntry8_Source_Empty ()
		{
			EventLog eventLog = new EventLog ("monologtemp");
			eventLog.WriteEntry ("test", EventLogEntryType.Information, 56, 5, new byte [0]);
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))] // Enum argument value 666 is not valid for type. type should be a value from EventLogEntryType.
		public void WriteEntry8_Type_NotDefined ()
		{
			EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource");
			eventLog.WriteEntry ("test", (EventLogEntryType) 666, 44, 8, new byte [0]);
		}

		[Test]
		public void WriteEntry9 ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					EventLog.WriteEntry ("monotempsource", "WriteEntry9a",
						EventLogEntryType.Information, 54, 5);

					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(5)", entry.Category, "#B3");
					Assert.AreEqual (5, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.Information, entry.EntryType, "#B7");
					Assert.AreEqual (54, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("WriteEntry9a", entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");

					EventLog.WriteEntry ("monotempsource", "WriteEntry9b"
						+ Environment.NewLine + "ok", EventLogEntryType.Error,
						0, 0);

					entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#C1");
					Assert.IsNotNull (entry.Category, "#C2");
					Assert.AreEqual ("(0)", entry.Category, "#C3");
					Assert.AreEqual (0, entry.CategoryNumber, "#C4");
					Assert.IsNotNull (entry.Data, "#C5");
					Assert.AreEqual (0, entry.Data.Length, "#C6");
					Assert.AreEqual (EventLogEntryType.Error, entry.EntryType, "#C7");
					Assert.AreEqual (0, entry.EventID, "#C8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#C9");
#endif
					Assert.IsNotNull (entry.MachineName, "#C10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#C11");
					Assert.IsNotNull (entry.ReplacementStrings, "#C12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#C13");
					Assert.AreEqual ("WriteEntry9b" + Environment.NewLine + "ok", entry.ReplacementStrings [0], "#C14");
					Assert.IsNotNull (entry.Source, "#C15");
					Assert.AreEqual ("monotempsource", entry.Source, "#C16");
					Assert.IsNull (entry.UserName, "#C17");

					EventLog.WriteEntry ("monotempsource", "WriteEntry9c"
						+ Environment.NewLine + "ok", EventLogEntryType.Error,
						ushort.MaxValue, short.MaxValue);

					entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#D1");
					Assert.IsNotNull (entry.Category, "#D2");
					Assert.AreEqual ("(32767)", entry.Category, "#D3");
					Assert.AreEqual (32767, entry.CategoryNumber, "#D4");
					Assert.IsNotNull (entry.Data, "#D5");
					Assert.AreEqual (0, entry.Data.Length, "#D6");
					Assert.AreEqual (EventLogEntryType.Error, entry.EntryType, "#D7");
					Assert.AreEqual (ushort.MaxValue, entry.EventID, "#D8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#D9");
#endif
					Assert.IsNotNull (entry.MachineName, "#D10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#D11");
					Assert.IsNotNull (entry.ReplacementStrings, "#D12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#D13");
					Assert.AreEqual ("WriteEntry9c" + Environment.NewLine + "ok", entry.ReplacementStrings [0], "#D14");
					Assert.IsNotNull (entry.Source, "#D15");
					Assert.AreEqual ("monotempsource", entry.Source, "#D16");
					Assert.IsNull (entry.UserName, "#D17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry9_EventID_Invalid ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			bool applicationLogExists = EventLog.Exists ("Application", ".");
			try {
				EventLog.WriteEntry ("monotempsource", "test",
					EventLogEntryType.Information, -1, 5);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Invalid eventID value '-1'. It must be in the range between '0' and '65535'.
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsTrue (ex.Message.IndexOf ("'-1'") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'0'") != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf ("'65535'") != -1, "#A6");
				Assert.IsNull (ex.InnerException, "#A7");
#if NET_2_0
				if (!applicationLogExists)
					Assert.IsFalse (EventLog.Exists ("Application"), "#A8");
				Assert.IsFalse (EventLog.SourceExists ("monotempsource"), "#A9");
				Assert.AreEqual (string.Empty, EventLog.LogNameFromSourceName ("monotempsource", "."), "#A10");
#else
				Assert.IsTrue (EventLog.Exists ("Application"), "#A8");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A9");
				Assert.AreEqual ("Application", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A10");
#endif
			} finally {
				if (!applicationLogExists) {
					if (EventLog.Exists ("Application", "."))
						EventLog.Delete ("Application", ".");
				} else {
					if (EventLog.SourceExists ("monotempsource", "."))
						EventLog.DeleteEventSource ("monotempsource", ".");
				}
			}

			try {
				EventLog.WriteEntry ("monotempsource", "test",
					EventLogEntryType.Information, 65536, 5);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Invalid eventID value '65536'. It must be in the range between '0' and '65535'.
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf ("'65536'") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'0'") != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf ("'65535'") != -1, "#B6");
				Assert.IsNull (ex.InnerException, "#B7");
#if NET_2_0
				if (!applicationLogExists)
					Assert.IsFalse (EventLog.Exists ("Application"), "#B8");
				Assert.IsFalse (EventLog.SourceExists ("monotempsource"), "#B9");
				Assert.AreEqual (string.Empty, EventLog.LogNameFromSourceName ("monotempsource", "."), "#B10");
#else
				Assert.IsTrue (EventLog.Exists ("Application"), "#B8");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#B9");
				Assert.AreEqual ("Application", EventLog.LogNameFromSourceName ("monotempsource", "."), "#B10");
#endif
			} finally {
				if (!applicationLogExists) {
					if (EventLog.Exists ("Application", "."))
						EventLog.Delete ("Application", ".");
				} else {
					if (EventLog.SourceExists ("monotempsource", "."))
						EventLog.DeleteEventSource ("monotempsource", ".");
				}
			}
		}

		[Test]
		public void WriteEntry9_Message_Empty ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				EventLog.WriteEntry ("monotempsource", string.Empty,
					EventLogEntryType.Error, 56, 5);

				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#A1");
					Assert.IsNotNull (entry.Category, "#A2");
					Assert.AreEqual ("(5)", entry.Category, "#A3");
					Assert.AreEqual (5, entry.CategoryNumber, "#A4");
					Assert.IsNotNull (entry.Data, "#A5");
					Assert.AreEqual (0, entry.Data.Length, "#A6");
					Assert.AreEqual (EventLogEntryType.Error, entry.EntryType, "#A7");
					Assert.AreEqual (56, entry.EventID, "#A8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#A9");
#endif
					Assert.IsNotNull (entry.MachineName, "#A10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#A11");
					Assert.IsNotNull (entry.ReplacementStrings, "#A12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#A13");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [0], "#A14");
					Assert.IsNotNull (entry.Source, "#A15");
					Assert.AreEqual ("monotempsource", entry.Source, "#A16");
					Assert.IsNull (entry.UserName, "#A17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry9_Message_Null ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				EventLog.WriteEntry ("monotempsource", null,
					EventLogEntryType.FailureAudit, 76, 8);

				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#A1");
					Assert.IsNotNull (entry.Category, "#A2");
					Assert.AreEqual ("(8)", entry.Category, "#A3");
					Assert.AreEqual (8, entry.CategoryNumber, "#A4");
					Assert.IsNotNull (entry.Data, "#A5");
					Assert.AreEqual (0, entry.Data.Length, "#A6");
					Assert.AreEqual (EventLogEntryType.FailureAudit, entry.EntryType, "#A7");
					Assert.AreEqual (76, entry.EventID, "#A8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#A9");
#endif
					Assert.IsNotNull (entry.MachineName, "#A10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#A11");
					Assert.IsNotNull (entry.ReplacementStrings, "#A12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#A13");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [0], "#A14");
					Assert.IsNotNull (entry.Source, "#A15");
					Assert.AreEqual ("monotempsource", entry.Source, "#A16");
					Assert.IsNull (entry.UserName, "#A17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry9_Source_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			bool applicationLogExists = EventLog.Exists ("Application");
			try {
				EventLog.WriteEntry ("monotempsource", "test",
					EventLogEntryType.SuccessAudit, 89, 3);

				Assert.IsTrue (EventLog.Exists ("Application"), "#A1");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A2");
				Assert.AreEqual ("Application", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A3");

				if (EventLogImplType == WIN32_IMPL)
					// win32 API does not return entries in order for
					// Application log
					return;

				using (EventLog eventLog = new EventLog ("Application", ".", "monotempsource")) {
					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(3)", entry.Category, "#B3");
					Assert.AreEqual (3, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.SuccessAudit, entry.EntryType, "#B7");
					Assert.AreEqual (89, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("test", entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");
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
		[ExpectedException (typeof (ArgumentException))] // Source property was not set before writing to the event log
		public void WriteEntry9_Source_Empty ()
		{
			EventLog.WriteEntry (string.Empty, "test", EventLogEntryType.Warning,
				5, 4);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Source property was not set before writing to the event log
		public void WriteEntry9_Source_Null ()
		{
			EventLog.WriteEntry (null, "test", EventLogEntryType.Error, 5, 4);
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))] // Enum argument value 666 is not valid for type. type should be a value from EventLogEntryType.
		public void WriteEntry9_Type_NotDefined ()
		{
			EventLog.WriteEntry ("monotempsource", "test", (EventLogEntryType) 666, 4, 3);
		}

		[Test]
		public void WriteEntry10 ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					byte [] data = new byte [] { 56, 55, 23, 24 };

					EventLog.WriteEntry ("monotempsource", "WriteEntry9a",
						EventLogEntryType.Information, 54, 5, data);

					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(5)", entry.Category, "#B3");
					Assert.AreEqual (5, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (data, entry.Data, "#B6");
					Assert.AreEqual (EventLogEntryType.Information, entry.EntryType, "#B7");
					Assert.AreEqual (54, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("WriteEntry9a", entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");

					EventLog.WriteEntry ("monotempsource", "WriteEntry9b"
						+ Environment.NewLine + "ok", EventLogEntryType.Error,
						0, 0, new byte [0]);

					entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#C1");
					Assert.IsNotNull (entry.Category, "#C2");
					Assert.AreEqual ("(0)", entry.Category, "#C3");
					Assert.AreEqual (0, entry.CategoryNumber, "#C4");
					Assert.IsNotNull (entry.Data, "#C5");
					Assert.AreEqual (0, entry.Data.Length, "#C6");
					Assert.AreEqual (EventLogEntryType.Error, entry.EntryType, "#C7");
					Assert.AreEqual (0, entry.EventID, "#C8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#C9");
#endif
					Assert.IsNotNull (entry.MachineName, "#C10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#C11");
					Assert.IsNotNull (entry.ReplacementStrings, "#C12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#C13");
					Assert.AreEqual ("WriteEntry9b" + Environment.NewLine + "ok", entry.ReplacementStrings [0], "#C14");
					Assert.IsNotNull (entry.Source, "#C15");
					Assert.AreEqual ("monotempsource", entry.Source, "#C16");
					Assert.IsNull (entry.UserName, "#C17");

					EventLog.WriteEntry ("monotempsource", "WriteEntry9c"
						+ Environment.NewLine + "ok", EventLogEntryType.Error,
						ushort.MaxValue, short.MaxValue, null);

					entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#D1");
					Assert.IsNotNull (entry.Category, "#D2");
					Assert.AreEqual ("(32767)", entry.Category, "#D3");
					Assert.AreEqual (32767, entry.CategoryNumber, "#D4");
					Assert.IsNotNull (entry.Data, "#D5");
					Assert.AreEqual (0, entry.Data.Length, "#D6");
					Assert.AreEqual (EventLogEntryType.Error, entry.EntryType, "#D7");
					Assert.AreEqual (ushort.MaxValue, entry.EventID, "#D8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#D9");
#endif
					Assert.IsNotNull (entry.MachineName, "#D10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#D11");
					Assert.IsNotNull (entry.ReplacementStrings, "#D12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#D13");
					Assert.AreEqual ("WriteEntry9c" + Environment.NewLine + "ok", entry.ReplacementStrings [0], "#D14");
					Assert.IsNotNull (entry.Source, "#D15");
					Assert.AreEqual ("monotempsource", entry.Source, "#D16");
					Assert.IsNull (entry.UserName, "#D17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry10_EventID_Invalid ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			bool applicationLogExists = EventLog.Exists ("Application", ".");
			try {
				EventLog.WriteEntry ("monotempsource", "test",
					EventLogEntryType.Information, -1, 5, new byte[0]);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Invalid eventID value '-1'. It must be in the range between '0' and '65535'.
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsTrue (ex.Message.IndexOf ("'-1'") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'0'") != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf ("'65535'") != -1, "#A6");
				Assert.IsNull (ex.InnerException, "#A7");
#if NET_2_0
				if (!applicationLogExists)
					Assert.IsFalse (EventLog.Exists ("Application"), "#A8");
				Assert.IsFalse (EventLog.SourceExists ("monotempsource"), "#A9");
				Assert.AreEqual (string.Empty, EventLog.LogNameFromSourceName ("monotempsource", "."), "#A10");
#else
				Assert.IsTrue (EventLog.Exists ("Application"), "#A8");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A9");
				Assert.AreEqual ("Application", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A10");
#endif
			} finally {
				if (!applicationLogExists) {
					if (EventLog.Exists ("Application", "."))
						EventLog.Delete ("Application", ".");
				} else {
					if (EventLog.SourceExists ("monotempsource", "."))
						EventLog.DeleteEventSource ("monotempsource", ".");
				}
			}

			try {
				EventLog.WriteEntry ("monotempsource", "test",
					EventLogEntryType.Information, 65536, 5, new byte[0]);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Invalid eventID value '65536'. It must be in the range between '0' and '65535'.
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf ("'65536'") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'0'") != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf ("'65535'") != -1, "#B6");
				Assert.IsNull (ex.InnerException, "#B7");
#if NET_2_0
				if (!applicationLogExists)
					Assert.IsFalse (EventLog.Exists ("Application"), "#B8");
				Assert.IsFalse (EventLog.SourceExists ("monotempsource"), "#B9");
				Assert.AreEqual (string.Empty, EventLog.LogNameFromSourceName ("monotempsource", "."), "#B10");
#else
				Assert.IsTrue (EventLog.Exists ("Application"), "#B8");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#B9");
				Assert.AreEqual ("Application", EventLog.LogNameFromSourceName ("monotempsource", "."), "#B10");
#endif
			} finally {
				if (!applicationLogExists) {
					if (EventLog.Exists ("Application", "."))
						EventLog.Delete ("Application", ".");
				} else {
					if (EventLog.SourceExists ("monotempsource", "."))
						EventLog.DeleteEventSource ("monotempsource", ".");
				}
			}
		}

		[Test]
		public void WriteEntry10_Message_Empty ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				EventLog.WriteEntry ("monotempsource", string.Empty,
					EventLogEntryType.Error, 56, 5, new byte [0]);

				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#A1");
					Assert.IsNotNull (entry.Category, "#A2");
					Assert.AreEqual ("(5)", entry.Category, "#A3");
					Assert.AreEqual (5, entry.CategoryNumber, "#A4");
					Assert.IsNotNull (entry.Data, "#A5");
					Assert.AreEqual (0, entry.Data.Length, "#A6");
					Assert.AreEqual (EventLogEntryType.Error, entry.EntryType, "#A7");
					Assert.AreEqual (56, entry.EventID, "#A8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#A9");
#endif
					Assert.IsNotNull (entry.MachineName, "#A10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#A11");
					Assert.IsNotNull (entry.ReplacementStrings, "#A12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#A13");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [0], "#A14");
					Assert.IsNotNull (entry.Source, "#A15");
					Assert.AreEqual ("monotempsource", entry.Source, "#A16");
					Assert.IsNull (entry.UserName, "#A17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry10_Message_Null ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				EventLog.WriteEntry ("monotempsource", null,
					EventLogEntryType.FailureAudit, 76, 8, new byte [0]);

				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#A1");
					Assert.IsNotNull (entry.Category, "#A2");
					Assert.AreEqual ("(8)", entry.Category, "#A3");
					Assert.AreEqual (8, entry.CategoryNumber, "#A4");
					Assert.IsNotNull (entry.Data, "#A5");
					Assert.AreEqual (0, entry.Data.Length, "#A6");
					Assert.AreEqual (EventLogEntryType.FailureAudit, entry.EntryType, "#A7");
					Assert.AreEqual (76, entry.EventID, "#A8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#A9");
#endif
					Assert.IsNotNull (entry.MachineName, "#A10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#A11");
					Assert.IsNotNull (entry.ReplacementStrings, "#A12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#A13");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [0], "#A14");
					Assert.IsNotNull (entry.Source, "#A15");
					Assert.AreEqual ("monotempsource", entry.Source, "#A16");
					Assert.IsNull (entry.UserName, "#A17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEntry10_Source_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			bool applicationLogExists = EventLog.Exists ("Application");
			try {
				EventLog.WriteEntry ("monotempsource", "test",
					EventLogEntryType.SuccessAudit, 89, 3, new byte [0]);

				Assert.IsTrue (EventLog.Exists ("Application"), "#A1");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A2");
				Assert.AreEqual ("Application", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A3");

				if (EventLogImplType == WIN32_IMPL)
					// win32 API does not return entries in order for
					// Application log
					return;

				using (EventLog eventLog = new EventLog ("Application", ".", "monotempsource")) {
					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(3)", entry.Category, "#B3");
					Assert.AreEqual (3, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.SuccessAudit, entry.EntryType, "#B7");
					Assert.AreEqual (89, entry.EventID, "#B8");
#if NET_2_0
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
#endif
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("test", entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");
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
		[ExpectedException (typeof (ArgumentException))] // Source property was not set before writing to the event log
		public void WriteEntry10_Source_Empty ()
		{
			EventLog.WriteEntry (string.Empty, "test", EventLogEntryType.Warning,
				5, 4, new byte [0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Source property was not set before writing to the event log
		public void WriteEntry10_Source_Null ()
		{
			EventLog.WriteEntry (null, "test", EventLogEntryType.Error, 5, 4,
				new byte [0]);
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))] // Enum argument value 666 is not valid for type. type should be a value from EventLogEntryType.
		public void WriteEntry10_Type_NotDefined ()
		{
			EventLog.WriteEntry ("monotempsource", "test", (EventLogEntryType) 666,
				4, 3, new byte [0]);
		}

#if NET_2_0
		[Test]
		public void WriteEvent1 ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					EventInstance instance = new EventInstance (5, 666,
						EventLogEntryType.FailureAudit);
					eventLog.WriteEvent (instance, 5, "new" + Environment.NewLine + "line", true, null);

					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.IsTrue (EventLog.SourceExists ("monologtemp"), "#A6");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A7");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(666)", entry.Category, "#B3");
					Assert.AreEqual (666, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.FailureAudit, entry.EntryType, "#B7");
					Assert.AreEqual (5, entry.EventID, "#B8");
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (4, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("5", entry.ReplacementStrings [0], "#B14");
					Assert.AreEqual ("new" + Environment.NewLine + "line", entry.ReplacementStrings [1], "#B15");
					Assert.AreEqual (true.ToString (), entry.ReplacementStrings [2], "#B16");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [3], "#B17");
					Assert.IsNotNull (entry.Source, "#B18");
					Assert.AreEqual ("monotempsource", entry.Source, "#B19");
					Assert.IsNull (entry.UserName, "#B20");

					eventLog.WriteEvent (instance);

					entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#C1");
					Assert.IsNotNull (entry.Category, "#C2");
					Assert.AreEqual ("(666)", entry.Category, "#C3");
					Assert.AreEqual (666, entry.CategoryNumber, "#C4");
					Assert.IsNotNull (entry.Data, "#C5");
					Assert.AreEqual (0, entry.Data.Length, "#C6");
					Assert.AreEqual (EventLogEntryType.FailureAudit, entry.EntryType, "#C7");
					Assert.AreEqual (5, entry.EventID, "#C8");
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#C9");
					Assert.IsNotNull (entry.MachineName, "#C10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#C11");
					Assert.IsNotNull (entry.ReplacementStrings, "#C12");
					Assert.AreEqual (0, entry.ReplacementStrings.Length, "#C13");
					Assert.IsNotNull (entry.Source, "#C14");
					Assert.AreEqual ("monotempsource", entry.Source, "#C15");
					Assert.IsNull (entry.UserName, "#C16");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WriteEvent1_Instance_Null ()
		{
			using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
				eventLog.WriteEvent (null, "replace");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Source property was not set before writing to the event log
		public void WriteEvent1_Source_Empty ()
		{
			using (EventLog eventLog = new EventLog ("monologtemp")) {
				EventInstance instance = new EventInstance (5, 1,
					EventLogEntryType.Information);
				eventLog.WriteEvent (instance, "replace");
			}
		}

		[Test]
		public void WriteEvent1_Source_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.SourceExists ("monoothersource", "."))
				Assert.Ignore ("Event log source 'monoothersource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monoothersource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					EventInstance instance = new EventInstance (5, 1,
						EventLogEntryType.Error);
					eventLog.WriteEvent (instance, "replace1", "replace2");

					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A6");

					EventLogEntry entry = eventLog.Entries[eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(1)", entry.Category, "#B3");
					Assert.AreEqual (1, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.Error, entry.EntryType, "#B7");
					Assert.AreEqual (5, entry.EventID, "#B8");
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (2, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("replace1", entry.ReplacementStrings[0], "#B14");
					Assert.AreEqual ("replace2", entry.ReplacementStrings [1], "#B15");
					Assert.IsNotNull (entry.Source, "#B16");
					Assert.AreEqual ("monotempsource", entry.Source, "#B17");
					Assert.IsNull (entry.UserName, "#B18");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEvent1_Values_Null ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					EventInstance instance = new EventInstance (5, 666,
						EventLogEntryType.Warning);
					eventLog.WriteEvent (instance, (object) null);

					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.IsTrue (EventLog.SourceExists ("monologtemp"), "#A6");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A7");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(666)", entry.Category, "#B3");
					Assert.AreEqual (666, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.Warning, entry.EntryType, "#B7");
					Assert.AreEqual (5, entry.EventID, "#B8");
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEvent2 ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					byte [] data = new byte [] { 23, 54 };
					EventInstance instance = new EventInstance (5, 666,
						EventLogEntryType.FailureAudit);
					eventLog.WriteEvent (instance, data, 5, "new" + Environment.NewLine + "line", true, null);

					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.IsTrue (EventLog.SourceExists ("monologtemp"), "#A6");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A7");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(666)", entry.Category, "#B3");
					Assert.AreEqual (666, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (data, entry.Data, "#B6");
					Assert.AreEqual (EventLogEntryType.FailureAudit, entry.EntryType, "#B7");
					Assert.AreEqual (5, entry.EventID, "#B8");
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (4, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("5", entry.ReplacementStrings [0], "#B14");
					Assert.AreEqual ("new" + Environment.NewLine + "line", entry.ReplacementStrings [1], "#B15");
					Assert.AreEqual (true.ToString (), entry.ReplacementStrings [2], "#B16");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [3], "#B17");
					Assert.IsNotNull (entry.Source, "#B18");
					Assert.AreEqual ("monotempsource", entry.Source, "#B19");
					Assert.IsNull (entry.UserName, "#B20");

					eventLog.WriteEvent (instance, data);

					entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#C1");
					Assert.IsNotNull (entry.Category, "#C2");
					Assert.AreEqual ("(666)", entry.Category, "#C3");
					Assert.AreEqual (666, entry.CategoryNumber, "#C4");
					Assert.IsNotNull (entry.Data, "#C5");
					Assert.AreEqual (data, entry.Data, "#C6");
					Assert.AreEqual (EventLogEntryType.FailureAudit, entry.EntryType, "#C7");
					Assert.AreEqual (5, entry.EventID, "#C8");
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#C9");
					Assert.IsNotNull (entry.MachineName, "#C10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#C11");
					Assert.IsNotNull (entry.ReplacementStrings, "#C12");
					Assert.AreEqual (0, entry.ReplacementStrings.Length, "#C13");
					Assert.IsNotNull (entry.Source, "#C14");
					Assert.AreEqual ("monotempsource", entry.Source, "#C15");
					Assert.IsNull (entry.UserName, "#C16");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEvent2_Data_Null ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					EventInstance instance = new EventInstance (5, 444,
						EventLogEntryType.Warning);
					eventLog.WriteEvent (instance, null, "replace1", null, "replace3");

					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.IsTrue (EventLog.SourceExists ("monologtemp"), "#A6");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A7");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(444)", entry.Category, "#B3");
					Assert.AreEqual (444, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.Warning, entry.EntryType, "#B7");
					Assert.AreEqual (5, entry.EventID, "#B8");
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (3, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("replace1", entry.ReplacementStrings [0], "#B14");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [1], "#B15");
					Assert.AreEqual ("replace3", entry.ReplacementStrings [2], "#B16");
					Assert.IsNotNull (entry.Source, "#B17");
					Assert.AreEqual ("monotempsource", entry.Source, "#B18");
					Assert.IsNull (entry.UserName, "#B19");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}


		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WriteEvent2_Instance_Null ()
		{
			using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
				eventLog.WriteEvent (null, new byte [0], "replace");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Source property was not set before writing to the event log
		public void WriteEvent2_Source_Empty ()
		{
			using (EventLog eventLog = new EventLog ("monologtemp")) {
				EventInstance instance = new EventInstance (5, 1,
					EventLogEntryType.Information);
				eventLog.WriteEvent (instance, new byte [0], "replace");
			}
		}

		[Test]
		public void WriteEvent2_Source_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.SourceExists ("monoothersource", "."))
				Assert.Ignore ("Event log source 'monoothersource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monoothersource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					byte [] data = new byte [] { 23, 54 };
					EventInstance instance = new EventInstance (5, 1,
						EventLogEntryType.Error);
					eventLog.WriteEvent (instance, data, "replace1", "replace2");

					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.IsTrue (EventLog.SourceExists ("monologtemp"), "#A6");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A7");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(1)", entry.Category, "#B3");
					Assert.AreEqual (1, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (data, entry.Data, "#B6");
					Assert.AreEqual (EventLogEntryType.Error, entry.EntryType, "#B7");
					Assert.AreEqual (5, entry.EventID, "#B8");
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (2, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("replace1", entry.ReplacementStrings [0], "#B14");
					Assert.AreEqual ("replace2", entry.ReplacementStrings [1], "#B15");
					Assert.IsNotNull (entry.Source, "#B16");
					Assert.AreEqual ("monotempsource", entry.Source, "#B17");
					Assert.IsNull (entry.UserName, "#B18");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEvent2_Values_Null ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					byte [] data = new byte [] { 23, 54 };
					EventInstance instance = new EventInstance (5, 556,
						EventLogEntryType.Warning);
					eventLog.WriteEvent (instance, data, (object) null);

					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.IsTrue (EventLog.SourceExists ("monologtemp"), "#A6");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A7");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(556)", entry.Category, "#B3");
					Assert.AreEqual (556, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (data, entry.Data, "#B6");
					Assert.AreEqual (EventLogEntryType.Warning, entry.EntryType, "#B7");
					Assert.AreEqual (5, entry.EventID, "#B8");
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.IsNotNull (entry.Source, "#B14");
					Assert.AreEqual ("monotempsource", entry.Source, "#B15");
					Assert.IsNull (entry.UserName, "#B16");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEvent3 ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				EventInstance instance = new EventInstance (5, 666,
					EventLogEntryType.FailureAudit);
				EventLog.WriteEvent ("monotempsource", instance, 5, "new" 
					+ Environment.NewLine + "line", true, null);

				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.IsTrue (EventLog.SourceExists ("monologtemp"), "#A6");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A7");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(666)", entry.Category, "#B3");
					Assert.AreEqual (666, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.FailureAudit, entry.EntryType, "#B7");
					Assert.AreEqual (5, entry.EventID, "#B8");
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (4, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("5", entry.ReplacementStrings [0], "#B14");
					Assert.AreEqual ("new" + Environment.NewLine + "line", entry.ReplacementStrings [1], "#B15");
					Assert.AreEqual (true.ToString (), entry.ReplacementStrings [2], "#B16");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [3], "#B17");
					Assert.IsNotNull (entry.Source, "#B18");
					Assert.AreEqual ("monotempsource", entry.Source, "#B19");
					Assert.IsNull (entry.UserName, "#B20");

					EventLog.WriteEvent ("monotempsource", instance);

					entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#C1");
					Assert.IsNotNull (entry.Category, "#C2");
					Assert.AreEqual ("(666)", entry.Category, "#C3");
					Assert.AreEqual (666, entry.CategoryNumber, "#C4");
					Assert.IsNotNull (entry.Data, "#C5");
					Assert.AreEqual (0, entry.Data.Length, "#C6");
					Assert.AreEqual (EventLogEntryType.FailureAudit, entry.EntryType, "#C7");
					Assert.AreEqual (5, entry.EventID, "#C8");
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#C9");
					Assert.IsNotNull (entry.MachineName, "#C10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#C11");
					Assert.IsNotNull (entry.ReplacementStrings, "#C12");
					Assert.AreEqual (0, entry.ReplacementStrings.Length, "#C13");
					Assert.IsNotNull (entry.Source, "#C14");
					Assert.AreEqual ("monotempsource", entry.Source, "#C15");
					Assert.IsNull (entry.UserName, "#C16");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WriteEvent3_Instance_Null ()
		{
			EventLog.WriteEvent ("monotempsource", null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Source property was not set before writing to the event log
		public void WriteEvent3_Source_Empty ()
		{
			EventInstance instance = new EventInstance (5, 1,
				EventLogEntryType.Information);
			EventLog.WriteEvent (string.Empty, instance);
		}

		[Test]
		public void WriteEvent3_Source_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			bool applicationLogExists = EventLog.Exists ("Application");
			try {
				EventInstance instance = new EventInstance (666, 1,
					EventLogEntryType.Error);
				EventLog.WriteEvent ("monotempsource", instance, "replace1", "replace2");

				Assert.IsTrue (EventLog.Exists ("Application"), "#A1");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A2");
				Assert.AreEqual ("Application", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A3");

				if (EventLogImplType == WIN32_IMPL)
					// win32 API does not return entries in order for
					// Application log
					return;

				using (EventLog eventLog = new EventLog ("Application", ".", "monotempsource")) {
					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(1)", entry.Category, "#B3");
					Assert.AreEqual (1, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.Error, entry.EntryType, "#B7");
					Assert.AreEqual (666, entry.EventID, "#B8");
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (2, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("replace1", entry.ReplacementStrings [0], "#B14");
					Assert.AreEqual ("replace2", entry.ReplacementStrings [1], "#B15");
					Assert.IsNotNull (entry.Source, "#B16");
					Assert.AreEqual ("monotempsource", entry.Source, "#B17");
					Assert.IsNull (entry.UserName, "#B18");
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
		public void WriteEvent3_Values_Null ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				EventInstance instance = new EventInstance (5, 666,
					EventLogEntryType.Warning);
				EventLog.WriteEvent ("monotempsource", instance, (object) null);

				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.IsTrue (EventLog.SourceExists ("monologtemp"), "#A6");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A7");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(666)", entry.Category, "#B3");
					Assert.AreEqual (666, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.Warning, entry.EntryType, "#B7");
					Assert.AreEqual (5, entry.EventID, "#B8");
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEvent4 ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				byte [] data = new byte [] { 23, 54 };
				EventInstance instance = new EventInstance (5, 666,
					EventLogEntryType.FailureAudit);
				EventLog.WriteEvent ("monotempsource", instance, data, 5, "new"
					+ Environment.NewLine + "line", true, null);

				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.IsTrue (EventLog.SourceExists ("monologtemp"), "#A6");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A7");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(666)", entry.Category, "#B3");
					Assert.AreEqual (666, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (data, entry.Data, "#B6");
					Assert.AreEqual (EventLogEntryType.FailureAudit, entry.EntryType, "#B7");
					Assert.AreEqual (5, entry.EventID, "#B8");
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (4, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("5", entry.ReplacementStrings [0], "#B14");
					Assert.AreEqual ("new" + Environment.NewLine + "line", entry.ReplacementStrings [1], "#B15");
					Assert.AreEqual (true.ToString (), entry.ReplacementStrings [2], "#B16");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [3], "#B17");
					Assert.IsNotNull (entry.Source, "#B18");
					Assert.AreEqual ("monotempsource", entry.Source, "#B19");
					Assert.IsNull (entry.UserName, "#B20");

					EventLog.WriteEvent ("monotempsource", instance, data);

					entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#C1");
					Assert.IsNotNull (entry.Category, "#C2");
					Assert.AreEqual ("(666)", entry.Category, "#C3");
					Assert.AreEqual (666, entry.CategoryNumber, "#C4");
					Assert.IsNotNull (entry.Data, "#C5");
					Assert.AreEqual (data, entry.Data, "#C6");
					Assert.AreEqual (EventLogEntryType.FailureAudit, entry.EntryType, "#C7");
					Assert.AreEqual (5, entry.EventID, "#C8");
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#C9");
					Assert.IsNotNull (entry.MachineName, "#C10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#C11");
					Assert.IsNotNull (entry.ReplacementStrings, "#C12");
					Assert.AreEqual (0, entry.ReplacementStrings.Length, "#C13");
					Assert.IsNotNull (entry.Source, "#C14");
					Assert.AreEqual ("monotempsource", entry.Source, "#C15");
					Assert.IsNull (entry.UserName, "#C16");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		public void WriteEvent4_Data_Null ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				EventInstance instance = new EventInstance (5, 444,
					EventLogEntryType.Warning);
				EventLog.WriteEvent ("monotempsource", instance, null, "replace1", null, "replace3");

				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.IsTrue (EventLog.SourceExists ("monologtemp"), "#A6");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A7");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(444)", entry.Category, "#B3");
					Assert.AreEqual (444, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (0, entry.Data.Length, "#B6");
					Assert.AreEqual (EventLogEntryType.Warning, entry.EntryType, "#B7");
					Assert.AreEqual (5, entry.EventID, "#B8");
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (3, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("replace1", entry.ReplacementStrings [0], "#B14");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [1], "#B15");
					Assert.AreEqual ("replace3", entry.ReplacementStrings [2], "#B16");
					Assert.IsNotNull (entry.Source, "#B17");
					Assert.AreEqual ("monotempsource", entry.Source, "#B18");
					Assert.IsNull (entry.UserName, "#B19");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WriteEvent4_Instance_Null ()
		{
			EventLog.WriteEvent ("monotempsource", null, new byte [0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Source property was not set before writing to the event log
		public void WriteEvent4_Source_Empty ()
		{
			EventInstance instance = new EventInstance (5, 1,
				EventLogEntryType.Information);
			EventLog.WriteEvent (string.Empty, instance, new byte [0]);
		}

		[Test]
		public void WriteEvent4_Source_DoesNotExist ()
		{
			if (EventLogImplType == NULL_IMPL)
				// test cannot pass with NULL implementation
				return;

			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			bool applicationLogExists = EventLog.Exists ("Application");
			try {
				byte [] data = new byte [] { 23, 54 };
				EventInstance instance = new EventInstance (666, 1,
					EventLogEntryType.Error);
				EventLog.WriteEvent ("monotempsource", instance, data, "replace1", "replace2");

				Assert.IsTrue (EventLog.Exists ("Application"), "#A1");
				Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A2");
				Assert.AreEqual ("Application", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A3");

				if (EventLogImplType == WIN32_IMPL)
					// win32 API does not return entries in order for
					// Application log
					return;

				using (EventLog eventLog = new EventLog ("Application", ".", "monotempsource")) {
					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(1)", entry.Category, "#B3");
					Assert.AreEqual (1, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (data, entry.Data, "#B6");
					Assert.AreEqual (EventLogEntryType.Error, entry.EntryType, "#B7");
					Assert.AreEqual (666, entry.EventID, "#B8");
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (2, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual ("replace1", entry.ReplacementStrings [0], "#B14");
					Assert.AreEqual ("replace2", entry.ReplacementStrings [1], "#B15");
					Assert.IsNotNull (entry.Source, "#B16");
					Assert.AreEqual ("monotempsource", entry.Source, "#B17");
					Assert.IsNull (entry.UserName, "#B18");
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
		public void WriteEvent4_Values_Null ()
		{
			if (EventLog.SourceExists ("monotempsource", "."))
				Assert.Ignore ("Event log source 'monotempsource' should not exist.");

			if (EventLog.Exists ("monologtemp", "."))
				Assert.Ignore ("Event log 'monologtemp' should not exist.");

			EventLog.CreateEventSource ("monotempsource", "monologtemp");
			try {
				byte [] data = new byte [] { 23, 54 };
				EventInstance instance = new EventInstance (5, 666,
					EventLogEntryType.Warning);
				EventLog.WriteEvent ("monotempsource", instance, data, (object) null);

				using (EventLog eventLog = new EventLog ("monologtemp", ".", "monotempsource")) {
					// MSBUG: Assert.AreEqual (1, eventLog.Entries.Count, "#A1");
					Assert.AreEqual ("monologtemp", eventLog.Log, "#A2");
					Assert.AreEqual ("monotempsource", eventLog.Source, "#A3");
					Assert.IsTrue (EventLog.Exists ("monologtemp"), "#A4");
					Assert.IsTrue (EventLog.SourceExists ("monotempsource"), "#A5");
					Assert.IsTrue (EventLog.SourceExists ("monologtemp"), "#A6");
					Assert.AreEqual ("monologtemp", EventLog.LogNameFromSourceName ("monotempsource", "."), "#A7");

					EventLogEntry entry = eventLog.Entries [eventLog.Entries.Count - 1];
					Assert.IsNotNull (entry, "#B1");
					Assert.IsNotNull (entry.Category, "#B2");
					Assert.AreEqual ("(666)", entry.Category, "#B3");
					Assert.AreEqual (666, entry.CategoryNumber, "#B4");
					Assert.IsNotNull (entry.Data, "#B5");
					Assert.AreEqual (data, entry.Data, "#B6");
					Assert.AreEqual (EventLogEntryType.Warning, entry.EntryType, "#B7");
					Assert.AreEqual (5, entry.EventID, "#B8");
					Assert.AreEqual (entry.EventID, entry.InstanceId, "#B9");
					Assert.IsNotNull (entry.MachineName, "#B10");
					Assert.AreEqual (Environment.MachineName, entry.MachineName, "#B11");
					Assert.IsNotNull (entry.ReplacementStrings, "#B12");
					Assert.AreEqual (1, entry.ReplacementStrings.Length, "#B13");
					Assert.AreEqual (string.Empty, entry.ReplacementStrings [0], "#B14");
					Assert.IsNotNull (entry.Source, "#B15");
					Assert.AreEqual ("monotempsource", entry.Source, "#B16");
					Assert.IsNull (entry.UserName, "#B17");
				}
			} finally {
				if (EventLog.Exists ("monologtemp"))
					EventLog.Delete ("monologtemp");
			}
		}
#endif

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

		private static bool Win32EventLogEnabled {
			get {
				return (Environment.OSVersion.Platform == PlatformID.Win32NT);
			}
		}

		// IMPORTANT: keep this in sync with System.Diagnostics.EventLog.EventLogImplType
		private static string EventLogImplType {
			get {
				string implType = Environment.GetEnvironmentVariable (EVENTLOG_TYPE_VAR);
				if (implType == null) {
					if (Win32EventLogEnabled)
						return WIN32_IMPL;
					implType = NULL_IMPL;
				} else {
					if (Win32EventLogEnabled && string.Compare (implType, WIN32_IMPL, true) == 0)
						implType = WIN32_IMPL;
					else if (string.Compare (implType, NULL_IMPL, true) == 0)
						implType = NULL_IMPL;
					else if (string.Compare (implType, 0, LOCAL_FILE_IMPL, 0, LOCAL_FILE_IMPL.Length, true) == 0)
						implType = LOCAL_FILE_IMPL;
					else
						throw new NotSupportedException (string.Format (
							CultureInfo.InvariantCulture, "Eventlog implementation"
							+ " '{0}' is not supported.", implType));
				}
				return implType;
			}
		}
	}
}

#endif