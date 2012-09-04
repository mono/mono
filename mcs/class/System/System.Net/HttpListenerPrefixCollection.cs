//
// System.Net.HttpListenerPrefixCollection.cs
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
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
#if SECURITY_DEP || EMBEDDED_IN_1_0

using System.Collections;
using System.Collections.Generic;
namespace System.Net {
#if EMBEDDED_IN_1_0
	public class HttpListenerPrefixCollection : IEnumerable, ICollection {
		ArrayList prefixes;
		
#else
	public class HttpListenerPrefixCollection : ICollection<string>, IEnumerable<string>, IEnumerable {
		List<string> prefixes = new List<string> ();
#endif
		HttpListener listener;

		internal HttpListenerPrefixCollection (HttpListener listener)
		{
			this.listener = listener;
		}

		public int Count {
			get { return prefixes.Count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public bool IsSynchronized {
			get { return false; }
		}

		public void Add (string uriPrefix)
		{
			listener.CheckDisposed ();
			ListenerPrefix.CheckUri (uriPrefix);
			if (prefixes.Contains (uriPrefix))
				return;

			prefixes.Add (uriPrefix);
			if (listener.IsListening)
				EndPointManager.AddPrefix (uriPrefix, listener);
		}

		public void Clear ()
		{
			listener.CheckDisposed ();
			prefixes.Clear ();
			if (listener.IsListening)
				EndPointManager.RemoveListener (listener);
		}

		public bool Contains (string uriPrefix)
		{
			listener.CheckDisposed ();
			return prefixes.Contains (uriPrefix);
		}

		public void CopyTo (string [] array, int offset)
		{
			listener.CheckDisposed ();
			prefixes.CopyTo (array, offset);
		}

		public void CopyTo (Array array, int offset)
		{
			listener.CheckDisposed ();
			((ICollection) prefixes).CopyTo (array, offset);
		}

#if !EMBEDDED_IN_1_0
		public IEnumerator<string> GetEnumerator ()
		{
			return prefixes.GetEnumerator ();
		}
#endif
	
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return prefixes.GetEnumerator ();
		}

		public bool Remove (string uriPrefix)
		{
			listener.CheckDisposed ();
			if (uriPrefix == null)
				throw new ArgumentNullException ("uriPrefix");

			bool result = prefixes.Remove (uriPrefix);
			if (result && listener.IsListening)
				EndPointManager.RemovePrefix (uriPrefix, listener);

			return result;
		}
	}
}
#endif

