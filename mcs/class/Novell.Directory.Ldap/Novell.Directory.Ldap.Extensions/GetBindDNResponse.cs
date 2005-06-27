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
// Novell.Directory.Ldap.Extensions.GetBindDNResponse.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.Utilclass;
using Novell.Directory.Ldap.Rfc2251;

namespace Novell.Directory.Ldap.Extensions
{
	
	/// <summary>  Retrieves the identity from an GetBindDNResponse object.
	/// 
	/// An object in this class is generated from an LdapExtendedResponse object
	/// using the ExtendedResponseFactory class.
	/// 
	/// The GetBindDNResponse extension uses the following OID:
	/// 2.16.840.1.113719.1.27.100.32 
	/// 
	/// </summary>
	public class GetBindDNResponse:LdapExtendedResponse
	{
		/// <summary> Returns the identity of the object.
		/// 
		/// </summary>
		/// <returns> A string value specifying the bind dn returned by the server.
		/// </returns>
		virtual public System.String Identity
		{
			get
			{
				return identity;
			}
			
		}
		
		// Identity returned by the server
		private System.String identity;
		
		/// <summary> Constructs an object from the responseValue which contains the bind dn.
		/// 
		/// The constructor parses the responseValue which has the following
		/// format:
		/// responseValue ::=
		/// identity   OCTET STRING
		/// 
		/// </summary>
		/// <exception> IOException The return value could not be decoded.
		/// </exception>
		public GetBindDNResponse(RfcLdapMessage rfcMessage):base(rfcMessage)
		{
			
			if (ResultCode == LdapException.SUCCESS)
			{
				// parse the contents of the reply
				sbyte[] returnedValue = this.Value;
				if (returnedValue == null)
					throw new System.IO.IOException("No returned value");
				
				// Create a decoder object
				LBERDecoder decoder = new LBERDecoder();
				if (decoder == null)
					throw new System.IO.IOException("Decoding error");
				
				// The only parameter returned should be an octet string
				Asn1OctetString asn1_identity = (Asn1OctetString) decoder.decode(returnedValue);
				if (asn1_identity == null)
					throw new System.IO.IOException("Decoding error");
				
				// Convert to normal string object
				identity = asn1_identity.stringValue();
				if ((System.Object) identity == null)
					throw new System.IO.IOException("Decoding error");
			}
			else
			{
				identity = "";
			}
		}
	}
}
