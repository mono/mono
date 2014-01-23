using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Collections.Generic;

using NUnit.Framework;

using Monodoc;
using Monodoc.Generators;
using Monodoc.Generators.Html;

namespace MonoTests.Monodoc.Generators
{
	[TestFixture]
	public class AvoidCDataTextReaderTest
	{
		void AssertSameInputOutput (string expected, string input)
		{
			var processed = new AvoidCDataTextReader (new StringReader (input)).ReadToEnd ();
			Assert.AreEqual (expected, processed);
		}

		[Test]
		public void NoCDataXmlTest ()
		{
			var input = @"<elements><summary>Addressbook APIs.</summary><remarks /><class name=""ABAddressBook"" fullname=""MonoTouch.AddressBook.ABAddressBook"" assembly=""monotouch""><summary>
      Provides access to the system Address Book.
    </summary></class></elements>";

			AssertSameInputOutput (input, input);
		}

		[Test]
		public void WithCDataXmlTest ()
		{
			var input = @"<elements><summary>Addressbook APIs.</summary><remarks /><class name=""ABAddressBook"" fullname=""MonoTouch.AddressBook.ABAddressBook"" assembly=""monotouch""><summary><![CDATA[
      Provides access to the system Address Book.]]>
    </summary></class></elements>";

			AssertSameInputOutput (input.Replace ("<![CDATA[", string.Empty).Replace ("]]>", string.Empty), input);
		}

		[Test]
		public void PartialCDataXmlTest ()
		{
			var input = @"<elements><summary>Addressbook APIs.</summary><remarks /><class name=""ABAddressBook"" fullname=""MonoTouch.AddressBook.ABAddressBook"" assembly=""monotouch""><summary><![CDA[
      Provides access to the system Address Book.]]>
    </summary></class></elements>";

			AssertSameInputOutput (input, input);
		}

		[Test]
		public void FinishWithPartialCDataXmlTest ()
		{
			var input = @"<elements><summary>Addressbook APIs.</summary><remarks /><class name=""ABAddressBook"" fullname=""MonoTouch.AddressBook.ABAddressBook"" assembly=""monotouch""><summary>
      Provides access to the system Address Book.
    </summary></class></elements><![CDA[";

			AssertSameInputOutput (input, input);
		}

		[Test]
		public void FinishWithCDataXmlTest ()
		{
			var input = @"<elements><summary>Addressbook APIs.</summary><remarks /><class name=""ABAddressBook"" fullname=""MonoTouch.AddressBook.ABAddressBook"" assembly=""monotouch""><summary>
      Provides access to the system Address Book.
    </summary></class></elements><![CDATA[";

			AssertSameInputOutput (input.Replace ("<![CDATA[", string.Empty), input);
		}

		[Test]
		public void EmptyInputTest ()
		{
			AssertSameInputOutput (string.Empty, string.Empty);
		}

		[Test]
		public void LimitedInputTest ()
		{
			AssertSameInputOutput ("foo", "foo");
		}
	}
}
