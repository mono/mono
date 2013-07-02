//
// System.Security.Cryptography.RandomNumberGenerator
//
// author:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//
//   (C) 2002 Duco Fijma
// Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;

namespace System.Security.Cryptography {
	
#if !NET_2_1
	[ComVisible (true)]
#endif
	public abstract class RandomNumberGenerator
#if NET_4_0
	: IDisposable
#endif
	{

		protected RandomNumberGenerator ()
		{
		}

		public static RandomNumberGenerator Create () 
	        {
#if FULL_AOT_RUNTIME
			return new System.Security.Cryptography.RNGCryptoServiceProvider ();
#else
			// create the default random number generator
			return Create ("System.Security.Cryptography.RandomNumberGenerator");
#endif
		}

		public static RandomNumberGenerator Create (string rngName) 
	        {
			return (RandomNumberGenerator) (CryptoConfig.CreateFromName (rngName));
		}

		public abstract void GetBytes (byte[] data);

#if NET_4_5
		public virtual void GetNonZeroBytes (byte[] data)
		{
			throw new NotImplementedException ();
		}
#else
		public abstract void GetNonZeroBytes (byte[] data);
#endif
		
		
#if NET_4_0
		public void Dispose ()
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{}
#endif
	}
}
