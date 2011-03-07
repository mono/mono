//
// System.Security.Cryptography.CngProvider
//
// Authors:
//      Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
// Copyright (C) 2011 Juho Vähä-Herttua
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

namespace System.Security.Cryptography {

	// note: CNG stands for "Cryptography API: Next Generation"

	[Serializable]
	public sealed class CngProvider : IEquatable<CngProvider> {

		private string m_Provider;

		public CngProvider (string provider)
		{
			if (provider == null)
				throw new ArgumentNullException ("provider");
			if (provider.Length == 0)
				throw new ArgumentException ("provider");

			m_Provider = provider;
		}

		public string Provider {
			get { return m_Provider; }
		}

		public bool Equals (CngProvider other)
		{
			if (other == null)
				return false;
			return m_Provider == other.m_Provider;
		}

		public override bool Equals (object obj)
		{
			return Equals (obj as CngProvider);
		}

		public override int GetHashCode ()
		{
			return m_Provider.GetHashCode ();
		}

		public override string ToString ()
		{
			return m_Provider;
		}

		// static

		private static CngProvider microsoftSmartCardKeyStorageProvider;
		private static CngProvider microsoftSoftwareKeyStorageProvider;

		public static CngProvider MicrosoftSmartCardKeyStorageProvider {
			get {
				if (microsoftSmartCardKeyStorageProvider == null)
					microsoftSmartCardKeyStorageProvider = new CngProvider ("Microsoft Smart Card Key Storage Provider");
				return microsoftSmartCardKeyStorageProvider;
			}
		}

		public static CngProvider MicrosoftSoftwareKeyStorageProvider {
			get {
				if (microsoftSoftwareKeyStorageProvider == null)
					microsoftSoftwareKeyStorageProvider = new CngProvider ("Microsoft Software Key Storage Provider");
				return microsoftSoftwareKeyStorageProvider;
			}
		}

		public static bool operator == (CngProvider left, CngProvider right)
		{
			if ((object)left == null)
				return ((object)right == null);
			return left.Equals (right);
		}

		public static bool operator != (CngProvider left, CngProvider right)
		{
			if ((object)left == null)
				return ((object)right != null);
			return !left.Equals (right);
		}
	}
}
