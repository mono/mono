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
// Novell.Directory.Ldap.Extensions.GetEffectivePrivilegesRequest.cs
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

namespace Novell.Directory.Ldap.Extensions
{
	
	/// <summary> 
	/// Returns the effective rights of one object to an attribute of another object.
	/// 
	/// To use this class, you must instantiate an object of this class and then
	/// call the extendedOperation method with this object as the required
	/// LdapExtendedOperation parameter.
	/// 
	/// The returned LdapExtendedResponse object can then be converted to
	/// a GetEffectivePrivilegesResponse object with the ExtendedResponseFactory class.
	/// The GetEffectivePrivilegesResponse class  contains methods for
	/// retrieving the effective rights.
	/// 
	/// The getEffectivePrivilegesRequest extension uses the following OID:
	/// 2.16.840.1.113719.1.27.100.33
	/// 
	/// The requestValue has the following format:
	/// 
	/// requestValue ::=
	///   dn         LdapDN
	///   trusteeDN  LdapDN
	///   attrName   LdapDN 
	/// </summary>
	public class GetEffectivePrivilegesRequest:LdapExtendedOperation
	{
		static GetEffectivePrivilegesRequest() 
		{
			/*
				* Register the extendedresponse class which is returned by the
				* server in response to a ListReplicasRequest
				*/
			try
			{
				LdapExtendedResponse.register(ReplicationConstants.GET_EFFECTIVE_PRIVILEGES_RES, System.Type.GetType("Novell.Directory.Ldap.Extensions.GetEffectivePrivilegesResponse"));
			}
			catch (System.Exception e)
			{
				System.Console.Error.WriteLine("Could not register Extended Response -" + " Class not found");
			}
		}
		
		/// <summary> Constructs an extended operation object for checking effective rights.
		/// 
		/// </summary>
		/// <param name="dn">       The distinguished name of the entry whose attribute is
		/// being checked.
		/// 
		/// </param>
		/// <param name="trusteeDN">The distinguished name of the entry whose trustee rights
		/// are being returned
		/// 
		/// </param>
		/// <param name="attrName"> The Ldap attribute name.
		/// 
		/// </param>
		/// <exception> LdapException A general exception which includes an error
		/// message and an Ldap error code.
		/// </exception>
		
		public GetEffectivePrivilegesRequest(System.String dn, System.String trusteeDN, System.String attrName):base(ReplicationConstants.GET_EFFECTIVE_PRIVILEGES_REQ, null)
		{
			
			try
			{
				
				if (((System.Object) dn == null))
					throw new System.ArgumentException(ExceptionMessages.PARAM_ERROR);
				
				System.IO.MemoryStream encodedData = new System.IO.MemoryStream();
				LBEREncoder encoder = new LBEREncoder();
				
				Asn1OctetString asn1_dn = new Asn1OctetString(dn);
				Asn1OctetString asn1_trusteeDN = new Asn1OctetString(trusteeDN);
				Asn1OctetString asn1_attrName = new Asn1OctetString(attrName);
				
				asn1_dn.encode(encoder, encodedData);
				asn1_trusteeDN.encode(encoder, encodedData);
				asn1_attrName.encode(encoder, encodedData);
				
				setValue(SupportClass.ToSByteArray(encodedData.ToArray()));
			}
			catch (System.IO.IOException ioe)
			{
				throw new LdapException(ExceptionMessages.ENCODING_ERROR, LdapException.ENCODING_ERROR, (System.String) null);
			}
		}
	}
}
