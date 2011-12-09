//
// Parser.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2011 Xamarin Inc (http://www.xamarin.com)
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

using System.Net.Mail;

namespace System.Net.Http.Headers
{
	static class Parser
	{
		public static class Token
		{
			public static bool TryParse (string input, out string result)
			{
				throw new NotImplementedException ();
			}

			public static void Check (string s)
			{
				if (s == null)
					throw new ArgumentNullException ();

				if (!Lexer.IsValidToken (s)) {
					if (s.Length == 0)
						throw new ArgumentException ();

					throw new FormatException (s);
				}
			}
		}

		public static class DateTime
		{
			public static bool TryParse (string input, out DateTimeOffset result)
			{
				throw new NotImplementedException ();
			}
		}

		public static class EmailAddress
		{
			public static bool TryParse (string input, out string result)
			{
				try {
					new MailAddress (input);
					result = input;
					return true;
				} catch {
					result = null;
					return false;
				}
			}
		}

		public static class Int
		{
			public static bool TryParse (string input, out int result)
			{
				throw new NotImplementedException ();
			}
		}

		public static class TimeSpanSeconds
		{
			public static bool TryParse (string input, out TimeSpan result)
			{
				throw new NotImplementedException ();
			}
		}

		public static class Uri
		{
			public static bool TryParse (string input, out System.Uri result)
			{
				throw new NotImplementedException ();
			}
		}
	}
}
