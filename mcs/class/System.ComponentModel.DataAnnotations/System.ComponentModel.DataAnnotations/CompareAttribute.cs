//
// CompareAttribute.cs
//
// Authors:
//	Pablo Ruiz García <pablo.ruiz@gmail.com>
//
// Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)
// Copyright (C) 2013 Pablo Ruiz García
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

#if NET_4_5

using System;
using System.Linq;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;

namespace System.ComponentModel.DataAnnotations
{
	[AttributeUsageAttribute (AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
	public class CompareAttribute : ValidationAttribute
	{
		private const string DefaultErrorMessage = "'{0}' and '{1}' do not match.";
		private const string NonExistingPropertyErrorMessage = "Could not find a property named {0}.";
		private string _otherProperty;
		private string _otherPropertyDisplayName;

		public CompareAttribute (string otherProperty)
			: base (() => DefaultErrorMessage)
		{
			if (string.IsNullOrEmpty (otherProperty))
				throw new ArgumentNullException ("otherProperty");

			_otherProperty = otherProperty;
		}

		public string OtherProperty { get { return _otherProperty; } }
		public string OtherPropertyDisplayName { get { return _otherPropertyDisplayName; } }
		public override bool RequiresValidationContext { get { return true; } }

		private IEnumerable<Attribute> GetPropertyAttributes (Type type, string propertyName)
		{
#if MOBILE
			return TypeDescriptor.GetProperties (type).Find (propertyName, false).Attributes.OfType<Attribute> ();
#else
			// Using AMTTDP seems the way to go to be able to relay on attributes declared
			// by means of associated classes not directly decorating the property.
			// See: http://msdn.microsoft.com/en-us/library/system.componentmodel.dataannotations.associatedmetadatatypetypedescriptionprovider.aspx
			return new AssociatedMetadataTypeTypeDescriptionProvider (type)
				.GetTypeDescriptor (type)
				.GetProperties ()
				.Find (propertyName, false)
				.Attributes.OfType<Attribute> ();
#endif
		}

		private void ResolveOtherPropertyDisplayName (ValidationContext context)
		{
			if (_otherPropertyDisplayName == null)
			{
				// NOTE: From my own tests, it seems MS.NET looksup displayName from various sources, what follows
				// 	 is a best guess from my on tests, however, I am probably missing some corner cases. (pruiz)
				var attributes = GetPropertyAttributes (context.ObjectType, _otherProperty);
				var displayAttr = attributes.FirstOrDefault (x => x is DisplayAttribute) as DisplayAttribute;
				var displayNameAttr = attributes.FirstOrDefault (x => x is DisplayNameAttribute) as DisplayNameAttribute;

				if (displayAttr != null) _otherPropertyDisplayName = displayAttr.GetName ();
				else if (displayNameAttr != null) _otherPropertyDisplayName = displayNameAttr.DisplayName;
				_otherPropertyDisplayName = _otherProperty;
			}
		}

		public override string FormatErrorMessage (string name)
		{
			var oname = string.IsNullOrEmpty (_otherPropertyDisplayName) ? _otherProperty : _otherPropertyDisplayName;
			return string.Format (ErrorMessageString, name, oname);
		}

		protected override ValidationResult IsValid(object value, ValidationContext context)
		{
			var property = context.ObjectType.GetProperty (_otherProperty);

			if (property == null) {
				string message = string.Format (NonExistingPropertyErrorMessage, _otherProperty);
				return new ValidationResult (message);
			}

			// XXX: Could not find a better place to call this, as this is 
			// 	the only place we have access to a ValidationContext. (pruiz)
			ResolveOtherPropertyDisplayName (context);

			return object.Equals (property.GetValue (context.ObjectInstance, null), value) ? null
				: new ValidationResult (FormatErrorMessage (context.DisplayName));
		}
	}
}

#endif
