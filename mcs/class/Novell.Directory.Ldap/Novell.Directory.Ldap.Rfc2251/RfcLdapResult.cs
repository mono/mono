/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.Rfc2251.RfcLdapResult.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Rfc2251
{
	
	/// <summary> Represents an LdapResult.
	/// 
	/// <pre>
	/// LdapResult ::= SEQUENCE {
	/// resultCode      ENUMERATED {
	/// success                      (0),
	/// operationsError              (1),
	/// protocolError                (2),
	/// timeLimitExceeded            (3),
	/// sizeLimitExceeded            (4),
	/// compareFalse                 (5),
	/// compareTrue                  (6),
	/// authMethodNotSupported       (7),
	/// strongAuthRequired           (8),
	/// -- 9 reserved --
	/// referral                     (10),  -- new
	/// adminLimitExceeded           (11),  -- new
	/// unavailableCriticalExtension (12),  -- new
	/// confidentialityRequired      (13),  -- new
	/// saslBindInProgress           (14),  -- new
	/// noSuchAttribute              (16),
	/// undefinedAttributeType       (17),
	/// inappropriateMatching        (18),
	/// constraintViolation          (19),
	/// attributeOrValueExists       (20),
	/// invalidAttributeSyntax       (21),
	/// -- 22-31 unused --
	/// noSuchObject                 (32),
	/// aliasProblem                 (33),
	/// invalidDNSyntax              (34),
	/// -- 35 reserved for undefined isLeaf --
	/// aliasDereferencingProblem    (36),
	/// -- 37-47 unused --
	/// inappropriateAuthentication  (48),
	/// 
	/// invalidCredentials           (49),
	/// insufficientAccessRights     (50),
	/// busy                         (51),
	/// unavailable                  (52),
	/// unwillingToPerform           (53),
	/// loopDetect                   (54),
	/// -- 55-63 unused --
	/// namingViolation              (64),
	/// objectClassViolation         (65),
	/// notAllowedOnNonLeaf          (66),
	/// notAllowedOnRDN              (67),
	/// entryAlreadyExists           (68),
	/// objectClassModsProhibited    (69),
	/// -- 70 reserved for CLdap --
	/// affectsMultipleDSAs          (71), -- new
	/// -- 72-79 unused --
	/// other                        (80) },
	/// -- 81-90 reserved for APIs --
	/// matchedDN       LdapDN,
	/// errorMessage    LdapString,
	/// referral        [3] Referral OPTIONAL }
	/// </pre>
	/// 
	/// </summary>
	public class RfcLdapResult:Asn1Sequence, RfcResponse
	{
		
		/// <summary> Context-specific TAG for optional Referral.</summary>
		public const int REFERRAL = 3;
		
		//*************************************************************************
		// Constructors for RfcLdapResult
		//*************************************************************************
		
		/// <summary> Constructs an RfcLdapResult from parameters
		/// 
		/// </summary>
		/// <param name="resultCode">the result code of the operation
		/// 
		/// </param>
		/// <param name="matchedDN">the matched DN returned from the server
		/// 
		/// </param>
		/// <param name="errorMessage">the diagnostic message returned from the server
		/// </param>
		public RfcLdapResult(Asn1Enumerated resultCode, RfcLdapDN matchedDN, RfcLdapString errorMessage):this(resultCode, matchedDN, errorMessage, null)
		{
			return ;
		}
		
		/// <summary> Constructs an RfcLdapResult from parameters
		/// 
		/// </summary>
		/// <param name="resultCode">the result code of the operation
		/// 
		/// </param>
		/// <param name="matchedDN">the matched DN returned from the server
		/// 
		/// </param>
		/// <param name="errorMessage">the diagnostic message returned from the server
		/// 
		/// </param>
		/// <param name="referral">the referral(s) returned by the server
		/// </param>
		public RfcLdapResult(Asn1Enumerated resultCode, RfcLdapDN matchedDN, RfcLdapString errorMessage, RfcReferral referral):base(4)
		{
			add(resultCode);
			add(matchedDN);
			add(errorMessage);
			if (referral != null)
				add(referral);
			return ;
		}
		
		/// <summary> Constructs an RfcLdapResult from the inputstream</summary>
		[CLSCompliantAttribute(false)]
		public RfcLdapResult(Asn1Decoder dec, System.IO.Stream in_Renamed, int len):base(dec, in_Renamed, len)
		{
			
			// Decode optional referral from Asn1OctetString to Referral.
			if (size() > 3)
			{
				Asn1Tagged obj = (Asn1Tagged) get_Renamed(3);
				Asn1Identifier id = obj.getIdentifier();
				if (id.Tag == RfcLdapResult.REFERRAL)
				{
					sbyte[] content = ((Asn1OctetString) obj.taggedValue()).byteValue();
					System.IO.MemoryStream bais = new System.IO.MemoryStream(SupportClass.ToByteArray(content));
					set_Renamed(3, new RfcReferral(dec, bais, content.Length));
				}
			}
			return ;
		}
		
		//*************************************************************************
		// Accessors
		//*************************************************************************
		
		/// <summary> Returns the result code from the server
		/// 
		/// </summary>
		/// <returns> the result code
		/// </returns>
		public Asn1Enumerated getResultCode()
		{
			return (Asn1Enumerated) get_Renamed(0);
		}
		
		/// <summary> Returns the matched DN from the server
		/// 
		/// </summary>
		/// <returns> the matched DN
		/// </returns>
		public RfcLdapDN getMatchedDN()
		{
			return new RfcLdapDN(((Asn1OctetString) get_Renamed(1)).byteValue());
		}
		
		/// <summary> Returns the error message from the server
		/// 
		/// </summary>
		/// <returns> the server error message
		/// </returns>
		public RfcLdapString getErrorMessage()
		{
			return new RfcLdapString(((Asn1OctetString) get_Renamed(2)).byteValue());
		}
		
		/// <summary> Returns the referral(s) from the server
		/// 
		/// </summary>
		/// <returns> the referral(s)
		/// </returns>
		public RfcReferral getReferral()
		{
			return (size() > 3)?(RfcReferral) get_Renamed(3):null;
		}
	}
}
