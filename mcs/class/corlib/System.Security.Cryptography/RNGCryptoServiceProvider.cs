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
	
#if !MOBILE
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

		unsafe public RNGCryptoServiceProvider ()
		{
			_handle = RngInitialize (null, IntPtr.Zero);
			Check ();
		}

		unsafe public RNGCryptoServiceProvider (byte[] rgb)
		{
			fixed (byte* fixed_rgb = rgb)
				_handle = RngInitialize (fixed_rgb, (rgb != null) ? (IntPtr)rgb.Length : IntPtr.Zero);
			Check ();
		}
		
		unsafe public RNGCryptoServiceProvider (CspParameters cspParams)
		{
			// CSP selection isn't supported but we still return 
			// random data (no exception) for compatibility
			_handle = RngInitialize (null, IntPtr.Zero);
			Check ();
		}
		
		unsafe public RNGCryptoServiceProvider (string str)
		{
			if (str == null)
				_handle = RngInitialize (null, IntPtr.Zero);
			else {
				byte[] bytes = Encoding.UTF8.GetBytes (str);
				fixed (byte* fixed_bytes = bytes)
					_handle = RngInitialize (fixed_bytes, (IntPtr)bytes.Length);
			}
			Check ();
		}

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
		unsafe private static extern IntPtr RngInitialize (byte* seed, IntPtr seed_length);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		unsafe private static extern IntPtr RngGetBytes (IntPtr handle, byte* data, IntPtr data_length);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void RngClose (IntPtr handle);
		
		unsafe public override void GetBytes (byte[] data)
		{
			if (data == null)
				throw new ArgumentNullException ("data");

			fixed (byte* fixed_data = data) {
				if (_lock == null) {
					_handle = RngGetBytes (_handle, fixed_data, (IntPtr)data.LongLength);
				} else {
					// using a global handle for randomness
					lock (_lock) {
						_handle = RngGetBytes (_handle, fixed_data, (IntPtr)data.LongLength);
					}
				}
			}
			Check ();
		}
		
		unsafe public override void GetNonZeroBytes (byte[] data)
		{
			if (data == null)
				throw new ArgumentNullException ("data");

			byte[] random = new byte [data.LongLength * 2];
			long i = 0;
			// one pass should be enough but hey this is random ;-)
			while (i < data.LongLength) {
				fixed (byte* fixed_random = random)
					_handle = RngGetBytes (_handle, fixed_random, (IntPtr)random.LongLength);
				Check ();
				for (long j = 0; j < random.LongLength; j++) {
					if (i == data.LongLength)
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

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}
	}
}
