//
// System.Security.Permissions.KeyContainerPermissionAccessEntry class
//
// Author
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System.Globalization;
using System.Security.Cryptography;

namespace System.Security.Permissions {

	[Serializable]
	public sealed class KeyContainerPermissionAccessEntry {

		private KeyContainerPermissionFlags _flags;
		private string _containerName;
		private int _spec;
		private string _store;
		private string _providerName;
		private int _type;


		public KeyContainerPermissionAccessEntry (CspParameters csp, KeyContainerPermissionFlags flags)
		{
			if (csp == null)
				throw new ArgumentNullException ("csp");

			ProviderName = csp.ProviderName;
			ProviderType = csp.ProviderType;
			KeyContainerName = csp.KeyContainerName;
			KeySpec = csp.KeyNumber;
			Flags = flags;
		}

		public KeyContainerPermissionAccessEntry (string keyContainerName, KeyContainerPermissionFlags flags)
		{
			KeyContainerName = keyContainerName;
			Flags = flags;
		}

		public KeyContainerPermissionAccessEntry (string keyStore, string providerName, int providerType, 
			string keyContainerName, int keySpec, KeyContainerPermissionFlags flags)
		{
			KeyStore = keyStore;
			ProviderName = providerName;
			ProviderType = providerType;
			KeyContainerName = keyContainerName;
			KeySpec = keySpec;
			Flags = flags;
		}


		public KeyContainerPermissionFlags Flags {
			get { return _flags; }
			set {
				if ((value & KeyContainerPermissionFlags.AllFlags) != 0) {
					string msg = String.Format (Locale.GetText ("Invalid enum {0}"), value);
					throw new ArgumentException (msg, "KeyContainerPermissionFlags");
				}
				_flags = value;
			}
		}

		public string KeyContainerName {
			get { return _containerName; }
			set { _containerName = value; }
		}

		public int KeySpec {
			get { return _spec; }
			set { _spec = value; }
		}

		public string KeyStore {
			get { return _store; }
			set { _store = value; }
		}

		public string ProviderName {
			get { return _providerName; }
			set { _providerName = value; }
		}

		public int ProviderType {
			get { return _type; }
			set { _type = value; }
		}


		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			KeyContainerPermissionAccessEntry kcpae = (obj as KeyContainerPermissionAccessEntry);
			if (kcpae == null)
				return false;
			if (_flags != kcpae._flags)
				return false;
			if (_containerName != kcpae._containerName)
				return false;
			if (_store != kcpae._store)
				return false;
			if (_providerName != kcpae._providerName)
				return false;
			if (_type != kcpae._type)
				return false;
			return true;
		}

		public override int GetHashCode ()
		{
			int result = _type ^ _spec ^ (int) _flags;
			if (_containerName != null)
				result ^= _containerName.GetHashCode ();
			if (_store != null)
				result ^= _store.GetHashCode ();
			if (_providerName != null)
				result ^= _providerName.GetHashCode ();
			return result;
		}
	}
}

#endif
