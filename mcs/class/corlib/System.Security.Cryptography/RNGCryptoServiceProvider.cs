//
// System.Security.Cryptography.RNGCryptoServiceProvider
//
// Authors:
//	Mark Crichton (crichton@gimp.org)
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002
//

// "In the beginning there was Chaos,
// and within this Chaos was Power,
// Great Power without form."
// -- The Verrah Rubicon of Verena, Book One

using System;
using System.Runtime.CompilerServices;

namespace System.Security.Cryptography {
	
#if USE_VERSION_1_0
	public class RNGCryptoServiceProvider : RandomNumberGenerator {
#else
	public sealed class RNGCryptoServiceProvider : RandomNumberGenerator {
#endif
		
		[MonoTODO]
		public RNGCryptoServiceProvider () 
		{
			// This will get some meaning when I figure out what the other
			// three constructors do.
		}
		
		[MonoTODO]
		public RNGCryptoServiceProvider (byte[] rgb) 
		{
			// Ok, not called by app code... someone must call it, though.
		}
		
		[MonoTODO]
		public RNGCryptoServiceProvider (CspParameters cspParams) 
		{
			// Why do I have this feeling this is the MS CryptoAPI...
		}
		
		[MonoTODO]
		public RNGCryptoServiceProvider (string str) 
		{
			// More !application code.  Interesting...
		}
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void InternalGetBytes (byte[] data);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void InternalGetNonZeroBytes (byte[] data);

		public override void GetBytes (byte[] data) 
		{
			InternalGetBytes (data);
		}
		
		public override void GetNonZeroBytes (byte[] data) 
		{
			InternalGetNonZeroBytes (data);
		}
		
		~RNGCryptoServiceProvider () 
		{
			// in our case we have nothing unmamanged to dispose
		}
	}
}
