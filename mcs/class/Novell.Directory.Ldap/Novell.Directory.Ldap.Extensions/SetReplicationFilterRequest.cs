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
// Novell.Directory.Ldap.Extensions.SetReplicationFilterRequest.cs
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
	/// Sets the Replication filter for all replicas on the server.
	/// 
	/// The filter specified is a an array of classnames-attribute names pairs. 
	/// 
	/// To set the filter for all replicas on the connected server, you must
	/// create an instance of this class and then call the
	/// extendedOperation method with this object as the required
	/// LdapExtendedOperation parameter.
	/// 
	/// The SetReplicationFilterRequest extension uses the following OID:
	/// 2.16.840.1.113719.1.27.100.35
	/// 
	/// The requestValue has the following format:
	/// 
	/// requestValue ::=
	///  serverName  LdapDN
	///  SEQUENCE of SEQUENCE {
	///  classname  OCTET STRING
	///  SEQUENCE of ATTRIBUTES
	/// }
	/// where
	/// ATTRIBUTES:: OCTET STRING
	/// </summary>
	public class SetReplicationFilterRequest:LdapExtendedOperation
	{
		
		/// <summary> 
		/// Constructs an extended operations object which contains the ber encoded
		/// replication filter.
		/// 
		/// </summary>
		/// <param name="serverDN">The server on which the replication filter needs to be set
		/// 
		/// </param>
		/// <param name="replicationFilter">An array of String Arrays. Each array starting with
		/// a class name followed by the attribute names for that class that should comprise
		/// the replication filter.
		/// 
		/// </param>
		/// <exception> LdapException A general exception which includes an error
		/// message and an Ldap error code.
		/// </exception>
		public SetReplicationFilterRequest(System.String serverDN, System.String[][] replicationFilter):base(ReplicationConstants.SET_REPLICATION_FILTER_REQ, null)
		{
			
			try
			{
				
				if ((System.Object) serverDN == null)
					throw new System.ArgumentException(ExceptionMessages.PARAM_ERROR);
				
				System.IO.MemoryStream encodedData = new System.IO.MemoryStream();
				LBEREncoder encoder = new LBEREncoder();
				
				Asn1OctetString asn1_serverDN = new Asn1OctetString(serverDN);
				
				// Add the serverDN to encoded data
				asn1_serverDN.encode(encoder, encodedData);
				
				// The toplevel sequenceOF
				Asn1SequenceOf asn1_replicationFilter = new Asn1SequenceOf();
				
				if (replicationFilter == null)
				{
					asn1_replicationFilter.encode(encoder, encodedData);
					setValue(SupportClass.ToSByteArray(encodedData.ToArray()));
					return ;
				}
				
				int i = 0;
				// for every element in the array
				while ((i < replicationFilter.Length) && (replicationFilter[i] != null))
				{
					
					
					// The following additional Sequence is not needed
					// as defined by the Asn1. But the server and the
					// C client are encoding it. Remove this when server
					// and C client are fixed to conform to the published Asn1.
					Asn1Sequence buginAsn1Representation = new Asn1Sequence();
					
					// Add the classname to the sequence -
					buginAsn1Representation.add(new Asn1OctetString(replicationFilter[i][0]));
					
					// Start a sequenceOF for attributes
					Asn1SequenceOf asn1_attributeList = new Asn1SequenceOf();
					
					// For every attribute in the array - remember attributes start after
					// the first element
					int j = 1;
					while ((j < replicationFilter[i].Length) && ((System.Object) replicationFilter[i][j] != null))
					{
						
						// Add the attribute name to the inner SequenceOf
						asn1_attributeList.add(new Asn1OctetString(replicationFilter[i][j]));
						j++;
					}
					
					
					// Add the attributeList to the sequence - extra add due to bug
					buginAsn1Representation.add(asn1_attributeList);
					asn1_replicationFilter.add(buginAsn1Representation);
					i++;
				}
				
				asn1_replicationFilter.encode(encoder, encodedData);
				setValue(SupportClass.ToSByteArray(encodedData.ToArray()));
			}
			catch (System.IO.IOException ioe)
			{
				throw new LdapException(ExceptionMessages.ENCODING_ERROR, LdapException.ENCODING_ERROR, (System.String) null);
			}
		}
	}
}
