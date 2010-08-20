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
using MonoTests.Common;

namespace MonoTests.System.ComponentModel.DataAnnotations
{
	[TestFixture]
	public class StringLengthAttributeTest
	{
		class StringLengthAttributePoker : StringLengthAttribute
		{
			public StringLengthAttributePoker (int maximumLength)
				: base (maximumLength)
			{ }

			public string GetErrorMessageString ()
			{
				return ErrorMessageString;
			}
		}

		[Test]
		public void Constructor ()
		{
			var sla = new StringLengthAttributePoker (10);

			Assert.AreEqual (10, sla.MaximumLength, "#A1-1");
			Assert.AreEqual (null, sla.ErrorMessage, "#A1-2");
			Assert.AreEqual ("The field {0} must be a string with a maximum length of {1}.", sla.GetErrorMessageString (), "#A1-3");
#if NET_4_0
			Assert.AreEqual (0, sla.MinimumLength, "#A1-4");

			sla = new StringLengthAttributePoker (-10);
			Assert.AreEqual (-10, sla.MaximumLength, "#B1");
#else
			AssertExtensions.Throws<ArgumentOutOfRangeException> (() => {
				sla = new StringLengthAttributePoker (-10);
			}, "#B1");
#endif
			sla = new StringLengthAttributePoker (0);
			Assert.AreEqual (0, sla.MaximumLength, "#C1");
		}

		[Test]
		public void FormatMessageString ()
		{
			var sla = new StringLengthAttributePoker (10);

			Assert.AreEqual ("The field MyField must be a string with a maximum length of 10.", sla.FormatErrorMessage ("MyField"), "#A1-1");
#if !NET_4_0
			sla = new StringLengthAttributePoker (10);
#endif
			sla.ErrorMessage = "Param 0: {0}";
			Assert.AreEqual ("Param 0: MyField", sla.FormatErrorMessage ("MyField"), "#A1-2");
#if !NET_4_0
			sla = new StringLengthAttributePoker (10);
#endif
			sla.ErrorMessage = "Param 0: {0}; Param 1: {1}";
			Assert.AreEqual ("Param 0: MyField; Param 1: 10", sla.FormatErrorMessage ("MyField"), "#A1-2");
			Assert.AreEqual ("Param 0: ; Param 1: 10", sla.FormatErrorMessage (null), "#A1-3");
		}

		[Test]
		public void IsValid ()
		{
			var sla = new StringLengthAttributePoker (10);

			Assert.IsTrue (sla.IsValid (null), "#A1-1");
			Assert.IsTrue (sla.IsValid (String.Empty), "#A1-2");
			Assert.IsTrue (sla.IsValid ("string"), "#A1-3");
			Assert.IsTrue (sla.IsValid ("0123456789"), "#A1-4");
			Assert.IsFalse (sla.IsValid ("0123456789A"), "#A1-5");
			AssertExtensions.Throws<InvalidCastException> (() => {
				sla.IsValid (123);
			}, "#A1-6");
			AssertExtensions.Throws<InvalidCastException> (() => {
				sla.IsValid (DateTime.Now);
			}, "#A1-7");

			sla = new StringLengthAttributePoker (0);
			Assert.IsTrue (sla.IsValid (null), "#B1-1");
			Assert.IsTrue (sla.IsValid (String.Empty), "#B1-2");
			Assert.IsFalse (sla.IsValid ("string"), "#B1-3");
#if NET_4_0
			sla = new StringLengthAttributePoker (-10);
			AssertExtensions.Throws <InvalidOperationException> (() => {
				sla.IsValid ("123");
			}, "#C1-1");

			sla = new StringLengthAttributePoker (10);
			sla.MinimumLength = 20;
			AssertExtensions.Throws<InvalidOperationException> (() => {
				sla.IsValid ("123");
			}, "#C1-2");

			sla.MinimumLength = 5;
			Assert.IsFalse (sla.IsValid ("123"), "#C2-1");
			Assert.IsTrue (sla.IsValid ("12345"), "#C2-2");
#endif
		}
	}
}
