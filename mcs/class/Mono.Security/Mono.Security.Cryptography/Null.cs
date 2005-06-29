// *** WARNING *** DO NOT INCLUDE IN ANY DEFAULT/RELEASE BUILDS *** WARNING
// *** WARNING *** DO NOT INCLUDE IN ANY DEFAULT/RELEASE BUILDS *** WARNING
// *** WARNING *** DO NOT INCLUDE IN ANY DEFAULT/RELEASE BUILDS *** WARNING

//
// System.Security.Cryptography.Null.cs
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

// *** WARNING *** DO NOT INCLUDE IN ANY DEFAULT/RELEASE BUILDS *** WARNING
// *** WARNING *** DO NOT INCLUDE IN ANY DEFAULT/RELEASE BUILDS *** WARNING
// *** WARNING *** DO NOT INCLUDE IN ANY DEFAULT/RELEASE BUILDS *** WARNING

using System;
using System.Security.Cryptography;

namespace Mono.Security.Cryptography {

	public abstract class Null : SymmetricAlgorithm {

		public static new Null Create () 
		{
			return Create ("Mono.Security.Cryptography.Null");
		}

		public static new Null Create (string algName) 
		{
			return (Null) CryptoConfig.CreateFromName (algName);
		}
		
		public Null () 
		{
			KeySizeValue = 128;
			BlockSizeValue = 128;
			FeedbackSizeValue = 128;
	
			LegalKeySizesValue = new KeySizes [1];
			LegalKeySizesValue [0] = new KeySizes (0, 1024, 8);

			LegalBlockSizesValue = new KeySizes [1];
			LegalBlockSizesValue [0] = new KeySizes (0, 1024, 8);
		}
	}

	public sealed class NullManaged : Null {
		
		public NullManaged ()
		{
		}
		
		public override void GenerateIV ()
		{
			IVValue = new byte [BlockSizeValue >> 3];
		}
		
		public override void GenerateKey ()
		{
			KeyValue = new byte [KeySizeValue >> 3];
		}
		
		public override ICryptoTransform CreateDecryptor (byte[] rgbKey, byte[] rgbIV) 
		{
			Key = rgbKey;
			IV = rgbIV;

			return new NullTransform (this, false, rgbKey, rgbIV);
		}
		
		public override ICryptoTransform CreateEncryptor (byte[] rgbKey, byte[] rgbIV) 
		{
			Key = rgbKey;
			IV = rgbIV;

			return new NullTransform (this, true, rgbKey, rgbIV);
		}
	}

	internal class NullTransform : SymmetricTransform {

		private int _block;
		private bool _debug;

		public NullTransform (Null algo, bool encryption, byte[] key, byte[] iv)
			: base (algo, encryption, iv)
		{
			_block = 0;
			_debug = (Environment.GetEnvironmentVariable ("MONO_DEBUG") != null);
			if (_debug) {
				Console.WriteLine ("Mode: {0}", encryption ? "encryption" : "decryption");
				Console.WriteLine ("Key:  {0}", BitConverter.ToString (key));
				Console.WriteLine ("IV:   {0}", BitConverter.ToString (iv));
			}
		}

		public void Clear () 
		{
			_block = 0;
			Dispose (true);
		}
	
		// note: this method is guaranteed to be called with a valid blocksize
		// for both input and output
		protected override void ECB (byte[] input, byte[] output) 
		{
			Buffer.BlockCopy (input, 0, output, 0, output.Length);
			if (_debug) {
				Console.WriteLine ("ECB on block #{0}: {1}", _block, BitConverter.ToString (input));
			}
			_block++;
		}
	}
}

// *** WARNING *** DO NOT INCLUDE IN ANY DEFAULT/RELEASE BUILDS *** WARNING
// *** WARNING *** DO NOT INCLUDE IN ANY DEFAULT/RELEASE BUILDS *** WARNING
// *** WARNING *** DO NOT INCLUDE IN ANY DEFAULT/RELEASE BUILDS *** WARNING
