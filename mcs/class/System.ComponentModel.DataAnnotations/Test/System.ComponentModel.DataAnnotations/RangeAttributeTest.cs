//
// RequiredAttributeTest.cs
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

using DA = global::System.ComponentModel.DataAnnotations;

namespace MonoTests.System.ComponentModel.DataAnnotations
{
	class RangeAttributePoker : DA.RangeAttribute
	{
		public RangeAttributePoker (double min, double max)
			: base (min, max)
		{ }

		public string GetErrorMessageString ()
		{
			return ErrorMessageString;
		}
	}

	[TestFixture]
	public class RangeAttributeTest
	{
		[Test]
		public void ErrorMessage ()
		{
			var attr = new RangeAttributePoker (-10.123D, 10.123D);

			Assert.IsNull (attr.ErrorMessage, "#A1-1");
			Assert.AreEqual ("The field {0} must be between {1} and {2}.", attr.GetErrorMessageString (), "#A1-2");
		}

		[Test]
		public void Constructor_Double_Double ()
		{
			var attr = new DA.RangeAttribute (-10.123D, 10.123D);

			Assert.IsNotNull (attr.Minimum, "#A1-1");
			Assert.IsNotNull (attr.Maximum, "#A1-2");

			Assert.AreEqual (typeof (double), attr.Minimum.GetType (), "#A2-1");
			Assert.AreEqual (typeof (double), attr.Maximum.GetType (), "#A2-2");

			Assert.AreEqual (-10.123D, attr.Minimum, "#A3-1");
			Assert.AreEqual (10.123D, attr.Maximum, "#A3-2");

			Assert.IsNotNull (attr.OperandType, "#A4-1");
			Assert.AreEqual (typeof (double), attr.OperandType, "#A4-2");
		}

		[Test]
		public void Constructor_Int_Int ()
		{
			var attr = new DA.RangeAttribute (-10, 10);

			Assert.IsNotNull (attr.Minimum, "#A1-1");
			Assert.IsNotNull (attr.Maximum, "#A1-2");

			Assert.AreEqual (typeof (int), attr.Minimum.GetType (), "#A2-1");
			Assert.AreEqual (typeof (int), attr.Maximum.GetType (), "#A2-2");

			Assert.AreEqual (-10, attr.Minimum, "#A3-1");
			Assert.AreEqual (10, attr.Maximum, "#A3-2");

			Assert.IsNotNull (attr.OperandType, "#A4-1");
			Assert.AreEqual (typeof (int), attr.OperandType, "#A4-2");
		}

		[Test]
		public void Constructor_Type_String_String ()
		{
			var attr = new DA.RangeAttribute (typeof (int), "-10", "10");

			Assert.IsNotNull (attr.Minimum, "#A1-1");
			Assert.IsNotNull (attr.Maximum, "#A1-2");
			Assert.AreEqual (typeof (string), attr.Minimum.GetType (), "#A2-1");
			Assert.AreEqual (typeof (string), attr.Maximum.GetType (), "#A2-2");
			Assert.AreEqual ("-10", attr.Minimum, "#A3-1");
			Assert.AreEqual ("10", attr.Maximum, "#A3-2");
			Assert.IsNotNull (attr.OperandType, "#A4-1");
			Assert.AreEqual (typeof (int), attr.OperandType, "#A4-2");
		}

		[Test]
		//LAMESPEC: documented to throw
		public void Constructor_Type_String_String_Null_Type ()
		{
			var attr = new DA.RangeAttribute (null, "-10", "10");

			Assert.IsNull (attr.OperandType, "#A1");
		}

		[Test]
		public void IsValid ()
		{
			var attr = new DA.RangeAttribute (typeof (int), "-10", "10");

			Assert.IsTrue (attr.IsValid ("0"), "#A1-1");
			Assert.IsFalse (attr.IsValid ("12"), "#A1-2");
			Assert.IsTrue (attr.IsValid (null), "#A1-3");
			Assert.IsTrue (attr.IsValid (String.Empty), "#A1-4");
			Assert.Throws <ArgumentException> (() => {
				attr.IsValid ("zero");
			}, "#A1-5");
			Assert.IsTrue (attr.IsValid (null), "#A1-6");
			attr = new DA.RangeAttribute (typeof (int), "minus ten", "ten");
			Assert.Throws<ArgumentException> (() => {
				attr.IsValid ("0");
			}, "#A2-1");
			Assert.Throws<ArgumentException> (() => {
				attr.IsValid ("12");
			}, "#A2-2");
			Assert.Throws<ArgumentException> (() => {
				attr.IsValid ("zero");
			}, "#A2-3");

			attr = new DA.RangeAttribute (typeof (RangeAttributeTest), "-10", "10");
			Assert.Throws<InvalidOperationException> (() => {
				attr.IsValid (null);
			}, "#A3-1");

			Assert.Throws<InvalidOperationException> (() => {
				attr.IsValid (String.Empty);
			}, "#A3-2");

			Assert.Throws<InvalidOperationException> (() => {
				// The type MonoTests.System.ComponentModel.DataAnnotations.RangeAttributeTest must implement System.IComparable.
				attr.IsValid ("10");
			}, "#A3-3");

			attr = new DA.RangeAttribute (null, "-10", "10");
			Assert.Throws<InvalidOperationException> (() => {
				// The OperandType must be set when strings are used for minimum and maximum values.
				attr.IsValid ("10");
			}, "#A4");

			attr = new DA.RangeAttribute (typeof (int), null, "10");
			Assert.Throws<InvalidOperationException> (() => {
				// The minimum and maximum values must be set.
				attr.IsValid (10);
			}, "#A5-1");

			attr = new DA.RangeAttribute (typeof (int), "10", null);
			Assert.Throws<InvalidOperationException> (() => {
				// The minimum and maximum values must be set.
				attr.IsValid (10);
			}, "#A5-2");

			attr = new DA.RangeAttribute (typeof (int), "-10", "10");
			Assert.AreEqual (typeof (string), attr.Minimum.GetType (), "#A6-1");
			Assert.AreEqual (typeof (string), attr.Maximum.GetType (), "#A6-2");

			// IsValid appears to reassign Minimum and Maximum with objects of the OperandType type, converted from the original strings
			attr.IsValid (12);
			Assert.AreEqual (typeof (int), attr.Minimum.GetType (), "#A7-1");
			Assert.AreEqual (typeof (int), attr.Maximum.GetType (), "#A7-2");
		}

		[Test]
		public void FormatMessageString ()
		{
			var attr = new DA.RangeAttribute (-10, 10);

			Assert.AreEqual ("The field MyField must be between -10 and 10.", attr.FormatErrorMessage ("MyField"), "#A1");

			attr.ErrorMessage = "Param 0: {0}";
			Assert.AreEqual ("Param 0: MyField", attr.FormatErrorMessage ("MyField"), "#A2-1");
			attr.ErrorMessage = "Param 0: {0}; Param 1: {1}";
			Assert.AreEqual ("Param 0: MyField; Param 1: -10", attr.FormatErrorMessage ("MyField"), "#A2-2");
			attr.ErrorMessage = "Param 0: {0}; Param 1: {1}; Param 2: {2}";
			Assert.AreEqual ("Param 0: MyField; Param 1: -10; Param 2: 10", attr.FormatErrorMessage ("MyField"), "#A2-3");
			attr.ErrorMessage = "Param 0: {0}; Param 1: {1}; Param 2: {2}; Param 3: {3}";
			Assert.Throws<FormatException> (() => {
				attr.FormatErrorMessage ("MyField");
			}, "#A2-1");
		}
	}
}
