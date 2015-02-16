//
// System.Guid.cs
//
// Authors:
//	Duco Fijma (duco@lorentz.xs4all.nl)
//	Sebastien Pouliot (sebastien@ximian.com)
//	Jb Evain (jbevain@novell.com)
//	Marek Safar (marek.safar@gmail.com)
//
// (C) 2002 Duco Fijma
// Copyright (C) 2004-2010 Novell, Inc (http://www.novell.com)
// Copyright 2012, 2014 Xamarin, Inc (http://www.xamarin.com)
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

#if !FULL_AOT_RUNTIME

using System.Security.Cryptography;

namespace System
{
	partial struct Guid
	{
		private static object _rngAccess = new object ();
		private static RandomNumberGenerator _rng;
		private static RandomNumberGenerator _fastRng;

		public static Guid NewGuid ()
		{
			byte[] b = new byte [16];
			// thread-safe access to the prng
			lock (_rngAccess) {
				if (_rng == null)
					_rng = RandomNumberGenerator.Create ();
				_rng.GetBytes (b);
			}

			Guid res = new Guid (b);
			// Mask in Variant 1-0 in Bit[7..6]
			res._d = (byte) ((res._d & 0x3fu) | 0x80u);
			// Mask in Version 4 (random based Guid) in Bits[15..13]
			res._c = (short) ((res._c & 0x0fffu) | 0x4000u);

			return res;
		}

		// used in ModuleBuilder so mcs doesn't need to invoke 
		// CryptoConfig for simple assemblies.
		internal static byte[] FastNewGuidArray ()
		{
			byte[] guid = new byte [16];

			// thread-safe access to the prng
			lock (_rngAccess) {
				// if known, use preferred RNG
				if (_rng != null)
					_fastRng = _rng;
				// else use hardcoded default RNG (bypassing CryptoConfig)
				if (_fastRng == null)
					_fastRng = new RNGCryptoServiceProvider ();
				_fastRng.GetBytes (guid);
			}

			// Mask in Variant 1-0 in Bit[7..6]
			guid [8] = (byte) ((guid [8] & 0x3f) | 0x80);
			// Mask in Version 4 (random based Guid) in Bits[15..13]
			guid [7] = (byte) ((guid [7] & 0x0f) | 0x40);

			return guid;
		}
	}
}

#endif