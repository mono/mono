
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Security;
using NUnit.Framework;

namespace MonoTests.System.Web.Security {

	[TestFixture]
	public class MembershipPasswordAttributeTest
	{
		private ValidationContext _validationContext;

		public MembershipPasswordAttributeTest ()
		{
			_validationContext = new ValidationContext (new object ())
				{
					DisplayName = "testDisplay",
					MemberName = "testMember"
				};
		}

		[Test]
		public void IsValid ()
		{
			var passwordAttribute = new MembershipPasswordAttributeTestClass ();
			Assert.IsTrue (passwordAttribute.IsValid (""), "sending an empty password password should should be treated as valid");
		}

		[Test]
		public void IsValid_with_ValidationContext ()
		{
			var passwordAttribute = new MembershipPasswordAttributeTestClass ();
			var result = passwordAttribute.TestValidation ("", _validationContext);
			Assert.IsNull (result, "sending an empty password password should return a null response");

			result = passwordAttribute.TestValidation ("a!12345", _validationContext);
			Assert.AreEqual (ValidationResult.Success, result, "Should suceed with a 7 character length and a single nonalphanumeric");

			// test error priority
			passwordAttribute.MinRequiredPasswordLength = 4;
			passwordAttribute.MinRequiredNonAlphanumericCharacters = 2;
			result = passwordAttribute.TestValidation ("aaa", _validationContext);
			Assert.AreEqual ("The 'testDisplay' field is an invalid password. Password must have 4 or more characters.", result.ErrorMessage);
			
		}

		[Test]
		public void MinRequiredPasswordLength ()
		{
			var passwordAttribute = new MembershipPasswordAttributeTestClass ();
			var result = passwordAttribute.TestValidation ("a!1234", _validationContext);
			Assert.AreEqual ("The 'testDisplay' field is an invalid password. Password must have 7 or more characters.", result.ErrorMessage, "Error message not correct for lower Min characters");
			Assert.AreEqual (_validationContext.MemberName, result.MemberNames.FirstOrDefault (), "Member name not correct");

			passwordAttribute.MinRequiredPasswordLength = 6;
			result = passwordAttribute.TestValidation ("a!1234", _validationContext);
			Assert.AreEqual (ValidationResult.Success, result, "Should suceed with a 6 character length after it's reset");

			result = passwordAttribute.TestValidation ("a!123", _validationContext);
			Assert.AreEqual("The 'testDisplay' field is an invalid password. Password must have 6 or more characters.", result.ErrorMessage, "Error message not correct for Min characters of 6");


			passwordAttribute.MinRequiredPasswordLength = 1;
			result = passwordAttribute.TestValidation ("!", _validationContext);
			Assert.AreEqual(ValidationResult.Success, result, "Should suceed with a 6 character length after it's reset");

			// Note there is no test for empty password here as it returns null and is therefore in the generic test

			// Error Message changes
			passwordAttribute.MinRequiredPasswordLength = 5;
			passwordAttribute.MinPasswordLengthError = "There was an error";
			result = passwordAttribute.TestValidation ("a!13", _validationContext);
			Assert.AreEqual("There was an error", result.ErrorMessage, "Error Message wasn't correct without parameters.");

			passwordAttribute.MinPasswordLengthError = "There was an error parameter1: {0}";
			result = passwordAttribute.TestValidation ("a!13", _validationContext);
			Assert.AreEqual("There was an error parameter1: testDisplay", result.ErrorMessage, "Error Message wasn't correct with 1 parameter.");

			passwordAttribute.MinPasswordLengthError = "There was an error parameter1: {0} parameter2: {1}";
			result = passwordAttribute.TestValidation ("a!13", _validationContext);
			Assert.AreEqual ("There was an error parameter1: testDisplay parameter2: 5", result.ErrorMessage, "Error Message wasn't correct with 2 parameters.");

		}

		[Test]
		public void MinRequiredNonAlphanumericCharacters ()
		{
			var passwordAttribute = new MembershipPasswordAttributeTestClass ();
			var result = passwordAttribute.TestValidation ("a!12345", _validationContext);
			Assert.AreEqual (ValidationResult.Success, result, "Should succeed with the default 1 non alpha numeric");

			result = passwordAttribute.TestValidation ("a123456", _validationContext);
			Assert.AreEqual ("The 'testDisplay' field is an invalid password. Password must have 1 or more non-alphanumeric characters.", result.ErrorMessage, "Expected validation to fail without non-alphanumerics");


			passwordAttribute.MinRequiredNonAlphanumericCharacters = 3;
			result = passwordAttribute.TestValidation ("a!&12345", _validationContext);
			Assert.AreEqual ("The 'testDisplay' field is an invalid password. Password must have 3 or more non-alphanumeric characters.", result.ErrorMessage, "Expected validation to fail without 3 non-alphanumerics");

			result = passwordAttribute.TestValidation ("a!?&132154", _validationContext);
			Assert.AreEqual (ValidationResult.Success, result, "Should succeed with 3 non alpha numerics");

			passwordAttribute.MinRequiredNonAlphanumericCharacters = 0;
			result = passwordAttribute.TestValidation ("a123456", _validationContext);
			Assert.AreEqual (ValidationResult.Success, result, "Should succeed with 0 non alpha numerics");

			// Error Message changes
			passwordAttribute.MinRequiredNonAlphanumericCharacters = 1;
			passwordAttribute.MinNonAlphanumericCharactersError = "There was an error";
			result = passwordAttribute.TestValidation ("a123456", _validationContext);
			Assert.AreEqual ("There was an error", result.ErrorMessage, "Error Message wasn't correct without parameters.");

			passwordAttribute.MinNonAlphanumericCharactersError = "There was an error parameter1: {0}";
			result = passwordAttribute.TestValidation ("a123456", _validationContext);
			Assert.AreEqual("There was an error parameter1: testDisplay", result.ErrorMessage, "Error Message wasn't correct with 1 parameter.");

			passwordAttribute.MinNonAlphanumericCharactersError = "There was an error parameter1: {0} parameter2: {1}";
			result = passwordAttribute.TestValidation ("a123456", _validationContext);
			Assert.AreEqual ("There was an error parameter1: testDisplay parameter2: 1", result.ErrorMessage, "Error Message wasn't correct with 2 parameters.");
		}

		[Test]
		public void FormatErrorMessage ()
		{
			var passwordAttribute = new MembershipPasswordAttribute ();
			Assert.AreEqual ("The field testDisplay2 is invalid.", passwordAttribute.FormatErrorMessage ("testDisplay2"));
		}

		internal class MembershipPasswordAttributeTestClass : MembershipPasswordAttribute
		{
			public ValidationResult TestValidation (object val, ValidationContext context)
			{
				return IsValid (val, context);
			}
		}
	}
}
