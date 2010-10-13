//
// MailAddressTest.cs - NUnit Test Cases for System.Net.MailAddress.MailAddress
//
// Authors:
//   John Luke (john.luke@gmail.com)
//
// (C) 2005 John Luke
//
#if NET_2_0
using NUnit.Framework;
using System;
using System.Net.Mail;

namespace MonoTests.System.Net.Mail
{
	[TestFixture]
	public class MailAddressTest
	{
		MailAddress address;

		[SetUp]
		public void GetReady ()
		{
			address = new MailAddress ("foo@example.com", "Mr. Foo Bar");
		}

		[Test]
		public void Constructor0 ()
		{
			address = new MailAddress (" foo@example.com ");
			Assert.AreEqual ("foo@example.com", address.Address, "#A1");
			Assert.AreEqual (string.Empty, address.DisplayName, "#A2");
			Assert.AreEqual ("example.com", address.Host, "#A3");
			Assert.AreEqual ("foo@example.com", address.ToString (), "#A4");
			Assert.AreEqual ("foo", address.User, "#A5");

			address = new MailAddress ("Mr. Foo Bar <foo@example.com>");
			Assert.AreEqual ("foo@example.com", address.Address, "#B1");
			Assert.AreEqual ("Mr. Foo Bar", address.DisplayName, "#B2");
			Assert.AreEqual ("example.com", address.Host, "#B3");
			Assert.AreEqual ("\"Mr. Foo Bar\" <foo@example.com>", address.ToString (), "#B4");
			Assert.AreEqual ("foo", address.User, "#B5");

			address = new MailAddress ("Mr. F@@ Bar <foo@example.com> Whatever@You@Want");
			Assert.AreEqual ("foo@example.com", address.Address, "#C1");
			Assert.AreEqual ("Mr. F@@ Bar", address.DisplayName, "#C2");
			Assert.AreEqual ("example.com", address.Host, "#C3");
			Assert.AreEqual ("\"Mr. F@@ Bar\" <foo@example.com>", address.ToString (), "#C4");
			Assert.AreEqual ("foo", address.User, "#C5");

			address = new MailAddress ("\"Mr. F@@ Bar\" <foo@example.com> Whatever@You@Want");
			Assert.AreEqual ("foo@example.com", address.Address, "#D1");
			Assert.AreEqual ("Mr. F@@ Bar", address.DisplayName, "#D2");
			Assert.AreEqual ("example.com", address.Host, "#D3");
			Assert.AreEqual ("\"Mr. F@@ Bar\" <foo@example.com>", address.ToString (), "#D4");
			Assert.AreEqual ("foo", address.User, "#D5");

			address = new MailAddress ("FooBar <foo@example.com>");
			Assert.AreEqual ("foo@example.com", address.Address, "#E1");
			Assert.AreEqual ("FooBar", address.DisplayName, "#E2");
			Assert.AreEqual ("example.com", address.Host, "#E3");
			Assert.AreEqual ("\"FooBar\" <foo@example.com>", address.ToString (), "#E4");
			Assert.AreEqual ("foo", address.User, "#E5");

			address = new MailAddress ("\"FooBar\"foo@example.com   ");
			Assert.AreEqual ("foo@example.com", address.Address, "#F1");
			Assert.AreEqual ("FooBar", address.DisplayName, "#F2");
			Assert.AreEqual ("example.com", address.Host, "#F3");
			Assert.AreEqual ("\"FooBar\" <foo@example.com>", address.ToString (), "#F4");
			Assert.AreEqual ("foo", address.User, "#F5");

			address = new MailAddress ("\"   FooBar   \"< foo@example.com >");
			Assert.AreEqual ("foo@example.com", address.Address, "#G1");
			Assert.AreEqual ("FooBar", address.DisplayName, "#G2");
			Assert.AreEqual ("example.com", address.Host, "#G3");
			Assert.AreEqual ("\"FooBar\" <foo@example.com>", address.ToString (), "#G4");
			Assert.AreEqual ("foo", address.User, "#G5");

			address = new MailAddress ("<foo@example.com>");
			Assert.AreEqual ("foo@example.com", address.Address, "#H1");
			Assert.AreEqual (string.Empty, address.DisplayName, "#H2");
			Assert.AreEqual ("example.com", address.Host, "#H3");
			Assert.AreEqual ("foo@example.com", address.ToString (), "#H4");
			Assert.AreEqual ("foo", address.User, "#H5");

			address = new MailAddress ("    <  foo@example.com  >");
			Assert.AreEqual ("foo@example.com", address.Address, "#H1");
			Assert.AreEqual (string.Empty, address.DisplayName, "#H2");
			Assert.AreEqual ("example.com", address.Host, "#H3");
			Assert.AreEqual ("foo@example.com", address.ToString (), "#H4");
			Assert.AreEqual ("foo", address.User, "#H5");
		}

		[Test]
		public void Constructor0_Address_Null ()
		{
			try {
				new MailAddress ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Constructor_Address_Empty ()
		{
			new MailAddress ("");
		}

		[Test]
		public void Constructor0_Address_Invalid ()
		{
			try {
				new MailAddress ("Mr. Foo Bar");
				Assert.Fail ("#A1");
			} catch (FormatException ex) {
				// The specified string is not in the form required for an
				// e-mail address
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				new MailAddress ("foo@b@ar");
				Assert.Fail ("#B1");
			} catch (FormatException ex) {
				// The specified string is not in the form required for an
				// e-mail address
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}

			try {
				new MailAddress ("Mr. Foo Bar <foo@exa<mple.com");
				Assert.Fail ("#C1");
			} catch (FormatException ex) {
				// The specified string is not in the form required for an
				// e-mail address
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
			}

			try {
				new MailAddress ("Mr. Foo Bar <foo@example.com");
				Assert.Fail ("#D1");
			} catch (FormatException ex) {
				// The specified string is not in the form required for an
				// e-mail address
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
			}

			try {
				new MailAddress ("Mr. \"F@@ Bar\" <foo@example.com> Whatever@You@Want");
				Assert.Fail ("#E1");
			} catch (FormatException ex) {
				// The specified string is not in the form required for an
				// e-mail address
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#E2");
				Assert.IsNull (ex.InnerException, "#E3");
				Assert.IsNotNull (ex.Message, "#E4");
			}

			try {
				new MailAddress ("Mr. F@@ Bar <foo@example.com> What\"ever@You@Want");
				Assert.Fail ("#F1");
			} catch (FormatException ex) {
				// The specified string is not in the form required for an
				// e-mail address
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#F2");
				Assert.IsNull (ex.InnerException, "#F3");
				Assert.IsNotNull (ex.Message, "#F4");
			}

			try {
				new MailAddress ("\"MrFo@Bar\"");
				Assert.Fail ("#G1");
			} catch (FormatException ex) {
				// The specified string is not in the form required for an
				// e-mail address
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#G2");
				Assert.IsNull (ex.InnerException, "#G3");
				Assert.IsNotNull (ex.Message, "#G4");
			}

			try {
				new MailAddress ("\"MrFo@Bar\"<>");
				Assert.Fail ("#H1");
			} catch (FormatException ex) {
				// The specified string is not in the form required for an
				// e-mail address
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#H2");
				Assert.IsNull (ex.InnerException, "#H3");
				Assert.IsNotNull (ex.Message, "#H4");
			}
		}

		[Test]
		public void Constructor1 ()
		{
			address = new MailAddress (" foo@example.com ", (string) null);
			Assert.AreEqual ("foo@example.com", address.Address, "#A1");
			Assert.AreEqual (string.Empty, address.DisplayName, "#A2");
			Assert.AreEqual ("example.com", address.Host, "#A3");
			Assert.AreEqual ("foo@example.com", address.ToString (), "#A4");
			Assert.AreEqual ("foo", address.User, "#A5");

			address = new MailAddress ("<foo@example.com> WhatEver", " Mr. Foo Bar ");
			Assert.AreEqual ("foo@example.com", address.Address, "#B1");
			Assert.AreEqual ("Mr. Foo Bar", address.DisplayName, "#B2");
			Assert.AreEqual ("example.com", address.Host, "#B3");
			Assert.AreEqual ("\"Mr. Foo Bar\" <foo@example.com>", address.ToString (), "#B4");
			Assert.AreEqual ("foo", address.User, "#B5");

			address = new MailAddress ("Mr. F@@ Bar <foo@example.com> Whatever", "BarFoo");
			Assert.AreEqual ("foo@example.com", address.Address, "#C1");
			Assert.AreEqual ("BarFoo", address.DisplayName, "#C2");
			Assert.AreEqual ("example.com", address.Host, "#C3");
			Assert.AreEqual ("\"BarFoo\" <foo@example.com>", address.ToString (), "#C4");
			Assert.AreEqual ("foo", address.User, "#C5");

			address = new MailAddress ("Mr. F@@ Bar <foo@example.com> Whatever", string.Empty);
			Assert.AreEqual ("foo@example.com", address.Address, "#D1");
			Assert.AreEqual (string.Empty, address.DisplayName, "#D2");
			Assert.AreEqual ("example.com", address.Host, "#D3");
			Assert.AreEqual ("foo@example.com", address.ToString (), "#D4");
			Assert.AreEqual ("foo", address.User, "#D5");

			address = new MailAddress ("Mr. F@@ Bar <foo@example.com> Whatever", (string) null);
			Assert.AreEqual ("foo@example.com", address.Address, "#E1");
			Assert.AreEqual ("Mr. F@@ Bar", address.DisplayName, "#E2");
			Assert.AreEqual ("example.com", address.Host, "#E3");
			Assert.AreEqual ("\"Mr. F@@ Bar\" <foo@example.com>", address.ToString (), "#E4");
			Assert.AreEqual ("foo", address.User, "#E5");

			address = new MailAddress ("Mr. F@@ Bar <foo@example.com> Whatever", " ");
			Assert.AreEqual ("foo@example.com", address.Address, "#F1");
			Assert.AreEqual (string.Empty, address.DisplayName, "#F2");
			Assert.AreEqual ("example.com", address.Host, "#F3");
			Assert.AreEqual ("foo@example.com", address.ToString (), "#F4");
			Assert.AreEqual ("foo", address.User, "#F5");
		}

		[Test]
		public void DisplayName_Precedence ()
		{
			var ma = new MailAddress ("Hola <foo@bar.com>");
			Assert.AreEqual (ma.DisplayName, "Hola");
			ma = new MailAddress ("Hola <foo@bar.com>", "Adios");
			Assert.AreEqual (ma.DisplayName, "Adios");
			ma = new MailAddress ("Hola <foo@bar.com>", "");
			Assert.AreEqual (ma.DisplayName, "");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void Address_Invalid ()
		{
			new MailAddress ("foobar");
		}

		[Test]
		public void Address_QuoteFirst ()
		{
			new MailAddress ("\"Hola\" <foo@bar.com>");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void Address_QuoteNotFirst ()
		{
			new MailAddress ("H\"ola\" <foo@bar.com>");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void Address_NoClosingQuote ()
		{
			new MailAddress ("\"Hola <foo@bar.com>");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void Address_NoUser ()
		{
			new MailAddress ("Hola <@bar.com>");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void Address_NoUserNoHost ()
		{
			new MailAddress ("Hola <@>");
		}

		[Test]
		public void Address ()
		{
			Assert.AreEqual ("foo@example.com", address.Address);
		}

		[Test]
		public void DisplayName ()
		{
			Assert.AreEqual ("Mr. Foo Bar", address.DisplayName);
		}

		[Test]
		public void Host ()
		{
			Assert.AreEqual ("example.com", address.Host);
		}

		[Test]
		public void User ()
		{
			Assert.AreEqual ("foo", address.User);
		}

		[Test]
		public void ToStringTest ()
		{
			Assert.AreEqual ("\"Mr. Foo Bar\" <foo@example.com>", address.ToString ());
		}

		[Test]
		public void EqualsTest ()
		{
			var n = new MailAddress ("Mr. Bar <a@example.com>");
			var n2 = new MailAddress ("a@example.com", "Mr. Bar");
			Assert.AreEqual (n, n2);
		}
		[Test]
		public void EqualsTest2 ()
		{
			var n = new MailAddress ("Mr. Bar <a@example.com>");
			var n2 = new MailAddress ("MR. BAR <a@EXAMPLE.com>");
			Assert.AreEqual (n, n2);
		}
	}
}
#endif
