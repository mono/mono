//
// CustomValidationAttribute.cs
//
// Authors:
//	Marek Habersack <grendel@twistedcode.net>
//
// Copyright (C) 2010-2011 Novell Inc. (http://novell.com)
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
#if NET_4_0
using System;
using System.Collections.Generic;
using System.Reflection;

namespace System.ComponentModel.DataAnnotations
{
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = true)]
	public sealed class CustomValidationAttribute : ValidationAttribute
	{
		Tuple <string, Type> typeId;
		MethodInfo validationMethod;
		bool validationMethodChecked;
		bool validationMethodSignatureValid;
		int validationMethodParamCount;
		
		public string Method { get; private set; }

		public override object TypeId {
			get { return typeId; }
		}
		
		public Type ValidatorType { get; private set; }
		
		public CustomValidationAttribute (Type validatorType, string method)
		{
			this.ValidatorType = validatorType;
			this.Method = method;
			this.typeId = new Tuple <string, Type> (method, validatorType);
		}

		public override string FormatErrorMessage (string name)
		{
			ThrowIfAttributeNotWellFormed ();
			return String.Format ("{0} is not valid.", name);
		}

		// LAMESPEC: MSDN doesn't document it at all, but corcompare shows it in the type
		protected override ValidationResult IsValid (object value, ValidationContext validationContext)
		{
			ThrowIfAttributeNotWellFormed ();
			object[] p;
				
			if (validationMethodParamCount == 2)
				p = new object [] {value, validationContext};
			else
				p = new object [] {value};
			try {
				return validationMethod.Invoke (null, p) as ValidationResult;
			} catch (TargetInvocationException ex) {
				if (ex.InnerException != null)
					throw ex.InnerException;
				throw;
			}
		}

		void ThrowIfAttributeNotWellFormed ()
		{
			Type type = ValidatorType;
			if (type == null)
				throw new InvalidOperationException ("The CustomValidationAttribute.ValidatorType was not specified.");

			if (type.IsNotPublic)
				throw new InvalidOperationException (String.Format ("The custom validation type '{0}' must be public.", type.Name));

			string method = Method;
			if (String.IsNullOrEmpty (method))
				throw new InvalidOperationException ("The CustomValidationAttribute.Method was not specified.");

			if (validationMethod == null) {
				if (!validationMethodChecked) {
					validationMethod = type.GetMethod (method, BindingFlags.Public | BindingFlags.Static);
					validationMethodChecked = true;
				}
				
				if (validationMethod == null)
					throw new InvalidOperationException (
						String.Format ("The CustomValidationAttribute method '{0}' does not exist in type '{1}' or is not public and static.",
							       method, type.Name));

				if (!typeof (ValidationResult).IsAssignableFrom (validationMethod.ReturnType))
					throw new InvalidOperationException (String.Format ("The CustomValidationAttribute method '{0}' in type '{1}' must return System.ComponentModel.DataAnnotations.ValidationResult.  Use System.ComponentModel.DataAnnotations.ValidationResult.Success to represent success.", method, type.Name));

				validationMethodSignatureValid = true;
				ParameterInfo[] parameters = validationMethod.GetParameters ();
				if (parameters == null)
					validationMethodSignatureValid = false;
				else {
					validationMethodParamCount = parameters.Length;
					switch (validationMethodParamCount) {
						case 1:
							break;

						case 2:
							if (parameters [1].ParameterType != typeof (ValidationContext))
								validationMethodSignatureValid = false;
							break;

						default:
							validationMethodSignatureValid = false;
							break;
					}
				}
			}
			
			if (!validationMethodSignatureValid)
				throw new InvalidOperationException (String.Format ("The CustomValidationAttribute method '{0}' in type '{1}' must match the expected signature: public static ValidationResult MethodTwo(object value, ValidationContext context).  The value can be strongly typed.  The ValidationContext parameter is optional.", method, type.Name));
			
		}
	}
}
#endif