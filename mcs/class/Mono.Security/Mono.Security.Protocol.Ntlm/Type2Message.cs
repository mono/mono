//
// Mono.Security.Protocol.Ntlm.Type2Message - Challenge
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Text;
using System.Security.Cryptography;

namespace Mono.Security.Protocol.Ntlm {

#if INSIDE_SYSTEM
	internal
#else
	public
#endif
	class Type2Message : MessageBase {

		private byte[] _nonce;
		private string _targetName;
		private byte[] _targetInfo;

		public Type2Message () : base (2)
		{
			_nonce = new byte [8];
			RandomNumberGenerator rng = RandomNumberGenerator.Create ();
			rng.GetBytes (_nonce);
			// default values
			Flags = (NtlmFlags) 0x8201;
		}

		public Type2Message (byte[] message) : base (2)
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

		public string TargetName {
			get { return _targetName; }
		}

		public byte[] TargetInfo {
			get { return (byte[])_targetInfo.Clone (); }
		}

		// methods

		protected override void Decode (byte[] message)
		{
			base.Decode (message);

			Flags = (NtlmFlags)BitConverterLE.ToUInt32 (message, 20);

			Buffer.BlockCopy (message, 24, _nonce, 0, 8);

			var tname_len = BitConverterLE.ToUInt16 (message, 12);
			var tname_off = BitConverterLE.ToUInt16 (message, 16);
			if (tname_len > 0) {
				if ((Flags & NtlmFlags.NegotiateOem) != 0)
					_targetName = Encoding.ASCII.GetString (message, tname_off, tname_len);
				else
					_targetName = Encoding.Unicode.GetString (message, tname_off, tname_len);
			}
			
			// The Target Info block is optional.
			if (message.Length >= 48) {
				var tinfo_len = BitConverterLE.ToUInt16 (message, 40);
				var tinfo_off = BitConverterLE.ToUInt16 (message, 44);
				if (tinfo_len > 0) {
					_targetInfo = new byte [tinfo_len];
					Buffer.BlockCopy (message, tinfo_off, _targetInfo, 0, tinfo_len);
				}
			}
		}

		public override byte[] GetBytes ()
		{
			byte[] data = PrepareMessage (40);

			// message length
			short msg_len = (short)data.Length;
			data [16] = (byte) msg_len;
			data [17] = (byte)(msg_len >> 8);

			// flags
			data [20] = (byte) Flags;
			data [21] = (byte)((uint)Flags >> 8);
			data [22] = (byte)((uint)Flags >> 16);
			data [23] = (byte)((uint)Flags >> 24);

			Buffer.BlockCopy (_nonce, 0, data, 24, _nonce.Length);
			return data;
		}
	}
}
