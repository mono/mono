//
// Authors:
//      Marek Habersack <grendel@twistedcode.net>
//
// Copyright (C) 2011 Novell, Inc. (http://novell.com/)
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
using System.ComponentModel.Design;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel.DataAnnotations
{
	[TestFixture]
	public class CustomValidationAttributeTest
	{
		[Test]
		public void Constructor ()
		{
			var attr = new CustomValidationAttribute (null, "MyMethod");
			Assert.IsNull (attr.ValidatorType, "#A1-1");
			Assert.AreEqual ("MyMethod", attr.Method, "#A1-2");

			attr = new CustomValidationAttribute (typeof (string), null);
			Assert.AreEqual (typeof (string), attr.ValidatorType, "#A2-1");
			Assert.IsNull (attr.Method, "#A2-2");

			attr = new CustomValidationAttribute (null, null);
			Assert.IsNull (attr.ValidatorType, "#A3-1");
			Assert.IsNull (attr.Method, "#A3-2");

			attr = new CustomValidationAttribute (typeof (string), "NoSuchMethod");
			Assert.AreEqual (typeof (string), attr.ValidatorType, "#A5-1");
			Assert.AreEqual ("NoSuchMethod", attr.Method, "#A5-2");
		}

		[Test]
		public void TypeId ()
		{
			var attr = new CustomValidationAttribute (null, "MyMethod");
			Assert.IsNotNull (attr.TypeId, "#A1-1");
			Assert.AreEqual (typeof (Tuple<string, Type>), attr.TypeId.GetType (), "#A1-2");

			var typeid = attr.TypeId as Tuple <string, Type>;
			Assert.IsNotNull (typeid.Item1, "#A2-1");
			Assert.AreEqual ("MyMethod", typeid.Item1, "#A2-2");
			Assert.IsNull (typeid.Item2, "#A2-3");

			attr = new CustomValidationAttribute (typeof (CustomValidationAttributeTest), "MyMethod");
			typeid = attr.TypeId as Tuple<string, Type>;
			Assert.IsNotNull (typeid.Item1, "#A3-1");
			Assert.AreEqual ("MyMethod", typeid.Item1, "#A3-2");
			Assert.IsNotNull (typeid.Item2, "#A3-3");
			Assert.AreEqual (typeof (CustomValidationAttributeTest), typeid.Item2, "#A3-4");

			var typeid2 = attr.TypeId as Tuple<string, Type>;
			Assert.IsTrue (Object.ReferenceEquals (typeid, typeid2), "#A4");
		}

		[Test]
		public void FormatErrorMessage ()
		{
			var attr = new CustomValidationAttribute (null, null);
			string msg = null;

			Assert.Throws<InvalidOperationException> (() => {
				// MonoTests.System.ComponentModel.DataAnnotations.CustomValidationAttributeTest.FormatErrorMessage:
				// System.InvalidOperationException : The CustomValidationAttribute.ValidatorType was not specified.
				//
				// at System.ComponentModel.DataAnnotations.CustomValidationAttribute.ThrowIfAttributeNotWellFormed()
				// at System.ComponentModel.DataAnnotations.CustomValidationAttribute.FormatErrorMessage(String name)
				// at MonoTests.System.ComponentModel.DataAnnotations.CustomValidationAttributeTest.FormatErrorMessage() in C:\Users\grendel\Documents\Visual Studio 2010\Projects\System.Web.Test\System.Web.Test\System.ComponentModel.DataAnnotations\CustomValidationAttributeTest.cs:line 88

				msg = attr.FormatErrorMessage (null);
			}, "#A1");

			attr = new CustomValidationAttribute (typeof (string), null);
			Assert.Throws<InvalidOperationException> (() => {
				// MonoTests.System.ComponentModel.DataAnnotations.CustomValidationAttributeTest.FormatErrorMessage:
				// System.InvalidOperationException : The CustomValidationAttribute.Method was not specified.
				//
				// at System.ComponentModel.DataAnnotations.CustomValidationAttribute.ThrowIfAttributeNotWellFormed()
				// at System.ComponentModel.DataAnnotations.CustomValidationAttribute.FormatErrorMessage(String name)
				// at MonoTests.System.ComponentModel.DataAnnotations.CustomValidationAttributeTest.FormatErrorMessage() in C:\Users\grendel\Documents\Visual Studio 2010\Projects\System.Web.Test\System.Web.Test\System.ComponentModel.DataAnnotations\CustomValidationAttributeTest.cs:line 102

				msg = attr.FormatErrorMessage (null);
			}, "#A2");

			attr = new CustomValidationAttribute (typeof (string), String.Empty);
			Assert.Throws<InvalidOperationException> (() => {
				// MonoTests.System.ComponentModel.DataAnnotations.CustomValidationAttributeTest.FormatErrorMessage:
				// System.InvalidOperationException : The CustomValidationAttribute.Method was not specified.
				//
				// at System.ComponentModel.DataAnnotations.CustomValidationAttribute.ThrowIfAttributeNotWellFormed()
				// at System.ComponentModel.DataAnnotations.CustomValidationAttribute.FormatErrorMessage(String name)
				// at MonoTests.System.ComponentModel.DataAnnotations.CustomValidationAttributeTest.FormatErrorMessage() in C:\Users\grendel\Documents\Visual Studio 2010\Projects\System.Web.Test\System.Web.Test\System.ComponentModel.DataAnnotations\CustomValidationAttributeTest.cs:line 117

				msg = attr.FormatErrorMessage (null);
			}, "#A3");

			attr = new CustomValidationAttribute (typeof (string), "NoSuchMethod");
			Assert.Throws<InvalidOperationException> (() => {
				// MonoTests.System.ComponentModel.DataAnnotations.CustomValidationAttributeTest.FormatErrorMessage:
				// System.InvalidOperationException : The CustomValidationAttribute method 'NoSuchMethod' does not exist in type 'String' or is not public and static.
				//
				// at System.ComponentModel.DataAnnotations.CustomValidationAttribute.ThrowIfAttributeNotWellFormed()
				// at System.ComponentModel.DataAnnotations.CustomValidationAttribute.FormatErrorMessage(String name)
				// at MonoTests.System.ComponentModel.DataAnnotations.CustomValidationAttributeTest.FormatErrorMessage() in C:\Users\grendel\Documents\Visual Studio 2010\Projects\System.Web.Test\System.Web.Test\System.ComponentModel.DataAnnotations\CustomValidationAttributeTest.cs:line 126

				msg = attr.FormatErrorMessage (null);
			}, "#A4");

			attr = new CustomValidationAttribute (typeof (PrivateValidatorMethodContainer), "MethodOne");
			Assert.Throws<InvalidOperationException> (() => {
				// MonoTests.System.ComponentModel.DataAnnotations.CustomValidationAttributeTest.FormatErrorMessage:
				// System.InvalidOperationException : The custom validation type 'PrivateValidatorMethodContainer' must be public.
				//
				// at System.ComponentModel.DataAnnotations.CustomValidationAttribute.ThrowIfAttributeNotWellFormed()
				// at System.ComponentModel.DataAnnotations.CustomValidationAttribute.FormatErrorMessage(String name)
				// at MonoTests.System.ComponentModel.DataAnnotations.CustomValidationAttributeTest.FormatErrorMessage() in C:\Users\grendel\Documents\Visual Studio 2010\Projects\System.Web.Test\System.Web.Test\System.ComponentModel.DataAnnotations\CustomValidationAttributeTest.cs:line 138

				msg = attr.FormatErrorMessage (null);
			}, "#A5");

			attr = new CustomValidationAttribute (typeof (PublicValidatorMethodContainer), "MethodOne");
			Assert.Throws<InvalidOperationException> (() => {
				// MonoTests.System.ComponentModel.DataAnnotations.CustomValidationAttributeTest.FormatErrorMessage:
				// System.InvalidOperationException : The CustomValidationAttribute method 'MethodOne' in type 'PublicValidatorMethodContainer' 
				//        must return System.ComponentModel.DataAnnotations.ValidationResult.  Use System.ComponentModel.DataAnnotations.ValidationResult.Success 
				//        to represent success.
				//
				// at System.ComponentModel.DataAnnotations.CustomValidationAttribute.ThrowIfAttributeNotWellFormed()
				// at System.ComponentModel.DataAnnotations.CustomValidationAttribute.FormatErrorMessage(String name)
				// at MonoTests.System.ComponentModel.DataAnnotations.CustomValidationAttributeTest.FormatErrorMessage() in C:\Users\grendel\Documents\Visual Studio 2010\Projects\System.Web.Test\System.Web.Test\System.ComponentModel.DataAnnotations\CustomValidationAttributeTest.cs:line 150
				msg = attr.FormatErrorMessage (null);
			}, "#A6");

			attr = new CustomValidationAttribute (typeof (PublicValidatorMethodContainer), "MethodTwo");
			Assert.Throws<InvalidOperationException> (() => {
				// MonoTests.System.ComponentModel.DataAnnotations.CustomValidationAttributeTest.FormatErrorMessage:
				// System.InvalidOperationException : The CustomValidationAttribute method 'MethodTwo' in type 'PublicValidatorMethodContainer' must match the expected signature: public static ValidationResult MethodTwo(object value, ValidationContext context).  The value can be strongly typed.  The ValidationContext parameter is optional.
				//
				// at System.ComponentModel.DataAnnotations.CustomValidationAttribute.ThrowIfAttributeNotWellFormed()
				// at System.ComponentModel.DataAnnotations.CustomValidationAttribute.FormatErrorMessage(String name)
				// at MonoTests.System.ComponentModel.DataAnnotations.CustomValidationAttributeTest.FormatErrorMessage() in C:\Users\grendel\Documents\Visual Studio 2010\Projects\System.Web.Test\System.Web.Test\System.ComponentModel.DataAnnotations\CustomValidationAttributeTest.cs:line 163
				msg = attr.FormatErrorMessage (null);
			}, "#A7");

			attr = new CustomValidationAttribute (typeof (PublicValidatorMethodContainer), "MethodThree");
			msg = attr.FormatErrorMessage (null);
			Assert.IsNotNull (msg, "#A8-1");
			Assert.IsTrue (msg.Length > 0, "#A8-2");
			Assert.AreEqual (" is not valid.", msg, "#A8-3");
			
			attr = new CustomValidationAttribute (typeof (PublicValidatorMethodContainer), "MethodFour");
			msg = attr.FormatErrorMessage ("test");
			Assert.IsNotNull (msg, "#A9-1");
			Assert.IsTrue (msg.Length > 0, "#A9-2");
			Assert.AreEqual ("test is not valid.", msg, "#A9-3");
			
			attr = new CustomValidationAttribute (typeof (PublicValidatorMethodContainer), "MethodFive");
			Assert.Throws<InvalidOperationException> (() => {
				// MonoTests.System.ComponentModel.DataAnnotations.CustomValidationAttributeTest.FormatErrorMessage:
				// System.InvalidOperationException : The CustomValidationAttribute method 'MethodFive' in type 'PublicValidatorMethodContainer' must match the expected signature: public static ValidationResult MethodFive(object value, ValidationContext context).  The value can be strongly typed.  The ValidationContext parameter is optional.
				//
				// at System.ComponentModel.DataAnnotations.CustomValidationAttribute.ThrowIfAttributeNotWellFormed()
				// at System.ComponentModel.DataAnnotations.CustomValidationAttribute.FormatErrorMessage(String name)
				// at MonoTests.System.ComponentModel.DataAnnotations.CustomValidationAttributeTest.FormatErrorMessage() in C:\Users\grendel\Documents\Visual Studio 2010\Projects\System.Web.Test\System.Web.Test\System.ComponentModel.DataAnnotations\CustomValidationAttributeTest.cs:line 180
				msg = attr.FormatErrorMessage (null);
			}, "#A10");

			attr = new CustomValidationAttribute (typeof (PublicValidatorMethodContainer), "MethodSix");
			Assert.Throws<InvalidOperationException> (() => {
				// MonoTests.System.ComponentModel.DataAnnotations.CustomValidationAttributeTest.FormatErrorMessage:
				// System.InvalidOperationException : The CustomValidationAttribute method 'MethodSix' in type 'PublicValidatorMethodContainer' must match the expected signature: public static ValidationResult MethodSix(object value, ValidationContext context).  The value can be strongly typed.  The ValidationContext parameter is optional.
				//
				// at System.ComponentModel.DataAnnotations.CustomValidationAttribute.ThrowIfAttributeNotWellFormed()
				// at System.ComponentModel.DataAnnotations.CustomValidationAttribute.FormatErrorMessage(String name)
				// at MonoTests.System.ComponentModel.DataAnnotations.CustomValidationAttributeTest.FormatErrorMessage() in C:\Users\grendel\Documents\Visual Studio 2010\Projects\System.Web.Test\System.Web.Test\System.ComponentModel.DataAnnotations\CustomValidationAttributeTest.cs:line 191
				msg = attr.FormatErrorMessage (null);
			}, "#A11");
		}

		[Test]
		public void IsValid ()
		{
			var attr = new CustomValidationAttribute (null, null);

			Assert.Throws<InvalidOperationException> (() => {
				attr.IsValid ("test");
			}, "#A1");

			attr = new CustomValidationAttribute (typeof (string), null);
			Assert.Throws<InvalidOperationException> (() => {
				attr.IsValid ("test");
			}, "#A2");

			attr = new CustomValidationAttribute (typeof (string), String.Empty);
			Assert.Throws<InvalidOperationException> (() => {
				attr.IsValid ("test");
			}, "#A3");

			attr = new CustomValidationAttribute (typeof (string), "NoSuchMethod");
			Assert.Throws<InvalidOperationException> (() => {
				attr.IsValid ("test");
			}, "#A4");

			attr = new CustomValidationAttribute (typeof (PrivateValidatorMethodContainer), "MethodOne");
			Assert.Throws<InvalidOperationException> (() => {
				attr.IsValid ("test");
			}, "#A5");

			attr = new CustomValidationAttribute (typeof (PublicValidatorMethodContainer), "MethodOne");
			Assert.Throws<InvalidOperationException> (() => {
				attr.IsValid ("test");
			}, "#A6");

			attr = new CustomValidationAttribute (typeof (PublicValidatorMethodContainer), "MethodTwo");
			Assert.Throws<InvalidOperationException> (() => {
				attr.IsValid ("test");
			}, "#A7");

			attr = new CustomValidationAttribute (typeof (PublicValidatorMethodContainer), "MethodThree");
			bool valid = attr.IsValid ("test");
			Assert.IsTrue (valid, "#A8-1");
			valid = attr.IsValid (null);
			Assert.IsFalse (valid, "#A8-2");
			valid = attr.IsValid ("failTest");
			Assert.IsFalse (valid, "#A8-3");

			attr = new CustomValidationAttribute (typeof (PublicValidatorMethodContainer), "MethodFour");
			valid = attr.IsValid ("test");
			Assert.IsTrue (valid, "#A9-1");
			valid = attr.IsValid (null);
			Assert.IsFalse (valid, "#A9-2");
			valid = attr.IsValid ("failTest");
			Assert.IsFalse (valid, "#A9-3");

			attr = new CustomValidationAttribute (typeof (PublicValidatorMethodContainer), "MethodFive");
			Assert.Throws<InvalidOperationException> (() => {
				attr.IsValid ("test");
			}, "#A10");

			attr = new CustomValidationAttribute (typeof (PublicValidatorMethodContainer), "MethodSix");
			Assert.Throws<InvalidOperationException> (() => {
				attr.IsValid ("test");
			}, "#A11");

			attr = new CustomValidationAttribute (typeof (PublicValidatorMethodContainer), "MethodSeven");
			Assert.Throws<ApplicationException> (() => {
				attr.IsValid ("test");
			}, "#A12");
		}
	}

		class PrivateValidatorMethodContainer
		{
			public static void MethodOne ()
			{ }
		}

		public class PublicValidatorMethodContainer
		{
			public static void MethodOne ()
			{ }

			public static ValidationResult MethodTwo ()
			{
				return ValidationResult.Success;
			}

			public static ValidationResult MethodThree (object o)
			{
				if (o == null)
					return new ValidationResult ("Object cannot be null");
				string s = o as string;
				if (s == null || s != "failTest")
					return ValidationResult.Success;
				return new ValidationResult ("Test failed as requested");
			}

			public static ValidationResult MethodFour (object o, ValidationContext ctx)
			{
				if (o == null)
					return new ValidationResult ("Object cannot be null");
				string s = o as string;
				if (s == null || s != "failTest")
					return ValidationResult.Success;
				return new ValidationResult ("Test failed as requested");
			}

			public static ValidationResult MethodFive (object o, string s)
			{
				return ValidationResult.Success;
			}

			public static ValidationResult MethodSix (object o, ValidationContext ctx, string s)
			{
				return ValidationResult.Success;
			}

			public static ValidationResult MethodSeven (object o, ValidationContext ctx)
			{
				throw new ApplicationException ("SNAFU");
			}
		}
}
