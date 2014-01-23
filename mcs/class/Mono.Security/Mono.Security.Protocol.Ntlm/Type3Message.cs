//
// Mono.Security.Protocol.Ntlm.Type3Message - Authentication
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
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
using System.Text;

namespace Mono.Security.Protocol.Ntlm {

#if INSIDE_SYSTEM
	internal
#else
	public
#endif
	class Type3Message : MessageBase {

		private NtlmAuthLevel _level;
		private byte[] _challenge;
		private string _host;
		private string _domain;
		private string _username;
		private string _password;
		private Type2Message _type2;
		private byte[] _lm;
		private byte[] _nt;
		
		internal const string LegacyAPIWarning = 
			"Use of this API is highly discouraged, " +
			"it selects legacy-mode LM/NTLM authentication, which sends " +
			"your password in very weak encryption over the wire even if " +
			"the server supports the more secure NTLMv2 / NTLMv2 Session. " +
			"You need to use the new `Type3Message (Type2Message)' constructor " +
			"to use the more secure NTLMv2 / NTLMv2 Session authentication modes. " +
			"These require the Type 2 message from the server to compute the response.";
		
		[Obsolete (LegacyAPIWarning)]
		public Type3Message () : base (3)
		{
			if (DefaultAuthLevel != NtlmAuthLevel.LM_and_NTLM)
				throw new InvalidOperationException (
					"Refusing to use legacy-mode LM/NTLM authentication " +
					"unless explicitly enabled using DefaultAuthLevel.");

			// default values
			_domain = Environment.UserDomainName;
			_host = Environment.MachineName;
			_username = Environment.UserName;
			_level = NtlmAuthLevel.LM_and_NTLM;
			Flags = (NtlmFlags) 0x8201;
		}

		public Type3Message (byte[] message) : base (3)
		{
			Decode (message);
		}

		public Type3Message (Type2Message type2) : base (3)
		{
			_type2 = type2;
			_level = DefaultAuthLevel;
			_challenge = (byte[]) type2.Nonce.Clone ();

			_domain = type2.TargetName;
			_host = Environment.MachineName;
			_username = Environment.UserName;

			Flags = (NtlmFlags) 0x8200;
			if ((type2.Flags & NtlmFlags.NegotiateUnicode) != 0)
				Flags |= NtlmFlags.NegotiateUnicode;
			else
				Flags |= NtlmFlags.NegotiateOem;

			if ((type2.Flags & NtlmFlags.NegotiateNtlm2Key) != 0)
				Flags |= NtlmFlags.NegotiateNtlm2Key;
		}

		~Type3Message () 
		{
			if (_challenge != null)
				Array.Clear (_challenge, 0, _challenge.Length);
			if (_lm != null)
				Array.Clear (_lm, 0, _lm.Length);
			if (_nt != null)
				Array.Clear (_nt, 0, _nt.Length);
		}

		// Default auth level

		static NtlmAuthLevel _default = NtlmAuthLevel.LM_and_NTLM_and_try_NTLMv2_Session;

		public static NtlmAuthLevel DefaultAuthLevel {
			get { return _default; }
			set { _default = value; }
		}

		public NtlmAuthLevel Level {
			get { return _level; }
			set { _level = value; }
		}
		
		// properties

		[Obsolete (LegacyAPIWarning)]
		public byte[] Challenge {
			get { 
				if (_challenge == null)
					return null;
				return (byte[]) _challenge.Clone (); }
			set { 
				if ((_type2 != null) || (_level != NtlmAuthLevel.LM_and_NTLM))
					throw new InvalidOperationException (
						"Refusing to use legacy-mode LM/NTLM authentication " +
							"unless explicitly enabled using DefaultAuthLevel.");
				
				if (value == null)
					throw new ArgumentNullException ("Challenge");
				if (value.Length != 8) {
					string msg = Locale.GetText ("Invalid Challenge Length (should be 8 bytes).");
					throw new ArgumentException (msg, "Challenge");
				}
				_challenge = (byte[]) value.Clone (); 
			}
		}

		public string Domain {
			get { return _domain; }
			set {
				if (value == null)
					value = "";
				if (value == "")
					Flags &= ~NtlmFlags.NegotiateDomainSupplied;
				else
					Flags |= NtlmFlags.NegotiateDomainSupplied;

				_domain = value;
			}
		}

		public string Host {
			get { return _host; }
			set {
				if (value == null)
					value = "";
				if (value == "")
					Flags &= ~NtlmFlags.NegotiateWorkstationSupplied;
				else
					Flags |= NtlmFlags.NegotiateWorkstationSupplied;

				_host = value;
			}
		}

		public string Password {
			get { return _password; }
			set { _password = value; }
		}

		public string Username {
			get { return _username; }
			set { _username = value; }
		}

		public byte[] LM {
			get { return _lm; }
		}

		public byte[] NT {
			get { return _nt; }
			set { _nt = value; }
		}

		// methods

		protected override void Decode (byte[] message)
		{
			base.Decode (message);

			_password = null;

			if (message.Length >= 64)
				Flags = (NtlmFlags)BitConverterLE.ToUInt32 (message, 60);
			else
				Flags = (NtlmFlags)0x8201;
			
			int lm_len = BitConverterLE.ToUInt16 (message, 12);
			int lm_off = BitConverterLE.ToUInt16 (message, 16);
			_lm = new byte [lm_len];
			Buffer.BlockCopy (message, lm_off, _lm, 0, lm_len);

			int nt_len = BitConverterLE.ToUInt16 (message, 20);
			int nt_off = BitConverterLE.ToUInt16 (message, 24);
			_nt = new byte [nt_len];
			Buffer.BlockCopy (message, nt_off, _nt, 0, nt_len);
			
			int dom_len = BitConverterLE.ToUInt16 (message, 28);
			int dom_off = BitConverterLE.ToUInt16 (message, 32);
			_domain = DecodeString (message, dom_off, dom_len);

			int user_len = BitConverterLE.ToUInt16 (message, 36);
			int user_off = BitConverterLE.ToUInt16 (message, 40);
			_username = DecodeString (message, user_off, user_len);
			
			int host_len = BitConverterLE.ToUInt16 (message, 44);
			int host_off = BitConverterLE.ToUInt16 (message, 48);
			_host = DecodeString (message, host_off, host_len);
			
			// Session key.  We don't use it yet.
			// int skey_len = BitConverterLE.ToUInt16 (message, 52);
			// int skey_off = BitConverterLE.ToUInt16 (message, 56);
		}

		string DecodeString (byte[] buffer, int offset, int len)
		{
			if ((Flags & NtlmFlags.NegotiateUnicode) != 0)
				return Encoding.Unicode.GetString (buffer, offset, len);
			else
				return Encoding.ASCII.GetString (buffer, offset, len);
		}

		byte[] EncodeString (string text)
		{
			if (text == null)
				return new byte [0];
			if ((Flags & NtlmFlags.NegotiateUnicode) != 0)
				return Encoding.Unicode.GetBytes (text);
			else
				return Encoding.ASCII.GetBytes (text);
		}

		public override byte[] GetBytes ()
		{
			byte[] target = EncodeString (_domain);
			byte[] user = EncodeString (_username);
			byte[] host = EncodeString (_host);

			byte[] lm, ntlm;
			if (_type2 == null) {
				if (_level != NtlmAuthLevel.LM_and_NTLM)
					throw new InvalidOperationException (
						"Refusing to use legacy-mode LM/NTLM authentication " +
							"unless explicitly enabled using DefaultAuthLevel.");
				
				using (var legacy = new ChallengeResponse (_password, _challenge)) {
					lm = legacy.LM;
					ntlm = legacy.NT;
				}
			} else {
				ChallengeResponse2.Compute (_type2, _level, _username, _password, _domain, out lm, out ntlm);
			}

			var lmresp_len = lm != null ? lm.Length : 0;
			var ntresp_len = ntlm != null ? ntlm.Length : 0;

			byte[] data = PrepareMessage (64 + target.Length + user.Length + host.Length + lmresp_len + ntresp_len);

			// LM response
			short lmresp_off = (short)(64 + target.Length + user.Length + host.Length);
			data [12] = (byte)lmresp_len;
			data [13] = (byte)0x00;
			data [14] = (byte)lmresp_len;
			data [15] = (byte)0x00;
			data [16] = (byte)lmresp_off;
			data [17] = (byte)(lmresp_off >> 8);

			// NT response
			short ntresp_off = (short)(lmresp_off + lmresp_len);
			data [20] = (byte)ntresp_len;
			data [21] = (byte)(ntresp_len >> 8);
			data [22] = (byte)ntresp_len;
			data [23] = (byte)(ntresp_len >> 8);
			data [24] = (byte)ntresp_off;
			data [25] = (byte)(ntresp_off >> 8);

			// target
			short dom_len = (short)target.Length;
			short dom_off = 64;
			data [28] = (byte)dom_len;
			data [29] = (byte)(dom_len >> 8);
			data [30] = data [28];
			data [31] = data [29];
			data [32] = (byte)dom_off;
			data [33] = (byte)(dom_off >> 8);

			// username
			short uname_len = (short)user.Length;
			short uname_off = (short)(dom_off + dom_len);
			data [36] = (byte)uname_len;
			data [37] = (byte)(uname_len >> 8);
			data [38] = data [36];
			data [39] = data [37];
			data [40] = (byte)uname_off;
			data [41] = (byte)(uname_off >> 8);

			// host
			short host_len = (short)host.Length;
			short host_off = (short)(uname_off + uname_len);
			data [44] = (byte)host_len;
			data [45] = (byte)(host_len >> 8);
			data [46] = data [44];
			data [47] = data [45];
			data [48] = (byte)host_off;
			data [49] = (byte)(host_off >> 8);

			// message length
			short msg_len = (short)data.Length;
			data [56] = (byte)msg_len;
			data [57] = (byte)(msg_len >> 8);

			int flags = (int)Flags;

			// options flags
			data [60] = (byte)flags;
			data [61] = (byte)((uint)flags >> 8);
			data [62] = (byte)((uint)flags >> 16);
			data [63] = (byte)((uint)flags >> 24);

			Buffer.BlockCopy (target, 0, data, dom_off, target.Length);
			Buffer.BlockCopy (user, 0, data, uname_off, user.Length);
			Buffer.BlockCopy (host, 0, data, host_off, host.Length);

			if (lm != null) {
				Buffer.BlockCopy (lm, 0, data, lmresp_off, lm.Length);
				Array.Clear (lm, 0, lm.Length);
			}
			Buffer.BlockCopy (ntlm, 0, data, ntresp_off, ntlm.Length);
			Array.Clear (ntlm, 0, ntlm.Length);

			return data;
		}
	}
}
