//
// CollectionExtensions.cs
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
using System.Text;

namespace System.Net.Http.Headers
{
	static class CollectionExtensions
	{
		public static bool SequenceEqual<TSource> (this List<TSource> first, List<TSource> second)
		{
			if (first == null)
				return second == null || second.Count == 0;

			if (second == null)
				return first == null || first.Count == 0;

			return Enumerable.SequenceEqual (first, second);
		}

		public static void SetValue (this List<NameValueHeaderValue> parameters, string key, string value)
		{
			for (int i = 0; i < parameters.Count; ++i) {
				var entry = parameters[i];
				if (!string.Equals (entry.Name, key, StringComparison.OrdinalIgnoreCase))
					continue;

				if (value == null) {
					parameters.RemoveAt (i);
				} else {
					parameters[i].Value = value;
				}

				return;
			}

			if (!string.IsNullOrEmpty (value))
				parameters.Add (new NameValueHeaderValue (key, value));
		}

		public static string ToString<T> (this List<T> list)
		{
			if (list == null || list.Count == 0)
				return null;

			const string separator = "; ";

			var sb = new StringBuilder ();
			for (int i = 0; i < list.Count; ++i) {
				sb.Append (separator);
				sb.Append (list [i]);
			}

			return sb.ToString ();
		}

		public static void ToStringBuilder<T> (this List<T> list, StringBuilder sb)
		{
			if (list == null || list.Count == 0)
				return;

			const string separator = ", ";

			for (int i = 0; i < list.Count; ++i) {
				if (i > 0) {
					sb.Append (separator);
				}

				sb.Append (list[i]);
			}
		}
	}
}
