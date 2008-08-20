using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using Mono.Security;
using Mono.Security.Protocol.Ntlm;

namespace System.ServiceModel.Security
{
	internal abstract class SspiSession
	{
		internal static readonly byte [] NtlmSSP = new byte [] {
			0x4E, 0x54, 0x4C, 0x4D, 0x53, 0x53, 0x50, 0x00};

		public long Challenge, Context, ClientOSVersion, ServerOSVersion;
		public string ServerName, DomainName, DnsHostName, DnsDomainName;

		public bool Verify (byte [] expected, byte [] actual, int offset, int length)
		{
			if (expected.Length != length)
				return false;
			for (int i = 0; i < length; i++)
				if (expected [i] != actual [i + offset])
					return false;
			return true;
		}

		public SspiSecurityBufferStruct ReadSecurityBuffer (BinaryReader reader)
		{
			return new SspiSecurityBufferStruct (
				reader.ReadInt16 (),
				reader.ReadInt16 (),
				reader.ReadInt32 ());
		}
	}

	internal struct SspiSecurityBufferStruct
	{
		public SspiSecurityBufferStruct (short length, short allocatedSpace, int offset)
		{
			Length = length;
			AllocatedSpace = allocatedSpace;
			Offset = offset;
		}

		public readonly short Length;
		public readonly short AllocatedSpace;
		public readonly int Offset;
	}

	internal class SspiClientSession : SspiSession
	{
		Type2Message type2;
		Type3Message type3;

		// Class(60) {
		//   OID(spnego),
		//   Class(A0) {
		//     Class(30) {
		//       Class(A0) {
		//         Class(30) { OID,OID,OID} },
		//       Class(A2) { OctetStream } } } }
		public byte [] ProcessSpnegoInitialContextTokenRequest ()
		{
			Type1Message type1 = new Type1Message (NtlmVersion.Version3);
			type1.Flags = unchecked ((NtlmFlags) 0xE21882B7);
			type1.Domain = "WORKGROUP"; // FIXME: remove it

			ASN1 asn = new ASN1 (0x60);
			ASN1 asn2 = new ASN1 (0xA0);
			ASN1 asn21 = new ASN1 (0x30);
			ASN1 asn211 = new ASN1 (0xA0);
			ASN1 asn2111 = new ASN1 (0x30);
			asn211.Add (asn2111);
			asn2111.Add (ASN1Convert.FromOid (Constants.OidNtlmSsp));
			asn2111.Add (ASN1Convert.FromOid (Constants.OidKerberos5));
			asn2111.Add (ASN1Convert.FromOid (Constants.OidMIT));
			ASN1 asn212 = new ASN1 (0xA2);
			ASN1 asn2121 = new ASN1 (0x4);
			asn2121.Value = type1.GetBytes ();
			asn212.Add (asn2121);
			asn21.Add (asn211);
			asn21.Add (asn212);
			asn2.Add (asn21);
			asn.Add (ASN1Convert.FromOid (Constants.OidSpnego));
			asn.Add (asn2);
			return asn.GetBytes ();
		}

		// Example buffer:
		// A18181 307F A003
		//   0A0101
		//   A10C 060A2B06010401823702020A
		//   A26A 0468 NTLM
		//   NTLM = 4E544C4D53535000 0200000004000400 3800000035829AE2
		//    0D1A7FF0F171F339 0000000000000000 2C002C003C000000
		//    0501280A0000000F 5000430002000400 5000430001000400
		//    5000430004000400 5000430003000400 5000430006000400
		//    0100000000000000
		public void ProcessSpnegoInitialContextTokenResponse (byte [] raw)
		{
			ASN1 asn1 = new ASN1 (raw);
			// FIXME: check OIDs and structure
			ProcessMessageType2 (asn1 [0] [2] [0].Value);
		}

		// Class { Class { Class { OctetStream } } }
		public byte [] ProcessSpnegoProcessContextToken (string user, string pass)
		{
			ASN1 asn = new ASN1 (0xA1);
			ASN1 asn2 = new ASN1 (0x30);
			ASN1 asn3 = new ASN1 (0xA2);
			asn3.Add (new ASN1 (0x04, ProcessMessageType3 (user, pass)));
			asn2.Add (asn3);
			asn.Add (asn2);
			return asn.GetBytes ();
		}

		public byte [] ProcessMessageType1 ()
		{
			Type1Message type1 = new Type1Message (NtlmVersion.Version3);
			type1.Flags = unchecked ((NtlmFlags) 0xE21882B7);
			return type1.GetBytes ();
		}

		string TargetName;

		public void ProcessMessageType2 (byte [] raw)
		{
			type2 = new Type2Message (raw);
		}

		public byte [] ProcessMessageType3 (string user, string password)
		{
			TargetName = Environment.MachineName;
			ServerName = Environment.MachineName;
			// FIXME
			DomainName = ServerName;// IPGlobalProperties.GetIPGlobalProperties ().DomainName;
			DnsHostName = Dns.GetHostName ();
			DnsDomainName = DnsHostName; // FIXME

			type3 = new Type3Message (NtlmVersion.Version3);
			type3.Flags = (NtlmFlags) (unchecked ((int) 0xE2188235));
			type3.Domain = DomainName;
			type3.Host = DnsHostName;
			type3.Challenge = type2.Nonce;
			type3.Username = user;
			type3.Password = password;

			return type3.GetBytes ();
		}
	}

	internal class SspiServerSession : SspiSession
	{
		public string TargetName;
		public long SuppliedDomain, SuppliedWorkstation;
		Type1Message type1;
		Type2Message type2;
		Type3Message type3;

		// Example buffer:
		// 6069 0606 2B0601050502 A05F 305D A024 3022
		//	  060A 2B06010401823702020A
		//	  0609 2A864882F712010202
		//	  0609 2A864886F712010202
		// A235 0433 NTLM
		// NTLM = 4E544C4D53535000 01000000 B7B218E2 090009002A000000
		//  0200020028000000 0501280A0000000F 5043 574F524B47524F5550
		public void ProcessSpnegoInitialContextTokenRequest (byte [] raw)
		{
			ASN1 asn1 = new ASN1 (raw);
			// FIXME: check OIDs
			ProcessMessageType1 (asn1 [1] [0] [1] [0].Value);
		}

		// Class {
		//   Class {
		//     Class { Enum },
		//     Class { OID(NTLMSSP) },
		//     Class { OctetStream } } }
		public byte [] ProcessSpnegoInitialContextTokenResponse ()
		{
			ASN1 top = new ASN1 (0xA1);
			ASN1 asn = new ASN1 (0x30);
			ASN1 asn1 = new ASN1 (0xA0);
			// FIXME: what is this enum?
			asn1.Add (new ASN1 (0x0A, new byte [] {1})); // Enum whatever
			ASN1 asn2 = new ASN1 (0xA1);
			asn2.Add (ASN1Convert.FromOid (Constants.OidNtlmSsp));
			ASN1 asn3 = new ASN1 (0xA2);
			asn3.Add (new ASN1 (0x04, ProcessMessageType2 ()));
			asn.Add (asn1);
			asn.Add (asn2);
			asn.Add (asn3);
			top.Add (asn);
			return top.GetBytes ();
		}

		// Example buffer:
		// A181A7
		//   3081A4
		//     A281A1
		//       04819E
		// 4E544C4D53535000 03000000 
		// 180018005E000000 1800180076000000 0400040048000000
		// 0E000E004C000000 040004005A000000 100010008E000000
		// 358218E2 0501280A0000000F
		// 50004300 6100740073007500730068006900 50004300
		// [8 bytes LM] [16 bytes of 0s]
		// [24 bytes of NTLM]
		// C94EE2ADE7E32244 BD60D3B33609C167
		public void ProcessSpnegoProcessContextToken (byte [] raw)
		{
			ASN1 asn1 = new ASN1 (raw);
			// FIXME: check structure
			ProcessMessageType3 (asn1 [0] [0] [0].Value);
		}

		public void ProcessMessageType1 (byte [] raw)
		{
			type1 = new Type1Message (raw, NtlmVersion.Version3);
		}

		public byte [] ProcessMessageType2 ()
		{
			byte [] bytes = new byte [8];
			RandomNumberGenerator.Create ().GetNonZeroBytes (bytes);
			Challenge = bytes [0] << 24 + bytes [1] << 16 + bytes [2] << 8 + bytes [3];
			Context = 0; // FIXME
			ServerOSVersion = 0x0F00000A28010500; // FIXME
			TargetName = Environment.MachineName;
			ServerName = Environment.MachineName;
			// FIXME
			DomainName = ServerName;// IPGlobalProperties.GetIPGlobalProperties ().DomainName;
			DnsHostName = Dns.GetHostName ();
			DnsDomainName = DnsHostName; // FIXME

			type2 = new Type2Message (NtlmVersion.Version3);
			type2.Flags = (NtlmFlags) (unchecked ((int) 0xE21882B7));
			type2.TargetName = TargetName;
			type2.Target.ServerName = ServerName;
			type2.Target.DomainName = DomainName;
			type2.Target.DnsHostName = DnsHostName;
			type2.Target.DnsDomainName = DnsDomainName;
			return type2.GetBytes ();
		}

		public void ProcessMessageType3 (byte [] raw)
		{
			/*
			MemoryStream ms = new MemoryStream (raw);
			if (!Verify (NtlmSSP, raw, 0, 8))
				throw new SecurityNegotiationException ("Expected NTLM SSPI header not found");
			BinaryReader reader = new BinaryReader (ms);
			reader.ReadInt64 (); // skip 8 bytes
			if (reader.ReadInt32 () != 3)
				throw new SecurityNegotiationException ("SSPI type 3 message is expected");
			SspiSecurityBufferStruct lmResInfo = ReadSecurityBuffer (reader);
			SspiSecurityBufferStruct ntlmResInfo = ReadSecurityBuffer (reader);
			SspiSecurityBufferStruct targetNameInfo = ReadSecurityBuffer (reader);
			SspiSecurityBufferStruct userNameInfo = ReadSecurityBuffer (reader);
			SspiSecurityBufferStruct wsNameInfo = ReadSecurityBuffer (reader);
			SspiSecurityBufferStruct sessionKeyInfo = ReadSecurityBuffer (reader);
			int flags = reader.ReadInt32 ();
			ServerOSVersion = reader.ReadInt64 ();
			*/
			type3 = new Type3Message (raw, NtlmVersion.Version3);
		}
	}
}
