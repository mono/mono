//
// System.Security.SecureString class
//
// Authors
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Security.Permissions;

using Mono.Security.Cryptography;

namespace System.Security {

	[MonoTODO ("current version ISN'T encrypted")]
	[ComVisible (false)]
	public sealed class SecureString : CriticalFinalizerObject, IDisposable {

		static private SymmetricAlgorithm _cipher;
		static private int _blockSize;

		private int _length;
		private bool _disposed;
		private bool _readonly;
		private byte[] _enc;	// encrypted (permanent buffer)
		private char[] _dec;	// decrypted (temporary buffer)
		private byte[] _iv;	// initialization vector

		static SecureString ()
		{
			_cipher = SymmetricAlgorithm.Create ();
			_cipher.Mode = CipherMode.CBC;
			_cipher.Padding = PaddingMode.PKCS7;
			_blockSize = _cipher.BlockSize << 3; // in bytes
		}

		public SecureString ()
		{
			_iv = KeyBuilder.IV (_blockSize);
			// default size
			Alloc (_blockSize >> 1, false);
		}

		[CLSCompliant (false)]
		public unsafe SecureString (char* value, int length)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			_iv = KeyBuilder.IV (_blockSize);
			Alloc (length, false);
			for (_length=1; _length <= length; _length++)
				_dec [_length] = *value++;
			Encrypt ();
		}

		~SecureString ()
		{
			Dispose ();
		}

		// properties

		public int Length {
			get {
				if (_disposed)
					throw new ObjectDisposedException ("SecureString");
				return _length;
			}
		}

		public void AppendChar (char c)
		{
			if (_disposed)
				throw new ObjectDisposedException ("SecureString");
			if (_readonly) {
				throw new InvalidOperationException (Locale.GetText (
					"SecureString is read-only."));
			}
			if (_length == 65535)
				throw new ArgumentOutOfRangeException ("length", "> 65536");

			try {
				Decrypt ();
				if (_length >= _dec.Length) {
					Alloc (_length + 1, true);
				}
				_dec [_length++] = c;
				Encrypt ();
			}
			catch {
				Array.Clear (_dec, 0, _dec.Length);
				_length = 0;
				throw;
			}
		}

		public void Clear ()
		{
			if (_disposed)
				throw new ObjectDisposedException ("SecureString");
			if (_readonly) {
				throw new InvalidOperationException (Locale.GetText (
					"SecureString is read-only."));
			}

			Array.Clear (_dec, 0, _dec.Length);	// should be empty
			Array.Clear (_enc, 0, _enc.Length);
			// return to default sizes
			_dec = new char [_blockSize >> 1];
			_enc = new byte [_blockSize];
			_length = 0;
			// get a new IV
			_iv = KeyBuilder.IV (_blockSize);
		}

		public SecureString Copy () 
		{
			SecureString ss = new SecureString ();
			try {
				Decrypt ();
				ss._dec = (char[]) _dec.Clone ();
				Array.Clear (_dec, 0, _dec.Length);
				ss._enc = new byte [_dec.Length >> 1];
				ss.Encrypt ();
			}
			catch {
				Array.Clear (_dec, 0, _dec.Length);
				if (ss._dec != null)
					Array.Clear (ss._dec, 0, ss._dec.Length);
			}
			return ss;
		}

		public void Dispose ()
		{
			if (_dec != null) {
				Array.Clear (_enc, 0, _enc.Length);
				Array.Clear (_dec, 0, _dec.Length);
				_dec = null;
				_length = 0;
			}
			_disposed = true;
		}

		public void InsertAt (int index, char c)
		{
			if (_disposed)
				throw new ObjectDisposedException ("SecureString");
			if ((index < 0) || (index >= _length))
				throw new ArgumentOutOfRangeException ("index", "< 0 || > length");
			if (_readonly) {
				throw new InvalidOperationException (Locale.GetText (
					"SecureString is read-only."));
			}

			try {
				Decrypt ();
				// TODO
				Encrypt ();
			}
			catch {
				Array.Clear (_dec, 0, _dec.Length);
				_length = 0;
				throw;
			}
		}

		public bool IsReadOnly ()
		{
			if (_disposed)
				throw new ObjectDisposedException ("SecureString");
			return _readonly;
		}

		public void MakeReadOnly ()
		{
			_readonly = true;
		}

		public void RemoveAt (int index)
		{
			if (_disposed)
				throw new ObjectDisposedException ("SecureString");
			if ((index < 0) || (index >= _length))
				throw new ArgumentOutOfRangeException ("index", "< 0 || > length");
			if (_readonly) {
				throw new InvalidOperationException (Locale.GetText (
					"SecureString is read-only."));
			}

			try {
				Decrypt ();
				Buffer.BlockCopy (_dec, index, _dec, index - 1, _dec.Length - index);
				_length--;
				Encrypt ();
			}
			catch {
				Array.Clear (_dec, 0, _dec.Length);
				_length = 0;
				throw;
			}
		}

		public void SetAt (int index, char c)
		{
			if (_disposed)
				throw new ObjectDisposedException ("SecureString");
			if ((index < 0) || (index >= _length))
				throw new ArgumentOutOfRangeException ("index", "< 0 || > length");
			if (_readonly) {
				throw new InvalidOperationException (Locale.GetText (
					"SecureString is read-only."));
			}

			try {
				Decrypt ();
				_dec [index] = c;
				Encrypt ();
			}
			catch {
				Array.Clear (_dec, 0, _dec.Length);
				_length = 0;
				throw;
			}
		}

		// internal/private stuff

		// note: realloc only work for bigger buffers. Clear will 
		// reset buffers to default (and small) size.
		private void Alloc (int length, bool realloc) 
		{
			if ((length < 0) || (length > 65536))
				throw new ArgumentOutOfRangeException ("length", "< 0 || > 65536");

			int size = (((length * 2) / _blockSize) + 1) * _blockSize;

			char[] dec = new char [size >> 1];
			byte[] enc = new byte [size];

			if (realloc) {
				// copy, then clear
				Array.Copy (_dec, 0, dec, 0, _dec.Length);
				Array.Clear (_dec, 0, _dec.Length);
				_dec = null;

				Array.Copy (_enc, 0, enc, 0, _enc.Length);
				Array.Clear (_enc, 0, _enc.Length);
				_enc = null;
			}

			_dec = dec;
			_enc = enc;
		}

		[MonoTODO ("no decryption - data is only copied")]
		private void Decrypt ()
		{
			Buffer.BlockCopy (_enc, 0, _dec, 0, _enc.Length);
		}

		[MonoTODO ("no encryption - data is only copied")]
		private void Encrypt ()
		{
			Buffer.BlockCopy (_dec, 0, _enc, 0, _enc.Length);
			Array.Clear (_dec, 0, _dec.Length);
		}

		// dangerous method (put a LinkDemand on it)
		internal byte[] GetBuffer ()
		{
			byte[] secret = null;
			try {
				Decrypt ();
				secret = (byte[]) _dec.Clone ();
			}
			finally {
				Array.Clear (_dec, 0, _dec.Length);
			}
			// NOTE: CALLER IS RESPONSIBLE TO ZEROIZE THE DATA
			return secret;
		}
	}
}

#endif
