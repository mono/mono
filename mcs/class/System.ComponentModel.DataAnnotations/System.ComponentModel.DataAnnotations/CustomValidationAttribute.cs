//
// CustomValidationAttribute.cs
//
// Authors:
//	Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell Inc. (http://novell.com)
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

namespace System.ComponentModel.DataAnnotations
{
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = true)]
	public sealed class CustomValidationAttribute : ValidationAttribute
	{
		public string Method { get; private set; }
		public override object TypeId {
			get {
				throw new NotImplementedException ();
			}
		}
		public Type ValidatorType { get; private set; }
		
		public CustomValidationAttribute (Type validatorType, string method)
		{
			this.ValidatorType = validatorType;
			this.Method = method;
		}

		public override string FormatErrorMessage (string name)
		{
			throw new NotImplementedException ();
		}

		// LAMESPEC: MSDN doesn't document it at all, but corcompare shows it in the type
		protected override ValidationResult IsValid (object value, ValidationContext validationContext)
		{
			throw new NotImplementedException ();
		}
	}
}
#endif