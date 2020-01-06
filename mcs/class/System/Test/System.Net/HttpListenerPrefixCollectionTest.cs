//
// HttpListenerPrefixCollectionTest.cs
//	- Unit tests for System.Net.HttpListenePrefixCollection
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Net;
using NUnit.Framework;
using HLPC=System.Net.HttpListenerPrefixCollection;

using MonoTests.Helpers;

namespace MonoTests.System.Net {
	[TestFixture]
	public class HttpListenerPrefixCollectionTest {
		// NL -> Not listening -> tests when listener.IsListening == false
		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void NL_DefaultProperties ()
		{
			HttpListener listener = new HttpListener ();
			HLPC coll = listener.Prefixes;
			Assert.AreEqual (0, coll.Count, "Count");
			Assert.IsFalse (coll.IsReadOnly, "IsReadOnly");
			Assert.IsFalse (coll.IsSynchronized, "IsSynchronized");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void DefaultProperties ()
		{
			HttpListener listener = new HttpListener ();
			HLPC coll = listener.Prefixes;
			coll.Add ("http://127.0.0.1:8181/");
			Assert.AreEqual (1, coll.Count, "Count");
			Assert.IsFalse (coll.IsReadOnly, "IsReadOnly");
			Assert.IsFalse (coll.IsSynchronized, "IsSynchronized");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void AddOne ()
		{
			var port = NetworkHelpers.FindFreePort ();
			HttpListener listener = new HttpListener ();
			HLPC coll = listener.Prefixes;
			listener.Start ();
			coll.Add ($"http://127.0.0.1:{port}/");
			Assert.AreEqual (1, coll.Count, "Count");
			Assert.IsFalse (coll.IsReadOnly, "IsReadOnly");
			Assert.IsFalse (coll.IsSynchronized, "IsSynchronized");
			listener.Stop ();
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Duplicate ()
		{
			var port = NetworkHelpers.FindFreePort ();
			HttpListener listener = new HttpListener ();
			HLPC coll = listener.Prefixes;
			coll.Add ($"http://127.0.0.1:{port}/");
			coll.Add ($"http://127.0.0.1:{port}/");
			listener.Start ();
			Assert.AreEqual (1, coll.Count, "Count");
			Assert.IsFalse (coll.IsReadOnly, "IsReadOnly");
			Assert.IsFalse (coll.IsSynchronized, "IsSynchronized");
			listener.Stop ();
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void EndsWithSlash ()
		{
			HttpListener listener = new HttpListener ();
			listener.Prefixes.Add ("http://127.0.0.1:7777/");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void DifferentPath ()
		{
			HttpListener listener = new HttpListener ();
			listener.Prefixes.Add ("http://127.0.0.1:7777/");
			listener.Prefixes.Add ("http://127.0.0.1:7777/hola/");
			Assert.AreEqual (2, listener.Prefixes.Count, "#01");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void NL_Clear ()
		{
			HttpListener listener = new HttpListener ();
			HLPC coll = listener.Prefixes;
			coll.Clear ();
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void NL_Remove ()
		{
			HttpListener listener = new HttpListener ();
			HLPC coll = listener.Prefixes;
			Assert.IsFalse (coll.Remove ("http://127.0.0.1:8181/"));
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void NL_RemoveBadUri ()
		{
			HttpListener listener = new HttpListener ();
			HLPC coll = listener.Prefixes;
			Assert.IsFalse (coll.Remove ("httpblah://127.0.0.1:8181/"));
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#else
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void NL_AddBadUri ()
		{
			HttpListener listener = new HttpListener ();
			HLPC coll = listener.Prefixes;
			coll.Add ("httpblah://127.0.0.1:8181/");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#else
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void NoHostInUrl ()
		{
			HttpListener listener = new HttpListener ();
			listener.Prefixes.Add ("http://:7777/hola/");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void MultipleSlashes ()
		{
			// this one throws on Start(), not when adding it.
			// See same test name in HttpListenerTest.
			HttpListener listener = new HttpListener ();
			HLPC coll = listener.Prefixes;
			coll.Add ("http://localhost:7777/hola////");
			string [] strs = new string [1];
			coll.CopyTo (strs, 0);
			Assert.AreEqual ("http://localhost:7777/hola////", strs [0]);
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void PercentSign ()
		{
			HttpListener listener = new HttpListener ();
			HLPC coll = listener.Prefixes;
			// this one throws on Start(), not when adding it.
			// See same test name in HttpListenerTest.
			coll.Add ("http://localhost:7777/hola%3E/");
			string [] strs = new string [1];
			coll.CopyTo (strs, 0);
			Assert.AreEqual ("http://localhost:7777/hola%3E/", strs [0]);
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Disposed1 ()
		{
			HttpListener listener = new HttpListener ();
			HLPC coll = listener.Prefixes;
			listener.Close ();
			Assert.AreEqual (0, coll.Count, "Count");
			Assert.IsFalse (coll.IsReadOnly, "IsReadOnly");
			Assert.IsFalse (coll.IsSynchronized, "IsSynchronized");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#else
		[ExpectedException (typeof (ObjectDisposedException))]
#endif
		public void Disposed2 ()
		{
			HttpListener listener = new HttpListener ();
			HLPC coll = listener.Prefixes;
			listener.Close ();
			coll.Add ("http://localhost:7777/hola/");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#else
		[ExpectedException (typeof (ObjectDisposedException))]
#endif
		public void Disposed3 ()
		{
			HttpListener listener = new HttpListener ();
			HLPC coll = listener.Prefixes;
			listener.Close ();
			coll.Clear ();
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#else
		[ExpectedException (typeof (ObjectDisposedException))]
#endif
		public void Disposed4 ()
		{
			HttpListener listener = new HttpListener ();
			HLPC coll = listener.Prefixes;
			listener.Close ();
			coll.Remove ("http://localhost:7777/hola/");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#else
		[ExpectedException (typeof (ObjectDisposedException))]
#endif
		public void Disposed5 ()
		{
			HttpListener listener = new HttpListener ();
			HLPC coll = listener.Prefixes;
			listener.Close ();
			string [] strs = new string [0];
			coll.CopyTo (strs, 0);
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Disposed6 ()
		{
			HttpListener listener = new HttpListener ();
			HLPC coll = listener.Prefixes;
			listener.Close ();
			string a = null;
			foreach (string s in coll) {
				a = s; // just to make the compiler happy
			}
			Assert.IsNull (a);
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Disposed7 ()
		{
			HttpListener listener = new HttpListener ();
			HLPC coll = listener.Prefixes;
			coll.Add ("http://127.0.0.1/");
			listener.Close ();
			int items = 0;
			foreach (string s in coll) {
				items++;
				Assert.AreEqual (s, "http://127.0.0.1/");
			}
			Assert.AreEqual (items, 1);
		}
	}
}

