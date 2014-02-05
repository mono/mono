//
// MembershipPasswordAttribute.cs
//
// Authors:
//	Matthias Dittrich <matthi.d@gmail.com>
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
using System.Globalization;
using System.Linq;
using System.Runtime;
using System.Text.RegularExpressions;

namespace System.Web.Security {
	[AttributeUsage (AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
	public class MembershipPasswordAttribute : ValidationAttribute {
		private static String default_MinPasswordLength = "{0} must contain at least {1} characters";
		private static String default_MinNonAlphanumericCharacters = "{0} must contain at least {1} special characters";
		private static String default_PasswordStrength = "{0} is too weak";

		private String error_MinPasswordLength = default_MinPasswordLength;
		private String error_MinNonAlphanumericCharacters = default_MinNonAlphanumericCharacters;
		private String error_PasswordStrength = default_PasswordStrength;
		private int? minRequiredPasswordLength;
		private int? minRequiredNonAlphanumericCharacters;
		private string passwordStrengthRegularExpression;
		private Type resourceType;

		public int MinRequiredPasswordLength
		{
			get
			{
				if (!this.minRequiredPasswordLength.HasValue)
					return Membership.Provider.MinRequiredPasswordLength;
				else
					return this.minRequiredPasswordLength.Value;
			}
			set
			{
				this.minRequiredPasswordLength = value;
			}
		}

		public int MinRequiredNonAlphanumericCharacters
		{
			get
			{
				if (!this.minRequiredNonAlphanumericCharacters.HasValue)
					return Membership.Provider.MinRequiredNonAlphanumericCharacters;
				else
					return this.minRequiredNonAlphanumericCharacters.Value;
			}
			set
			{
				this.minRequiredNonAlphanumericCharacters = value;
			}
		}

		public string PasswordStrengthRegularExpression
		{
			get
			{
				return this.passwordStrengthRegularExpression ?? Membership.Provider.PasswordStrengthRegularExpression;
			}
			set
			{
				this.passwordStrengthRegularExpression = value;
			}
		}

		public Type ResourceType
		{
			get
			{
				return this.resourceType;
			}
			set
			{
				this.resourceType = value;
			}
		}

		public string MinPasswordLengthError
		{
			get
			{
				return this.error_MinPasswordLength;
			}
			set
			{
				this.error_MinPasswordLength = value;
			}
		}

		public string MinNonAlphanumericCharactersError
		{
			get
			{
				return this.error_MinNonAlphanumericCharacters;
			}
			set
			{
				this.error_MinNonAlphanumericCharacters = value;
			}
		}

		public string PasswordStrengthError
		{
			get
			{
				return this.error_PasswordStrength;
			}
			set
			{
				this.error_PasswordStrength = value;
			}
		}

		protected override ValidationResult IsValid (object value, ValidationContext validationContext)
		{
			string input = value as string;
			string name = validationContext != null ? validationContext.DisplayName : string.Empty;
			string [] memberNames;
			if (validationContext == null)
				memberNames = null;
			else
				memberNames = new [] { validationContext.MemberName };

			if (string.IsNullOrEmpty (input))
				return ValidationResult.Success;

			if (input.Length < this.MinRequiredPasswordLength)
				return
					new ValidationResult (
						this.FormatErrorMessage (this.error_MinPasswordLength ?? default_MinPasswordLength, name, this.MinRequiredPasswordLength),
						memberNames);

			if (input.Count ((c) => !char.IsLetterOrDigit (c)) < this.MinRequiredNonAlphanumericCharacters)
				return
					new ValidationResult (
						this.FormatErrorMessage (this.error_MinNonAlphanumericCharacters ?? default_MinNonAlphanumericCharacters, name, this.MinRequiredNonAlphanumericCharacters),
						memberNames);

			string regularExpression = this.PasswordStrengthRegularExpression;
			if (regularExpression != null) {
				Regex regex;
				try {
					regex = new Regex (regularExpression);
				} catch (ArgumentException ex) {
					throw new InvalidOperationException ("The provided regular Expression is invalid", ex);
				}
				if (!regex.IsMatch (input))
					return new ValidationResult (this.FormatErrorMessage (this.error_PasswordStrength ?? default_PasswordStrength, name, string.Empty), memberNames);
			}
			return ValidationResult.Success;
		}

		public override string FormatErrorMessage (string name)
		{
			return this.FormatErrorMessage (this.ErrorMessageString, name, string.Empty);
		}
		
		private string FormatErrorMessage (string errorMessageString, string name, object additionalArgument)
		{
			return
				string.Format (
					(IFormatProvider) CultureInfo.CurrentCulture,
					errorMessageString,
					name, additionalArgument);
		}
	}
}
