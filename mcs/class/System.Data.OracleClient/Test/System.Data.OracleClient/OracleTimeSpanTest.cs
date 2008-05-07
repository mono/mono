//
// OracleTimeSpanTest.cs - NUnit Test Cases for OracleTimeSpan
//
// Author:
//      Gert Driesen  <drieseng@users.sourceforge.net>
//
// Copyright (C) 2008 Gert Driesen
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
using System.Data;
using System.Data.OracleClient;

using NUnit.Framework;

namespace MonoTests.System.Data.OracleClient
{
	[TestFixture]
	public class OracleTimeSpanTest
	{
		[SetUp]
		public void SetUp ()
		{
		}

		[TearDown]
		public void TearDown ()
		{
		}

		[Test] // ctor (Int64)
		public void Constructor1 ()
		{
			OracleTimeSpan ots;
			TimeSpan ts;
			
			ts = new TimeSpan (29, 7, 34, 58, 200);
			ots = new OracleTimeSpan (ts.Ticks);
			Assert.AreEqual (ts.Days, ots.Days, "#A1");
			Assert.AreEqual (ts.Hours, ots.Hours, "#A2");
			Assert.IsFalse (ots.IsNull, "#A3");
			Assert.AreEqual (ts.Milliseconds, ots.Milliseconds, "#A4");
			Assert.AreEqual (ts.Minutes, ots.Minutes, "#A5");
			Assert.AreEqual (ts.Seconds, ots.Seconds, "#A6");
			Assert.AreEqual (ts, ots.Value, "#A7");
			Assert.AreEqual (ts.ToString (), ots.ToString (), "#A8");

			ts = new TimeSpan (0L);
			ots = new OracleTimeSpan (0L);
			Assert.AreEqual (ts.Days, ots.Days, "#B1");
			Assert.AreEqual (ts.Hours, ots.Hours, "#B2");
			Assert.IsFalse (ots.IsNull, "#B3");
			Assert.AreEqual (ts.Milliseconds, ots.Milliseconds, "#B4");
			Assert.AreEqual (ts.Minutes, ots.Minutes, "#B5");
			Assert.AreEqual (ts.Seconds, ots.Seconds, "#B6");
			Assert.AreEqual (ts, ots.Value, "#B7");
			Assert.AreEqual (ts.ToString (), ots.ToString (), "#B8");
		}

		[Test] // ctor (OracleTimeSpan)
		public void Constructor2 ()
		{
			TimeSpan ts = new TimeSpan (29, 7, 34, 58, 200);

			OracleTimeSpan ots = new OracleTimeSpan (new OracleTimeSpan (ts));
			Assert.AreEqual (ts.Days, ots.Days, "#1");
			Assert.AreEqual (ts.Hours, ots.Hours, "#2");
			Assert.IsFalse (ots.IsNull, "#3");
			Assert.AreEqual (ts.Milliseconds, ots.Milliseconds, "#4");
			Assert.AreEqual (ts.Minutes, ots.Minutes, "#5");
			Assert.AreEqual (ts.Seconds, ots.Seconds, "#6");
			Assert.AreEqual (ts, ots.Value, "#7");
			Assert.AreEqual (ts.ToString (), ots.ToString (), "#8");
		}

		[Test] // ctor (OracleTimeSpan)
		[ExpectedException (typeof (NullReferenceException))]
		public void Constructor2_From_Null ()
		{
			new OracleTimeSpan (OracleTimeSpan.Null);
		}

		[Test] // ctor (TimeSpan)
		public void Constructor3 ()
		{
			TimeSpan ts = new TimeSpan (29, 7, 34, 58, 200);

			OracleTimeSpan ots = new OracleTimeSpan (ts);
			Assert.AreEqual (ts.Days, ots.Days, "#1");
			Assert.AreEqual (ts.Hours, ots.Hours, "#2");
			Assert.IsFalse (ots.IsNull, "#3");
			Assert.AreEqual (ts.Milliseconds, ots.Milliseconds, "#4");
			Assert.AreEqual (ts.Minutes, ots.Minutes, "#5");
			Assert.AreEqual (ts.Seconds, ots.Seconds, "#6");
			Assert.AreEqual (ts, ots.Value, "#7");
			Assert.AreEqual (ts.ToString (), ots.ToString (), "#8");
		}

		[Test]
		public void IsNull ()
		{
			Assert.IsFalse (OracleTimeSpan.MaxValue.IsNull, "#1");
			Assert.IsFalse (OracleTimeSpan.MinValue.IsNull, "#2");
			Assert.IsTrue (OracleTimeSpan.Null.IsNull, "#3");
		}

		[Test]
		public void MaxValue ()
		{
			TimeSpan ts = TimeSpan.MaxValue;
			OracleTimeSpan ots = OracleTimeSpan.MaxValue;
			Assert.AreEqual (ts.Days, ots.Days, "#1");
			Assert.AreEqual (ts.Hours, ots.Hours, "#2");
			Assert.IsFalse (ots.IsNull, "#3");
			Assert.AreEqual (ts.Milliseconds, ots.Milliseconds, "#4");
			Assert.AreEqual (ts.Minutes, ots.Minutes, "#5");
			Assert.AreEqual (ts.Seconds, ots.Seconds, "#6");
			Assert.AreEqual (ts, ots.Value, "#7");
			Assert.AreEqual (ts.ToString (), ots.ToString (), "#8");
		}

		[Test]
		public void MinValue ()
		{
			TimeSpan ts = TimeSpan.MinValue;
			OracleTimeSpan ots = OracleTimeSpan.MinValue;
			Assert.AreEqual (ts.Days, ots.Days, "#1");
			Assert.AreEqual (ts.Hours, ots.Hours, "#2");
			Assert.IsFalse (ots.IsNull, "#3");
			Assert.AreEqual (ts.Milliseconds, ots.Milliseconds, "#4");
			Assert.AreEqual (ts.Minutes, ots.Minutes, "#5");
			Assert.AreEqual (ts.Seconds, ots.Seconds, "#6");
			Assert.AreEqual (ts, ots.Value, "#7");
			Assert.AreEqual (ts.ToString (), ots.ToString (), "#8");
		}

		[Test]
		public void Null ()
		{
			OracleTimeSpan ots = OracleTimeSpan.Null;

			try {
				int days = ots.Days;
				Assert.Fail ("#A1:" + days);
			} catch (InvalidOperationException ex) {
				// The value is Null.
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("Null") != -1, "#A5");
			}

			try {
				int hours = ots.Hours;
				Assert.Fail ("#B1:" + hours);
			} catch (InvalidOperationException ex) {
				// The value is Null.
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("Null") != -1, "#B5");
			}

			try {
				int milliseconds = ots.Milliseconds;
				Assert.Fail ("#C1:" + milliseconds);
			} catch (InvalidOperationException ex) {
				// The value is Null.
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsTrue (ex.Message.IndexOf ("Null") != -1, "#C5");
			}

			try {
				int minutes = ots.Minutes;
				Assert.Fail ("#D1:" + minutes);
			} catch (InvalidOperationException ex) {
				// The value is Null.
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsTrue (ex.Message.IndexOf ("Null") != -1, "#D5");
			}

			try {
				int seconds = ots.Seconds;
				Assert.Fail ("#E1:" + seconds);
			} catch (InvalidOperationException ex) {
				// The value is Null.
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#E2");
				Assert.IsNull (ex.InnerException, "#E3");
				Assert.IsNotNull (ex.Message, "#E4");
				Assert.IsTrue (ex.Message.IndexOf ("Null") != -1, "#E5");
			}

			try {
				TimeSpan value = ots.Value;
				Assert.Fail ("#F1:" + value);
			} catch (InvalidOperationException ex) {
				// The value is Null.
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#F2");
				Assert.IsNull (ex.InnerException, "#F3");
				Assert.IsNotNull (ex.Message, "#F4");
				Assert.IsTrue (ex.Message.IndexOf ("Null") != -1, "#F5");
			}
		}

		[Test]
		public void ToStringTest ()
		{
			OracleTimeSpan ots;

			ots = OracleTimeSpan.MaxValue;
			Assert.AreEqual (TimeSpan.MaxValue.ToString (), ots.ToString (), "#1");
			ots = OracleTimeSpan.MinValue;
			Assert.AreEqual (TimeSpan.MinValue.ToString (), ots.ToString (), "#2");
			ots = OracleTimeSpan.Null;
			Assert.AreEqual ("Null", ots.ToString (), "#3");
		}
	}
}
