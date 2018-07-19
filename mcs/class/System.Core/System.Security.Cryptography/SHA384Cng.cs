//
// NOTE: DO NOT EDIT - This file was automatically generated using
//	/mcs/class/System.Core/tools/hashwrap.cs
//
// System.Security.Cryptography.SHA384Cng
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// 'Software'), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED 'AS IS', WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if !MOBILE

namespace System.Security.Cryptography {

	// this is a wrapper around SHA384Managed
	// see README.CNG and README.CSP for more details

	public sealed class SHA384Cng : SHA384 {

		static byte[] Empty = new byte [0];

		private SHA384 hash;

		[SecurityCritical]
		public SHA384Cng ()
		{
			// note: we don't use SHA384.Create since CryptoConfig could, 
			// if set to use this class, result in a endless recursion
			hash = new SHA384Managed ();
		}

		[SecurityCritical]
		public override void Initialize ()
		{
			hash.Initialize ();
		}

		[SecurityCritical]
		protected override void HashCore (byte[] array, int ibStart, int cbSize)
		{
			hash.TransformBlock (array, ibStart, cbSize, null, 0);
		}

		[SecurityCritical]
		protected override byte[] HashFinal ()
		{
			hash.TransformFinalBlock (Empty, 0, 0);
			HashValue = hash.Hash;
			return HashValue;
		}

		[SecurityCritical]
		protected override void Dispose (bool disposing)
		{
			(hash as IDisposable).Dispose ();
			base.Dispose (disposing);
		}
	}
}

#endif
