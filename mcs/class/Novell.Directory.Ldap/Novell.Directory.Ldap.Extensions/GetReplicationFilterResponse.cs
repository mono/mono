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
// Novell.Directory.Ldap.Extensions.GetReplicationFilterResponse.cs
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
	
	/// <summary>  This object represent the filter returned fom a GetReplicationFilterRequest.
	/// 
	/// An object in this class is generated from an ExtendedResponse object
	/// using the ExtendedResponseFactory class.
	/// 
	/// The GetReplicationFilterResponse extension uses the following OID:
	/// 2.16.840.1.113719.1.27.100.38
	/// 
	/// </summary>
	public class GetReplicationFilterResponse:LdapExtendedResponse
	{
		/// <summary> Returns the replicationFilter as an array of classname-attribute name pairs
		/// 
		/// </summary>
		/// <returns> String array contining a two dimensional array of strings.  The first
		/// element of each array is the class name the others are the attribute names
		/// </returns>
		virtual public System.String[][] ReplicationFilter
		{
			get
			{
				return returnedFilter;
			}
			
		}
		
		
		
		// Replication filter returned by the server goes here
		internal System.String[][] returnedFilter;
		
		/// <summary> Constructs an object from the responseValue which contains the replication
		/// filter.
		/// 
		/// The constructor parses the responseValue which has the following
		/// format:
		/// responseValue ::=
		///  SEQUENCE of SEQUENCE {
		///  classname  OCTET STRING
		///  SEQUENCE of ATTRIBUTES
		/// }
		/// where
		/// ATTRIBUTES:: OCTET STRING
		/// 
		/// </summary>
		/// <exception> IOException The responseValue could not be decoded.
		/// </exception>
		public GetReplicationFilterResponse(RfcLdapMessage rfcMessage):base(rfcMessage)
		{
			
			if (ResultCode != LdapException.SUCCESS)
			{
				returnedFilter = new System.String[0][];
				for (int i = 0; i < 0; i++)
				{
					returnedFilter[i] = new System.String[0];
				}
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
				
				// How many sequences in this list
				int numberOfSequences = returnedSequence.size();
				returnedFilter = new System.String[numberOfSequences][];
				
				// Parse each returned sequence object
				for (int classNumber = 0; classNumber < numberOfSequences; classNumber++)
				{
					
					// Get the next Asn1Sequence
					Asn1Sequence asn1_innerSequence = (Asn1Sequence) returnedSequence.get_Renamed(classNumber);
					if (asn1_innerSequence == null)
						throw new System.IO.IOException("Decoding error");
					
					// Get the asn1 encoded classname
					Asn1OctetString asn1_className = (Asn1OctetString) asn1_innerSequence.get_Renamed(0);
					if (asn1_className == null)
						return ;
					
					// Get the attribute List
					Asn1Sequence asn1_attributeList = (Asn1Sequence) asn1_innerSequence.get_Renamed(1);
					if (asn1_attributeList == null)
						throw new System.IO.IOException("Decoding error");
					
					int numberOfAttributes = asn1_attributeList.size();
					returnedFilter[classNumber] = new System.String[numberOfAttributes + 1];
					
					// Get the classname
					returnedFilter[classNumber][0] = asn1_className.stringValue();
					if ((System.Object) returnedFilter[classNumber][0] == null)
						throw new System.IO.IOException("Decoding error");
					
					for (int attributeNumber = 0; attributeNumber < numberOfAttributes; attributeNumber++)
					{
						
						// Get the asn1 encoded attribute name
						Asn1OctetString asn1_attributeName = (Asn1OctetString) asn1_attributeList.get_Renamed(attributeNumber);
						if (asn1_attributeName == null)
							throw new System.IO.IOException("Decoding error");
						
						// Get attributename string
						returnedFilter[classNumber][attributeNumber + 1] = asn1_attributeName.stringValue();
						if ((System.Object) returnedFilter[classNumber][attributeNumber + 1] == null)
							throw new System.IO.IOException("Decoding error");
					}
				}
			}
		}
	}
}
