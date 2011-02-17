//
// System.Security.Cryptography.CngKeyCreationParameters
//
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

	public class CngKeyCreationParameters {
		private Nullable<CngExportPolicies> exportPolicy;
		private CngKeyCreationOptions keyCreationOptions;
		private Nullable<CngKeyUsages> keyUsage;
		private CngPropertyCollection parameters;
		private IntPtr parentWindowHandle;
		private CngProvider provider;
		private CngUIPolicy uiPolicy;

		public CngKeyCreationParameters ()
		{
			parameters = new CngPropertyCollection ();
			provider = CngProvider.MicrosoftSoftwareKeyStorageProvider;
		}

		public Nullable<CngExportPolicies> ExportPolicy
		{
			get { return exportPolicy; }
			set { exportPolicy = value; }
		}

		public CngKeyCreationOptions KeyCreationOptions
		{
			get { return keyCreationOptions; }
			set { keyCreationOptions = value; }
		}

		public Nullable<CngKeyUsages> KeyUsage
		{
			get { return keyUsage; }
			set { keyUsage = value; }
		}

		public CngPropertyCollection Parameters
		{
			get { return parameters; }
		}

		public IntPtr ParentWindowHandle
		{
			get { return parentWindowHandle; }
			set { parentWindowHandle = value; }
		}

		public CngProvider Provider
		{
			get { return provider; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				provider = value;
			}
		}

		public CngUIPolicy UIPolicy
		{
			get { return uiPolicy; }
			set { uiPolicy = value; }
		}
	}
}
