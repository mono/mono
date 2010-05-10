//
// SubjectAltNameExtension.cs: Handles X.509 SubjectAltName extensions.
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
	 * id-ce-subjectAltName OBJECT IDENTIFIER ::=  { id-ce 17 }
	 * 
	 * SubjectAltName ::= GeneralNames
	 * 
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

	// TODO: Directories not supported
#if MOONLIGHT
	internal
#else
	public 
#endif
	class SubjectAltNameExtension : X509Extension {

		private GeneralNames _names;

		public SubjectAltNameExtension ()
		{
			extnOid = "2.5.29.17";
			_names = new GeneralNames ();
		}

		public SubjectAltNameExtension (ASN1 asn1)
			: base (asn1)
		{
		}

		public SubjectAltNameExtension (X509Extension extension) 
			: base (extension) 
		{
		}

		public SubjectAltNameExtension (string[] rfc822, string[] dnsNames,
				string[] ipAddresses, string[] uris)
		{
			_names = new GeneralNames(rfc822, dnsNames, ipAddresses, uris);
			// 0x04 for string decoding and then the General Names!
			extnValue = new ASN1 (0x04, _names.GetBytes());
			extnOid = "2.5.29.17";
		//	extnCritical = true;
		}

		protected override void Decode () 
		{
			ASN1 sequence = new ASN1 (extnValue.Value);
			if (sequence.Tag != 0x30)
				throw new ArgumentException ("Invalid SubjectAltName extension");
			_names = new GeneralNames (sequence);
		}

		public override string Name {
			get { return "Subject Alternative Name"; }
		}

		public string[] RFC822 {
			get { return _names.RFC822; }
		}

		public string[] DNSNames {
			get { return _names.DNSNames; }
		}

		public string[] IPAddresses {
			get { return _names.IPAddresses; }
		}

		public string[] UniformResourceIdentifiers {
			get { return _names.UniformResourceIdentifiers; }
		}

		public override string ToString () 
		{
			return _names.ToString ();
		}
	}
}
