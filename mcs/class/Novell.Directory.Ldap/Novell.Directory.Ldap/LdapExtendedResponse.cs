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
// Novell.Directory.Ldap.LdapExtendedResponse.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap.Rfc2251;
using Novell.Directory.Ldap.Asn1;
using RespExtensionSet = Novell.Directory.Ldap.Utilclass.RespExtensionSet;

namespace Novell.Directory.Ldap
{
	
	/// <summary> 
	/// Encapsulates the response returned by an Ldap server on an
	/// asynchronous extended operation request.  It extends LdapResponse.
	/// 
	/// The response can contain the OID of the extension, an octet string
	/// with the operation's data, both, or neither.
	/// </summary>
	public class LdapExtendedResponse:LdapResponse
	{
		/// <summary> Returns the message identifier of the response.
		/// 
		/// </summary>
		/// <returns> OID of the response.
		/// </returns>
		virtual public System.String ID
		{
			get
			{
				RfcLdapOID respOID = ((RfcExtendedResponse) message.Response).ResponseName;
				if (respOID == null)
					return null;
				return respOID.stringValue();
			}
			
		}

		static LdapExtendedResponse()
		{
			registeredResponses = new RespExtensionSet();
		}

		public static RespExtensionSet RegisteredResponses
		{
			/* package */
			
			get
			{
				return registeredResponses;
			}
			
		}

		/// <summary> Returns the value part of the response in raw bytes.
		/// 
		/// </summary>
		/// <returns> The value of the response.
		/// </returns>
		[CLSCompliantAttribute(false)]
		virtual public sbyte[] Value
		{
			get
			{
				Asn1OctetString tempString = ((RfcExtendedResponse) message.Response).Response;
				if (tempString == null)
					return null;
				else
					return (tempString.byteValue());
			}
			
		}
		private static RespExtensionSet registeredResponses;
		
		/// <summary> Creates an LdapExtendedResponse object which encapsulates
		/// a server response to an asynchronous extended operation request.
		/// 
		/// </summary>
		/// <param name="message"> The RfcLdapMessage to convert to an
		/// LdapExtendedResponse object.
		/// </param>
		public LdapExtendedResponse(RfcLdapMessage message):base(message)
		{
		}

		/// <summary> Registers a class to be instantiated on receipt of a extendedresponse
		/// with the given OID.
		/// 
		/// <p>Any previous registration for the OID is overridden. The 
		/// extendedResponseClass object MUST be an extension of 
		/// LDAPExtendedResponse. </p>
		/// 
		/// </summary>
		/// <param name="oid">           The object identifier of the control.
		/// </param>
		/// <param name="extendedResponseClass"> A class which can instantiate an 
		/// LDAPExtendedResponse.
		/// </param>
		public static void  register(System.String oid, System.Type extendedResponseClass)
		{
			registeredResponses.registerResponseExtension(oid, extendedResponseClass);
			return ;
		}

	}
}
