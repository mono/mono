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
using Novell.Directory.Ldap.Rfc2251;



/**
 *  This object represent the data returned from a LdapBackupRequest.
 *
 *  <p>An object in this class is generated from an ExtendedResponse object
 *  using the ExtendedResponseFactory class.</p>
 *
 *  <p>The LdapBackupResponse extension uses the following OID:<br>
 *  &nbsp;&nbsp;&nbsp;2.16.840.1.113719.1.27.100.97</p>
 *
 */

namespace Novell.Directory.Ldap.Extensions
{

	public class LdapBackupResponse:LdapExtendedResponse 
	{

		private int bufferLength; //Represents the length of backup data
		private String stateInfo; //Represent the state Information of data
	
		/*
		 * The String representing the number of chunks and each elements in chunk
		* array as returned by server.
		* Data from server is parsed as follows before sending to any Application::
		* no_of_chunks;sizeOf(chunk1);sizeOf(chunk2)…sizeOf(chunkn)
		* where
		* no_of_chunks => Represents the number of chunks of data returned from server
		* sizeOf(chunkn) => Represents the size of data in chunkn
		*/	
		private String chunkSizesString;
	
		/*
		 * Actual data of returned eDirectoty Object in byte[]
		*/
		private byte[] returnedBuffer;
	
		/**
		* Constructs an object from the responseValue which contains the backup data.
		*  <p>The constructor parses the responseValue which has the following
		*  format:<br>
		*  responseValue ::=<br>
		*  <p>databufferLength ::= INTEGER <br>
		*  mts(modification time stamp) ::= INTEGER<br>
		*  revision ::= INTEGER<br>
		*  returnedBuffer ::= OCTET STRING<br>
		*  dataChunkSizes ::= <br>
		*  SEQUENCE{<br>
		*  noOfChunks INTEGER<br>
		*  SET of [<br>
		*  SEQUENCE of {eachChunksize INTEGER}]<br>
		*  }</p>
		* 
		* @exception IOException The responseValue could not be decoded.
		*/
	
		public LdapBackupResponse(RfcLdapMessage rfcMessage): base(rfcMessage)
		{		
			int modificationTime = 0; // Modifaction timestamp of the Object
			int revision = 0; // Revision number of the Object
			int chunksSize = 0;
			int[] chunks = null; //Holds size of each chunks returned from server

			//Verify if returned ID is not proper
			if (ID == null	|| !(ID.Equals(BackupRestoreConstants.NLDAP_LDAP_BACKUP_RESPONSE)))
				throw new IOException("LDAP Extended Operation not supported");

			if (ResultCode == LdapException.SUCCESS) {
			// Get the contents of the reply

			byte[] returnedValue = SupportClass.ToByteArray(this.Value);
			if (returnedValue == null)
				throw new Exception("LDAP Operations error. No returned value.");

			// Create a decoder object
			LBERDecoder decoder = new LBERDecoder();

			if (decoder == null)
				throw new Exception("Decoding error");

			// Parse the parameters in the order
			MemoryStream currentPtr = new MemoryStream(returnedValue);

			// Parse bufferLength
			Asn1Integer asn1_bufferLength = (Asn1Integer) decoder
					.decode(currentPtr);
			if (asn1_bufferLength == null)
				throw new IOException("Decoding error");
			bufferLength = asn1_bufferLength.intValue();
			
			// Parse modificationTime
			Asn1Integer asn1_modificationTime = (Asn1Integer) decoder
					.decode(currentPtr);
			if (asn1_modificationTime == null)
				throw new IOException("Decoding error");
			modificationTime = asn1_modificationTime.intValue();

			// Parse revision
			Asn1Integer asn1_revision = (Asn1Integer) decoder
					.decode(currentPtr);
			if (asn1_revision == null)
				throw new IOException("Decoding error");
			revision = asn1_revision.intValue();
			
			//Format stateInfo to contain both modificationTime and revision
			this.stateInfo = modificationTime + "+" + revision;

			// Parse returnedBuffer
			Asn1OctetString asn1_returnedBuffer = (Asn1OctetString) decoder.decode(currentPtr);
			if (asn1_returnedBuffer == null)
				throw new IOException("Decoding error");

			returnedBuffer = SupportClass.ToByteArray(asn1_returnedBuffer.byteValue());
		
			
			/* 
			 * Parse chunks array 
			 * Chunks returned from server is encoded as shown below::
			 * SEQUENCE{
			 * 			chunksSize	INTEGER
			 * 			SET of [
			 * 				SEQUENCE of {eacChunksize        INTEGER}]
			 * 	       }
			 */
		
			Asn1Sequence asn1_chunksSeq = (Asn1Sequence) decoder
					.decode(currentPtr);
			if (asn1_chunksSeq == null)
				throw new IOException("Decoding error");
			
			//Get number of chunks returned from server
			chunksSize = ((Asn1Integer)asn1_chunksSeq.get_Renamed(0)).intValue();
			
			//Construct chunks array
			chunks = new int[chunksSize];
			
			Asn1Set asn1_chunksSet =  (Asn1Set)asn1_chunksSeq.get_Renamed(1);
			//Iterate through asn1_chunksSet and put each size into chunks array

			for (int index = 0; index < chunksSize; index++) 
			{
				Asn1Sequence asn1_eachSeq = (Asn1Sequence)asn1_chunksSet.get_Renamed(index);
				chunks[index] = ((Asn1Integer)asn1_eachSeq.get_Renamed(0)).intValue();
			}
						
			//Construct a temporary StringBuffer and append chunksSize, each size
			//element in chunks array and actual data of eDirectoty Object
			System.Text.StringBuilder tempBuffer = new System.Text.StringBuilder();
			tempBuffer.Append(chunksSize);
			tempBuffer.Append(";");
			int i = 0;

			for (; i < (chunksSize - 1); i++) 
			{
				tempBuffer.Append(chunks[i]);
				tempBuffer.Append(";");
			}
			
			tempBuffer.Append(chunks[i]);

			//Assign tempBuffer to parsedString to be returned to Application
			this.chunkSizesString = tempBuffer.ToString();
		} 
		else 
		{
			//Intialize all these if getResultCode() != LdapException.SUCCESS
			this.bufferLength = 0;
			this.stateInfo = null;
			this.chunkSizesString = null;
			this.returnedBuffer = null;
		}

	}
	
	/**
     * Returns the data buffer length
     *
     * @return bufferLength as integer.
     */
	public int getBufferLength() 
	{
		return bufferLength;
	}
	
	/**
     * Returns the stateInfo of returned eDirectory Object.
     * This is combination of MT (Modification Timestamp) and
     * Revision value with char '+' as separator between two.<br>
     * Client application if want to use both MT and Revision need to break
     * this string to get both these data.
     *
     * @return stateInfo as String.
     */
	public String getStatusInfo()
	{
		return stateInfo;
	}

	/**
     * Returns the data in String as::<br>
     * no_of_chunks;sizeOf(chunk1);sizeOf(chunk2)…sizeOf(chunkn)<br>
     * where<br>
     * no_of_chunks => Represents the number of chunks of data returned from server<br>
	 * sizeOf(chunkn) => Represents the size of data in chunkn<br>
	 * 
     * @return chunkSizesString as String.
     */
	public String getChunkSizesString() 
	{
				return chunkSizesString;
	}
	
	/**
     * Returns the data buffer as byte[]
     *
     * @return returnedBuffer as byte[].
     */
	public byte[] getReturnedBuffer() 
	{
		return returnedBuffer;
	}
	
}
}