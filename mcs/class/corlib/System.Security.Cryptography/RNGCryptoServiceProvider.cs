//
// System.Security.Cryptography.RNGCryptoServiceProvider
//
// Authors:
//	Mark Crichton (crichton@gimp.org)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2002
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

// "In the beginning there was Chaos,
// and within this Chaos was Power,
// Great Power without form."
// -- The Verrah Rubicon of Verena, Book One

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Security.Cryptography {
	
#if !NET_2_1
	[ComVisible (true)]
#endif
	public sealed class RNGCryptoServiceProvider : RandomNumberGenerator {
		private static object _lock;
		private IntPtr _handle;

		static RNGCryptoServiceProvider ()
		{
			if (RngOpen ())
				_lock = new object ();
		}

		public RNGCryptoServiceProvider ()
		{
			_handle = RngInitialize (null);
			Check ();
		}
#if !NET_2_1
		public RNGCryptoServiceProvider (byte[] rgb)
		{
			_handle = RngInitialize (rgb);
			Check ();
		}
		
		public RNGCryptoServiceProvider (CspParameters cspParams)
		{
			// CSP selection isn't supported but we still return 
			// random data (no exception) for compatibility
			_handle = RngInitialize (null);
			Check ();
		}
		
		public RNGCryptoServiceProvider (string str) 
		{
			if (str == null)
				_handle = RngInitialize (null);
			else
				_handle = RngInitialize (Encoding.UTF8.GetBytes (str));
			Check ();
		}
#endif
		private void Check () 
		{
			if (_handle == IntPtr.Zero) {
				throw new CryptographicException (
					Locale.GetText ("Couldn't access random source."));
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern bool RngOpen ();
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern IntPtr RngInitialize (byte[] seed);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern IntPtr RngGetBytes (IntPtr handle, byte[] data);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void RngClose (IntPtr handle);
		
		public override void GetBytes (byte[] data) 
		{
			if (data == null)
				throw new ArgumentNullException ("data");

			if (_lock == null) {
				_handle = RngGetBytes (_handle, data);
			} else {
				// using a global handle for randomness
				lock (_lock) {
					_handle = RngGetBytes (_handle, data);
				}
			}
			Check ();
		}
		
		public override void GetNonZeroBytes (byte[] data) 
		{
			if (data == null)
				throw new ArgumentNullException ("data");

        		byte[] random = new byte [data.Length * 2];
        		int i = 0;
        		// one pass should be enough but hey this is random ;-)
        		while (i < data.Length) {
                		_handle = RngGetBytes (_handle, random);
				Check ();
                		for (int j=0; j < random.Length; j++) {
                        		if (i == data.Length)
                                		break;
                        		if (random [j] != 0)
                                		data [i++] = random [j];
                		}
        		}
		}
		
		~RNGCryptoServiceProvider () 
		{
			if (_handle != IntPtr.Zero) {
				RngClose (_handle);
				_handle = IntPtr.Zero;
			}
		}

#if NET_4_0
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}
#endif
	}
}
