//
// FileCertificateStore.cs: Handles a file-based certificate store.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

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
using System.IO;

using Mono.Security.Authenticode;
using Mono.Security.X509;
using Mono.Security.X509.Stores;

namespace Mono.Security.X509.Stores {

	public class FileCertificateStore : ICertificateStore {

		private string _name;
		private string _location;
		private bool _readOnly;
		private bool _createIfRequired;
		private bool _includeArchives;
		private bool _saveOnClose;
		private SoftwarePublisherCertificate _spc;

		public FileCertificateStore ()
		{
			_readOnly = true;
			_includeArchives = false;
			_createIfRequired = true;
		}

		// properties

		public X509CertificateCollection Certificates {
			get { 
				if (_spc != null)
					return _spc.Certificates; 
				return null;
			}
		}
                
		public IntPtr Handle {
			get { return (IntPtr) 0; }
		}

		// methods

		public void Open (string name, string location, bool readOnly, bool createIfNonExisting, bool includeArchives) 
		{
			_name = name;
			_location = _location;
			_readOnly = readOnly;
			_createIfRequired = createIfNonExisting;
			_includeArchives = includeArchives;
			_saveOnClose = false;

			if (File.Exists (_name)) {
				_spc = SoftwarePublisherCertificate.CreateFromFile (_name);
			}
			else if (_createIfRequired) {
				_spc = new SoftwarePublisherCertificate ();
				_saveOnClose = true;
			}
		}

		public void Close () 
		{
			if (_saveOnClose) {
				byte[] store = _spc.GetBytes ();
				using (FileStream fs = File.Create (_name)) {
					fs.Write (store, 0, store.Length);
				}
			}
		}
		
		public void Add (X509Certificate certificate) 
		{
			if ((!_readOnly) && (_spc != null)) {
				_spc.Certificates.Add (certificate);
				_saveOnClose = true;
			}
		}
		
		public void Remove (X509Certificate certificate) 
		{
			if ((!_readOnly) && (_spc != null)) {
				_spc.Certificates.Remove (certificate);
				_saveOnClose = true;
			}
		}
	}
}
