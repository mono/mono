//
// HttpHeaderValueCollection.cs
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
using System.Collections;

namespace System.Net.Http.Headers
{
	public sealed class HttpHeaderValueCollection<T> : ICollection<T> where T : class
	{
		readonly List<T> list;
		readonly HttpHeaders headers;
		readonly HeaderInfo headerInfo;
		List<string> invalidValues;

		internal HttpHeaderValueCollection (HttpHeaders headers, HeaderInfo headerInfo)
		{
			list = new List<T> ();
			this.headers = headers;
			this.headerInfo = headerInfo;
		}

		public int Count {
			get {
				return list.Count;
			}
		}

		internal List<string> InvalidValues {
			get {
				return invalidValues;
			}
		}

		public bool IsReadOnly {
			get {
				return false;
			}
		}

		public void Add (T item)
		{
			list.Add (item);
		}

		internal void AddRange (List<T> values)
		{
			list.AddRange (values);
		}

		internal void AddInvalidValue (string invalidValue)
		{
			if (invalidValues == null)
				invalidValues = new List<string> ();

			invalidValues.Add (invalidValue);
		}

		public void Clear ()
		{
			list.Clear ();
			invalidValues = null;
		}

		public bool Contains (T item)
		{
			return list.Contains (item);
		}

		public void CopyTo (T[] array, int arrayIndex)
		{
			list.CopyTo (array, arrayIndex);
		}

		public void ParseAdd (string input)
		{
			headers.AddValue (input, headerInfo, false);
		}

		public bool Remove (T item)
		{
			return list.Remove (item);
		}

		public override string ToString ()
		{
			var res = string.Join (headerInfo.Separator, list);

			if (invalidValues != null)
				res += string.Join (headerInfo.Separator, invalidValues);

			return res;
		}

		public bool TryParseAdd (string input)
		{
			return headers.AddValue (input, headerInfo, true);
		}

		public IEnumerator<T> GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		internal T Find (Predicate<T> predicate)
		{
			return list.Find (predicate);
		}

		internal void Remove (Predicate<T> predicate)
		{
			T item = Find (predicate);
			if (item != null)
				Remove (item);
		}
	}
}
