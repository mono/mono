//
// EmailAddressAttribute.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//      Pablo Ruiz García <pablo.ruiz@gmail.com>
//
// Copyright (C) 2013 Xamarin Inc (http://www.xamarin.com)
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
using System.Globalization;
using System.Text.RegularExpressions;

namespace System.ComponentModel.DataAnnotations
{
	[AttributeUsageAttribute (AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
	public class EmailAddressAttribute : DataTypeAttribute
	{
		private const string DefaultErrorMessage = "The {0} field is not a valid e-mail address.";
		const string AtomCharacters = "!#$%&'*+-/=?^_`{|}~";

		static bool IsLetterOrDigit (char c)
		{
			return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9');
		}

		static bool IsAtom (char c)
		{
			return IsLetterOrDigit (c) || AtomCharacters.IndexOf (c) != -1;
		}

		static bool IsDomain (char c)
		{
			return IsLetterOrDigit (c) || c == '-';
		}

		static bool SkipAtom (string text, ref int index)
		{
			int startIndex = index;

			while (index < text.Length && IsAtom (text[index]))
				index++;

			return index > startIndex;
		}

		static bool SkipSubDomain (string text, ref int index)
		{
			if (!IsDomain (text[index]) || text[index] == '-')
				return false;

			index++;

			while (index < text.Length && IsDomain (text[index]))
				index++;

			return true;
		}

		static bool SkipDomain (string text, ref int index)
		{
			if (!SkipSubDomain (text, ref index))
				return false;

			while (index < text.Length && text[index] == '.') {
				index++;

				if (index == text.Length)
					return false;

				if (!SkipSubDomain (text, ref index))
					return false;
			}

			return true;
		}

		static bool SkipQuoted (string text, ref int index)
		{
			bool escaped = false;

			// skip over leading '"'
			index++;

			while (index < text.Length) {
				if (text[index] == (byte) '\\') {
					escaped = !escaped;
				} else if (!escaped) {
					if (text[index] == (byte) '"')
						break;
				} else {
					escaped = false;
				}

				index++;
			}

			if (index >= text.Length || text[index] != (byte) '"')
				return false;

			index++;

			return true;
		}

		static bool SkipWord (string text, ref int index)
		{
			if (text[index] == (byte) '"')
				return SkipQuoted (text, ref index);

			return SkipAtom (text, ref index);
		}

		static bool SkipIPv4Literal (string text, ref int index)
		{
			int groups = 0;

			while (index < text.Length && groups < 4) {
				int startIndex = index;
				int value = 0;

				while (index < text.Length && text[index] >= '0' && text[index] <= '9') {
					value = (value * 10) + (text[index] - '0');
					index++;
				}

				if (index == startIndex || index - startIndex > 3 || value > 255)
					return false;

				groups++;

				if (groups < 4 && index < text.Length && text[index] == '.')
					index++;
			}

			return groups == 4;
		}

		static bool IsHexDigit (char c)
		{
			return (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f') || (c >= '0' && c <= '9');
		}

		// This needs to handle the following forms:
		//
		// IPv6-addr = IPv6-full / IPv6-comp / IPv6v4-full / IPv6v4-comp
		// IPv6-hex  = 1*4HEXDIG
		// IPv6-full = IPv6-hex 7(":" IPv6-hex)
		// IPv6-comp = [IPv6-hex *5(":" IPv6-hex)] "::" [IPv6-hex *5(":" IPv6-hex)]
		//             ; The "::" represents at least 2 16-bit groups of zeros
		//             ; No more than 6 groups in addition to the "::" may be
		//             ; present
		// IPv6v4-full = IPv6-hex 5(":" IPv6-hex) ":" IPv4-address-literal
		// IPv6v4-comp = [IPv6-hex *3(":" IPv6-hex)] "::"
		//               [IPv6-hex *3(":" IPv6-hex) ":"] IPv4-address-literal
		//             ; The "::" represents at least 2 16-bit groups of zeros
		//             ; No more than 4 groups in addition to the "::" and
		//             ; IPv4-address-literal may be present
		static bool SkipIPv6Literal (string text, ref int index)
		{
			bool compact = false;
			int colons = 0;

			while (index < text.Length) {
				int startIndex = index;

				while (index < text.Length && IsHexDigit (text[index]))
					index++;

				if (index >= text.Length)
					break;

				if (index > startIndex && colons > 2 && text[index] == '.') {
					// IPv6v4
					index = startIndex;

					if (!SkipIPv4Literal (text, ref index))
						return false;

					break;
				}

				int count = index - startIndex;
				if (count > 4)
					return false;

				if (text[index] != ':')
					break;

				startIndex = index;
				while (index < text.Length && text[index] == ':')
					index++;

				count = index - startIndex;
				if (count > 2)
					return false;

				if (count == 2) {
					if (compact)
						return false;

					compact = true;
					colons += 2;
				} else {
					colons++;
				}
			}

			if (colons < 2)
				return false;

			if (compact)
				return colons < 6;

			return colons < 7;
		}

		static bool Validate (string email)
		{
			int index = 0;

			if (email.Length == 0)
				return false;

			if (!SkipWord (email, ref index) || index >= email.Length)
				return false;

			while (index < email.Length && email[index] == '.') {
				index++;

				if (!SkipWord (email, ref index) || index >= email.Length)
					return false;
			}

			if (index + 1 >= email.Length || email[index++] != '@')
				return false;

			if (email[index] != '[') {
				// domain
				if (!SkipDomain (email, ref index))
					return false;

				return index == email.Length;
			}

			// address literal
			index++;

			// we need at least 8 more characters
			if (index + 8 >= email.Length)
				return false;

			var ipv6 = email.Substring (index, 5);
			if (ipv6.ToLowerInvariant () == "ipv6:") {
				index += "IPv6:".Length;
				if (!SkipIPv6Literal (email, ref index))
					return false;
			} else {
				if (!SkipIPv4Literal (email, ref index))
					return false;
			}

			if (index >= email.Length || email[index++] != ']')
				return false;

			return index == email.Length;
		}

		public EmailAddressAttribute ()
			: base(DataType.EmailAddress)
		{
			// XXX: There is no .ctor accepting Func<string> on DataTypeAttribute.. :?
			base.ErrorMessage = DefaultErrorMessage;
		}

		public override bool IsValid(object value)
		{
			if (value == null)
				return true;

			string email = value as string;
			if (email == null)
				return false;

			return Validate (email);
		}
	}
}

#endif
