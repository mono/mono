//
// PasswordDeriveBytes.cs: Handles PKCS#5 key derivation using password
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2007 Novell, Inc (http://www.novell.com)
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
using System.Text;

namespace System.Security.Cryptography {

// References:
// a.	PKCS #5 - Password-Based Cryptography Standard 
//	http://www.rsasecurity.com/rsalabs/pkcs/pkcs-5/index.html
// b.	IETF RFC2898: PKCS #5: Password-Based Cryptography Specification Version 2.0
//	http://www.rfc-editor.org/rfc/rfc2898.txt

[ComVisible (true)]
public class PasswordDeriveBytes : DeriveBytes {

	private string HashNameValue;
	private byte[] SaltValue;
	private int IterationsValue;

	private HashAlgorithm hash;
	private int state;
	private byte[] password;
	private byte[] initial;
	private byte[] output;
	private int position;
	private int hashnumber;

	public PasswordDeriveBytes (string strPassword, byte[] rgbSalt) 
	{
		Prepare (strPassword, rgbSalt, "SHA1", 100);
	}

	public PasswordDeriveBytes (string strPassword, byte[] rgbSalt, CspParameters cspParams) 
	{
		Prepare (strPassword, rgbSalt, "SHA1", 100);
		if (cspParams != null) {
			throw new NotSupportedException (
				Locale.GetText ("CspParameters not supported by Mono for PasswordDeriveBytes."));
		}
	}

	public PasswordDeriveBytes (string strPassword, byte[] rgbSalt, string strHashName, int iterations) 
	{
		Prepare (strPassword, rgbSalt, strHashName, iterations);
	}

	public PasswordDeriveBytes (string strPassword, byte[] rgbSalt, string strHashName, int iterations, CspParameters cspParams) 
	{
		Prepare (strPassword, rgbSalt, strHashName, iterations);
		if (cspParams != null) {
			throw new NotSupportedException (
				Locale.GetText ("CspParameters not supported by Mono for PasswordDeriveBytes."));
		}
	}

	public PasswordDeriveBytes (byte[] password, byte[] salt) 
	{
		Prepare (password, salt, "SHA1", 100);
	}

	public PasswordDeriveBytes (byte[] password, byte[] salt, CspParameters cspParams) 
	{
		Prepare (password, salt, "SHA1", 100);
		if (cspParams != null) {
			throw new NotSupportedException (
				Locale.GetText ("CspParameters not supported by Mono for PasswordDeriveBytes."));
		}
	}

	public PasswordDeriveBytes (byte[] password, byte[] salt, string hashName, int iterations) 
	{
		Prepare (password, salt, hashName, iterations);
	}

	public PasswordDeriveBytes (byte[] password, byte[] salt, string hashName, int iterations, CspParameters cspParams) 
	{
		Prepare (password, salt, hashName, iterations);
		if (cspParams != null) {
			throw new NotSupportedException (
				Locale.GetText ("CspParameters not supported by Mono for PasswordDeriveBytes."));
		}
	}

#if NET_4_0
	protected override void Dispose (bool disposing)
	{
		// zeroize buffer
		if (initial != null) {
			Array.Clear (initial, 0, initial.Length);
			initial = null;
		}
		// zeroize temporary password storage
		if (password != null) {
			Array.Clear (password, 0, password.Length);
			password = null;
		}
		base.Dispose (disposing);
	}
#endif

	private void Prepare (string strPassword, byte[] rgbSalt, string strHashName, int iterations) 
	{
		if (strPassword == null)
			throw new ArgumentNullException ("strPassword");

		byte[] pwd = Encoding.UTF8.GetBytes (strPassword);
		Prepare (pwd, rgbSalt, strHashName, iterations);
		Array.Clear (pwd, 0, pwd.Length);
	}

	private void Prepare (byte[] password, byte[] rgbSalt, string strHashName, int iterations)
	{
		if (password == null)
			throw new ArgumentNullException ("password");

		this.password = (byte[]) password.Clone ();

		Salt = rgbSalt;

		HashName = strHashName;
		IterationCount = iterations;
		state = 0;
	}
	public string HashName {
		get { return HashNameValue; } 
		set {
			if (value == null)
				throw new ArgumentNullException ("HashName");
			if (state != 0) {
				throw new CryptographicException (
					Locale.GetText ("Can't change this property at this stage"));
			}
			HashNameValue = value;
		}
	}

	public int IterationCount {
		get { return IterationsValue; }
		set {
			if (value < 1)
				throw new ArgumentOutOfRangeException ("> 0", "IterationCount");
			if (state != 0) {
				throw new CryptographicException (
					Locale.GetText ("Can't change this property at this stage"));
			}
			IterationsValue = value;
		}
	}

	public byte[] Salt {
		get { 
			if (SaltValue == null)
				return null;
			return (byte[]) SaltValue.Clone ();
		}
		set {
			if (state != 0) {
				throw new CryptographicException (
					Locale.GetText ("Can't change this property at this stage"));
			}
			if (value != null)
				SaltValue = (byte[]) value.Clone ();
			else
				SaltValue = null;
		}
	}

	public byte[] CryptDeriveKey (string algname, string alghashname, int keySize, byte[] rgbIV) 
	{
		if (keySize > 128) {
			throw new CryptographicException (
				Locale.GetText ("Key Size can't be greater than 128 bits"));
		}
		throw new NotSupportedException (
			Locale.GetText ("CspParameters not supported by Mono"));
	}

	// note: Key is returned - we can't zeroize it ourselve :-(
	[Obsolete ("see Rfc2898DeriveBytes for PKCS#5 v2 support")]
 #pragma warning disable 809
	public override byte[] GetBytes (int cb) 
	{
 #pragma warning restore 809

		if (cb < 1)
			throw new IndexOutOfRangeException ("cb");

		if (state == 0) {
			// it's now impossible to change the HashName, Salt
			// and IterationCount
			Reset ();
			state = 1;
		}

		byte[] result = new byte [cb];
		int cpos = 0;
		// the initial hash (in reset) + at least one iteration
		int iter = Math.Max (1, IterationsValue - 1);

		// start with the PKCS5 key
		if (output == null) {
			// calculate the PKCS5 key
			output = initial;

			// generate new key material
			for (int i = 0; i < iter - 1; i++)
				output = hash.ComputeHash (output);
		}

		while (cpos < cb) {
			byte[] output2 = null;
			if (hashnumber == 0) {
				// last iteration on output
				output2 = hash.ComputeHash (output);
			}
			else if (hashnumber < 1000) {
				string n = Convert.ToString (hashnumber);
				output2 = new byte [output.Length + n.Length];
				for (int j=0; j < n.Length; j++)
					output2 [j] = (byte)(n [j]);
				Buffer.BlockCopy (output, 0, output2, n.Length, output.Length);
				// don't update output
				output2 = hash.ComputeHash (output2);
			}
			else {
				throw new CryptographicException (
					Locale.GetText ("too long"));
			}

			int rem = output2.Length - position;
			int l = Math.Min (cb - cpos, rem);
			Buffer.BlockCopy (output2, position, result, cpos, l);
			cpos += l;
			position += l;
			while (position >= output2.Length) {
				position -= output2.Length;
				hashnumber++;
			}
		}
		return result;
	}

	public override void Reset () 
	{
		state = 0;
		position = 0;
		hashnumber = 0;

		hash = HashAlgorithm.Create (HashNameValue);
		if (SaltValue != null) {
			hash.TransformBlock (password, 0, password.Length, password, 0);
			hash.TransformFinalBlock (SaltValue, 0, SaltValue.Length);
			initial = hash.Hash;
		}
		else
			initial = hash.ComputeHash (password);
	}
} 
	
} 
