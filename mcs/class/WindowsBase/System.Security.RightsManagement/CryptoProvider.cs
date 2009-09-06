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
using System.Security;
using System.Collections.ObjectModel;

namespace System.Security.RightsManagement {

	[SecurityCritical (SecurityCriticalScope.Everything)]
	public class CryptoProvider : IDisposable
	{
		internal CryptoProvider ()
		{
		}

		~CryptoProvider ()
		{
		}

		public int BlockSize {
			get { throw new NotImplementedException (); }
		}

		public ReadOnlyCollection<ContentGrant> BoundGrants {
			get { throw new NotImplementedException (); }
		}

		public bool CanDecrypt {
			get { throw new NotImplementedException (); }
		}

		public bool CanEncrypt {
			get { throw new NotImplementedException (); }
		}

		public bool CanMergeBlocks {
			get { throw new NotImplementedException (); }
		}

		public byte[] Decrypt (byte[] cryptoText)
		{
			throw new NotImplementedException ();			
		}

		public byte[] Encrypt (byte[] clearText)
		{
			throw new NotImplementedException ();			
		}

		protected virtual void Dispose (bool disposing)
		{
			throw new NotImplementedException ();			
		}

		public void Dispose ()
		{
			throw new NotImplementedException ();			
		}
	}

}