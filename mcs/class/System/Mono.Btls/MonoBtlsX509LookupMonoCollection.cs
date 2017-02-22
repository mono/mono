//
// MonoBtlsX509LookupMonoCollection.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2016 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#if SECURITY_DEP && MONO_FEATURE_BTLS
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

#if MONOTOUCH
using MonoTouch;
#endif

namespace Mono.Btls
{
	internal class MonoBtlsX509LookupMonoCollection : MonoBtlsX509LookupMono
	{
		long[] hashes;
		MonoBtlsX509[] certificates;
		X509CertificateCollection collection;
		MonoBtlsX509TrustKind trust;

		internal MonoBtlsX509LookupMonoCollection (X509CertificateCollection collection, MonoBtlsX509TrustKind trust)
		{
			this.collection = collection;
			this.trust = trust;
		}

		void Initialize ()
		{
			if (certificates != null)
				return;

			hashes = new long [collection.Count];
			certificates = new MonoBtlsX509 [collection.Count];
			for (int i = 0; i < collection.Count; i++) {
				// Create new 'X509 *' instance since we need to modify it to add the
				// trust settings.
				var data = collection [i].GetRawCertData ();
				certificates [i] = MonoBtlsX509.LoadFromData (data, MonoBtlsX509Format.DER);
				certificates [i].AddExplicitTrust (trust);
				hashes [i] = certificates [i].GetSubjectNameHash ();
			}
		}

		protected override MonoBtlsX509 OnGetBySubject (MonoBtlsX509Name name)
		{
			Initialize ();

			var hash = name.GetHash ();
			MonoBtlsX509 found = null;

			for (int i = 0; i < certificates.Length; i++) {
				if (hashes [i] != hash)
					continue;
				found = certificates [i];
				AddCertificate (found);
			}

			return found;
		}

		protected override void Close ()
		{
			try {
				if (certificates != null) {
					for (int i = 0; i < certificates.Length; i++) {
						if (certificates [i] != null) {
							certificates [i].Dispose ();
							certificates [i] = null;
						}
					}
					certificates = null;
					hashes = null;
				}
			} finally {
				base.Close ();
			}
		}
	}
}
#endif
