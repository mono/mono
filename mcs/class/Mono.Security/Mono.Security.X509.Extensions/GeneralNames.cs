//
// GeneralNames.cs: Handles GeneralNames for SubjectAltNameExtension and
//	CRLDistributionPointsExtension
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
using System.Net;
using System.Collections;
using System.Text;

using Mono.Security;
using Mono.Security.X509;

namespace Mono.Security.X509.Extensions {

	/*
	 * GeneralNames ::= SEQUENCE SIZE (1..MAX) OF GeneralName
	 * 
	 * GeneralName ::= CHOICE {
	 *    otherName                       [0]     OtherName,
	 *    rfc822Name                      [1]     IA5String,
	 *    dNSName                         [2]     IA5String,
	 *    x400Address                     [3]     ORAddress,
	 *    directoryName                   [4]     Name,
	 *    ediPartyName                    [5]     EDIPartyName,
	 *    uniformResourceIdentifier       [6]     IA5String,
	 *    iPAddress                       [7]     OCTET STRING,
	 *    registeredID                    [8]     OBJECT IDENTIFIER 
	 * }
	 * 
	 * OtherName ::= SEQUENCE {
	 *    type-id    OBJECT IDENTIFIER,
	 *    value      [0] EXPLICIT ANY DEFINED BY type-id 
	 * }
	 * 
	 * EDIPartyName ::= SEQUENCE {
	 *    nameAssigner            [0]     DirectoryString OPTIONAL,
	 *    partyName               [1]     DirectoryString 
	 * }
	 */

	// TODO - incomplete (only rfc822Name, dNSName are supported)
	internal class GeneralNames {

		private ArrayList rfc822Name;
		private ArrayList dnsName;
		private ArrayList directoryNames;
		private ArrayList uris;
		private ArrayList ipAddr;
		private ASN1 asn;

		public GeneralNames ()
		{
		}

		public GeneralNames (string[] rfc822s, string[] dnsNames, string[] ipAddresses, string[] uris)
		{
			// This is an extension
			asn = new ASN1 (0x30);

			if (rfc822s != null) {
				rfc822Name = new ArrayList ();
				foreach (string rfc822 in rfc822s) {
					asn.Add (new ASN1 (0x81, Encoding.ASCII.GetBytes (rfc822)));
					rfc822Name.Add (rfc822s);
				}
			}

			if (dnsNames != null) {
				dnsName = new ArrayList ();
				foreach (string dnsname in dnsNames) {
					asn.Add (new ASN1 (0x82, Encoding.ASCII.GetBytes (dnsname)));
					dnsName.Add(dnsname);
				}
			}

			if (ipAddresses != null) {
				ipAddr = new ArrayList ();
				foreach (string ipaddress in ipAddresses) {
					string[] parts = ipaddress.Split ('.', ':');
					byte[] bytes = new byte[parts.Length];
					for (int i = 0; i < parts.Length; i++) {
						bytes[i] = Byte.Parse (parts[i]);
					}
					asn.Add (new ASN1 (0x87, bytes));
					ipAddr.Add (ipaddress);
				}
			}

			if (uris != null) {
				this.uris = new ArrayList();
				foreach (string uri in uris) {
					asn.Add (new ASN1 (0x86, Encoding.ASCII.GetBytes (uri)));
					this.uris.Add (uri);
				}
			}
		}

		public GeneralNames (ASN1 sequence)
		{
			for (int i = 0; i < sequence.Count; i++) {
				switch (sequence[i].Tag) {
				case 0x81: // rfc822Name			[1]	IA5String
					if (rfc822Name == null)
						rfc822Name = new ArrayList ();
					rfc822Name.Add (Encoding.ASCII.GetString (sequence[i].Value));
					break;
				case 0x82: // dNSName				[2]     IA5String
					if (dnsName == null)
						dnsName = new ArrayList ();
					dnsName.Add (Encoding.ASCII.GetString (sequence[i].Value));
					break;
				case 0x84: // directoryName			[4]     Name
				case 0xA4:
					if (directoryNames == null)
						directoryNames = new ArrayList ();
					directoryNames.Add (X501.ToString (sequence[i][0]));
					break;
				case 0x86:  // uniformResourceIdentifier	[6]     IA5String
					if (uris == null)
						uris = new ArrayList ();
					uris.Add (Encoding.ASCII.GetString (sequence[i].Value));
					break;
				case 0x87: // iPAddress				[7]     OCTET STRING
					if (ipAddr == null)
						ipAddr = new ArrayList ();
					byte[] bytes = sequence[i].Value;
					string space = (bytes.Length == 4) ? "." : ":";
					StringBuilder sb = new StringBuilder();
					for (int j = 0; j < bytes.Length; j++) {
						sb.Append (bytes[j].ToString ());
						if (j < bytes.Length - 1)
							sb.Append (space); 
					}
					ipAddr.Add (sb.ToString());
					if (ipAddr == null)
						ipAddr = new ArrayList ();
					break;
				default:
					break;
				}
			}
		}

		public string[] RFC822 {
			get {
				if (rfc822Name == null)
					return new string[0];
				return (string[])rfc822Name.ToArray (typeof (string));
			}
		}

		public string[] DirectoryNames {
			get {
				if (directoryNames == null)
					return new string[0];
				return (string[])directoryNames.ToArray (typeof (string));
			}
		}

		public string[] DNSNames {
			get {
				if (dnsName == null)
					return new string[0];
				return (string[])dnsName.ToArray (typeof (string));
			}
		}

		public string[] UniformResourceIdentifiers {
			get {
				if (uris == null)
					return new string[0];
				return (string[])uris.ToArray (typeof (string));
			}
		}

		public string[] IPAddresses {
			get {
				if (ipAddr == null)
					return new string[0];
				return (string[])ipAddr.ToArray (typeof (string));
			}
		}

		public byte[] GetBytes ()
		{
			return asn.GetBytes ();
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			if (rfc822Name != null) {
				foreach (string s in rfc822Name) {
					sb.Append ("RFC822 Name=");
					sb.Append (s);
					sb.Append (Environment.NewLine);
				}
			}
			if (dnsName != null) {
				foreach (string s in dnsName) {
					sb.Append ("DNS Name=");
					sb.Append (s);
					sb.Append (Environment.NewLine);
				}
			}
			if (directoryNames != null) {
				foreach (string s in directoryNames) {
					sb.Append ("Directory Address: ");
					sb.Append (s);
					sb.Append (Environment.NewLine);
				}
			}
			if (uris != null) {
				foreach (string s in uris) {
					sb.Append ("URL=");
					sb.Append (s);
					sb.Append (Environment.NewLine);
				}
			}
			if (ipAddr != null) {
				foreach (string s in ipAddr) {
					sb.Append ("IP Address=");
					sb.Append (s);
					sb.Append (Environment.NewLine);
				}
			}
			return sb.ToString ();
		}
	}
}
