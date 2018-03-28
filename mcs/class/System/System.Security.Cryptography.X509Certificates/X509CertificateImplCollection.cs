//
// X509CertificateImplCollection.cs
//
// Authors:
//	Martin Baulig  <martin.baulig@xamarin.com>
//
// Copyright (C) 2016 Xamarin, Inc. (http://www.xamarin.com)
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
#if SECURITY_DEP
using System.Collections.Generic;

namespace System.Security.Cryptography.X509Certificates
{
	internal class X509CertificateImplCollection : IDisposable
	{
		List<X509CertificateImpl> list;

		public X509CertificateImplCollection ()
		{
			list = new List<X509CertificateImpl> ();
		}

		X509CertificateImplCollection (X509CertificateImplCollection other)
		{
			list = new List<X509CertificateImpl> ();
			foreach (var impl in other.list)
				list.Add (impl.Clone ());
		}

		public int Count {
			get {
				return list.Count;
			}
		}

		public X509CertificateImpl this[int index] {
			get {
				return list[index];
			}
		}

		public void Add (X509CertificateImpl impl, bool takeOwnership)
		{
			if (!takeOwnership)
				impl = impl.Clone ();
			list.Add (impl);
		}

		public X509CertificateImplCollection Clone ()
		{
			return new X509CertificateImplCollection (this);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			foreach (var impl in list) {
				try {
					impl.Dispose ();
				} catch {
					;
				}
			}
			list.Clear ();
		}

		~X509CertificateImplCollection ()
		{
			Dispose (false);
		}
	}
}
#endif
