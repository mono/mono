//
// Mono.Security.Protocol.Ntlm.MessageBase
//	abstract class for all NTLM messages
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

namespace Mono.Security.Protocol.Ntlm {

	public abstract class MessageBase {

		static private byte[] header = { 0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00 };
		
		private int _type;
		private NtlmFlags _flags;

		protected MessageBase (int messageType) 
		{
			_type = messageType;
		}
		
		public NtlmFlags Flags {
			get { return _flags; }
			set { _flags = value; }
		}

		public int Type { 
			get { return _type; }
		}

		protected byte[] PrepareMessage (int messageSize) 
		{
			byte[] message = new byte [messageSize];
			Buffer.BlockCopy (header, 0, message, 0, 8);
			
			message [ 8] = (byte) _type;
			message [ 9] = (byte)(_type >> 8);
			message [10] = (byte)(_type >> 16);
			message [11] = (byte)(_type >> 24);

			return message;
		}

		protected virtual void Decode (byte[] message) 
		{
			if (message == null)
				throw new ArgumentNullException ("message");

			if (message.Length < 12)
				throw new ArgumentOutOfRangeException ("message", message.Length, "minimum is 12 bytes");

			if (!CheckHeader (message))
				throw new ArgumentException ("Invalid Type" + _type + " message");
		}


		protected bool CheckHeader (byte[] message) 
		{
			for (int i=0; i < header.Length; i++) {
				if (message [i] != header [i])
					return false;
			}
			return (BitConverter.ToUInt32 (message, 8) == _type);
		}

		public abstract byte[] GetBytes ();
	}
}
