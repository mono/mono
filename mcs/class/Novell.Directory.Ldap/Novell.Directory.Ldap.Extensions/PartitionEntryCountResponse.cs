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
// Novell.Directory.Ldap.Extensions.PartitionEntryCountResponse.cs
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
	
	/// <summary> Returns the number of entries in the partition.
	/// 
	/// An object in this class is generated from an ExtendedResponse object
	/// using the ExtendedResponseFactory class.
	/// 
	/// The PartitionEntryCountResponse extension uses the following
	/// OID:
	/// 2.16.840.1.113719.1.27.100.14
	/// 
	/// </summary>
	public class PartitionEntryCountResponse:LdapExtendedResponse
	{
		/// <summary> Returns the number of entries in the naming context.
		/// 
		/// </summary>
		/// <returns> The count of the number of objects returned.
		/// </returns>
		virtual public int Count
		{
			get
			{
				return count;
			}
			
		}
		
		//The count of the objects returned by the server is saved here
		private int count;
		
		/// <summary> Constructs an object from the responseValue which contains the
		/// entry count.
		/// 
		/// The constructor parses the responseValue which has the following
		/// format:
		/// responseValue ::=
		///   count  INTEGER
		/// 
		/// </summary>
		/// <exception> IOException  The response value could not be decoded.
		/// </exception>
		public PartitionEntryCountResponse(RfcLdapMessage rfcMessage):base(rfcMessage)
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
				
				Asn1Integer asn1_count = (Asn1Integer) decoder.decode(returnedValue);
				if (asn1_count == null)
					throw new System.IO.IOException("Decoding error");
				
				count = asn1_count.intValue();
			}
			else
			{
				count = - 1;
			}
		}
	}
}
