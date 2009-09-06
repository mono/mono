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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//

using System;
using System.IO;
using System.Security.RightsManagement;

namespace System.IO.Packaging {

	public class EncryptedPackageEnvelope : IDisposable
	{
		internal EncryptedPackageEnvelope ()
		{
		}

		protected virtual void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		public FileAccess FileOpenAccess {
			get { throw new NotImplementedException (); }
		}

		public PackageProperties PackageProperties {
			get { throw new NotImplementedException (); }
		}

		public RightsManagementInformation RightsManagementInformation {
			get { throw new NotImplementedException (); }
		}

		public StorageInfo StorageInfo {
			get { throw new NotImplementedException (); }
		}

		public void Close ()
		{
			throw new NotImplementedException ();
		}
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}
		public void Flush ()
		{
			throw new NotImplementedException ();
		}

		public Package GetPackage ()
		{
			throw new NotImplementedException ();
		}

		public static bool IsEncryptedPackageEnvelope (Stream stream)
		{
			throw new NotImplementedException ();
		}
		public static bool IsEncryptedPackageEnvelope (string fileName)
		{
			throw new NotImplementedException ();
		}

		public static EncryptedPackageEnvelope Create (Stream envelopeStream, PublishLicense publishLicense, CryptoProvider cryptoProvider)
		{
			throw new NotImplementedException ();
		}
		public static EncryptedPackageEnvelope Create (string envelopeFileName, PublishLicense publishLicense, CryptoProvider cryptoProvider)
		{
			throw new NotImplementedException ();
		}
		public static EncryptedPackageEnvelope CreateFromPackage (Stream envelopeStream, Stream packageStream, PublishLicense publishLicense, CryptoProvider cryptoProvider)
		{
			throw new NotImplementedException ();
		}
		public static EncryptedPackageEnvelope CreateFromPackage (string envelopeFileName, Stream packageStream, PublishLicense publishLicense, CryptoProvider cryptoProvider)
		{
			throw new NotImplementedException ();
		}

		public static EncryptedPackageEnvelope Open (Stream envelopeStream)
		{
			throw new NotImplementedException ();
		}
		public static EncryptedPackageEnvelope Open (string envelopeFileName)
		{
			throw new NotImplementedException ();
		}
		public static EncryptedPackageEnvelope Open (string envelopeFileName, FileAccess access)
		{
			throw new NotImplementedException ();
		}
		public static EncryptedPackageEnvelope Open (string envelopeFileName, FileAccess access, FileShare sharing)
		{
			throw new NotImplementedException ();
		}
	}

}

