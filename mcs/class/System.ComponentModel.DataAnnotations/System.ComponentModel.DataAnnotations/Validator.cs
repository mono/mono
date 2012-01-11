//
// Authors:
//      Marek Habersack <grendel@twistedcode.net>
//
// Copyright (C) 2011 Novell Inc. http://novell.com
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace System.ComponentModel.DataAnnotations
{
	// TODO: we could probably use some kind of type cache here
	public static class Validator
	{
		public static bool TryValidateObject (object instance, ValidationContext validationContext, ICollection <ValidationResult> validationResults)
		{
			return TryValidateObject (instance, validationContext, validationResults, false);
		}

		public static bool TryValidateObject (object instance, ValidationContext validationContext, ICollection <ValidationResult> validationResults, bool validateAllProperties)
		{
			if (instance == null)
				throw new ArgumentNullException ("instance");

			if (validationContext == null)
				throw new ArgumentNullException ("validationContext");

			if (!Object.ReferenceEquals (instance, validationContext.ObjectInstance))
				throw new ArgumentException ("The instance provided must match the ObjectInstance on the ValidationContext supplied.", "instance");

			bool valid = true;
			Type instanceType = instance.GetType ();
			TypeDescriptor.GetAttributes (instanceType).Validate <ValidationAttribute> (instance, validationContext, validationResults, ref valid);
			
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties (instance);
			if (properties != PropertyDescriptorCollection.Empty && properties.Count > 0) {
				foreach (PropertyDescriptor pdesc in properties) {
					object value = pdesc.GetValue (instance);
					ValidateProperty (pdesc, value, validationContext, validationResults, validateAllProperties, ref valid);
				}
			}
			
			return valid;
		}

		static void ValidateProperty (PropertyDescriptor pdesc, object value, ValidationContext validationContext, ICollection <ValidationResult> validationResults,
					      bool validateAll, ref bool valid)
		{
			AttributeCollection attributes = pdesc.Attributes;
			attributes.Validate <RequiredAttribute> (value, validationContext, validationResults, ref valid);
			if (validateAll)
				attributes.ValidateExcept <RequiredAttribute> (value, validationContext, validationResults, ref valid);
		}
		
		static PropertyDescriptor GetProperty (Type type, string propertyName, object value)
		{
			if (String.IsNullOrEmpty (propertyName))
				throw new ArgumentNullException ("propertyName");

			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties (type);
			PropertyDescriptor pdesc = null;
			if (properties != PropertyDescriptorCollection.Empty && properties.Count > 0)
				pdesc = properties.Find (propertyName, false);

			if (pdesc == null)
				throw new ArgumentException (String.Format ("The type '{0}' does not contain a public property named '{1}'.", type.Name, propertyName), "propertyName");

			Type valueType = value == null ? null : value.GetType ();
			Type propertyType = pdesc.PropertyType;
			bool invalidType = false;

			Console.WriteLine ("valueType == {0}; propertyType == {1} (reference? {2})", valueType == null ? "<null>" : valueType.FullName,
					   propertyType, !propertyType.IsValueType || (Nullable.GetUnderlyingType (propertyType) != null));
			if (valueType == null)
				invalidType = !(!propertyType.IsValueType || (Nullable.GetUnderlyingType (propertyType) != null));
			else if (propertyType != valueType)
				invalidType = true;

			if (invalidType)
				throw new ArgumentException (String.Format ("The value of property '{0}' must be of type '{1}'.", propertyName, type.FullName), "propertyName");
			
			return pdesc;
		}
		
		public static bool TryValidateProperty (object value, ValidationContext validationContext, ICollection <ValidationResult> validationResults)
		{
			// LAMESPEC: value can be null, validationContext must not
			if (validationContext == null)
				throw new ArgumentNullException ("validationContext");

			PropertyDescriptor pdesc = GetProperty (validationContext.ObjectType, validationContext.MemberName, value);
			if (value == null)
				return true;

			bool valid = true;
			ValidateProperty (pdesc, value, validationContext, validationResults, true, ref valid);

			return valid;
		}

		public static bool TryValidateValue (object value, ValidationContext validationContext, ICollection<ValidationResult> validationResults,
						     IEnumerable <ValidationAttribute> validationAttributes)
		{
			if (validationContext == null)
				throw new ArgumentNullException ("validationContext");

			ValidationResult result;
			
			// It appears .NET makes this call before checking whether
			// validationAttributes is null...
			ValidationAttribute vattr = validationAttributes.FirstOrDefault <ValidationAttribute> (attr => attr is RequiredAttribute);
			if (vattr != null) {
				result = vattr.GetValidationResult (value, validationContext);
				if (result != ValidationResult.Success) {
					if (validationResults != null)
						validationResults.Add (result);
					return false;
				}
			}

			if (validationAttributes == null)
				return true;

			bool valid = true;
			foreach (ValidationAttribute attr in validationAttributes) {
				if (attr == null || (attr is RequiredAttribute))
					continue;
				
				result = attr.GetValidationResult (value, validationContext);
				if (result != ValidationResult.Success) {
					valid = false;
					if (validationResults != null)
						validationResults.Add (result);
				}
			}
			
			return valid;
		}

		public static void ValidateObject (object instance, ValidationContext validationContext)
		{
			ValidateObject (instance, validationContext, false);
		}

		public static void ValidateObject (object instance, ValidationContext validationContext, bool validateAllProperties)
		{
			if (instance == null)
				throw new ArgumentNullException ("instance");
			if (validationContext == null)
				throw new ArgumentNullException ("validationContext");

			var validationResults = new List <ValidationResult> ();
			if (TryValidateObject (instance, validationContext, validationResults, validateAllProperties))
				return;

			ValidationResult result = validationResults.Count > 0 ? validationResults [0] : null;
			throw new ValidationException (result, null, instance);
		}

		public static void ValidateProperty (object value, ValidationContext validationContext)
		{
			if (validationContext == null)
				throw new ArgumentNullException ("validationContext");

			var validationResults = new List <ValidationResult> ();
			if (TryValidateProperty (value, validationContext, validationResults))
				return;

			ValidationResult result = validationResults.Count > 0 ? validationResults [0] : null;
			throw new ValidationException (result, null, value);
		}

		public static void ValidateValue (object value, ValidationContext validationContext, IEnumerable <ValidationAttribute> validationAttributes)
		{
			if (validationContext == null)
				throw new ArgumentNullException ("validationContext");

			var validationResults = new List <ValidationResult> ();
			if (TryValidateValue (value, validationContext, validationResults, validationAttributes))
				return;

			ValidationResult result = validationResults.Count > 0 ? validationResults [0] : null;
			throw new ValidationException (result, null, value);
		}
	}
}
