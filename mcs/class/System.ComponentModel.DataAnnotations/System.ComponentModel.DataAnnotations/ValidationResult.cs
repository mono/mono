//
// ValidationResult.cs
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
using System;
using System.Collections.Generic;

namespace System.ComponentModel.DataAnnotations
{
	public class ValidationResult
	{
		public static readonly ValidationResult Success = null; // it is supposed to be null

		public string ErrorMessage { get; set; }
		public IEnumerable<string> MemberNames { get; private set; }
		
		public ValidationResult (string errorMessage)
		: this (errorMessage, new string[] {})
			
		{
		}

		protected ValidationResult (ValidationResult validationResult)
		{
		}

		public ValidationResult (string errorMessage, IEnumerable<string> memberNames)
		{
			ErrorMessage = errorMessage;
			if (memberNames != null)
				MemberNames = memberNames;
			else
				MemberNames = new string[] {};
		}

#if NET_4_5
		public override string ToString ()
		{
			// See: http://msdn.microsoft.com/en-us/library/system.componentmodel.dataannotations.validationresult.tostring.aspx
			if (!string.IsNullOrEmpty (ErrorMessage))
				return ErrorMessage;

			return base.ToString ();
		}
#endif
	}
}
