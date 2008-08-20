//
// Mono.Security.Protocol.Ntlm.Type2Message - Challenge
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004, 2007 Novell, Inc (http://www.novell.com)
//
// References
// a.	NTLM Authentication Scheme for HTTP, Ronald Tschalär
//	http://www.innovation.ch/java/ntlm.html
// b.	The NTLM Authentication Protocol, Copyright © 2003 Eric Glass
//	http://davenport.sourceforge.net/ntlm.html
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
using System.Text;

namespace Mono.Security.Protocol.Ntlm {

	public class Type2Message : MessageBase {

		private byte[] _nonce;
		private byte[] _context;
		private NtlmTargetInformation _target;
		private string _target_name;

		public Type2Message () : this (NtlmVersion.Version1)
		{
		}

		public Type2Message (NtlmVersion version) : base (2, version)
		{
			_nonce = new byte [8];
			RandomNumberGenerator rng = RandomNumberGenerator.Create ();
			rng.GetBytes (_nonce);
			// default values
			Flags = (NtlmFlags) 0x8201;
			if (Version != NtlmVersion.Version1) {
				_context = new byte [8];
				_target = new NtlmTargetInformation ();
			}
		}

		public Type2Message (byte[] message) : this (message, NtlmVersion.Version1)
		{
		}

		public Type2Message (byte[] message, NtlmVersion version) : base (2, version)
		{
			_nonce = new byte [8];
			Decode (message);
		}

		~Type2Message () 
		{
			if (_nonce != null)
				Array.Clear (_nonce, 0, _nonce.Length);
		}

		// properties

		public byte[] Context {
			get { return (byte[]) _context.Clone (); }
			set { 
				if (value == null)
					throw new ArgumentNullException ("Nonce");
				if (value.Length != 8) {
					string msg = Locale.GetText ("Invalid Nonce Length (should be 8 bytes).");
					throw new ArgumentException (msg, "Nonce");
				}
				_context = (byte[]) value.Clone (); 
			}
		}

		public byte[] Nonce {
			get { return (byte[]) _nonce.Clone (); }
			set { 
				if (value == null)
					throw new ArgumentNullException ("Nonce");
				if (value.Length != 8) {
					string msg = Locale.GetText ("Invalid Nonce Length (should be 8 bytes).");
					throw new ArgumentException (msg, "Nonce");
				}
				_nonce = (byte[]) value.Clone (); 
			}
		}

		public NtlmTargetInformation Target {
			get { return _target; }
		}

		public string TargetName {
			get { return _target_name; }
			set { _target_name = value; }
		}

		// methods

		protected override void Decode (byte[] message) 
		{
			base.Decode (message);

			short targetNameSize = BitConverterLE.ToInt16 (message, 12);
			int targetNameOffset = BitConverterLE.ToInt32 (message, 16);

			Flags = (NtlmFlags) BitConverterLE.ToUInt32 (message, 20);

			Buffer.BlockCopy (message, 24, _nonce, 0, 8);

			if (Version == NtlmVersion.Version1)
				return;

			Buffer.BlockCopy (message, 32, _context, 0, 8);
			short targetInfoSize = BitConverterLE.ToInt16 (message, 40);
			int targetInfoOffset = BitConverterLE.ToInt32 (message, 44);

			if (Version == NtlmVersion.Version3)
				Buffer.BlockCopy (OSVersion, 0, message, 48, OSVersion.Length);

			Encoding enc = (Flags & NtlmFlags.NegotiateUnicode) != 0 ? Encoding.Unicode : Encoding.UTF8;
			if (targetNameSize > 0)
				TargetName = enc.GetString (message, targetNameOffset, targetNameSize);

			_target.Decode (message, targetInfoOffset, targetInfoSize);
		}

		public override byte[] GetBytes ()
		{
			byte [] name_bytes = null, target = null;
			short name_len = 0, target_len = 0;
			if (TargetName != null) {
				Encoding enc = (Flags & NtlmFlags.NegotiateUnicode) != 0 ? Encoding.Unicode : Encoding.UTF8;
				name_bytes = enc.GetBytes (TargetName);
				name_len = (short) name_bytes.Length;
			}
			if (Version != NtlmVersion.Version1) {
				target = _target.ToBytes ();
				target_len = (short) target.Length;
			}

			uint name_offset = (uint) (Version == NtlmVersion.Version3 ? 56 : 40);

			int size = (int) name_offset +
				   (name_len > 0 ? name_len + 8 : 0) +
				   (target_len > 0 ? target_len + 8 : 0);
			byte[] data = PrepareMessage (size);

			// target name
			data [12] = (byte) name_len;
			data [13] = (byte) (name_len >> 8);
			data [14] = data [12];
			data [15] = data [13];
			data [16] = (byte) name_offset;
			data [17] = (byte) (name_offset >> 8);
			data [18] = (byte) (name_offset >> 16);
			data [19] = (byte) (name_offset >> 24);

			// flags
			data [20] = (byte) Flags;
			data [21] = (byte)((uint)Flags >> 8);
			data [22] = (byte)((uint)Flags >> 16);
			data [23] = (byte)((uint)Flags >> 24);

			Buffer.BlockCopy (_nonce, 0, data, 24, _nonce.Length);

			if (Version == NtlmVersion.Version1)
				return data;

			// context
			Buffer.BlockCopy (_context, 0, data, 32, 8);

			// target information
			data [40] = (byte) target_len;
			data [41] = (byte) (target_len >> 8);
			data [42] = data [40];
			data [43] = data [41];
			uint info_offset = (uint) (name_offset + name_bytes.Length);
			data [44] = (byte) info_offset;
			data [45] = (byte) (info_offset >> 8);
			data [46] = (byte) (info_offset >> 16);
			data [47] = (byte) (info_offset >> 24);

			if (Version == NtlmVersion.Version3)
				Buffer.BlockCopy (OSVersion, 0, data, 48, OSVersion.Length);

			Buffer.BlockCopy (name_bytes, 0, data, (int) name_offset, name_len);
			Buffer.BlockCopy (target, 0, data, (int) info_offset, target.Length);

			return data;
		}
	}
}
