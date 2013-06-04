//
// Mono.Security.Protocol.Ntlm.MessageBase
//	abstract class for all NTLM messages
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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
using System.Globalization;

namespace Mono.Security.Protocol.Ntlm {

#if INSIDE_SYSTEM
	internal
#else
	public
#endif
	abstract class MessageBase {

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

			if (message.Length < 12) {
				string msg = Locale.GetText ("Minimum message length is 12 bytes.");
				throw new ArgumentOutOfRangeException ("message", message.Length, msg);
			}

			if (!CheckHeader (message)) {
				string msg = String.Format (Locale.GetText ("Invalid Type{0} message."), _type);
				throw new ArgumentException (msg, "message");
			}
		}


		protected bool CheckHeader (byte[] message) 
		{
			for (int i=0; i < header.Length; i++) {
				if (message [i] != header [i])
					return false;
			}
			return (BitConverterLE.ToUInt32 (message, 8) == _type);
		}

		public abstract byte[] GetBytes ();
	}
}
