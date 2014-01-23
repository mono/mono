//
// System.Configuration.StringValidator.cs
//
// Authors:
//  Lluis Sanchez Gual (lluis@novell.com)
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
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

namespace System.Configuration
{
	public class StringValidator: ConfigurationValidatorBase
	{
		char[] invalidCharacters;
		int maxLength;
		int minLength;
		
		public StringValidator (int minLength)
		{
			this.minLength = minLength;
			maxLength = int.MaxValue;
		}
		
		public StringValidator (int minLength, int maxLength)
		{
			this.minLength = minLength;
			this.maxLength = maxLength;
		}
		
		public StringValidator (int minLength, int maxLength, string invalidCharacters)
		{
			this.minLength = minLength;
			this.maxLength = maxLength;
			if (invalidCharacters != null)
				this.invalidCharacters = invalidCharacters.ToCharArray ();
		}
		
		public override bool CanValidate (Type type)
		{
			return type == typeof(string);
		}

		public override void Validate (object value)
		{
			if (value == null && minLength <= 0)
				return;

			string s = (string) value;
			if (s == null || s.Length < minLength)
				throw new ArgumentException ("The string must be at least " + minLength + " characters long.");
			if (s.Length > maxLength)
				throw new ArgumentException ("The string must be no more than " + maxLength + " characters long.");
			if (invalidCharacters != null) {
				int i = s.IndexOfAny (invalidCharacters);
				if (i != -1)
					throw new ArgumentException (String.Format ("The string cannot contain any of the following characters: '{0}'.", invalidCharacters));
			}
		}
	}
}
