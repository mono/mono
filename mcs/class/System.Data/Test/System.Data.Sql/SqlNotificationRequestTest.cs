//
// SqlNotificationRequestTest.cs - NUnit Test Cases for testing
// System.Data.Sql.SqlNotificationRequest
//
// Author:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (c) 2008 Gert Driesen
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
using System.Data.Sql;

using NUnit.Framework;

namespace MonoTests.System.Data.Sql
{
	[TestFixture]
	public class SqlNotificationRequestTest
	{
		[Test] // ctor ()
		public void Constructor1 ()
		{
			SqlNotificationRequest nr = new SqlNotificationRequest ();
			Assert.IsNull (nr.Options, "#1");
			Assert.AreEqual (0, nr.Timeout, "#2");
			Assert.IsNull (nr.UserData, "#3");
		}

		[Test] // ctor (String, String, Int32)
		public void Constructor2 ()
		{
			SqlNotificationRequest nr;
			
			nr = new SqlNotificationRequest ("UD", "options", 5);
			Assert.AreEqual ("options", nr.Options, "#A1");
			Assert.AreEqual (5, nr.Timeout, "#A2");
			Assert.AreEqual ("UD", nr.UserData, "#A3");

			nr = new SqlNotificationRequest (string.Empty, " ", 0);
			Assert.AreEqual (" ", nr.Options, "#B1");
			Assert.AreEqual (0, nr.Timeout, "#B2");
			Assert.AreEqual (string.Empty, nr.UserData, "#B3");

			nr = new SqlNotificationRequest (" ", "O", int.MaxValue);
			Assert.AreEqual ("O", nr.Options, "#C1");
			Assert.AreEqual (int.MaxValue, nr.Timeout, "#C2");
			Assert.AreEqual (" ", nr.UserData, "#C3");

			nr = new SqlNotificationRequest ((string) null, "O", 7);
			Assert.AreEqual ("O", nr.Options, "#D1");
			Assert.AreEqual (7, nr.Timeout, "#D2");
			Assert.IsNull (nr.UserData, "#D3");

			nr = new SqlNotificationRequest ("UD", (string) null, 14);
			Assert.IsNull (nr.Options, "#E1");
			Assert.AreEqual (14, nr.Timeout, "#E2");
			Assert.AreEqual ("UD", nr.UserData, "#E3");

			nr = new SqlNotificationRequest (new string ('A', 0xffff), new string ('X', 0xffff), 3);
			Assert.AreEqual (new string ('X', 0xffff), nr.Options, "#F1");
			Assert.AreEqual (3, nr.Timeout, "#F2");
			Assert.AreEqual (new string ('A', 0xffff), nr.UserData, "#F3");
		}

		[Test] // ctor (String, String, Int32)
		public void Constructor2_Options_ExceedMaxLength ()
		{
			string options = new string ('X', 0x10000);
			try {
				new SqlNotificationRequest ("UD", options, 5);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("Options", ex.ParamName, "#5");
			}
		}

		[Test] // ctor (String, String, Int32)
		public void Constructor2_Timeout_Negative ()
		{
			try {
				new SqlNotificationRequest ("UD", "options", -1);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("Timeout", ex.ParamName, "#5");
			}
		}

		[Test] // ctor (String, String, Int32)
		public void Constructor2_UserData_ExceedMaxLength ()
		{
			string userData = new string ('X', 0x10000);
			try {
				new SqlNotificationRequest (userData, "options", 5);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("UserData", ex.ParamName, "#5");
			}
		}

		[Test]
		public void Options ()
		{
			SqlNotificationRequest nr = new SqlNotificationRequest ();
			nr.Options = "XYZ";
			Assert.AreEqual ("XYZ", nr.Options, "#1");
			nr.Options = null;
			Assert.IsNull (nr.Options, "#2");
			nr.Options = " \r ";
			Assert.AreEqual (" \r ", nr.Options, "#3");
			nr.Options = string.Empty;
			Assert.AreEqual (string.Empty, nr.Options, "#4");
			nr.Options = new string ('X', 0xffff);
			Assert.AreEqual (new string ('X', 0xffff), nr.Options, "#5");
		}

		[Test]
		public void Options_Value_ExceedMaxLength ()
		{
			SqlNotificationRequest nr = new SqlNotificationRequest ();
			string options = new string ('X', 0x10000);

			try {
				nr.Options = options;
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("Options", ex.ParamName, "#5");
			}
		}

		[Test]
		public void Timeout ()
		{
			SqlNotificationRequest nr = new SqlNotificationRequest ();
			nr.Timeout = 5;
			Assert.AreEqual (5, nr.Timeout, "#1");
			nr.Timeout = 0;
			Assert.AreEqual (0, nr.Timeout, "#2");
			nr.Timeout = int.MaxValue;
			Assert.AreEqual (int.MaxValue, nr.Timeout, "#3");
		}

		[Test]
		public void Timeout_Value_Negative ()
		{
			SqlNotificationRequest nr = new SqlNotificationRequest ();
			
			try {
				nr.Timeout = -1;
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("Timeout", ex.ParamName, "#5");
			}
		}

		[Test]
		public void UserData ()
		{
			SqlNotificationRequest nr = new SqlNotificationRequest ();
			nr.UserData = "XYZ";
			Assert.AreEqual ("XYZ", nr.UserData, "#1");
			nr.UserData = null;
			Assert.IsNull (nr.UserData, "#2");
			nr.UserData = " \r ";
			Assert.AreEqual (" \r ", nr.UserData, "#3");
			nr.UserData = string.Empty;
			Assert.AreEqual (string.Empty, nr.UserData, "#4");
			nr.UserData = new string ('X', 0xffff);
			Assert.AreEqual (new string ('X', 0xffff), nr.UserData, "#5");
		}

		[Test]
		public void UserData_Value_ExceedMaxLength ()
		{
			SqlNotificationRequest nr = new SqlNotificationRequest ();
			string userData = new string ('X', 0x10000);

			try {
				nr.UserData = userData;
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("UserData", ex.ParamName, "#5");
			}
		}
	}
}

