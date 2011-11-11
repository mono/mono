//
// TransferCodingHeaderValue.cs
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

using System.Collections.Generic;
using System.Linq;

namespace System.Net.Http.Headers
{
	public class TransferCodingHeaderValue
	{
		readonly string value;
		ICollection<NameValueHeaderValue> parameters;

		public TransferCodingHeaderValue (string value)
		{
			this.value = value;
		}

		public ICollection<NameValueHeaderValue> Parameters {
			get {
				return parameters ?? (parameters = new List<NameValueHeaderValue> ());
			}
		}

		public string Value {
			get {
				return value;
			}
		}

		public override bool Equals (object obj)
		{
			var fchv = obj as TransferCodingHeaderValue;
			return fchv != null &&
				string.Equals (value, fchv.value, StringComparison.OrdinalIgnoreCase) &&
				Enumerable.SequenceEqual (parameters, fchv.parameters);
		}

		public override int GetHashCode ()
		{
			return value.GetHashCode ();
		}

		//public static TransferCodingHeaderValue Parse (string input);
//		public override string ToString ();
		//public static bool TryParse (string input, out TransferCodingHeaderValue parsedValue);
	}
}
