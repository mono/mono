//
// BtlsX509Lookup.cs
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
using System;
using System.IO;
using System.Security.Cryptography;

namespace Mono.Btls.Interface
{
	public class BtlsX509Lookup : BtlsObject
	{
		new internal MonoBtlsX509Lookup Instance {
			get { return (MonoBtlsX509Lookup)base.Instance; }
		}

		internal BtlsX509Lookup (MonoBtlsX509Lookup lookup)
			: base (lookup)
		{
		}

		public void Initialize ()
		{
			Instance.Initialize ();
		}

		public void Shutdown ()
		{
			Instance.Shutdown ();
		}

		public BtlsX509 LookupBySubject (BtlsX509Name name)
		{
			var x509 = Instance.LookupBySubject (name.Instance);
			if (x509 == null)
				return null;

			return new BtlsX509 (x509);
		}

		public void LoadFile (string file, BtlsX509Format type)
		{
			Instance.LoadFile (file, (MonoBtlsX509FileType)type);
		}

		public void AddDirectory (string dir, BtlsX509Format type)
		{
			Instance.AddDirectory (dir, (MonoBtlsX509FileType)type);
		}
	}
}
