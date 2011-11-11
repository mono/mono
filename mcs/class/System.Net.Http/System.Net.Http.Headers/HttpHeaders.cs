//
// HttpHeaders.cs
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

using System.Collections;
using System.Collections.Generic;

namespace System.Net.Http.Headers
{
	public abstract class HttpHeaders : IEnumerable<KeyValuePair<string, IEnumerable<string>>>
	{
		public void Add (string name, string value)
		{
		}

		public void Add (string name, IEnumerable<string> values)
		{
		}

		public void AddWithoutValidation (string name, string value)
		{
		}

		public void AddWithoutValidation (string name, IEnumerable<string> values)
		{
		}

		public void Clear ()
		{
		}

		public bool Contains (string name)
		{
			throw new NotImplementedException ();
		}

		public IEnumerator<KeyValuePair<string, IEnumerable<string>>> GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public IEnumerable<string> GetValues (string name)
		{
			throw new NotImplementedException ();
		}

		public bool Remove (string name)
		{
			throw new NotImplementedException ();
		}

		public bool TryGetValues (string name, out IEnumerable<string> values)
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		internal T GetValue<T> (string name)
		{
			return default (T);
		}

		internal void SetValue<T> (string name, T value)
		{
		}

	}
}
