//
// Mono.Security.Protocol.Ntlm.Type3Message - Authentication
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
using System.Text;

namespace Mono.Security.Protocol.Ntlm {

	public class Type3Message : MessageBase {

		static private byte[] header = { 0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00, 0x03, 0x00, 0x00, 0x00, 0x18, 0x00, 0x18, 0x00 };

		private byte[] _challenge;
		private string _host;
		private string _domain;
		private string _username;
		private string _password;
		private byte[] _lm;
		private byte[] _nt;
		private int _options;

		public Type3Message () : base (3)
		{
			// default values
			_domain = Environment.UserDomainName;
			_host = Environment.MachineName;
			_username = Environment.UserName;
			_options = 0x8201;
		}

		public Type3Message (byte[] message) : base (3)
		{
			Decode (message);
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

		// properties

		public byte[] Challenge {
			get { 
				if (_challenge == null)
					return null;
				return (byte[]) _challenge.Clone (); }
			set { 
				if (value == null)
					throw new ArgumentNullException ("Challenge");
				if (value.Length != 8)
					throw new ArgumentException ("Invalid Challenge Length");
				_challenge = (byte[]) value.Clone (); 
			}
		}

		public string Domain {
			get { return _domain; }
			set { _domain = value; }
		}

		public string Host {
			get { return _host; }
			set { _host = value; }
		}

		public int Options {
			get { return _options; }
			set { _options = value; }
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
		}

		// methods

		private void Decode (byte[] message) 
		{
			if (message == null)
				throw new ArgumentNullException ("message");
		
			for (int i=0; i < header.Length; i++) {
				if (message [i] != header [i])
					throw new ArgumentException ("Invalid Type3 message");
			}

			if (BitConverter.ToUInt16 (message, 56) != message.Length)
				throw new ArgumentException ("Invalid Type3 message length");

			_password = null;

			int dom_len = BitConverter.ToUInt16 (message, 28);
			int dom_off = 64;
			_domain = Encoding.Unicode.GetString (message, dom_off, dom_len);

			int host_len = BitConverter.ToUInt16 (message, 44);
			int host_off = BitConverter.ToUInt16 (message, 48);
			_host = Encoding.Unicode.GetString (message, host_off, host_len);

			int user_len = BitConverter.ToUInt16 (message, 36);
			int user_off = BitConverter.ToUInt16 (message, 40);
			_username = Encoding.Unicode.GetString (message, user_off, user_len);

			_options = BitConverter.ToUInt16 (message, 60);

			_lm = new byte [24];
			int lm_off = BitConverter.ToUInt16 (message, 16);
			Buffer.BlockCopy (message, lm_off, _lm, 0, 24);
			
			_nt = new byte [24];
			int nt_off = BitConverter.ToUInt16 (message, 24);
			Buffer.BlockCopy (message, nt_off, _nt, 0, 24);

			if (message.Length >= 64)
				Flags = (NtlmFlags) BitConverter.ToUInt32 (message, 60);
		}

		public override byte[] GetBytes () 
		{
			byte[] domain = Encoding.Unicode.GetBytes (_domain.ToUpper ());
			byte[] user = Encoding.Unicode.GetBytes (_username);
			byte[] host = Encoding.Unicode.GetBytes (_host.ToUpper ());

			byte[] data = new byte [64 + domain.Length + user.Length + host.Length + 24 + 24];
			Buffer.BlockCopy (header, 0, data, 0, header.Length);

			// LM response
			short lmresp_off = (short)(64 + domain.Length + user.Length + host.Length);
			data [16] = (byte) lmresp_off;
			data [17] = (byte)(lmresp_off >> 8);

			// NT response
			short ntresp_off = (short)(lmresp_off + 24);
			data [20] = (byte) 0x18;
			data [21] = (byte) 0x00;
			data [22] = (byte) 0x18;
			data [23] = (byte) 0x00;
			data [24] = (byte) ntresp_off;
			data [25] = (byte)(ntresp_off >> 8);

			// domain
			short dom_len = (short)domain.Length;
			short dom_off = 64;
			data [28] = (byte) dom_len;
			data [29] = (byte)(dom_len >> 8);
			data [30] = data [28];
			data [31] = data [29];
			data [32] = (byte) dom_off;
			data [33] = (byte)(dom_off >> 8);

			// username
			short uname_len = (short)user.Length;
			short uname_off = (short)(dom_off + dom_len);
			data [36] = (byte) uname_len;
			data [37] = (byte)(uname_len >> 8);
			data [38] = data [36];
			data [39] = data [37];
			data [40] = (byte) uname_off;
			data [41] = (byte)(uname_off >> 8);

			// host
			short host_len = (short)host.Length;
			short host_off = (short)(uname_off + uname_len);
			data [44] = (byte) host_len;
			data [45] = (byte)(host_len >> 8);
			data [46] = data [44];
			data [47] = data [45];
			data [48] = (byte) host_off;
			data [49] = (byte)(host_off >> 8);

			// message length
			short msg_len = (short)data.Length;
			data [56] = (byte) msg_len;
			data [57] = (byte)(msg_len >> 8);

			// options flags
			data [60] = (byte) _options;
			data [61] = (byte)(_options >> 8);

			Buffer.BlockCopy (domain, 0, data, dom_off, domain.Length);
			Buffer.BlockCopy (user, 0, data, uname_off, user.Length);
			Buffer.BlockCopy (host, 0, data, host_off, host.Length);

			using (ChallengeResponse ntlm = new ChallengeResponse (_password, _challenge)) {
				Buffer.BlockCopy (ntlm.LM, 0, data, lmresp_off, 24);
				Buffer.BlockCopy (ntlm.NT, 0, data, ntresp_off, 24);
			}
			return data;
		}
	}
}
