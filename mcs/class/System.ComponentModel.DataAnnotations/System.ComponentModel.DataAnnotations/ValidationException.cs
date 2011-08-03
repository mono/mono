//
// ValidationException.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2008-2011 Novell Inc. http://novell.com
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
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.ComponentModel.DataAnnotations
{
	[Serializable]
	public class ValidationException : Exception
	{
		public ValidationException ()
		{
		}
		public ValidationException (string message)
			: base (message)
		{
		}
		public ValidationException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

		public ValidationException (string errorMessage, ValidationAttribute validatingAttribute, object value)
			: base (errorMessage)
		{
			ValidationAttribute = validatingAttribute;
			Value = value;
		}

		protected ValidationException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			throw new NotImplementedException ();
		}
#if NET_4_0
		public ValidationException (ValidationResult validationResult, ValidationAttribute validatingAttribute, object value)
			: this (validationResult != null ? validationResult.ErrorMessage : null, validatingAttribute, value)
		{
			this.ValidationResult = validationResult;
		}

		public ValidationResult ValidationResult { get; private set; }
#endif
		public ValidationAttribute ValidationAttribute { get; private set; }
		public object Value { get; private set; }

		[SecurityPermission (SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
