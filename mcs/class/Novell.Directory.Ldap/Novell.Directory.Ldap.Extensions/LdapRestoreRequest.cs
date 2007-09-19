/******************************************************************************
* The MIT License
* Copyright (c) 2006 Novell Inc.  www.novell.com
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
// Novell.Directory.Ldap.Extensions.BackupRestoreConstants.cs
//
// Author:
//   Palaniappan N (NPalaniappan@novell.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
//


using System;
using System.IO;

using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Asn1;

/**
*
* This class provides an LDAP interface for object based  
* restore of eDirectory objects.
*
* <p>The information need for restore includes such items as  object DN,
* data buffer length, string containing the number of chunks and each chunk
* elements representing the size of each chunk, data blob in byte[]. The API
* support restoring of both non-encrypted and encrypted objects.
* </p>
* 
* <p>To send this request to eDirectory, you must
* create an instance of this class and then call the
* extendedOperation method with this object as the required
* LdapExtendedOperation parameter.</p><br>
*
* <p>The getLdapRestoreRequest extension uses the following OID:<br>
* &nbsp;&nbsp;&nbsp;2.16.840.1.113719.1.27.100.98</p><br>
*
* <p>The requestValue has the following format:<br>
*
* <p>requestValue ::=<br>
* objectDN ::= LDAPDN<br>
* passwd	  ::= OCTET STRING<br>
* bufferLength ::= INTEGER<br>
* retunedBuffer::= OCTET STRING<br>
* dataChunkSizes ::=<br>
* SEQUENCE {<br>
* noOfChunks INTEGER<br>
* SET of [<br>
* SEQUENCE of {eacChunksize INTEGER}]<br>
* }<br> </p>
*/

namespace Novell.Directory.Ldap.Extensions
{
	public class LdapRestoreRequest : LdapExtendedOperation	
	{	
		/**
		*
		* Constructs an extended operations object which contains the ber encoded
		* restore data.
		*
		* @param objectDN The object DN to restore
		* <br>
		* @param passwd 		The encrypted password required for the object to
		* be backed up
		* <br>
		* @param bufferLength The length of backed up data
		* <br>
		* @param chunkSizesString The String containing number of chunks and 
		* each chunk elements representing chunk sizes
		* <br>
		* @param returnedBuffer The actual data in byte[]
		* <br><br>
		* @exception LdapException A general exception which includes an error
		*                          message and an LDAP error code.
		*/

		public LdapRestoreRequest(String objectDN, byte[] passwd, 
			int bufferLength, String chunkSizesString, byte[] returnedBuffer): 
			base(BackupRestoreConstants.NLDAP_LDAP_RESTORE_REQUEST, null)			
		{	
			try 
			{
				//Verify the validity of arguments
				if (objectDN == null || bufferLength == 0 || 
					chunkSizesString == null || returnedBuffer == null)
						throw new ArgumentException("PARAM_ERROR");
				
				//If encrypted password has null reference make it null String
				if(passwd == null)
					passwd = System.Text.Encoding.UTF8.GetBytes("");
			
				/*
				 * From the input argument chunkSizesString get::
				 * chunkSize => Represents the number of chunks of data returned from server
				 * sizeOf each chunk => int represents the size of each chunk
				*/
				int index;
				int chunkSize;
				int[] chunks = null;
				index = chunkSizesString.IndexOf(';');
				try 
				{
					chunkSize = int.Parse(chunkSizesString.Substring(0, index));
				} 
				catch (FormatException e) 
				{
					throw new LdapLocalException(
							"Invalid data buffer send in the request",
							LdapException.ENCODING_ERROR);
				}
				//Return exception if chunkSize == 0
				if (chunkSize == 0)
					throw new ArgumentException("PARAM_ERROR");

				chunkSizesString = chunkSizesString.Substring(index + 1);

				int chunkIndex;
				//Construct chunks array
				chunks = new int[chunkSize];
				/*
				* Iterate through each member in buffer and
				* assign to chunks array elements
				*/
				for (int i = 0; i < chunkSize; i++) 
				{
					chunkIndex = chunkSizesString.IndexOf(';');
					if(chunkIndex == -1)
					{
						chunks[i] = int.Parse(chunkSizesString);
						break;
					}
					chunks[i] = int.Parse(chunkSizesString.Substring(0,
															chunkIndex));
					chunkSizesString = chunkSizesString.Substring(chunkIndex + 1);
				}
			
				MemoryStream encodedData = new MemoryStream();
				LBEREncoder encoder = new LBEREncoder();

				//Form objectDN, passwd, bufferLength, data byte[] as ASN1 Objects
				Asn1OctetString asn1_objectDN = new Asn1OctetString(objectDN);
				Asn1OctetString asn1_passwd = new Asn1OctetString(SupportClass.ToSByteArray(passwd));
				Asn1Integer asn1_bufferLength = new Asn1Integer(bufferLength);
				Asn1OctetString asn1_buffer = new Asn1OctetString(SupportClass.ToSByteArray(returnedBuffer));
				
				//Form the chunks sequence to be passed to Server
				Asn1Sequence asn1_chunksSeq = new Asn1Sequence();
				asn1_chunksSeq.add(new Asn1Integer(chunkSize));
				Asn1Set asn1_chunksSet = new Asn1Set();
				for (int i = 0; i < chunkSize; i++) 
				{
					Asn1Integer tmpChunk = new Asn1Integer(chunks[i]);
					Asn1Sequence tmpSeq = new Asn1Sequence();
					tmpSeq.add(tmpChunk);
					asn1_chunksSet.add(tmpSeq);
				}
				asn1_chunksSeq.add(asn1_chunksSet);

				//Encode data to send to server
				asn1_objectDN.encode(encoder, encodedData);
				asn1_passwd.encode(encoder, encodedData);
				asn1_bufferLength.encode(encoder, encodedData);
				asn1_buffer.encode(encoder, encodedData);
				asn1_chunksSeq.encode(encoder, encodedData);
			
				// set the value of operation specific data
				setValue(SupportClass.ToSByteArray(encodedData.ToArray()));

			} 
			catch (IOException ioe) 
			{
				throw new LdapException("ENCODING_ERROR", LdapException.ENCODING_ERROR, (String) null);
			}
		}	
	}
}