//
// ValidationAttribute.cs
//
// Authors:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2008-2010 Novell Inc. http://novell.com
//

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
using System.ComponentModel;
using System.Reflection;

namespace System.ComponentModel.DataAnnotations
{
	public abstract class ValidationAttribute : Attribute
	{
		const string DEFAULT_ERROR_MESSAGE = "The field {0} is invalid.";
#if !NET_4_0
		string errorMessageResourceName;
		string errorMessageString;
		Type errorMessageResourceType;
#endif
		string errorMessage;
		string fallbackErrorMessage;
		Func <string> errorMessageAccessor;
		
		protected ValidationAttribute ()
		{
		}

		protected ValidationAttribute (Func<string> errorMessageAccessor)
		{
			this.errorMessageAccessor = errorMessageAccessor;
		}

		protected ValidationAttribute (string errorMessage)
		{
			fallbackErrorMessage = errorMessage;
		}

		public virtual string FormatErrorMessage (string name)
		{
			string format = ErrorMessageString;
			if (String.IsNullOrEmpty (format))
				return String.Empty;

			return String.Format (ErrorMessageString, name);
		}
#if NET_4_0
		public string ErrorMessage {
			get { return errorMessage; }
			set {
				errorMessage = value;
				if (errorMessage != null)
					errorMessageAccessor = null;
			}
		}
		public string ErrorMessageResourceName { get; set; }
		public Type ErrorMessageResourceType { get; set; }
#else
		public string ErrorMessage {
			get { return errorMessage; }

			set {
#if !NET_4_0
				if (errorMessage != null)
					throw new InvalidOperationException ("This property can be set only once.");
#endif
				if (String.IsNullOrEmpty (value))
					throw new ArgumentException ("Value cannot be null or empty.", "value");

				if (errorMessageResourceName != null || errorMessageResourceType != null)
					throw new InvalidOperationException ("This property cannot be set because the attribute is already in the resource mode.");
				
				errorMessage = value;
			}
		}

		public string ErrorMessageResourceName {
			get { return errorMessageResourceName; }
			
			set {
				if (errorMessageResourceName != null)
					throw new InvalidOperationException ("This property can be set only once.");

				if (String.IsNullOrEmpty (value))
					throw new ArgumentException ("Value cannot be null or empty.", "value");

				errorMessageResourceName = value;
				if (errorMessageResourceType != null)
					errorMessageString = GetStringFromResourceAccessor ();
			}
		}

		public Type ErrorMessageResourceType {
			get { return errorMessageResourceType; }
			set {
				errorMessageResourceType = value;
				if (!String.IsNullOrEmpty (errorMessageResourceName))
					errorMessageString = GetStringFromResourceAccessor ();
			}
		}
#endif		
		protected string ErrorMessageString {
			get { return GetStringFromResourceAccessor (); }
		}
#if NET_4_0
		public virtual bool IsValid (object value)
		{
			throw new NotImplementedException ("IsValid(object value) has not been implemented by this class.  The preferred entry point is GetValidationResult() and classes should override IsValid(object value, ValidationContext context).");
		}

		protected virtual ValidationResult IsValid (object value, ValidationContext validationContext)
		{
			// .NET emulation
			if (validationContext == null)
				throw new NullReferenceException (".NET emulation.");
			
			if (!IsValid (value)) {
				string memberName = validationContext.MemberName;
				return new ValidationResult (FormatErrorMessage (validationContext.DisplayName), memberName != null ? new string[] { memberName } : new string[] {});
			}

			return ValidationResult.Success;
		}
#else
		public abstract bool IsValid (object value);
#endif

#if NET_4_0
		public ValidationResult GetValidationResult (object value, ValidationContext validationContext)
		{
			if (validationContext == null)
				throw new ArgumentNullException ("validationContext");

			ValidationResult ret = IsValid (value, validationContext);
			if (ret != null && String.IsNullOrEmpty (ret.ErrorMessage))
				ret.ErrorMessage = FormatErrorMessage (validationContext.DisplayName);
				
			return ret;
		}
#endif
		string GetStringFromResourceAccessor ()
		{
			string resourceName = ErrorMessageResourceName;
			Type resourceType = ErrorMessageResourceType;
			string errorMessage = ErrorMessage;

			if (resourceName != null && errorMessage != null)
				throw new InvalidOperationException ("Either ErrorMessage or ErrorMessageResourceName must be set, but not both.");
			
			if (resourceType == null ^ resourceName == null)
				throw new InvalidOperationException ("Both ErrorMessageResourceType and ErrorMessageResourceName must be set on this attribute.");

			
			
			if (resourceType != null) {
				PropertyInfo pi = resourceType.GetProperty (resourceName, BindingFlags.Public | BindingFlags.Static);
				if (pi == null || !pi.CanRead)
					throw new InvalidOperationException (
						String.Format ("Resource type '{0}' does not have an accessible static property named '{1}'.",
							       resourceType, resourceName)
					);

				if (pi.PropertyType != typeof (string))
					throw new InvalidOperationException (
						String.Format ("The property '{0}' on resource type '{1}' is not a string type.",
							       resourceName, resourceType)
					);
				
				return pi.GetValue (null, null) as string;
			}
			
			if (errorMessage == null) {
				if (errorMessageAccessor != null)
					return errorMessageAccessor ();
				
				if (fallbackErrorMessage != null)
					return fallbackErrorMessage;
				else
					return DEFAULT_ERROR_MESSAGE;
			}
			
			return errorMessage;
		}
#if NET_4_0
		public void Validate (object value, ValidationContext validationContext)
		{
			if (validationContext == null)
				throw new ArgumentNullException ("validationContext");

			ValidationResult result = IsValid (value, validationContext);
			if (result != null) {
				string message = result.ErrorMessage;
				if (message == null)
					message = FormatErrorMessage (validationContext.DisplayName);
				
				throw new ValidationException (message, this, value);
			}
		}
#endif
		public void Validate (object value, string name)
		{
			if (!IsValid (value))
				throw new ValidationException (FormatErrorMessage (name), this, value);
		}
	}
}
