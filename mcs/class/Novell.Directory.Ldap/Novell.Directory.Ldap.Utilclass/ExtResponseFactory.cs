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
// Novell.Directory.Ldap.Utilclass.ExtResponseFactory.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Extensions;
using Novell.Directory.Ldap.Rfc2251;

namespace Novell.Directory.Ldap.Utilclass
{
	/// <summary> 
	/// Takes an LdapExtendedResponse and returns an object
	/// (that implements the base class ParsedExtendedResponse)
	/// based on the OID.
	/// 
	/// <p>You can then call methods defined in the child
	/// class to parse the contents of the response.  The methods available
	/// depend on the child class. All child classes inherit from the
	/// ParsedExtendedResponse.
	/// 
	/// </summary>
	public class ExtResponseFactory
	{
		
		/// <summary> Used to Convert an RfcLdapMessage object to the appropriate
		/// LdapExtendedResponse object depending on the operation being performed.
		/// 
		/// </summary>
		/// <param name="inResponse">  The LdapExtendedReponse object as returned by the
		/// extendedOperation method in the LdapConnection object.
		/// <br><br>
		/// </param>
		/// <returns> An object of base class LdapExtendedResponse.  The actual child
		/// class of this returned object depends on the operation being
		/// performed.
		/// 
		/// </returns>
		/// <exception cref=""> LdapException A general exception which includes an error message
		/// and an Ldap error code.
		/// </exception>
		
		static public LdapExtendedResponse convertToExtendedResponse(RfcLdapMessage inResponse)
		{
			
			LdapExtendedResponse tempResponse = new LdapExtendedResponse(inResponse);
			
			// Get the oid stored in the Extended response
			System.String inOID = tempResponse.ID;
			
			if ((System.Object) inOID == null)
				return tempResponse;
			// Is this an OID we support, if yes then build the
			// detailed LdapExtendedResponse object
			try
			{
				if (inOID.Equals(ReplicationConstants.NAMING_CONTEXT_COUNT_RES))
				{
					return new PartitionEntryCountResponse(inResponse);
				}
				if (inOID.Equals(ReplicationConstants.GET_IDENTITY_NAME_RES))
				{
					return new GetBindDNResponse(inResponse);
				}
				if (inOID.Equals(ReplicationConstants.GET_EFFECTIVE_PRIVILEGES_RES))
				{
					return new GetEffectivePrivilegesResponse(inResponse);
				}
				if (inOID.Equals(ReplicationConstants.GET_REPLICA_INFO_RES))
				{
					return new GetReplicaInfoResponse(inResponse);
				}
				if (inOID.Equals(ReplicationConstants.LIST_REPLICAS_RES))
				{
					return new ListReplicasResponse(inResponse);
				}
				if (inOID.Equals(ReplicationConstants.GET_REPLICATION_FILTER_RES))
				{
					return new GetReplicationFilterResponse(inResponse);
				}
				else
					return tempResponse;
			}
			catch (System.IO.IOException ioe)
			{
				throw new LdapException(ExceptionMessages.DECODING_ERROR, LdapException.DECODING_ERROR, (System.String) null);
			}
		}
	}
}
