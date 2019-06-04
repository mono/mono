//
// StringLengthAttributeTest.cs
//
// Authors:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc. (http://novell.com/)
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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel.DataAnnotations
{
	[TestFixture]
	public class RegularExpressionAttributeTest
	{
		class RegularExpressionAttributePoker : RegularExpressionAttribute
		{
			public RegularExpressionAttributePoker (string pattern)
				: base(pattern)
			{ }

			public string GetErrorMessageString ()
			{
				return ErrorMessageString;
			}
		}

		[Test]
		public void Constructor ()
		{
			var rea = new RegularExpressionAttributePoker (@"[A-Za-z]");
			Assert.AreEqual (@"[A-Za-z]", rea.Pattern, "Patterns not saved correctly.");
			Assert.AreEqual (null, rea.ErrorMessage, "Error message not null when not yet matched.");
			Assert.AreEqual ("The field {0} must match the regular expression '{1}'.", rea.GetErrorMessageString (), "Error message not valid.");
		}

		[Test]
		public void FormatMessageString ()
		{
			var rea = new RegularExpressionAttributePoker (@"[A-Za-z]");

			Assert.AreEqual ("The field MyField must match the regular expression '[A-Za-z]'.", 
				rea.FormatErrorMessage ("MyField"), 
				"Error message not correctly formatted.");

			rea.ErrorMessage = "Param 0: {0}";
			Assert.AreEqual ("Param 0: MyField", rea.FormatErrorMessage ("MyField"), "Error message not correctly updated.");

			rea.ErrorMessage = "Param 0: {0}; Param 1: {1}";
			Assert.AreEqual ("Param 0: MyField; Param 1: [A-Za-z]", rea.FormatErrorMessage ("MyField"), "Error message not correctly updated.");
			Assert.AreEqual ("Param 0: ; Param 1: [A-Za-z]", rea.FormatErrorMessage (null), "Error message fails on null value.");
		}

		[Test]
		public void IsValid ()
		{
			var rea = new RegularExpressionAttributePoker (@"[A-Za-z]");

			Assert.IsTrue (rea.IsValid (null), "Null does not match [A-Za-z].");
			Assert.IsTrue (rea.IsValid ("A"), "'A' does not match [A-Za-z].");
			Assert.IsTrue (rea.IsValid ("a"), "'a' does not match [A-Za-z].");
			Assert.IsFalse (rea.IsValid ("Bz"), "'Bz' does not match [A-Za-z].");
			Assert.IsFalse (rea.IsValid ("string"), "'string' does not match [A-Za-z].");
			Assert.IsTrue (rea.IsValid (String.Empty), "Empty string matches [A-Za-z].");
			Assert.IsFalse (rea.IsValid ("0123456789"), "'0123456789' matches [A-Za-z].");
			Assert.IsFalse (rea.IsValid ("0123456789"), "'0123456789A' matches [A-Za-z].");
			Assert.IsFalse (rea.IsValid (123), "Casting does not fails");
			Assert.IsFalse (rea.IsValid (DateTime.Now), "Casting does not fails");

			rea = new RegularExpressionAttributePoker ("");

			Assert.Throws<InvalidOperationException> (() => {
				rea.IsValid (null);
			}, "null does not match empty pattern");

			Assert.Throws<InvalidOperationException> (() => {
				rea.IsValid (String.Empty);
			}, "empty string does not match empty pattern");

			Assert.Throws<InvalidOperationException> (() => {
				rea.IsValid ("string");
			}, "'string' does not match empty pattern");
			
			rea = new RegularExpressionAttributePoker (null);
		}
	}
}
