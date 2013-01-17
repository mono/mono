//
// FormUrlEncodedContent.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)
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
using System.Net.Http.Headers;
using System.Text;

namespace System.Net.Http
{
	public class FormUrlEncodedContent : ByteArrayContent
	{
		public FormUrlEncodedContent (IEnumerable<KeyValuePair<string, string>> nameValueCollection)
			: base (EncodeContent (nameValueCollection))
		{
			Headers.ContentType = new MediaTypeHeaderValue ("application/x-www-form-urlencoded");
		}

		static byte[] EncodeContent (IEnumerable<KeyValuePair<string, string>> nameValueCollection)
		{
			if (nameValueCollection == null)
				throw new ArgumentNullException ("nameValueCollection");

			//
			// Serialization as application/x-www-form-urlencoded
			//
			// Element nodes selected for inclusion are encoded as EltName=value{sep}, where = is a literal
			// character, {sep} is the separator character from the separator attribute on submission,
			// EltName represents the element local name, and value represents the contents of the text node.
			//
			// The encoding of EltName and value are as follows: space characters are replaced by +, and then
			// non-ASCII and reserved characters (as defined by [RFC 2396] as amended by subsequent documents
			// in the IETF track) are escaped by replacing the character with one or more octets of the UTF-8
			// representation of the character, with each octet in turn replaced by %HH, where HH represents
			// the uppercase hexadecimal notation for the octet value and % is a literal character. Line breaks
			// are represented as "CR LF" pairs (i.e., %0D%0A).
			//
			var sb = new List<byte> ();
			foreach (var item in nameValueCollection) {
				if (sb.Count != 0)
					sb.Add ((byte) '&');

				var data = SerializeValue (item.Key);
				if (data != null)
					sb.AddRange (data);
				sb.Add ((byte) '=');

				data = SerializeValue (item.Value);
				if (data != null)
					sb.AddRange (data);
			}

			return sb.ToArray ();
		}

		static byte[] SerializeValue (string value)
		{
			if (value == null)
				return null;

			value = Uri.EscapeDataString (value).Replace ("%20", "+");
			return Encoding.ASCII.GetBytes (value);
		}
	}
}
