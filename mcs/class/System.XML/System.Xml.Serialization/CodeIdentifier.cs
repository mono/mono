//
// System.Xml.Serialization.CodeIdentifier.cs
//
// Author: 
//    Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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

namespace System.Xml.Serialization {
	public class CodeIdentifier {

#if NET_2_0
		[Obsolete ("Design mistake. It only contains static methods.")]
#endif
		public
		CodeIdentifier ()
		{
		}

		public static string MakeCamel (string identifier)
		{
			string validIdentifier = MakeValid (identifier);
			return (Char.ToLower (validIdentifier[0], CultureInfo.InvariantCulture) + validIdentifier.Substring (1));
		}

		public static string MakePascal (string identifier)
		{
			string validIdentifier = MakeValid (identifier);
			return (Char.ToUpper (validIdentifier[0], CultureInfo.InvariantCulture) + validIdentifier.Substring (1));
		}

		public static string MakeValid (string identifier)
		{
			if (identifier == null)
				throw new NullReferenceException ();
			if (identifier.Length == 0)
				return "Item";

			string output = "";

			if (!Char.IsLetter (identifier[0]) && (identifier[0]!='_') )
				output = "Item";

			foreach (char c in identifier) 
				if (Char.IsLetterOrDigit (c) || c == '_')
					output += c;

			if (output.Length > 400)
				output = output.Substring (0,400);
				
			return output;
		}
	}
}
