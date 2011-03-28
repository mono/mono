//
// ValidationAttributeTest.cs
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
	public class ValidationAttributeTest
	{
		const string TEST_ERROR_MESSAGE = "Test Error Message";
		string ErrorMessageAccessor ()
		{
			return TEST_ERROR_MESSAGE;
		}

		[Test]
		public void Constructor ()
		{
			var attr = new ValidateFooAttribute ();

			Assert.IsNull (attr.ErrorMessage, "#A1");
			Assert.IsNull (attr.ErrorMessageResourceName, "#A2");
			Assert.IsNull (attr.ErrorMessageResourceType, "#A3");
			Assert.IsNotNull (attr.GetErrorMessageString (), "#A4");
		}

		[Test]
		public void Constructor_Func ()
		{
			var attr = new ValidateFooAttribute (ErrorMessageAccessor);

			Assert.IsNull (attr.ErrorMessage, "#A1");
			Assert.IsNull (attr.ErrorMessageResourceName, "#A2");
			Assert.IsNull (attr.ErrorMessageResourceType, "#A3");
			Assert.IsNotNull (attr.GetErrorMessageString (), "#A4");
			Assert.AreEqual (TEST_ERROR_MESSAGE, attr.GetErrorMessageString (), "#A4");
		}

		[Test]
		public void Constructor_String ()
		{
			var attr = new ValidateFooAttribute ("Another Test Error Message");

			Assert.IsNull (attr.ErrorMessage, "#A1");
			Assert.IsNull (attr.ErrorMessageResourceName, "#A2");
			Assert.IsNull (attr.ErrorMessageResourceType, "#A3");
			Assert.IsNotNull (attr.GetErrorMessageString (), "#A4");
			Assert.IsNotNull (attr.GetErrorMessageString (), "#A4-1");
			Assert.AreEqual ("Another Test Error Message", attr.GetErrorMessageString (), "#A4-2");
		}

		[Test]
		public void ErrorMessage ()
		{
			var attr = new ValidateFooAttribute ();

			Assert.IsNull (attr.ErrorMessage, "#A1");

			attr.ErrorMessage = "Test";
			Assert.AreEqual ("Test", attr.ErrorMessage, "#A2");
#if NET_4_0
			attr.ErrorMessage = String.Empty;
			Assert.AreEqual (String.Empty, attr.ErrorMessage, "#A3");

			attr.ErrorMessage = null;
			Assert.IsNull (attr.ErrorMessage, "#A4");
#else
			try {
				attr.ErrorMessage = String.Empty;
				Assert.Fail ("#A3");
			} catch (InvalidOperationException) {
				// success
			}

			attr = new ValidateFooAttribute ("Test");
			try {
				attr.ErrorMessage = null;
				Assert.Fail ("#A4");
			} catch (ArgumentException) {
				// success
			}

			attr = new ValidateFooAttribute ("Test");
			try {
				attr.ErrorMessage = String.Empty;
				Assert.Fail ("#A4");
			} catch (ArgumentException) {
				// success
			}

			attr = new ValidateFooAttribute ();
			attr.ErrorMessageResourceName = "ErrorProperty1";

			try {
				attr.ErrorMessage = "Test Message";
				Assert.Fail ("#E1");
			} catch (InvalidOperationException) {
				// success
			}
#endif
			
		}

		[Test]
		public void ErrorMessageResourceName ()
		{
			var attr = new ValidateFooAttribute ();

			Assert.IsNull (attr.ErrorMessageResourceName, "#A1");

			attr.ErrorMessageResourceName = "Test";
			Assert.IsNotNull (attr.ErrorMessageResourceName, "#A2-1");
			Assert.AreEqual ("Test", attr.ErrorMessageResourceName, "#A2-2");
#if NET_4_0
			attr.ErrorMessageResourceName = String.Empty;
			Assert.IsNotNull (attr.ErrorMessageResourceName, "#A3-1");
			Assert.AreEqual (String.Empty, attr.ErrorMessageResourceName, "#A3-2");

			attr.ErrorMessageResourceName = null;
			Assert.IsNull (attr.ErrorMessageResourceName, "#A3-1");
#else
			try {
				attr.ErrorMessageResourceName = String.Empty;
				Assert.Fail ("#A3-1");
			} catch (InvalidOperationException) {
				// success
			}

			attr = new ValidateFooAttribute ("Test");
			try {
				attr.ErrorMessageResourceName = String.Empty;
				Assert.Fail ("#A3-2");
			} catch (ArgumentException) {
				// success
			}

			attr = new ValidateFooAttribute ("Test");
			try {
				attr.ErrorMessageResourceName = null;
				Assert.Fail ("#A3-3");
			} catch (ArgumentException) {
				// success
			}

			attr = new ValidateFooAttribute ();
			attr.ErrorMessageResourceType = typeof (FooErrorMessageProvider);

			try {
				attr.ErrorMessageResourceName = "NoSuchProperty";
				Assert.Fail ("#A3-4");
			} catch (InvalidOperationException) {
				// success
			}
#endif
		}

		[Test]
		public void ErrorMessageResourceType ()
		{
			var attr = new ValidateFooAttribute ();

			Assert.IsNull (attr.ErrorMessageResourceType, "#A1");

			attr.ErrorMessageResourceType = typeof (FooErrorMessageProvider);
			Assert.IsNotNull (attr.ErrorMessageResourceType, "#A2-1");
			Assert.AreEqual (typeof (FooErrorMessageProvider), attr.ErrorMessageResourceType, "#A2-2");
#if !NET_4_0
			attr = new ValidateFooAttribute ();
			attr.ErrorMessageResourceName = "NoSuchProperty";
			
			try {
				attr.ErrorMessageResourceType = typeof (FooErrorMessageProvider);
				Assert.Fail ("#A3");
			} catch (InvalidOperationException) {
				// success
			}
#endif
		}

		[Test]
		public void ErrorMessageString ()
		{
			var attr = new ValidateFooAttribute ();

			Assert.IsNotNull (attr.GetErrorMessageString (), "#A1-1");
			Assert.IsTrue (attr.GetErrorMessageString ().Length > 0, "#A1-2");

			attr = new ValidateFooAttribute ();
			attr.ErrorMessageResourceName = "TestResource";
			try {
				attr.GetErrorMessageString ();
				Assert.Fail ("#A2-1");
			} catch (InvalidOperationException) {
				// success
			}
#if NET_4_0
			attr = new ValidateFooAttribute ();
			attr.ErrorMessageResourceName = String.Empty;
			try {
				attr.GetErrorMessageString ();
				Assert.Fail ("#A2-1");
			} catch (InvalidOperationException) {
				// success
			}

			attr = new ValidateFooAttribute ();
			attr.ErrorMessageResourceType = typeof (FooErrorMessageProvider);
			attr.ErrorMessageResourceName = null;
			
			try {
				attr.GetErrorMessageString ();
				Assert.Fail ("#A3-1");
			} catch (InvalidOperationException) {
				// success
			}

			attr = new ValidateFooAttribute ();
			attr.ErrorMessageResourceName = String.Empty;
			attr.ErrorMessageResourceType = typeof (FooErrorMessageProvider);
			try {
				string s = attr.GetErrorMessageString ();
				Assert.Fail ("#A3-2");
			} catch (InvalidOperationException) {
				// success
			}

			attr = new ValidateFooAttribute ();
			attr.ErrorMessageResourceName = "NoSuchProperty";
			attr.ErrorMessageResourceType = typeof (FooErrorMessageProvider);
			try {
				attr.GetErrorMessageString ();
				Assert.Fail ("#A4");
			} catch (InvalidOperationException) {
				// success
			}

			attr = new ValidateFooAttribute ();
			attr.ErrorMessageResourceName = "ErrorProperty2";
			attr.ErrorMessageResourceType = typeof (FooErrorMessageProvider);
			try {
				attr.GetErrorMessageString ();
				Assert.Fail ("#A5");
			} catch (InvalidOperationException) {
				// success
			}

			attr = new ValidateFooAttribute ();
			attr.ErrorMessageResourceName = "ErrorProperty3";
			attr.ErrorMessageResourceType = typeof (FooErrorMessageProvider);
			try {
				attr.GetErrorMessageString ();
				Assert.Fail ("#A5");
			} catch (InvalidOperationException) {
				// success
			}

			attr = new ValidateFooAttribute ();
			attr.ErrorMessageResourceName = "ErrorProperty4";
			attr.ErrorMessageResourceType = typeof (FooErrorMessageProvider);
			try {
				attr.GetErrorMessageString ();
				Assert.Fail ("#A6");
			} catch (InvalidOperationException) {
				// success
			}

			attr = new ValidateFooAttribute ();
			attr.ErrorMessageResourceName = "ErrorProperty5";
			attr.ErrorMessageResourceType = typeof (FooErrorMessageProvider);
			try {
				attr.GetErrorMessageString ();
				Assert.Fail ("#A7");
			} catch (InvalidOperationException) {
				// success
			}

			attr = new ValidateFooAttribute ();
			attr.ErrorMessageResourceName = "ErrorField1";
			attr.ErrorMessageResourceType = typeof (FooErrorMessageProvider);
			try {
				attr.GetErrorMessageString ();
				Assert.Fail ("#B1");
			} catch (InvalidOperationException) {
				// success
			}

			attr = new ValidateFooAttribute ();
			attr.ErrorMessageResourceName = "ErrorField2";
			attr.ErrorMessageResourceType = typeof (FooErrorMessageProvider);
			try {
				attr.GetErrorMessageString ();
				Assert.Fail ("#B2");
			} catch (InvalidOperationException) {
				// success
			}
#endif

			attr = new ValidateFooAttribute ();
			attr.ErrorMessageResourceName = "ErrorProperty1";
			attr.ErrorMessageResourceType = typeof (FooErrorMessageProvider);
			Assert.IsNotNull (attr.GetErrorMessageString (), "#C1-1");
			Assert.AreEqual ("Error Message 1", attr.GetErrorMessageString (), "#C1-2");

			attr = new ValidateFooAttribute (ErrorMessageAccessor);
			Assert.IsNotNull (attr.GetErrorMessageString (), "#D1-1");
			Assert.AreEqual (TEST_ERROR_MESSAGE, attr.GetErrorMessageString (), "#D1-2");

			attr = new ValidateFooAttribute ();
			attr.ErrorMessageResourceName = "ErrorProperty1";
			attr.ErrorMessageResourceType = typeof (FooErrorMessageProvider);
			Assert.IsNotNull (attr.GetErrorMessageString (), "#D1-3");
			Assert.AreEqual ("Error Message 1", attr.GetErrorMessageString (), "#D1-4");
#if NET_4_0
			attr.ErrorMessage = "Test Message";
			try {
				attr.GetErrorMessageString ();
				Assert.Fail ("#E1");
			} catch (InvalidOperationException) {
				// success
			}
#endif
		}

		[Test]
		public void FormatErrorMessage ()
		{
			var attr = new ValidateFooAttribute ();

			Assert.IsNotNull (attr.FormatErrorMessage ("SomeField"), "#A1-1");
			Assert.AreEqual ("The field SomeField is invalid.", attr.FormatErrorMessage ("SomeField"), "#A1-2");

			attr.ErrorMessage = "Test: {0}";
			Assert.IsNotNull (attr.FormatErrorMessage ("SomeField"), "#A2-1");
			Assert.AreEqual ("Test: SomeField", attr.FormatErrorMessage ("SomeField"), "#A2-2");
#if !NET_4_0
			attr = new ValidateFooAttribute ();
#else
			attr.ErrorMessage = null;
#endif
			attr.ErrorMessageResourceName = "ErrorProperty1";
			attr.ErrorMessageResourceType = typeof (FooErrorMessageProvider);
			Assert.IsNotNull (attr.FormatErrorMessage ("SomeField"), "#B1-1");
			Assert.AreEqual ("Error Message 1", attr.FormatErrorMessage ("SomeField"), "#B1-2");
#if !NET_4_0
			attr = new ValidateFooAttribute ();
#endif
			attr.ErrorMessageResourceName = "ErrorProperty6";
			attr.ErrorMessageResourceType = typeof (FooErrorMessageProvider);
			Assert.IsNotNull (attr.FormatErrorMessage ("SomeField"), "#B2-1");
			Assert.AreEqual ("Error Message 6: SomeField", attr.FormatErrorMessage ("SomeField"), "#B2-2");
#if !NET_4_0
			attr = new ValidateFooAttribute ();
#endif
			attr.ErrorMessageResourceName = "ErrorProperty6";
			attr.ErrorMessageResourceType = typeof (FooErrorMessageProvider);
			Assert.IsNotNull (attr.FormatErrorMessage ("SomeField"), "#B3-1");
			Assert.AreEqual ("Error Message 6: ", attr.FormatErrorMessage (null), "#B3-2");
		}
#if NET_4_0
		[Test]
		public void GetValidationResult ()
		{
			var attr = new ValidateBarAttribute ();

			try {
				attr.GetValidationResult ("stuff", null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException) {
				// success
			}

			var vc = new ValidationContext ("stuff", null, null);
			vc.DisplayName = "MyStuff";
			var vr = attr.GetValidationResult ("stuff", vc);
			Assert.IsNull (vr, "#A2");

			vr = attr.GetValidationResult (null, vc);
			Assert.IsNotNull(vr, "#A3-1");
			Assert.IsNotNull (vr.ErrorMessage, "#A3-2");
			Assert.AreEqual ("The field MyStuff is invalid.", vr.ErrorMessage, "#A3-3");

			attr.ErrorMessage = "My Error Message: {0}";
			vr = attr.GetValidationResult (null, vc);
			Assert.IsNotNull (vr, "#A4-1");
			Assert.IsNotNull (vr.ErrorMessage, "#A4-2");
			Assert.AreEqual ("My Error Message: MyStuff", vr.ErrorMessage, "#A4-3");

			attr.ErrorMessage = null;
			attr.ErrorMessageResourceName = "ErrorProperty1";
			attr.ErrorMessageResourceType = typeof (FooErrorMessageProvider);
			vr = attr.GetValidationResult (null, vc);
			Assert.IsNotNull (vr, "#A5-1");
			Assert.IsNotNull (vr.ErrorMessage, "#A5-2");
			Assert.AreEqual ("Error Message 1", vr.ErrorMessage, "#A5-3");

			attr.ErrorMessage = "My Error Message: {0}";
			attr.ErrorMessageResourceName = null;
			attr.ErrorMessageResourceType = null;
			vr = attr.GetValidationResult (null, vc);
			Assert.IsNotNull (vr, "#A6-1");
			Assert.IsNotNull (vr.MemberNames, "#A6-2");
			int count = 0;
			foreach (string s in vr.MemberNames)
				count++;
			Assert.AreEqual (0, count, "#A6-3");
			Assert.AreEqual ("My Error Message: MyStuff", vr.ErrorMessage, "#A6-4");

			attr.ValidationResultErrorMessage = "My VR message";
			vr = attr.GetValidationResult (null, vc);
			Assert.IsNotNull (vr, "#A7-1");
			Assert.AreEqual ("My VR message", vr.ErrorMessage, "#A7-2");
		}

		[Test]
		public void IsValid_Object ()
		{
			var attr = new ValidateFooAttribute ();

			AssertExtensions.Throws <NotImplementedException> (() => {
				// It calls IsValid (object, validationContext) which throws the NIEX, but when that overload is called directly, there's
				// no exception.
				//
				// at System.ComponentModel.DataAnnotations.ValidationAttribute.IsValid(Object value, ValidationContext validationContext)
				// at System.ComponentModel.DataAnnotations.ValidationAttribute.IsValid(Object value)
				// at MonoTests.System.ComponentModel.DataAnnotations.ValidationAttributeTest.IsValid_Object() in C:\Users\grendel\Documents\Visual Studio 2010\Projects\System.Web.Test\System.Web.Test\System.ComponentModel.DataAnnotations\ValidationAttributeTest.cs:line 450
				attr.IsValid (null);
			}, "#A1-1");
			
			AssertExtensions.Throws <NotImplementedException> (() => {
				attr.IsValid ("stuff");
			}, "#A1-2");
		}

		[Test]
		public void IsValid_Object_ValidationContext ()
		{
			var attr = new ValidateBarAttribute ();

			AssertExtensions.Throws <NullReferenceException> (() => {
				attr.CallIsValid (null, null);
			}, "#A1");

			var vc = new ValidationContext ("stuff", null, null);
			var vr = attr.CallIsValid (null, vc);
			Assert.IsNotNull (vr, "#A2-1");
			Assert.IsNotNull (vr.ErrorMessage, "#A2-2");
			Assert.AreEqual ("The field String is invalid.", vr.ErrorMessage, "#A2-3");
			Assert.IsNotNull (vr.MemberNames, "#A2-4"); 
			
			int count = 0;
			foreach (string s in vr.MemberNames)
				count++;
			Assert.AreEqual (0, count, "#A2-5");

			vc.MemberName = "SomeMember";
			vr = attr.CallIsValid (null, vc);
			Assert.IsNotNull (vr, "#A3-1");
			Assert.IsNotNull (vr.ErrorMessage, "#A3-2");
			Assert.AreEqual ("The field String is invalid.", vr.ErrorMessage, "#A3-3");
			Assert.IsNotNull (vr.MemberNames, "#A3-4");

			var list = new List <string> ();
			foreach (string s in vr.MemberNames)
				list.Add (s);
			Assert.AreEqual (1, list.Count, "#A3-5");
			Assert.AreEqual ("SomeMember", list [0], "#A3-6");
		}

		[Test]
		public void IsValid_Object_ValidationContext_CrossCallsWithNIEX ()
		{
			var attr = new ValidateSomethingAttribute ();

			AssertExtensions.Throws<NotImplementedException> (() => {
				// Thrown from the IsValid (object, ValidationContext) overload!
				//
				// MonoTests.System.ComponentModel.DataAnnotations.ValidationAttributeTest.IsValid_Object_ValidationContext_02:
				// System.NotImplementedException : IsValid(object value) has not been implemented by this class.  The preferred entry point is GetValidationResult() and classes should override IsValid(object value, ValidationContext context).
				//
				// at System.ComponentModel.DataAnnotations.ValidationAttribute.IsValid(Object value, ValidationContext validationContext)
				// at MonoTests.System.ComponentModel.DataAnnotations.ValidateSomethingAttribute.IsValid(Object value) in C:\Users\grendel\Documents\Visual Studio 2010\Projects\System.Web.Test\System.Web.Test\System.ComponentModel.DataAnnotations\ValidationAttributeTest.cs:line 639
				// at System.ComponentModel.DataAnnotations.ValidationAttribute.IsValid(Object value, ValidationContext validationContext)
				// at MonoTests.System.ComponentModel.DataAnnotations.ValidateSomethingAttribute.IsValid(Object value) in C:\Users\grendel\Documents\Visual Studio 2010\Projects\System.Web.Test\System.Web.Test\System.ComponentModel.DataAnnotations\ValidationAttributeTest.cs:line 639
				// at MonoTests.System.ComponentModel.DataAnnotations.ValidationAttributeTest.IsValid_Object_ValidationContext_02() in C:\Users\grendel\Documents\Visual Studio 2010\Projects\System.Web.Test\System.Web.Test\System.ComponentModel.DataAnnotations\ValidationAttributeTest.cs:line 514
				attr.IsValid ("stuff");
			}, "#A1");

			AssertExtensions.Throws<NotImplementedException> (() => {
				// And this one is thrown from the IsValid (object) overload!
				//
				// MonoTests.System.ComponentModel.DataAnnotations.ValidationAttributeTest.IsValid_Object_ValidationContext_CrossCallsWithNIEX:
				// System.NotImplementedException : IsValid(object value) has not been implemented by this class.  The preferred entry point is GetValidationResult() and classes should override IsValid(object value, ValidationContext context).
				//
				// at System.ComponentModel.DataAnnotations.ValidationAttribute.IsValid(Object value)
				// at MonoTests.System.ComponentModel.DataAnnotations.ValidateSomethingAttribute.IsValid(Object value, ValidationContext validationContext) in C:\Users\grendel\Documents\Visual Studio 2010\Projects\System.Web.Test\System.Web.Test\System.ComponentModel.DataAnnotations\ValidationAttributeTest.cs:line 660
				// at System.ComponentModel.DataAnnotations.ValidationAttribute.IsValid(Object value)
				// at MonoTests.System.ComponentModel.DataAnnotations.ValidateSomethingAttribute.IsValid(Object value, ValidationContext validationContext) in C:\Users\grendel\Documents\Visual Studio 2010\Projects\System.Web.Test\System.Web.Test\System.ComponentModel.DataAnnotations\ValidationAttributeTest.cs:line 660
				// at MonoTests.System.ComponentModel.DataAnnotations.ValidateSomethingAttribute.CallIsValid(Object value, ValidationContext validationContext) in C:\Users\grendel\Documents\Visual Studio 2010\Projects\System.Web.Test\System.Web.Test\System.ComponentModel.DataAnnotations\ValidationAttributeTest.cs:line 667
				// at MonoTests.System.ComponentModel.DataAnnotations.ValidationAttributeTest.IsValid_Object_ValidationContext_CrossCallsWithNIEX() in C:\Users\grendel\Documents\Visual Studio 2010\Projects\System.Web.Test\System.Web.Test\System.ComponentModel.DataAnnotations\ValidationAttributeTest.cs:line 530

				attr.CallIsValid ("stuff", null);
			}, "#A2");
		}
		[Test]
		public void Validate_Object_ValidationContext ()
		{
			var attr = new ValidateBazAttribute ();

			try {
				attr.Validate ("stuff", (ValidationContext) null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException) {
				// success
			}

			var vc = new ValidationContext ("stuff", null, null);
			try {
				attr.Validate (null, vc);
				Assert.Fail ("#A2-1");
			} catch (ValidationException) {
				// success
			}
			Assert.AreEqual (3, attr.Calls.Count, "#A2-1");
			Assert.AreEqual ("ValidationResult IsValid (object value, ValidationContext validationContext)", attr.Calls [0], "#A2-2");
			Assert.AreEqual ("bool IsValid (object value)", attr.Calls [1], "#A2-3");
			Assert.AreEqual ("string FormatErrorMessage (string name)", attr.Calls [2], "#A2-4");
		}
#endif
		[Test]
		public void Validate_Object_String ()
		{
			var attr = new ValidateBazAttribute ();

			try {
				attr.Validate (null, (string) null);
				Assert.Fail ("#A2");
			} catch (ValidationException) {
				// success
			}

			Assert.AreEqual (2, attr.Calls.Count, "#A2-1");
			Assert.AreEqual ("bool IsValid (object value)", attr.Calls [0], "#A2-2");
			Assert.AreEqual ("string FormatErrorMessage (string name)", attr.Calls [1], "#A2-3");
		}
	}

	class ValidateFooAttribute : ValidationAttribute
	{
		public ValidateFooAttribute ()
			: base ()
		{ }

		public ValidateFooAttribute (Func<string> errorMessageAccessor)
			: base (errorMessageAccessor)
		{ }

		public ValidateFooAttribute (string errorMessage)
			: base (errorMessage)
		{ }

		public string GetErrorMessageString ()
		{
			return ErrorMessageString;
		}
#if !NET_4_0
		public override bool IsValid (object value)
		{
			return value != null;
		}
#endif
	}

	class ValidateBarAttribute : ValidateFooAttribute
	{
		public string ValidationResultErrorMessage
		{
			get;
			set;
		}

		public override bool IsValid (object value)
		{
			return value != null;
		}
#if NET_4_0
		protected override ValidationResult IsValid (object value, ValidationContext validationContext)
		{
			if (!IsValid (value))
				return new ValidationResult (ValidationResultErrorMessage);
			return null;
		}

		public ValidationResult CallIsValid (object value, ValidationContext validationContext)
		{
			return base.IsValid (value, validationContext);
		}
#endif
	}

	class ValidateBazAttribute : ValidateBarAttribute
	{
		public readonly List<string> Calls = new List<string> ();

		public override bool IsValid (object value)
		{
			Calls.Add ("bool IsValid (object value)");
			return base.IsValid (value);
		}
#if NET_4_0
		protected override ValidationResult IsValid (object value, ValidationContext validationContext)
		{
			Calls.Add ("ValidationResult IsValid (object value, ValidationContext validationContext)");
			return base.IsValid (value, validationContext);
		}
#endif
		public override string FormatErrorMessage (string name)
		{
			Calls.Add ("string FormatErrorMessage (string name)");
			return base.FormatErrorMessage (name);
		}
	}
#if NET_4_0
	class ValidateSomethingAttribute : ValidationAttribute
	{
		public override bool IsValid (object value)
		{
			return base.IsValid (value, null) == ValidationResult.Success;
		}

		protected override ValidationResult IsValid (object value, ValidationContext validationContext)
		{
			if (base.IsValid (value))
				return ValidationResult.Success;
			return new ValidationResult ("failed to validate in base class");
		}

		public ValidationResult CallIsValid (object value, ValidationContext validationContext)
		{
			return IsValid (value, validationContext);
		}
	}
#endif
	class FooErrorMessageProvider
	{
		public static string ErrorProperty1
		{
			get { return "Error Message 1"; }
		}

		public static int ErrorProperty2
		{
			get { return 1; }
		}

		public string ErrorProperty3
		{
			get { return "Error Message 2"; }
		}

		protected static string ErrorProperty4
		{
			get { return "Error Message 3"; }
		}

		public static string ErrorProperty5
		{
			set { }
		}

		public static string ErrorProperty6
		{
			get { return "Error Message 6: {0}"; }
		}

		public string ErrorField1 = "Error Message 4";
		public static string ErrorField2 = "Error Message 5";
	}
}
