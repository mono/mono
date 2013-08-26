//
// RegularExpressionAttribute.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//  Antoine Cailliau <a.cailliau@maximux.net>
//
// Copyright (C) 2008 Novell Inc. http://novell.com
// Copyright (C) 20011 Maximux Scris. http://maximux.net
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
using System.Globalization;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace System.ComponentModel.DataAnnotations
{

#if NET_4_0
	[AttributeUsage (AttributeTargets.Property|AttributeTargets.Field|AttributeTargets.Parameter, AllowMultiple = false)]
#else
	[AttributeUsage (AttributeTargets.Property|AttributeTargets.Field, AllowMultiple = false)]	
#endif
	public class RegularExpressionAttribute : ValidationAttribute
	{
		public RegularExpressionAttribute (string pattern)
			: base(GetDefaultErrorMessage)
		{
			if (pattern == null)
				throw new ArgumentNullException("pattern");
			Pattern = pattern;
		}

		public string Pattern { get; private set; }

		static string GetDefaultErrorMessage ()
		{
			return "The field {0} must match the regular expression {1}.";
		}
		
		public override string FormatErrorMessage (string name)
		{
			return string.Format (ErrorMessageString, name, Pattern);
		}

		// LAMESPEC: does not throw ValidationException when value does not match the regular expression
		public override bool IsValid (object value)
		{
			if (value == null) 
				return true;
			
			string str = (string) value;
			Regex regex = new Regex(Pattern);
			return regex.IsMatch(str);
		}
	}
}
