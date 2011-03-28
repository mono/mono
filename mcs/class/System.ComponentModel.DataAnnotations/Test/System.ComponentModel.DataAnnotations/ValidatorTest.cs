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
using MonoTests.Common;

namespace MonoTests.System.ComponentModel.DataAnnotations
{
#if NET_4_0
	[TestFixture]
	public class ValidatorTest
	{
		[Test]
		public void TryValidateObject_Object_ValidationContext_ICollection_01 ()
		{
			var dummy = new DummyNoAttributes ();
			var ctx = new ValidationContext (dummy, null, null);
			var results = new List<ValidationResult> ();

			AssertExtensions.Throws<ArgumentNullException> (() => {
				Validator.TryValidateObject (null, ctx, results);
			}, "#A1-1");

			AssertExtensions.Throws<ArgumentNullException> (() => {
				Validator.TryValidateObject (dummy, null, results);
			}, "#A1-2");

			bool valid = Validator.TryValidateObject (dummy, ctx, null);
			Assert.IsTrue (valid, "#A2-1");
			Assert.IsTrue (results.Count == 0, "#A2-2");
		}

		[Test]
		public void TryValidateObject_Object_ValidationContext_ICollection_02 ()
		{
			var dummy = new Dummy ();
			var ctx = new ValidationContext (dummy, null, null);
			var results = new List<ValidationResult> ();

			bool valid = Validator.TryValidateObject (dummy, ctx, results);
			Assert.IsTrue (valid, "#A1-1");
			Assert.AreEqual (0, results.Count, "#A1-2");

			dummy = new Dummy {
				NameField = null
			};
			AssertExtensions.Throws<ArgumentException> (() => {
				// The instance provided must match the ObjectInstance on the ValidationContext supplied.
				valid = Validator.TryValidateObject (dummy, ctx, results);
			}, "#A2");

			// Fields are ignored
			ctx = new ValidationContext (dummy, null, null);
			valid = Validator.TryValidateObject (dummy, ctx, results);
			Assert.IsTrue (valid, "#A3-1");
			Assert.AreEqual (0, results.Count, "#A3-2");

			// Required properties existence is validated
			dummy = new Dummy {
				RequiredDummyField = null
			};
			ctx = new ValidationContext (dummy, null, null);
			valid = Validator.TryValidateObject (dummy, ctx, results);
			Assert.IsTrue (valid, "#A4-1");
			Assert.AreEqual (0, results.Count, "#A4-2");

			dummy = new Dummy {
				RequiredDummyProperty = null
			};
			ctx = new ValidationContext (dummy, null, null);
			valid = Validator.TryValidateObject (dummy, ctx, results);
			Assert.IsFalse (valid, "#A5-1");
			Assert.AreEqual (1, results.Count, "#A5-2");

			results.Clear ();

			// validation attributes other than Required are ignored
			dummy = new Dummy {
				NameProperty = null
			};
			ctx = new ValidationContext (dummy, null, null);
			valid = Validator.TryValidateObject (dummy, ctx, results);
			Assert.IsTrue (valid, "#A6-1");
			Assert.AreEqual (0, results.Count, "#A6-2");

			dummy = new Dummy {
				MinMaxProperty = 0
			};
			ctx = new ValidationContext (dummy, null, null);
			valid = Validator.TryValidateObject (dummy, ctx, results);
			Assert.IsTrue (valid, "#A7-1");
			Assert.AreEqual (0, results.Count, "#A7-2");

			dummy = new Dummy {
				FailValidation = true
			};
			ctx = new ValidationContext (dummy, null, null);
			valid = Validator.TryValidateObject (dummy, ctx, results);
			Assert.IsFalse (valid, "#A8-1");
			Assert.AreEqual (1, results.Count, "#A8-2");
		}

		[Test]
		public void TryValidateObject_Object_ValidationContext_ICollection_Bool_01 ()
		{
			var dummy = new DummyNoAttributes ();
			var ctx = new ValidationContext (dummy, null, null);
			var results = new List<ValidationResult> ();

			AssertExtensions.Throws<ArgumentNullException> (() => {
				Validator.TryValidateObject (null, ctx, results, false);
			}, "#A1-1");

			AssertExtensions.Throws<ArgumentNullException> (() => {
				Validator.TryValidateObject (dummy, null, results, false);
			}, "#A1-2");

			bool valid = Validator.TryValidateObject (dummy, ctx, null, false);
			Assert.IsTrue (valid, "#A2-1");
			Assert.IsTrue (results.Count == 0, "#A2-2");

			valid = Validator.TryValidateObject (dummy, ctx, null, true);
			Assert.IsTrue (valid, "#A3-1");
			Assert.IsTrue (results.Count == 0, "#A3-2");
		}

		[Test]
		public void TryValidateObject_Object_ValidationContext_ICollection_Bool_02 ()
		{
			var dummy = new Dummy ();
			var ctx = new ValidationContext (dummy, null, null);
			var results = new List<ValidationResult> ();

			bool valid = Validator.TryValidateObject (dummy, ctx, results, false);
			Assert.IsTrue (valid, "#A1-1");
			Assert.AreEqual (0, results.Count, "#A1-2");

			valid = Validator.TryValidateObject (dummy, ctx, results, true);
			Assert.IsTrue (valid, "#A1-3");
			Assert.AreEqual (0, results.Count, "#A1-4");

			dummy = new Dummy {
				NameField = null
			};
			AssertExtensions.Throws<ArgumentException> (() => {
				// The instance provided must match the ObjectInstance on the ValidationContext supplied.
				valid = Validator.TryValidateObject (dummy, ctx, results, false);
			}, "#A2-1");

			AssertExtensions.Throws<ArgumentException> (() => {
				// The instance provided must match the ObjectInstance on the ValidationContext supplied.
				valid = Validator.TryValidateObject (dummy, ctx, results, true);
			}, "#A2-2");

			// Fields are ignored
			ctx = new ValidationContext (dummy, null, null);
			valid = Validator.TryValidateObject (dummy, ctx, results, false);
			Assert.IsTrue (valid, "#A3-1");
			Assert.AreEqual (0, results.Count, "#A3-2");

			valid = Validator.TryValidateObject (dummy, ctx, results, true);
			Assert.IsTrue (valid, "#A3-3");
			Assert.AreEqual (0, results.Count, "#A3-4");

			dummy = new Dummy {
				RequiredDummyField = null
			};
			ctx = new ValidationContext (dummy, null, null);
			valid = Validator.TryValidateObject (dummy, ctx, results, false);
			Assert.IsTrue (valid, "#A4-1");
			Assert.AreEqual (0, results.Count, "#A4-2");

			valid = Validator.TryValidateObject (dummy, ctx, results, true);
			Assert.IsTrue (valid, "#A4-3");
			Assert.AreEqual (0, results.Count, "#A4-4");

			// Required properties existence is validated
			dummy = new Dummy {
				RequiredDummyProperty = null
			};
			ctx = new ValidationContext (dummy, null, null);
			valid = Validator.TryValidateObject (dummy, ctx, results, false);
			Assert.IsFalse (valid, "#A5-1");
			Assert.AreEqual (1, results.Count, "#A5-2");
			results.Clear ();
			
			valid = Validator.TryValidateObject (dummy, ctx, results, true);
			Assert.IsFalse (valid, "#A5-3");
			Assert.AreEqual (1, results.Count, "#A5-4");
			results.Clear ();

			dummy = new Dummy {
				NameProperty = null
			};
			ctx = new ValidationContext (dummy, null, null);
			valid = Validator.TryValidateObject (dummy, ctx, results, false);
			Assert.IsTrue (valid, "#A6-1");
			Assert.AreEqual (0, results.Count, "#A6-2");

			// NameProperty is null, that causes the StringLength validator to skip its tests
			valid = Validator.TryValidateObject (dummy, ctx, results, true);
			Assert.IsTrue (valid, "#A6-3");
			Assert.AreEqual (0, results.Count, "#A6-4");

			dummy.NameProperty = "0";
			valid = Validator.TryValidateObject (dummy, ctx, results, true);
			Assert.IsFalse (valid, "#A6-5");
			Assert.AreEqual (1, results.Count, "#A6-6");
			results.Clear ();

			dummy.NameProperty = "name too long (invalid value)";
			valid = Validator.TryValidateObject (dummy, ctx, results, true);
			Assert.IsFalse (valid, "#A6-7");
			Assert.AreEqual (1, results.Count, "#A6-8");
			results.Clear ();

			dummy = new Dummy {
				MinMaxProperty = 0
			};
			ctx = new ValidationContext (dummy, null, null);
			valid = Validator.TryValidateObject (dummy, ctx, results, false);
			Assert.IsTrue (valid, "#A7-1");
			Assert.AreEqual (0, results.Count, "#A7-2");

			valid = Validator.TryValidateObject (dummy, ctx, results, true);
			Assert.IsFalse (valid, "#A7-3");
			Assert.AreEqual (1, results.Count, "#A7-4");
			results.Clear ();

			dummy = new Dummy {
				FailValidation = true
			};
			ctx = new ValidationContext (dummy, null, null);
			valid = Validator.TryValidateObject (dummy, ctx, results, false);
			Assert.IsFalse (valid, "#A8-1");
			Assert.AreEqual (1, results.Count, "#A8-2");
			results.Clear ();

			valid = Validator.TryValidateObject (dummy, ctx, results, true);
			Assert.IsFalse (valid, "#A8-3");
			Assert.AreEqual (1, results.Count, "#A8-4");
			results.Clear ();

			var dummy2 = new DummyWithException ();
			ctx = new ValidationContext (dummy2, null, null);
			AssertExtensions.Throws<ApplicationException> (() => {
				Validator.TryValidateObject (dummy2, ctx, results, true);
			}, "#A9");
		}

		[Test]
		public void TryValidateProperty ()
		{
			var dummy = new DummyNoAttributes ();
			var ctx = new ValidationContext (dummy, null, null) {
				MemberName = "NameProperty"
			};
			var results = new List<ValidationResult> ();

			AssertExtensions.Throws<ArgumentException> (() => {
				// MonoTests.System.ComponentModel.DataAnnotations.ValidatorTest.TryValidateProperty:
				// System.ArgumentException : The type 'DummyNoAttributes' does not contain a public property named 'NameProperty'.
				// Parameter name: propertyName
				//
				// at System.ComponentModel.DataAnnotations.ValidationAttributeStore.TypeStoreItem.GetPropertyStoreItem(String propertyName)
				// at System.ComponentModel.DataAnnotations.ValidationAttributeStore.GetPropertyType(ValidationContext validationContext)
				// at System.ComponentModel.DataAnnotations.Validator.TryValidateProperty(Object value, ValidationContext validationContext, ICollection`1 validationResults)
				// at MonoTests.System.ComponentModel.DataAnnotations.ValidatorTest.TryValidateProperty() in C:\Users\grendel\Documents\Visual Studio 2010\Projects\System.Web.Test\System.Web.Test\System.ComponentModel.DataAnnotations\ValidatorTest.cs:line 283

				Validator.TryValidateProperty ("dummy", ctx, results);
			}, "#A1-1");
			Assert.AreEqual (0, results.Count, "#A1-2");

			AssertExtensions.Throws<ArgumentNullException> (() => {
				Validator.TryValidateProperty ("dummy", null, results);
			}, "#A1-2");

			var dummy2 = new Dummy ();
			ctx = new ValidationContext (dummy2, null, null) {
				MemberName = "NameProperty"
			};
			
			bool valid = Validator.TryValidateProperty (null, ctx, results);
			Assert.IsTrue (valid, "#A1-3");
			Assert.AreEqual (0, results.Count, "#A1-4");

			ctx = new ValidationContext (dummy2, null, null) {
				MemberName = "MinMaxProperty"
			};

			AssertExtensions.Throws<ArgumentException> (() => {
				Validator.TryValidateProperty (null, ctx, results);
			}, "#A1-5");

			ctx = new ValidationContext (dummy2, null, null);
			AssertExtensions.Throws<ArgumentNullException> (() => {
				// MonoTests.System.ComponentModel.DataAnnotations.ValidatorTest.TryValidateProperty:
				// System.ArgumentNullException : Value cannot be null.
				// Parameter name: propertyName
				//
				// at System.ComponentModel.DataAnnotations.ValidationAttributeStore.TypeStoreItem.TryGetPropertyStoreItem(String propertyName, PropertyStoreItem& item)
				// at System.ComponentModel.DataAnnotations.ValidationAttributeStore.TypeStoreItem.GetPropertyStoreItem(String propertyName)
				// at System.ComponentModel.DataAnnotations.ValidationAttributeStore.GetPropertyType(ValidationContext validationContext)
				// at System.ComponentModel.DataAnnotations.Validator.TryValidateProperty(Object value, ValidationContext validationContext, ICollection`1 validationResults)
				// at MonoTests.System.ComponentModel.DataAnnotations.ValidatorTest.TryValidateProperty() in C:\Users\grendel\Documents\Visual Studio 2010\Projects\System.Web.Test\System.Web.Test\System.ComponentModel.DataAnnotations\ValidatorTest.cs:line 289

				Validator.TryValidateProperty ("dummy", ctx, results);
			}, "#A2-1");
			Assert.AreEqual (0, results.Count, "#A2-2");

			ctx = new ValidationContext (dummy2, null, null) {
				MemberName = String.Empty
			};

			AssertExtensions.Throws<ArgumentNullException> (() => {
				// MonoTests.System.ComponentModel.DataAnnotations.ValidatorTest.TryValidateProperty:
				// System.ArgumentNullException : Value cannot be null.
				// Parameter name: propertyName
				//
				// at System.ComponentModel.DataAnnotations.ValidationAttributeStore.TypeStoreItem.TryGetPropertyStoreItem(String propertyName, PropertyStoreItem& item)
				// at System.ComponentModel.DataAnnotations.ValidationAttributeStore.TypeStoreItem.GetPropertyStoreItem(String propertyName)
				// at System.ComponentModel.DataAnnotations.ValidationAttributeStore.GetPropertyType(ValidationContext validationContext)
				// at System.ComponentModel.DataAnnotations.Validator.TryValidateProperty(Object value, ValidationContext validationContext, ICollection`1 validationResults)
				// at MonoTests.System.ComponentModel.DataAnnotations.ValidatorTest.TryValidateProperty() in C:\Users\grendel\Documents\Visual Studio 2010\Projects\System.Web.Test\System.Web.Test\System.ComponentModel.DataAnnotations\ValidatorTest.cs:line 289

				Validator.TryValidateProperty ("dummy", ctx, results);
			}, "#A2-2");
			Assert.AreEqual (0, results.Count, "#A2-2");

			dummy2 = new Dummy ();
			ctx = new ValidationContext (dummy2, null, null) {
				MemberName = "NameProperty"
			};

			AssertExtensions.Throws<ArgumentException> (() => {
				// MonoTests.System.ComponentModel.DataAnnotations.ValidatorTest.TryValidateProperty:
				// System.ArgumentException : The value for property 'NameProperty' must be of type 'System.String'.
				// Parameter name: value
				//
				// at System.ComponentModel.DataAnnotations.Validator.EnsureValidPropertyType(String propertyName, Type propertyType, Object value)
				// at System.ComponentModel.DataAnnotations.Validator.TryValidateProperty(Object value, ValidationContext validationContext, ICollection`1 validationResults)
				// at MonoTests.System.ComponentModel.DataAnnotations.ValidatorTest.TryValidateProperty() in C:\Users\grendel\Documents\Visual Studio 2010\Projects\System.Web.Test\System.Web.Test\System.ComponentModel.DataAnnotations\ValidatorTest.cs:line 315

				Validator.TryValidateProperty (1234, ctx, results);
			}, "#A3-1");
			Assert.AreEqual (0, results.Count, "#A3-2");

			dummy2 = new Dummy ();
			ctx = new ValidationContext (dummy2, null, null) {
				MemberName = "NameProperty"
			};
			
			valid = Validator.TryValidateProperty (String.Empty, ctx, results);
			Assert.IsFalse (valid, "#A4-1");
			Assert.AreEqual (1, results.Count, "#A4-2");
			results.Clear ();

			valid = Validator.TryValidateProperty ("this value is way too long", ctx, results);
			Assert.IsFalse (valid, "#A4-3");
			Assert.AreEqual (1, results.Count, "#A4-4");
			results.Clear ();

			valid = Validator.TryValidateProperty ("good value", ctx, results);
			Assert.IsTrue (valid, "#A4-5");
			Assert.AreEqual (0, results.Count, "#A4-6");

			dummy2 = new Dummy ();
			ctx = new ValidationContext (dummy2, null, null) {
				MemberName = "CustomValidatedProperty"
			};

			valid = Validator.TryValidateProperty (String.Empty, ctx, results);
			Assert.IsFalse (valid, "#A5-1");
			Assert.AreEqual (1, results.Count, "#A5-2");
			results.Clear ();

			valid = Validator.TryValidateProperty ("fail", ctx, results);
			Assert.IsFalse (valid, "#A5-3");
			Assert.AreEqual (1, results.Count, "#A5-4");
			results.Clear ();

			valid = Validator.TryValidateProperty ("f", ctx, results);
			Assert.IsFalse (valid, "#A5-5");
			Assert.AreEqual (2, results.Count, "#A5-6");
			results.Clear ();

			valid = Validator.TryValidateProperty ("good value", ctx, results);
			Assert.IsTrue (valid, "#A5-7");
			Assert.AreEqual (0, results.Count, "#A5-8");
		}

		[Test]
		public void TryValidateValue_01 ()
		{
			var dummy = new DummyNoAttributes ();
			var ctx = new ValidationContext (dummy, null, null) {
				MemberName = "NameProperty"
			};
			var results = new List<ValidationResult> ();
			var attributes = new List <ValidationAttribute> ();
			
			bool valid = Validator.TryValidateValue (null, ctx, results, attributes);
			Assert.IsTrue (valid, "#A1-1");
			Assert.AreEqual (0, results.Count, "#A1-2");

			AssertExtensions.Throws<ArgumentNullException> (() => {
				Validator.TryValidateValue ("dummy", null, results, attributes);
			}, "#A2");

			valid = Validator.TryValidateValue ("dummy", ctx, null, attributes);
			Assert.IsTrue (valid, "#A3-1");
			Assert.AreEqual (0, results.Count, "#A3-2");

			AssertExtensions.Throws<ArgumentNullException> (() => {
				Validator.TryValidateValue ("dummy", ctx, results, null);
			}, "#A4");
		}

		[Test]
		public void TryValidateValue_02 ()
		{
			var dummy = new DummyNoAttributes ();
			var ctx = new ValidationContext (dummy, null, null);
			var results = new List<ValidationResult> ();
			var log = new List<string> ();
			var attributes = new List<ValidationAttribute> () {
				new StringLengthAttributePoker (10, log) {
					MinimumLength = 2
				},
				new RequiredAttributePoker (log)
			};

			bool valid = Validator.TryValidateValue (null, ctx, results, attributes);
			Assert.IsFalse (valid, "#A1-1");
			Assert.AreEqual (1, results.Count, "#A1-2");
			Assert.AreEqual (1, log.Count, "#A1-3");
			Assert.IsTrue (log [0].StartsWith ("RequiredAttributePoker.IsValid (object)"), "#A1-4");
			results.Clear ();
			log.Clear ();

			AssertExtensions.Throws<InvalidCastException> (() => {
				// Thrown by StringValidatorAttribute
				Validator.TryValidateValue (1234, ctx, results, attributes);
			}, "#A2-1");
			Assert.AreEqual (0, results.Count, "#A2-2");
			Assert.AreEqual (2, log.Count, "#A2-3");
			Assert.IsTrue (log[0].StartsWith ("RequiredAttributePoker.IsValid (object)"), "#A2-4");
			Assert.IsTrue (log[1].StartsWith ("StringLengthAttributePoker.IsValid (object)"), "#A2-5");
			results.Clear ();
			log.Clear ();

			attributes.Add (new CustomValidationAttribute (typeof (ValidatorTest), "ValueValidationMethod"));
			attributes.Add (new CustomValidationAttribute (typeof (ValidatorTest), "ValueValidationMethod"));
			valid = Validator.TryValidateValue ("test", ctx, results, attributes);
			Assert.IsFalse (valid, "#A3-1");
			Assert.AreEqual (2, results.Count, "#A3-2");
			Assert.AreEqual (2, log.Count, "#A3-3");
			Assert.IsTrue (log[0].StartsWith ("RequiredAttributePoker.IsValid (object)"), "#A3-4");
			Assert.IsTrue (log[1].StartsWith ("StringLengthAttributePoker.IsValid (object)"), "#A3-5");
			Assert.AreEqual ("ValueValidationMethod", results[0].ErrorMessage, "#A3-6");
			Assert.AreEqual ("ValueValidationMethod", results[1].ErrorMessage, "#A3-7");
			results.Clear ();
			log.Clear ();
			attributes.RemoveAt (2);
			attributes.RemoveAt (2);

			AssertExtensions.Throws<ArgumentNullException> (() => {
				Validator.TryValidateValue ("dummy", null, results, attributes);
			}, "#B1");

			valid = Validator.TryValidateValue ("dummy", ctx, null, attributes);
			Assert.IsTrue (valid, "#B2-1");
			Assert.AreEqual (0, results.Count, "#B2-2");

			AssertExtensions.Throws<ArgumentNullException> (() => {
				Validator.TryValidateValue ("dummy", ctx, results, null);
			}, "#B3");
		}

		[Test]
		public void ValidateObject_Object_ValidationContext_01 ()
		{
			var dummy = new DummyNoAttributes ();
			var ctx = new ValidationContext (dummy, null, null);
			
			AssertExtensions.Throws<ArgumentNullException> (() => {
				Validator.ValidateObject (null, ctx);
			}, "#A1-1");

			AssertExtensions.Throws<ArgumentNullException> (() => {
				Validator.ValidateObject (dummy, null);
			}, "#A1-2");

			try {
				Validator.ValidateObject (dummy, ctx);
			} catch (Exception ex) {
				Assert.Fail ("#A2 (exception {0} thrown: {1})", ex.GetType (), ex.Message);
			}
		}

		[Test]
		public void ValidateObject_Object_ValidationContext_02 ()
		{
			var dummy = new Dummy ();
			var ctx = new ValidationContext (dummy, null, null);

			try {
				Validator.ValidateObject (dummy, ctx);
			} catch (Exception ex) {
				Assert.Fail ("#A1 (exception {0} thrown: {1})", ex.GetType (), ex.Message);
			}

			dummy = new Dummy {
				NameField = null
			};
			AssertExtensions.Throws<ArgumentException> (() => {
				// The instance provided must match the ObjectInstance on the ValidationContext supplied.
				Validator.ValidateObject (dummy, ctx);
			}, "#A2");

			// Fields are ignored
			ctx = new ValidationContext (dummy, null, null);
			try {
				Validator.ValidateObject (dummy, ctx);
			}  catch (Exception ex) {
				Assert.Fail ("#A3 (exception {0} thrown: {1})", ex.GetType (), ex.Message);
			}
			
			dummy = new Dummy {
				RequiredDummyField = null
			};
			ctx = new ValidationContext (dummy, null, null);
			try {
				Validator.ValidateObject (dummy, ctx);
			} catch (Exception ex) {
				Assert.Fail ("#A4 (exception {0} thrown: {1})", ex.GetType (), ex.Message);
			}

			dummy = new Dummy {
				RequiredDummyProperty = null
			};
			ctx = new ValidationContext (dummy, null, null);
			AssertExtensions.Throws<ValidationException> (() => {
				Validator.ValidateObject (dummy, ctx);
			}, "#A5");

			// validation attributes other than Required are ignored
			dummy = new Dummy {
				NameProperty = null
			};
			ctx = new ValidationContext (dummy, null, null);
			try {
				Validator.ValidateObject (dummy, ctx);
			} catch (Exception ex) {
				Assert.Fail ("#A6 (exception {0} thrown: {1})", ex.GetType (), ex.Message);
			}
			
			dummy = new Dummy {
				MinMaxProperty = 0
			};
			ctx = new ValidationContext (dummy, null, null);
			try {
				Validator.ValidateObject (dummy, ctx);
			} catch (Exception ex) {
				Assert.Fail ("#A7 (exception {0} thrown: {1})", ex.GetType (), ex.Message);
			}

			dummy = new Dummy {
				FailValidation = true
			};
			ctx = new ValidationContext (dummy, null, null);
			AssertExtensions.Throws<ValidationException> (() => {
				Validator.ValidateObject (dummy, ctx);
			}, "#A8");

			var dummy2 = new DummyMultipleCustomValidators ();
			ctx = new ValidationContext (dummy2, null, null);
			try {
				Validator.ValidateObject (dummy2, ctx);
			} catch (Exception ex) {
				Assert.Fail ("#A9 (exception {0} thrown: {1})", ex.GetType (), ex.Message);
			}
		}

		[Test]
		public void ValidateObject_Object_ValidationContext_Bool_01 ()
		{
			var dummy = new DummyNoAttributes ();
			var ctx = new ValidationContext (dummy, null, null);

			AssertExtensions.Throws<ArgumentNullException> (() => {
				Validator.ValidateObject (null, ctx, false);
			}, "#A1-1");

			AssertExtensions.Throws<ArgumentNullException> (() => {
				Validator.ValidateObject (dummy, null, false);
			}, "#A1-2");

			try {
				Validator.ValidateObject (dummy, ctx, false);
			} catch (Exception ex) {
				Assert.Fail ("#A2 (exception {0} thrown: {1})", ex.GetType (), ex.Message);
			}

			try {
				Validator.ValidateObject (dummy, ctx, true);
			} catch (Exception ex) {
				Assert.Fail ("#A3 (exception {0} thrown: {1})", ex.GetType (), ex.Message);
			}
		}

		[Test]
		public void ValidateObject_Object_ValidationContext_Bool_02 ()
		{
			var dummy = new Dummy ();
			var ctx = new ValidationContext (dummy, null, null);

			try {
				Validator.ValidateObject (dummy, ctx, false);
			} catch (Exception ex) {
				Assert.Fail ("#A1 (exception {0} thrown: {1})", ex.GetType (), ex.Message);
			}

			try {
				Validator.ValidateObject (dummy, ctx, true);
			} catch (Exception ex) {
				Assert.Fail ("#A2 (exception {0} thrown: {1})", ex.GetType (), ex.Message);
			}

			dummy = new Dummy {
				NameField = null
			};
			AssertExtensions.Throws<ArgumentException> (() => {
				// The instance provided must match the ObjectInstance on the ValidationContext supplied.
				Validator.ValidateObject (dummy, ctx, false);
			}, "#A3-1");

			AssertExtensions.Throws<ArgumentException> (() => {
				// The instance provided must match the ObjectInstance on the ValidationContext supplied.
				Validator.ValidateObject (dummy, ctx, true);
			}, "#A3-2");

			// Fields are ignored
			ctx = new ValidationContext (dummy, null, null);
			try {
				Validator.ValidateObject (dummy, ctx, false);
			} catch (Exception ex) {
				Assert.Fail ("#A4-1 (exception {0} thrown: {1})", ex.GetType (), ex.Message);
			}

			try {
				Validator.ValidateObject (dummy, ctx, true);
			} catch (Exception ex) {
				Assert.Fail ("#A4-2 (exception {0} thrown: {1})", ex.GetType (), ex.Message);
			}

			dummy = new Dummy {
				RequiredDummyField = null
			};
			ctx = new ValidationContext (dummy, null, null);
			try {
				Validator.ValidateObject (dummy, ctx, false);
			} catch (Exception ex) {
				Assert.Fail ("#A5-1 (exception {0} thrown: {1})", ex.GetType (), ex.Message);
			}

			try {
				Validator.ValidateObject (dummy, ctx, true);
			} catch (Exception ex) {
				Assert.Fail ("#A5-2 (exception {0} thrown: {1})", ex.GetType (), ex.Message);
			}

			// Required properties existence is validated
			dummy = new Dummy {
				RequiredDummyProperty = null
			};
			ctx = new ValidationContext (dummy, null, null);
			AssertExtensions.Throws<ValidationException> (() => {
				Validator.ValidateObject (dummy, ctx, false);
			}, "#A6-1");

			AssertExtensions.Throws<ValidationException> (() => {
				Validator.ValidateObject (dummy, ctx, true);
			}, "#A6-2");

			dummy = new Dummy {
				NameProperty = null
			};
			ctx = new ValidationContext (dummy, null, null);
			try {
				Validator.ValidateObject (dummy, ctx, false);
			} catch (Exception ex) {
				Assert.Fail ("#A7 (exception {0} thrown: {1})", ex.GetType (), ex.Message);
			}

			// NameProperty is null, that causes the StringLength validator to skip its tests
			try {
				Validator.ValidateObject (dummy, ctx, true);
			} catch (Exception ex) {
				Assert.Fail ("#A8 (exception {0} thrown: {1})", ex.GetType (), ex.Message);
			}

			dummy.NameProperty = "0";
			AssertExtensions.Throws<ValidationException> (() => {
				Validator.ValidateObject (dummy, ctx, true);
			}, "#A9");

			dummy.NameProperty = "name too long (invalid value)";
			AssertExtensions.Throws<ValidationException> (() => {
				Validator.ValidateObject (dummy, ctx, true);
			}, "#A10");

			dummy = new Dummy {
				MinMaxProperty = 0
			};
			ctx = new ValidationContext (dummy, null, null);
			try {
				Validator.ValidateObject (dummy, ctx, false);
			} catch (Exception ex) {
				Assert.Fail ("#A11 (exception {0} thrown: {1})", ex.GetType (), ex.Message);
			}

			AssertExtensions.Throws<ValidationException> (() => {
				Validator.ValidateObject (dummy, ctx, true);
			}, "#A12");

			dummy = new Dummy {
				FailValidation = true
			};
			ctx = new ValidationContext (dummy, null, null);
			AssertExtensions.Throws<ValidationException> (() => {
				Validator.ValidateObject (dummy, ctx, false);
			}, "#A13-1");

			AssertExtensions.Throws<ValidationException> (() => {
				Validator.ValidateObject (dummy, ctx, true);
			}, "#A13-2");

			var dummy2 = new DummyWithException ();
			ctx = new ValidationContext (dummy2, null, null);
			AssertExtensions.Throws<ApplicationException> (() => {
				Validator.ValidateObject (dummy2, ctx, true);
			}, "#A14");

			var dummy3 = new DummyMultipleCustomValidators ();
			ctx = new ValidationContext (dummy3, null, null);
			try {
				Validator.ValidateObject (dummy3, ctx, false);
			} catch (Exception ex) {
				Assert.Fail ("#A9 (exception {0} thrown: {1})", ex.GetType (), ex.Message);
			}

			try {
				Validator.ValidateObject (dummy3, ctx, true);
			} catch (ValidationException ex) {
				Assert.AreEqual ("FirstPropertyValidationMethod", ex.Message, "#A10");
			} catch (Exception ex) {
				Assert.Fail ("#A10 (exception {0} thrown: {1})", ex.GetType (), ex.Message);
			}
		}

		[Test]
		public void ValidateProperty ()
		{
			var dummy = new DummyNoAttributes ();
			var ctx = new ValidationContext (dummy, null, null) {
				MemberName = "NameProperty"
			};

			AssertExtensions.Throws<ArgumentException> (() => {
				Validator.ValidateProperty ("dummy", ctx);
			}, "#A1-1");

			AssertExtensions.Throws<ArgumentNullException> (() => {
				Validator.ValidateProperty ("dummy", null);
			}, "#A1-2");

			var dummy2 = new Dummy ();
			ctx = new ValidationContext (dummy2, null, null) {
				MemberName = "NameProperty"
			};

			try {
				Validator.ValidateProperty (null, ctx);
			} catch (Exception ex) {
				Assert.Fail ("#A2 (exception {0} thrown: {1})", ex.GetType (), ex.Message);
			}

			ctx = new ValidationContext (dummy2, null, null) {
				MemberName = "MinMaxProperty"
			};

			AssertExtensions.Throws<ArgumentException> (() => {
				Validator.ValidateProperty (null, ctx);
			}, "#A3");

			ctx = new ValidationContext (dummy2, null, null);
			AssertExtensions.Throws<ArgumentNullException> (() => {
				Validator.ValidateProperty ("dummy", ctx);
			}, "#A4");

			ctx = new ValidationContext (dummy2, null, null) {
				MemberName = String.Empty
			};

			AssertExtensions.Throws<ArgumentNullException> (() => {
				Validator.ValidateProperty ("dummy", ctx);
			}, "#A5");

			dummy2 = new Dummy ();
			ctx = new ValidationContext (dummy2, null, null) {
				MemberName = "NameProperty"
			};

			AssertExtensions.Throws<ArgumentException> (() => {
				Validator.ValidateProperty (1234, ctx);
			}, "#A6");

			dummy2 = new Dummy ();
			ctx = new ValidationContext (dummy2, null, null) {
				MemberName = "NameProperty"
			};

			AssertExtensions.Throws<ValidationException> (() => {
				Validator.ValidateProperty (String.Empty, ctx);
			}, "#A7");

			AssertExtensions.Throws<ValidationException> (() => {
				Validator.ValidateProperty ("this value is way too long", ctx);
			}, "#A8");

			try {
				Validator.ValidateProperty ("good value", ctx);
			} catch (Exception ex) {
				Assert.Fail ("#A9 (exception {0} thrown: {1})", ex.GetType (), ex.Message);
			}

			dummy2 = new Dummy ();
			ctx = new ValidationContext (dummy2, null, null) {
				MemberName = "CustomValidatedProperty"
			};

			AssertExtensions.Throws<ValidationException> (() => {
				Validator.ValidateProperty (String.Empty, ctx);
			}, "#A10");

			AssertExtensions.Throws<ValidationException> (() => {
				Validator.ValidateProperty ("fail", ctx);
			}, "#A11");

			AssertExtensions.Throws<ValidationException> (() => {
				Validator.ValidateProperty ("f", ctx);
			}, "#A12");

			try {
				Validator.ValidateProperty ("good value", ctx);
			} catch (Exception ex) {
				Assert.Fail ("#A13 (exception {0} thrown: {1})", ex.GetType (), ex.Message);
			}
		}

		[Test]
		public void ValidateValue_01 ()
		{
			var dummy = new DummyNoAttributes ();
			var ctx = new ValidationContext (dummy, null, null) {
				MemberName = "NameProperty"
			};
			var attributes = new List<ValidationAttribute> ();

			try {
				Validator.ValidateValue (null, ctx, attributes);
			} catch (Exception ex) {
				Assert.Fail ("#A1 (exception {0} thrown: {1})", ex.GetType (), ex.Message);
			}

			AssertExtensions.Throws<ArgumentNullException> (() => {
				Validator.ValidateValue ("dummy", null, attributes);
			}, "#A2");

			try {
				Validator.ValidateValue ("dummy", ctx, attributes);
			} catch (Exception ex) {
				Assert.Fail ("#A3 (exception {0} thrown: {1})", ex.GetType (), ex.Message);
			}

			AssertExtensions.Throws<ArgumentNullException> (() => {
				Validator.ValidateValue ("dummy", ctx, null);
			}, "#A4");
		}

		[Test]
		public void ValidateValue_02 ()
		{
			var dummy = new DummyNoAttributes ();
			var ctx = new ValidationContext (dummy, null, null);
			var log = new List<string> ();
			var attributes = new List<ValidationAttribute> () {
				new StringLengthAttributePoker (10, log) {
					MinimumLength = 2
				},
				new RequiredAttributePoker (log)
			};

			AssertExtensions.Throws<ValidationException> (() => {
				Validator.ValidateValue (null, ctx, attributes);
			}, "#A1-1");
			Assert.AreEqual (1, log.Count, "#A1-2");
			Assert.IsTrue (log[0].StartsWith ("RequiredAttributePoker.IsValid (object)"), "#A1-3");
			log.Clear ();

			AssertExtensions.Throws<InvalidCastException> (() => {
				// Thrown by StringValidatorAttribute
				Validator.ValidateValue (1234, ctx, attributes);
			}, "#A2-1");;
			Assert.AreEqual (2, log.Count, "#A2-2");
			Assert.IsTrue (log[0].StartsWith ("RequiredAttributePoker.IsValid (object)"), "#A2-3");
			Assert.IsTrue (log[1].StartsWith ("StringLengthAttributePoker.IsValid (object)"), "#A2-4");
			log.Clear ();

			attributes.Add (new CustomValidationAttribute (typeof (ValidatorTest), "ValueValidationMethod"));
			attributes.Add (new CustomValidationAttribute (typeof (ValidatorTest), "ValueValidationMethod"));
			AssertExtensions.Throws<ValidationException> (() => {
				Validator.ValidateValue ("test", ctx, attributes);
			}, "#A3-1");
			Assert.AreEqual (2, log.Count, "#A3-2");
			Assert.IsTrue (log[0].StartsWith ("RequiredAttributePoker.IsValid (object)"), "#A3-3");
			Assert.IsTrue (log[1].StartsWith ("StringLengthAttributePoker.IsValid (object)"), "#A3-4");
			log.Clear ();
			attributes.RemoveAt (2);
			attributes.RemoveAt (2);

			AssertExtensions.Throws<ArgumentNullException> (() => {
				Validator.ValidateValue ("dummy", null, attributes);
			}, "#B1");

			try {
				Validator.ValidateValue ("dummy", ctx, attributes);
			} catch (Exception ex) {
				Assert.Fail ("#B2 (exception {0} thrown: {1})", ex.GetType (), ex.Message);
			}

			AssertExtensions.Throws<ArgumentNullException> (() => {
				Validator.ValidateValue ("dummy", ctx, null);
			}, "#B3");
		}

		public static ValidationResult DummyValidationMethod (object o)
		{
			var dummy = o as Dummy;
			if (dummy == null)
				return new ValidationResult ("Invalid DummyValidationMethod input - broken test?");

			if (dummy.FailValidation)
				return new ValidationResult ("Dummy validation failed.");
			return ValidationResult.Success;
		}

		public static ValidationResult CustomValidatedPropertyValidationMethod (object o)
		{
			var dummy = o as string;
			if (dummy != null && (dummy == "f" || dummy == "fail"))
				return new ValidationResult ("Dummy.CustomValidatedProperty validation failed.");
			return ValidationResult.Success;
		}

		public static ValidationResult ValidationMethodException (object o)
		{
			throw new ApplicationException ("SNAFU");
		}

		public static ValidationResult ValueValidationMethod (object o, ValidationContext validationContext)
		{
			return new ValidationResult ("ValueValidationMethod");
		}

		public static ValidationResult FirstPropertyValidationMethod (object o, ValidationContext validationContext)
		{
			return new ValidationResult ("FirstPropertyValidationMethod");
		}

		public static ValidationResult SecondPropertyValidationMethod (object o, ValidationContext validationContext)
		{
			return new ValidationResult ("SecondPropertyValidationMethod");
		}

		public class RequiredAttributePoker : RequiredAttribute
		{
			List <string> log;

			public RequiredAttributePoker (List<string> log)
			{
				if (log == null)
					throw new ArgumentNullException ("log");
				this.log = log;
			}

			public override bool IsValid (object value)
			{
				log.Add ("RequiredAttributePoker.IsValid (object)");
				return base.IsValid (value);
			}
		}

		public class StringLengthAttributePoker : StringLengthAttribute
		{
			List <string> log;

			public StringLengthAttributePoker (int maximumLength, List<string> log)
				: base (maximumLength)
			{
				if (log == null)
					throw new ArgumentNullException ("log");
				this.log = log;
			}

			public override bool IsValid (object value)
			{
				log.Add ("StringLengthAttributePoker.IsValid (object)");
				return base.IsValid (value);
			}
		}

		class DummyNoAttributes
		{ }

		[CustomValidation (typeof (ValidatorTest), "DummyValidationMethod")]
		class Dummy
		{
			[StringLength (10, MinimumLength=2)]
			public string NameField;

			[Required]
			public DummyNoAttributes RequiredDummyField;

			[StringLength (10, MinimumLength = 2)]
			public string NameProperty { get; set; }

			[Required]
			public DummyNoAttributes RequiredDummyProperty { get; set; }
			
			[global::System.ComponentModel.DataAnnotations.RangeAttribute ((int)1, (int)10)]
			public int MinMaxProperty { get; set; }

			[StringLength (10, MinimumLength = 2)]
			[CustomValidation (typeof (ValidatorTest), "CustomValidatedPropertyValidationMethod")]
			public string CustomValidatedProperty { get; set; }

			[CustomValidation (typeof (ValidatorTest), "CustomValidatedPropertyValidationMethod")]
			[StringLength (10, MinimumLength = 2)]
			public string AnotherCustomValidatedProperty { get; set; }

			public bool FailValidation { get; set; }

			public Dummy ()
			{
				NameField = "name";
				NameProperty = "name";
				RequiredDummyField = new DummyNoAttributes ();
				RequiredDummyProperty = new DummyNoAttributes ();
				MinMaxProperty = 5;
				AnotherCustomValidatedProperty = "I'm valid";
			}
		}

		class DummyWithException
		{
			[CustomValidation (typeof (ValidatorTest), "ValidationMethodException")]
			public string AnotherCustomValidatedProperty { get; set; }
		}

		class DummyForValueValidation
		{
			public string DummyNoAttributes;

			public DummyForValueValidation ()
			{
				this.DummyNoAttributes = "I am valid";
			}
		}

		class DummyMultipleCustomValidators
		{
			[CustomValidation (typeof (ValidatorTest), "FirstPropertyValidationMethod")]
			public string FirstProperty { get; set; }

			[CustomValidation (typeof (ValidatorTest), "SecondPropertyValidationMethod")]
			public string SecondProperty { get; set; }
		}
	}
#endif
}
