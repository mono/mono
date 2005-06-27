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
// Novell.Directory.Ldap.Extensions.RemoveReplicaRequest.cs
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
	/// Removes a replica from the specified directory server.
	/// 
	/// To remove a replica from a particular server, you must create an instance
	/// of this class and then call the extendedOperation method with this
	/// object as the required LdapExtendedOperation parameter.
	/// 
	/// The removeReplicaRequest extension uses the following OID:
	/// 2.16.840.1.113719.1.27.100.11
	/// 
	/// The requestValue has the following format:
	/// 
	/// requestValue ::=
	///         flags        INTEGER
	///         serverName   LdapDN
	///         dn           LdapDN
	/// </summary>
	public class RemoveReplicaRequest:LdapExtendedOperation
	{
		
		/// <summary> Constructs an extended operation object for removing a replica.
		/// 
		/// </summary>
		/// <param name="dn">         The distinguished name of the replica's
		/// partition root.
		/// 
		/// </param>
		/// <param name="serverDN">   The distinguished name of server from which the replica
		/// will be removed.
		/// 
		/// </param>
		/// <param name="flags">  Determines whether all servers in the replica ring must
		/// be up before proceeding. When set to zero, the status of the
		/// servers is not checked. When set to Ldap_ENSURE_SERVERS_UP,
		/// all servers must be up for the operation to proceed.
		/// 
		/// </param>
		/// <exception> LdapException A general exception which includes an error message
		/// and an Ldap error code.
		/// </exception>
		public RemoveReplicaRequest(System.String dn, System.String serverDN, int flags):base(ReplicationConstants.DELETE_REPLICA_REQ, null)
		{
			
			try
			{
				
				if (((System.Object) dn == null) || ((System.Object) serverDN == null))
					throw new System.ArgumentException(ExceptionMessages.PARAM_ERROR);
				
				System.IO.MemoryStream encodedData = new System.IO.MemoryStream();
				LBEREncoder encoder = new LBEREncoder();
				
				Asn1Integer asn1_flags = new Asn1Integer(flags);
				Asn1OctetString asn1_serverDN = new Asn1OctetString(serverDN);
				Asn1OctetString asn1_dn = new Asn1OctetString(dn);
				
				asn1_flags.encode(encoder, encodedData);
				asn1_serverDN.encode(encoder, encodedData);
				asn1_dn.encode(encoder, encodedData);
				
				setValue(SupportClass.ToSByteArray(encodedData.ToArray()));
			}
			catch (System.IO.IOException ioe)
			{
				throw new LdapException(ExceptionMessages.ENCODING_ERROR, LdapException.ENCODING_ERROR, (System.String) null);
			}
		}
	}
}
