//
// Mono.Security.Protocol.Ntlm.Type2Message - Challenge
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// Copyright (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//
// References
// a.	NTLM Authentication Scheme for HTTP, Ronald Tschalär
//	http://www.innovation.ch/java/ntlm.html
// b.	The NTLM Authentication Protocol, Copyright © 2003 Eric Glass
//	http://davenport.sourceforge.net/ntlm.html
//

using System;
using System.Security.Cryptography;

namespace Mono.Security.Protocol.Ntlm {

	public class Type2Message : MessageBase {

		private byte[] _nonce;
		private int _options;

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
				if (value.Length != 8)
					throw new ArgumentException ("Invalid Nonce Length");
				_nonce = (byte[]) value.Clone (); 
			}
		}

		// methods

		private void Decode (byte[] message)
		{
			if (message == null)
				throw new ArgumentNullException ("message");
			
			if (!CheckHeader (message))
				throw new ArgumentException ("Invalid Type2 message");

			Flags = (NtlmFlags) BitConverter.ToUInt32 (message, 20);

			Buffer.BlockCopy (message, 24, _nonce, 0, 8);
		}

		public override byte[] GetBytes ()
		{
			byte[] data = PrepareMessage (40);

			// message length
			short msg_len = (short)data.Length;
			data [16] = (byte) msg_len;
			data [17] = (byte)(msg_len >> 8);

			// flags
			uint f = (uint) Flags;
			data [20] = (byte) f;
			data [21] = (byte)(f >> 8);

			Buffer.BlockCopy (_nonce, 0, data, 24, _nonce.Length);
			return data;
		}
	}
}
