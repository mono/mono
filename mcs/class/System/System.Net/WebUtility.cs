//
// System.Net.WebUtility
//
// Author: Mike Kestner  <mkestner@novell.com>
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
using System.IO;
using System.Runtime.Serialization;

namespace System.Net 
{

	public static class WebUtility 
	{
		
		public static string HtmlDecode (string value)
		{
			return HttpUtility.HtmlDecode (value);
		}
		
		public static void HtmlDecode (string value, TextWriter output)
		{
			output.Write (HtmlDecode (value));
		}
		
		public static string HtmlEncode (string value)
		{
			return HttpUtility.HtmlEncode (value);
		}
		
		public static void HtmlEncode (string value, TextWriter output)
		{
			output.Write (HtmlEncode (value));
		}
		
		public static string UrlDecode (string encodedValue)
		{
			return HttpUtility.UrlDecode (encodedValue);
		}
		
		public static byte[] UrlDecodeToBytes (
			byte[] encodedValue, int offset, int count)
		{
			return HttpUtility.UrlDecodeToBytes (encodedValue, offset, count);
		}
		
		public static string UrlEncode (string value)
		{
			return HttpUtility.UrlEncode (value);
		}
		
		public static byte[] UrlEncodeToBytes (
			byte[] value, int offset, int count)
		{
			return HttpUtility.UrlEncodeToBytes (value, offset, count);
		}
	}
}
#endif
