//
// HttpMethod.cs
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

namespace System.Net.Http
{
	public class HttpMethod : IEquatable<HttpMethod>
	{
		static readonly HttpMethod delete_method = new HttpMethod ("DELETE");
		static readonly HttpMethod get_method = new HttpMethod ("GET");
		static readonly HttpMethod head_method = new HttpMethod ("HEAD");
		static readonly HttpMethod options_method = new HttpMethod ("OPTIONS");
		static readonly HttpMethod post_method = new HttpMethod ("POST");
		static readonly HttpMethod put_method = new HttpMethod ("PUT");
		static readonly HttpMethod trace_method = new HttpMethod ("TRACE");

		readonly string method;

		public HttpMethod (string method)
		{
			if (string.IsNullOrEmpty (method))
				throw new ArgumentException ("method");

			Headers.Parser.Token.Check (method);

			this.method = method;
		}

		public static HttpMethod Delete {
			get {
				return delete_method;
			}
		}

		public static HttpMethod Get {
			get {
				return get_method;
			}
		}

		public static HttpMethod Head {
			get {
				return head_method;
			}
		}

		public string Method {
			get {
				return method;
			}
		}

		public static HttpMethod Options {
			get {
				return options_method;
			}
		}

		public static HttpMethod Post {
			get {
				return post_method;
			}
		}

		public static HttpMethod Put {
			get {
				return put_method;
			}
		}

		public static HttpMethod Trace {
			get {
				return trace_method;
			}
		}

		public static bool operator == (HttpMethod left, HttpMethod right)
		{
			if ((object) left == null || (object) right == null)
				return ReferenceEquals (left, right);

			return left.Equals (right);
		}

		public static bool operator != (HttpMethod left, HttpMethod right)
		{
			return !(left == right);
		}

		public bool Equals(HttpMethod other)
		{
			return string.Equals (method, other.method, StringComparison.OrdinalIgnoreCase);
		}

		public override bool Equals (object obj)
		{
			var other = obj as HttpMethod;
			return !ReferenceEquals (other, null) && Equals (other);
		}

		public override int GetHashCode ()
		{
			return method.GetHashCode ();
		}

		public override string ToString ()
		{
			return method;
		}

		// NS2.1:
		public static System.Net.Http.HttpMethod Patch => throw new PlatformNotSupportedException ();
	}
}
