//
// System.Net.HttpListenerPrefixCollection.cs
//
// Author:
//	Rolf Bjarne Kvinge <rolf@xamarin.com>
//
// Copyright (C) 2016 Xamarin Inc (http://www.xamarin.com)
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

namespace System.Net {
	public class HttpListenerPrefixCollection : ICollection<string>, IEnumerable<string>, IEnumerable
	{
		const string EXCEPTION_MESSAGE = "System.Net.HttpListenerPrefixCollection is not supported on the current platform.";

		HttpListenerPrefixCollection ()
		{
		}

		public int Count {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool IsReadOnly {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool IsSynchronized {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public void Add (string uriPrefix)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void Clear ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public bool Contains (string uriPrefix)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void CopyTo (string [] array, int offset)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void CopyTo (Array array, int offset)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public IEnumerator<string> GetEnumerator ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public bool Remove (string uriPrefix)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}
	}
}
