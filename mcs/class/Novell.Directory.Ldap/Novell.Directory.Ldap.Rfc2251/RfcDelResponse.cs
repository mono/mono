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
// Novell.Directory.Ldap.Rfc2251.RfcDelResponse.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Rfc2251
{
	
	/// <summary> Represents and Ldap Delete Response.
	/// 
	/// <pre>
	/// DelResponse ::= [APPLICATION 11] LdapResult
	/// </pre>
	/// </summary>
	public class RfcDelResponse:RfcLdapResult
	{
		
		//*************************************************************************
		// Constructors for DelResponse
		//*************************************************************************
		
		/// <summary> The only time a client will create a DelResponse is when it is
		/// decoding it from an InputStream
		/// </summary>
		[CLSCompliantAttribute(false)]
		public RfcDelResponse(Asn1Decoder dec, System.IO.Stream in_Renamed, int len):base(dec, in_Renamed, len)
		{
		}
		
		/// <summary> Constructs an RfcDelResponse from parameters.
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
		public RfcDelResponse(Asn1Enumerated resultCode, RfcLdapDN matchedDN, RfcLdapString errorMessage, RfcReferral referral):base(resultCode, matchedDN, errorMessage, referral)
		{
			return ;
		}
		
		//*************************************************************************
		// Accessors
		//*************************************************************************
		
		/// <summary> Override getIdentifier to return an application-wide id.</summary>
		public override Asn1Identifier getIdentifier()
		{
			return new Asn1Identifier(Asn1Identifier.APPLICATION, true, LdapMessage.DEL_RESPONSE);
		}
	}
}
