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
// Novell.Directory.Ldap.Extensions.ListReplicasResponse.cs
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
	
	/// <summary> Retrieves the list of replicas from the specified server.
	/// 
	/// An object in this class is generated from an ExtendedResponse object
	/// using the ExtendedResponseFactory class.
	/// 
	/// The listReplicaResponse extension uses the following OID:
	/// 2.16.840.1.113719.1.27.20
	/// 
	/// </summary>
	public class ListReplicasResponse:LdapExtendedResponse
	{
		/// <summary> Returns a list of distinguished names for the replicas on the server.
		/// 
		/// </summary>
		/// <returns> String value specifying the identity returned by the server
		/// </returns>
		virtual public System.String[] ReplicaList
		{
			get
			{
				return replicaList;
			}
			
		}
		
		// Identity returned by the server
		private System.String[] replicaList;
		
		/// <summary> Constructs an object from the responseValue which contains the list
		/// of replicas.
		/// 
		/// The constructor parses the responseValue which has the following
		/// format:
		/// responseValue ::=
		///   replicaList
		/// SEQUENCE OF OCTET STRINGS
		/// 
		/// </summary>
		/// <exception> IOException  The responseValue could not be decoded.
		/// </exception>
		public ListReplicasResponse(RfcLdapMessage rfcMessage):base(rfcMessage)
		{
			
			if (ResultCode != LdapException.SUCCESS)
			{
				replicaList = new System.String[0];
			}
			else
			{
				// parse the contents of the reply
				sbyte[] returnedValue = this.Value;
				if (returnedValue == null)
					throw new System.IO.IOException("No returned value");
				
				// Create a decoder object
				LBERDecoder decoder = new LBERDecoder();
				if (decoder == null)
					throw new System.IO.IOException("Decoding error");
				
				// We should get back a sequence
				Asn1Sequence returnedSequence = (Asn1Sequence) decoder.decode(returnedValue);
				if (returnedSequence == null)
					throw new System.IO.IOException("Decoding error");
				
				// How many replicas were returned
				int len = returnedSequence.size();
				replicaList = new System.String[len];
				
				// Copy each one into our String array
				for (int i = 0; i < len; i++)
				{
					// Get the next Asn1Octet String in the sequence
					Asn1OctetString asn1_nextReplica = (Asn1OctetString) returnedSequence.get_Renamed(i);
					if (asn1_nextReplica == null)
						throw new System.IO.IOException("Decoding error");
					
					// Convert to a string
					replicaList[i] = asn1_nextReplica.stringValue();
					if ((System.Object) replicaList[i] == null)
						throw new System.IO.IOException("Decoding error");
				}
			}
		}
	}
}
