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
// Novell.Directory.Ldap.Rfc2251.RfcExtendedRequest.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap;

namespace Novell.Directory.Ldap.Rfc2251
{
	
	/// <summary> Represents an Ldap Extended Request.
	/// 
	/// <pre>
	/// ExtendedRequest ::= [APPLICATION 23] SEQUENCE {
	/// requestName      [0] LdapOID,
	/// requestValue     [1] OCTET STRING OPTIONAL }
	/// </pre>
	/// </summary>
	public class RfcExtendedRequest:Asn1Sequence, RfcRequest
	{
		
		/// <summary> Context-specific TAG for optional requestName.</summary>
		public const int REQUEST_NAME = 0;
		/// <summary> Context-specific TAG for optional requestValue.</summary>
		public const int REQUEST_VALUE = 1;
		
		//*************************************************************************
		// Constructors for ExtendedRequest
		//*************************************************************************
		
		/// <summary> Constructs an extended request.
		/// 
		/// </summary>
		/// <param name="requestName">The OID for this extended operation.
		/// </param>
		public RfcExtendedRequest(RfcLdapOID requestName):this(requestName, null)
		{
			return ;
		}
		
		/// <summary> Constructs an extended request.
		/// 
		/// </summary>
		/// <param name="requestName">The OID for this extended operation.
		/// </param>
		/// <param name="requestValue">An optional request value.
		/// </param>
		public RfcExtendedRequest(RfcLdapOID requestName, Asn1OctetString requestValue):base(2)
		{
			add(new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, REQUEST_NAME), requestName, false));
			if (requestValue != null)
				add(new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, REQUEST_VALUE), requestValue, false));
			return ;
		}
		
		
		/// <summary> Constructs an extended request from an existing request.
		/// 
		/// </summary>
		/// <param name="origRequest">Asn1Object of existing request
		/// </param>
		/* package */
		public RfcExtendedRequest(Asn1Object[] origRequest):base(origRequest, origRequest.Length)
		{
			return ;
		}
		//*************************************************************************
		// Accessors
		//*************************************************************************
		
		/// <summary> Override getIdentifier to return an application-wide id.
		/// 
		/// <pre>
		/// ID = CLASS: APPLICATION, FORM: CONSTRUCTED, TAG: 23.
		/// </pre>
		/// </summary>
		public override Asn1Identifier getIdentifier()
		{
			return new Asn1Identifier(Asn1Identifier.APPLICATION, true, LdapMessage.EXTENDED_REQUEST);
		}
		
		public RfcRequest dupRequest(System.String base_Renamed, System.String filter, bool request)
		{
			// Just dup the original request
			return new RfcExtendedRequest(toArray());
		}
		public System.String getRequestDN()
		{
			return null;
		}
	}
}
