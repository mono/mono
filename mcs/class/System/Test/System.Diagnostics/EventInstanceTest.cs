//
// EventInstanceTest.cs -
// NUnit Test Cases for System.Diagnostics.EvenInstance
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

#if !MOBILE

using System;
using System.ComponentModel;
using System.Diagnostics;

using NUnit.Framework;

namespace MonoTests.System.Diagnostics
{
	[TestFixture]
	public class EventInstanceTest
	{
		[Test]
		public void Constructor1 ()
		{
			EventInstance ei = null;

			ei = new EventInstance (5, 10);
			Assert.AreEqual (10, ei.CategoryId, "#A1");
			Assert.AreEqual (5, ei.InstanceId, "#A2");
			Assert.AreEqual (EventLogEntryType.Information, ei.EntryType, "#A3");

			ei = new EventInstance (0, 0);
			Assert.AreEqual (0, ei.CategoryId, "#B1");
			Assert.AreEqual (0, ei.InstanceId, "#B2");
			Assert.AreEqual (EventLogEntryType.Information, ei.EntryType, "#B3");

			ei = new EventInstance (uint.MaxValue, ushort.MaxValue);
			Assert.AreEqual (ushort.MaxValue, ei.CategoryId, "#C1");
			Assert.AreEqual (uint.MaxValue, ei.InstanceId, "#C2");
			Assert.AreEqual (EventLogEntryType.Information, ei.EntryType, "#C3");
		}

		[Test]
		public void Constructor1_InstanceId_Invalid ()
		{
			try {
				new EventInstance (-1, 10);
				Assert.Fail ("#A1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsNull (ex.InnerException, "#A4");
			}

			try {
				new EventInstance ((long) uint.MaxValue + 1, 10);
				Assert.Fail ("#B1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsNull (ex.InnerException, "#B4");
			}
		}

		[Test]
		public void Constructor1_CategoryId_Invalid ()
		{
			try {
				new EventInstance (5, -1);
				Assert.Fail ("#A1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsNull (ex.InnerException, "#A4");
			}

			try {
				new EventInstance (5, ushort.MaxValue + 1);
				Assert.Fail ("#A1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsNull (ex.InnerException, "#A4");
			}
		}

		[Test]
		public void Constructor2 ()
		{
			EventInstance ei = null;

			ei = new EventInstance (5, 10, EventLogEntryType.Information);
			Assert.AreEqual (10, ei.CategoryId, "#A1");
			Assert.AreEqual (5, ei.InstanceId, "#A2");
			Assert.AreEqual (EventLogEntryType.Information, ei.EntryType, "#A3");

			ei = new EventInstance (0, 0, EventLogEntryType.Error);
			Assert.AreEqual (0, ei.CategoryId, "#B1");
			Assert.AreEqual (0, ei.InstanceId, "#B2");
			Assert.AreEqual (EventLogEntryType.Error, ei.EntryType, "#B3");

			ei = new EventInstance (uint.MaxValue, ushort.MaxValue,
				EventLogEntryType.Warning);
			Assert.AreEqual (ushort.MaxValue, ei.CategoryId, "#C1");
			Assert.AreEqual (uint.MaxValue, ei.InstanceId, "#C2");
			Assert.AreEqual (EventLogEntryType.Warning, ei.EntryType, "#C3");
		}

		[Test]
		public void Constructor2_InstanceId_Invalid ()
		{
			try {
				new EventInstance (-1, 10, EventLogEntryType.Error);
				Assert.Fail ("#A1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsNull (ex.InnerException, "#A4");
			}

			try {
				new EventInstance ((long) uint.MaxValue + 1, 10,
					EventLogEntryType.Error);
				Assert.Fail ("#B1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsNull (ex.InnerException, "#B4");
			}
		}

		[Test]
		public void Constructor2_CategoryId_Invalid ()
		{
			try {
				new EventInstance (5, -1, EventLogEntryType.Error);
				Assert.Fail ("#A1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsNull (ex.InnerException, "#A4");
			}

			try {
				new EventInstance (5, (int) ushort.MaxValue + 1, EventLogEntryType.Error);
				Assert.Fail ("#A1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsNull (ex.InnerException, "#A4");
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))] // Enum argument value 666 is not valid for type. type should be a value from EventLogEntryType.
		public void Constructor2_EntryType_Invalid ()
		{
			new EventInstance (5, 5, (EventLogEntryType) 666);
		}

		[Test]
		public void CategoryId ()
		{
			EventInstance ei = new EventInstance (5, 10);
			ei.CategoryId = 0;
			Assert.AreEqual (0, ei.CategoryId, "#1");
			ei.CategoryId = 5;
			Assert.AreEqual (5, ei.CategoryId, "#2");
			ei.CategoryId = ushort.MaxValue;
			Assert.AreEqual (ushort.MaxValue, ei.CategoryId, "#3");
			ei.CategoryId = 0;
			Assert.AreEqual (0, ei.CategoryId, "#4");
			Assert.AreEqual (5, ei.InstanceId, "#5");
		}

		[Test]
		public void CategoryId_Invalid ()
		{
			EventInstance ei = new EventInstance (5, 10, EventLogEntryType.Error);

			try {
				ei.CategoryId = -1;
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsNull (ex.InnerException, "#A4");
			}

			try {
				ei.CategoryId = (int) ushort.MaxValue + 1;
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsNull (ex.InnerException, "#A4");
			}

			Assert.AreEqual (10, ei.CategoryId, "#C1");
			Assert.AreEqual (EventLogEntryType.Error, ei.EntryType, "#C2");
			Assert.AreEqual (5, ei.InstanceId, "#C2");
		}

		[Test]
		public void InstanceId ()
		{
			EventInstance ei = new EventInstance (5, 10);
			ei.InstanceId = 0;
			Assert.AreEqual (0, ei.InstanceId, "#1");
			ei.InstanceId = 5;
			Assert.AreEqual (5, ei.InstanceId, "#2");
			ei.InstanceId = uint.MaxValue;
			Assert.AreEqual (uint.MaxValue, ei.InstanceId, "#3");
			ei.InstanceId = 0;
			Assert.AreEqual (0, ei.InstanceId, "#4");
			Assert.AreEqual (10, ei.CategoryId, "#5");
		}

		[Test]
		public void InstanceId_Invalid ()
		{
			EventInstance ei = new EventInstance (5, 10, EventLogEntryType.Error);

			try {
				ei.InstanceId = -1;
				Assert.Fail ("#A1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsNull (ex.InnerException, "#A4");
			}

			try {
				ei.InstanceId = (long) uint.MaxValue + 1;
				Assert.Fail ("#B1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsNull (ex.InnerException, "#B4");
			}

			Assert.AreEqual (10, ei.CategoryId, "#C1");
			Assert.AreEqual (EventLogEntryType.Error, ei.EntryType, "#C2");
			Assert.AreEqual (5, ei.InstanceId, "#C3");
		}

		[Test]
		public void EntryType ()
		{
			EventInstance ei = new EventInstance (5, 10, EventLogEntryType.Error);
			ei.EntryType = EventLogEntryType.Warning;
			Assert.AreEqual (EventLogEntryType.Warning, ei.EntryType, "#1");
			ei.EntryType = EventLogEntryType.FailureAudit;
			Assert.AreEqual (EventLogEntryType.FailureAudit, ei.EntryType, "#2");
			ei.EntryType = EventLogEntryType.SuccessAudit;
			Assert.AreEqual (EventLogEntryType.SuccessAudit, ei.EntryType, "#3");
			ei.EntryType = EventLogEntryType.Information;
			Assert.AreEqual (EventLogEntryType.Information, ei.EntryType, "#4");
			Assert.AreEqual (5, ei.InstanceId, "#5");
			Assert.AreEqual (10, ei.CategoryId, "#6");
		}

		[Test]
		public void EntryType_Invalid ()
		{
			EventInstance ei = new EventInstance (5, 10, EventLogEntryType.Error);

			try {
				ei.EntryType = (EventLogEntryType) 666;
				Assert.Fail ("#A1");
			} catch (InvalidEnumArgumentException ex) {
				// The value of argument 'value' (666) is invalid for Enum type
				// 'EventLogEntryType'
				Assert.AreEqual (typeof (InvalidEnumArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsTrue (ex.Message.IndexOf ("666") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("value") != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf ("EventLogEntryType") != -1, "#A6");
				Assert.IsNull (ex.InnerException, "#A7");
			}

			try {
				ei.EntryType = new EventLogEntryType ();
				Assert.Fail ("#B1");
			} catch (InvalidEnumArgumentException ex) {
				// The value of argument 'value' (0) is invalid for Enum type
				// 'EventLogEntryType'
				Assert.AreEqual (typeof (InvalidEnumArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf ("0") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("value") != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf ("EventLogEntryType") != -1, "#B6");
				Assert.IsNull (ex.InnerException, "#B7");
			}

			Assert.AreEqual (10, ei.CategoryId, "#C1");
			Assert.AreEqual (EventLogEntryType.Error, ei.EntryType, "#C2");
			Assert.AreEqual (5, ei.InstanceId, "#C3");
		}
	}
}
#endif
