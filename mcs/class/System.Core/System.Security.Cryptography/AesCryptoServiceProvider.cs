//
// System.Security.Cryptography.AesCryptoServiceProvider class
//
// Authors:
//	Sebastien Pouliot (sebastien@xamarin.com)
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
// Copyright 2013 Xamarin Inc (http://www.xamarin.com)
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

using System.Security.Permissions;

using Mono.Security.Cryptography;

namespace System.Security.Cryptography {
	
	[HostProtection (SecurityAction.LinkDemand, MayLeakOnAbort=true)]
	public sealed class AesCryptoServiceProvider : Aes {
		
		public AesCryptoServiceProvider ()
		{
			FeedbackSizeValue = 8;
		}
		
		public override void GenerateIV ()
		{
			IVValue = KeyBuilder.IV (BlockSizeValue >> 3);
		}
		
		public override void GenerateKey ()
		{
			KeyValue = KeyBuilder.Key (KeySizeValue >> 3);
		}
		
		public override ICryptoTransform CreateDecryptor (byte[] rgbKey, byte[] rgbIV) 
		{
			if ((Mode == CipherMode.CFB) && (FeedbackSize > 64))
				throw new CryptographicException ("CFB with Feedbaack > 64 bits");
			return new AesTransform (this, false, rgbKey, rgbIV);
		}
		
		public override ICryptoTransform CreateEncryptor (byte[] rgbKey, byte[] rgbIV) 
		{
			if ((Mode == CipherMode.CFB) && (FeedbackSize > 64))
				throw new CryptographicException ("CFB with Feedbaack > 64 bits");
			return new AesTransform (this, true, rgbKey, rgbIV);
		}

		// I suppose some attributes differs ?!? because this does not look required

		public override byte[] IV {
			get { return base.IV; }
			set { base.IV = value; }
		}

		public override byte[] Key {
			get { return base.Key; }
			set { base.Key = value; }
		}

		public override int KeySize {
			get { return base.KeySize; }
			set { base.KeySize = value; }
		}

		public override int FeedbackSize {
			get { return base.FeedbackSize; }
			set { base.FeedbackSize = value; }
		}

		public override CipherMode Mode {
			get { return base.Mode; }
			set {
				switch (value) {
				case CipherMode.CTS:
					throw new CryptographicException ("CTS is not supported");
				default:
					base.Mode = value;
					break;
				}
			}
		}

		public override PaddingMode Padding {
			get { return base.Padding; }
			set { base.Padding = value; }
		}

		public override ICryptoTransform CreateDecryptor () 
		{
			return CreateDecryptor (Key, IV);
		}

		public override ICryptoTransform CreateEncryptor() 
		{
			return CreateEncryptor (Key, IV);
		}

		protected override void Dispose (bool disposing) 
		{
			base.Dispose (disposing);
		}
	}
}
