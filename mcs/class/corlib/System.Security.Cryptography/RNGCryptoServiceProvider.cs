//
// System.Security.Cryptography.RNGCryptoServiceProvider
//
// Authors:
//	Mark Crichton (crichton@gimp.org)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2002
// Copyright (C) 2004 Novell (http://www.novell.com)
//

// "In the beginning there was Chaos,
// and within this Chaos was Power,
// Great Power without form."
// -- The Verrah Rubicon of Verena, Book One

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Security.Cryptography {
	
#if NET_1_0
	public class RNGCryptoServiceProvider : RandomNumberGenerator {
#else
	public sealed class RNGCryptoServiceProvider : RandomNumberGenerator {
#endif
		private IntPtr _handle;

		public RNGCryptoServiceProvider ()
		{
			_handle = RngInitialize (null);
			Check ();
		}
		
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

		private void Check () 
		{
			if (_handle == IntPtr.Zero) {
				throw new CryptographicException (
					Locale.GetText ("Couldn't access random source."));
			}
		}
		
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

			_handle = RngGetBytes (_handle, data);
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
	}
}
