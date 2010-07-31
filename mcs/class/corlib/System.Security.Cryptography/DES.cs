//
// System.Security.Cryptography.DES.cs
//
// Author:
//	Sergey Chaban (serge@wildwestsoftware.com)
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
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

using System.Globalization;
using System.Runtime.InteropServices;

// References:
// a.	FIPS PUB 46-3: Data Encryption Standard
//	http://csrc.nist.gov/publications/fips/fips46-3/fips46-3.pdf

namespace System.Security.Cryptography {

[ComVisible (true)]
public abstract class DES : SymmetricAlgorithm {

	private const int keySizeByte = 8;

	protected DES ()
	{
		KeySizeValue = 64; 
		BlockSizeValue = 64; 
		FeedbackSizeValue = 8;

		LegalKeySizesValue = new KeySizes[1];
		LegalKeySizesValue[0] = new KeySizes(64, 64, 0);

		LegalBlockSizesValue = new KeySizes[1];
		LegalBlockSizesValue[0] = new KeySizes(64, 64, 0);
	}

	public static new DES Create () 
	{
		return Create ("System.Security.Cryptography.DES");
	}

	public static new DES Create (string algName) 
        {
		return (DES) CryptoConfig.CreateFromName (algName);
	}


	// Ek(Ek(m)) = m
	internal static readonly byte[,] weakKeys = {
		{ 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 },
		{ 0x1F, 0x1F, 0x1F, 0x1F, 0x0F, 0x0F, 0x0F, 0x0F },
		{ 0xE1, 0xE1, 0xE1, 0xE1, 0xF1, 0xF1, 0xF1, 0xF1 },
		{ 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF },
	};

	// Ek1(Ek2(m)) = m
	internal static readonly byte[,] semiWeakKeys = {
		{ 0x00, 0x1E, 0x00, 0x1E, 0x00, 0x0E, 0x00, 0x0E }, // map to packed key 011F011F010E010E
		{ 0x00, 0xE0, 0x00, 0xE0, 0x00, 0xF0, 0x00, 0xF0 }, // map to packed key 01E001E001F101F1
		{ 0x00, 0xFE, 0x00, 0xFE, 0x00, 0xFE, 0x00, 0xFE }, // map to packed key 01FE01FE01FE01FE
		{ 0x1E, 0x00, 0x1E, 0x00, 0x0E, 0x00, 0x0E, 0x00 }, // map to packed key 1F011F010E010E01
		{ 0x1E, 0xE0, 0x1E, 0xE0, 0x0E, 0xF0, 0x0E, 0xF0 }, // map to packed key 1FE01FE00EF10EF1
		{ 0x1E, 0xFE, 0x1E, 0xFE, 0x0E, 0xFE, 0x0E, 0xFE }, // map to packed key 1FFE1FFE0EFE0EFE
		{ 0xE0, 0x00, 0xE0, 0x00, 0xF0, 0x00, 0xF0, 0x00 }, // map to packed key E001E001F101F101
		{ 0xE0, 0x1E, 0xE0, 0x1E, 0xF0, 0x0E, 0xF0, 0x0E }, // map to packed key E01FE01FF10EF10E
		{ 0xE0, 0xFE, 0xE0, 0xFE, 0xF0, 0xFE, 0xF0, 0xFE }, // map to packed key E0FEE0FEF1FEF1FE
		{ 0xFE, 0x00, 0xFE, 0x00, 0xFE, 0x00, 0xFE, 0x00 }, // map to packed key FE01FE01FE01FE01
		{ 0xFE, 0x1E, 0xFE, 0x1E, 0xFE, 0x0E, 0xFE, 0x0E }, // map to packed key FE1FFE1FFE0EFE0E
		{ 0xFE, 0xE0, 0xFE, 0xE0, 0xFE, 0xF0, 0xFE, 0xF0 }, // map to packed key FEE0FEE0FEF1FEF1
	};

	public static bool IsWeakKey (byte[] rgbKey) 
	{
		if (rgbKey == null)
			throw new CryptographicException (Locale.GetText ("Null Key"));
		if (rgbKey.Length != keySizeByte)
			throw new CryptographicException (Locale.GetText ("Wrong Key Length"));

		// (fast) pre-check with "weak bytes"
		for (int i=0; i < rgbKey.Length; i++) {
			switch (rgbKey [i] | 0x11) {
				case 0x11:
				case 0x1F:
				case 0xF1:
				case 0xFF:
					break;
				default:
					return false;
			}
		}

		// compare with known weak keys
		for (int i=0; i < (weakKeys.Length >> 3); i++) {
			int j = 0;
			for (; j < rgbKey.Length; j++) {
				if ((rgbKey [j] ^ weakKeys [i,j]) > 1)
					break;
			}
			if (j==8)
				return true;
		}
		return false;
	}

	public static bool IsSemiWeakKey (byte[] rgbKey)
	{
		if (rgbKey == null)
			throw new CryptographicException (Locale.GetText ("Null Key"));
		if (rgbKey.Length != keySizeByte)
			throw new CryptographicException (Locale.GetText ("Wrong Key Length"));

		// (fast) pre-check with "weak bytes"
		for (int i=0; i < rgbKey.Length; i++) {
			switch (rgbKey [i] | 0x11) {
				case 0x11:
				case 0x1F:
				case 0xF1:
				case 0xFF:
					break;
				default:
					return false;
			}
		}

		// compare with known weak keys
		for (int i=0; i < (semiWeakKeys.Length >> 3); i++) {
			int j = 0;
			for (; j < rgbKey.Length; j++) {
				if ((rgbKey [j] ^ semiWeakKeys [i,j]) > 1)
					break;
			}
			if (j==8)
				return true;
		}
		return false;
	}

	public override byte[] Key {
		get {
			if (KeyValue == null) {
				// GenerateKey is responsible to return a valid key
				// e.g. no weak or semi-weak keys
				GenerateKey ();
			}
			return (byte[]) KeyValue.Clone ();
		}
		set {
			if (value == null)
				throw new ArgumentNullException ("Key");
			if (value.Length != keySizeByte)
				throw new ArgumentException (Locale.GetText ("Wrong Key Length"));
			if (IsWeakKey (value))
				throw new CryptographicException (Locale.GetText ("Weak Key"));
			if (IsSemiWeakKey (value))
				throw new CryptographicException (Locale.GetText ("Semi Weak Key"));

			KeyValue = (byte[]) value.Clone ();
		}
	}
}

}

