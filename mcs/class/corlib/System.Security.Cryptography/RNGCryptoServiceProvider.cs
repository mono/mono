//
// System.Security.Cryptography.RNGCryptoServiceProvider
//
// Authors:
//	Mark Crichton (crichton@gimp.org)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2002
// (C) 2004 Novell (http://www.novell.com)
//

// "In the beginning there was Chaos,
// and within this Chaos was Power,
// Great Power without form."
// -- The Verrah Rubicon of Verena, Book One

using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Security.Cryptography {
	
#if NET_1_0
	public class RNGCryptoServiceProvider : RandomNumberGenerator {
#else
	public sealed class RNGCryptoServiceProvider : RandomNumberGenerator {
#endif
		public RNGCryptoServiceProvider ()
		{
		}
		
		public RNGCryptoServiceProvider (byte[] rgb) 
		{
			Seed (rgb);
		}
		
		public RNGCryptoServiceProvider (CspParameters cspParams)
		{
			// CSP selection isn't supported
			// but we still return random (no exception) for compatibility
		}
		
		public RNGCryptoServiceProvider (string str) 
		{
			Seed (Encoding.UTF8.GetBytes (str));
		}
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void Seed (byte[] data);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void InternalGetBytes (byte[] data);
		
		public override void GetBytes (byte[] data) 
		{
			if (data == null)
				throw new ArgumentNullException ("data");

			InternalGetBytes (data);
		}
		
		public override void GetNonZeroBytes (byte[] data) 
		{
			if (data == null)
				throw new ArgumentNullException ("data");

        		byte[] random = new byte [data.Length * 2];
        		int i = 0;
        		// one pass should be enough but hey this is random ;-)
        		while (i < data.Length) {
                		GetBytes (random);
                		for (int j=0; j < random.Length; j++) {
                        		if (i == data.Length)
                                		break;
                        		if (random [j] != 0)
                                		data [i++] = random [j];
                		}
        		}
		}
		
		/* Commented as we don't require this right now (and it will perform better that way)
		~RNGCryptoServiceProvider () 
		{
			// in our case we have nothing unmanaged to dispose
		}*/
	}
}
