//
// MD4.cs - Message Digest 4 Abstract class
//
// Author:
//	Sebastien Pouliot (sebastien@xamarin.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright 2013 Xamarin Inc. (http://www.xamarin.com)
//

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
using System.Security.Cryptography;

namespace Mono.Security.Cryptography {

#if !INSIDE_CORLIB
	public
#endif
	abstract class MD4 : HashAlgorithm {

		protected MD4 () 
		{
			// MD4 hash length are 128 bits long
			HashSizeValue = 128; 
		}

		public static new MD4 Create () 
		{
#if FULL_AOT_RUNTIME
			return new MD4Managed ();
#else
			// for this to work we must register ourself with CryptoConfig
			return Create ("MD4");
#endif
		}

		public static new MD4 Create (string hashName) 
		{
			object o = CryptoConfig.CreateFromName (hashName);
			// in case machine.config isn't configured to use any MD4 implementation
			if (o == null) {
				o = new MD4Managed ();
			}
			return (MD4) o;
		}
	}
}
