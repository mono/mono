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
// Novell.Directory.Ldap.Controls.LdapVirtualListResponse.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Controls
{
	
	/// <summary> 
	/// LdapVirtualListResponse is a Server Control returned by the server in
	/// response to a virtual list search request.
	/// </summary>
	/// <summary> 
	/// In response to a VLV Search request the server returns an error code
	/// and if the search was successful returns the following information:
	/// <li> an index into the search results from where the returned list begins</li>
	/// <li> an estimate of the total number of elements in the search result</li>
	/// <li> an optional context field to be returned to the server with
	/// subsequent VLV request.</li>
	/// 
	/// </summary>
	public class LdapVirtualListResponse:LdapControl
	{
		/// <summary> Returns the size of the virtual search results list.  This integer as
		/// the servers current estimate of what the search result size.
		/// </summary>
		virtual public int ContentCount
		{
			get
			{
				return m_ContentCount;
			}
			
		}
		/// <summary> Returns the index of the first entry in the returned list.  The server uses
		/// the clients request information in conjunction with its current search result
		/// list to estimate what list of entries the client is requesting.  This integer
		/// is the index into the search results that is returned to the client.
		/// </summary>
		virtual public int FirstPosition
		{
			get
			{
				return m_firstPosition;
			}
			
		}
		/// <summary> Returns the result code for the virtual list search request.</summary>
		virtual public int ResultCode
		{
			get
			{
				return m_resultCode;
			}
			
		}
		/// <summary> Returns the cookie used by some servers to optimize the processing of
		/// virtual list requests. Subsequent VLV requests to the same server
		/// should return this String to the server.
		/// </summary>
		virtual public System.String Context
		{
			get
			{
				return m_context;
			}
			
		}
		/* The parsed fields are stored in these private variables */
		private int m_firstPosition;
		private int m_ContentCount;
		private int m_resultCode;
		
		/* The context field if one was returned by the server */
		private System.String m_context = null;
		
		/// <summary> This constructor is usually called by the SDK to instantiate an
		/// a LdapControl corresponding to the Server response to a Ldap
		/// VLV Control request.  Application programmers should not have
		/// any reason to call the constructor.  This constructor besides
		/// constructing a LdapVirtualListResponse control object also
		/// parses the contents of the response into local variables.
		/// 
		/// RFC 2891 defines this response control as follows:
		/// 
		/// The controlValue is an OCTET STRING, whose value is the BER
		/// encoding of a value of the following ASN.1:
		/// 
		/// VirtualListViewResponse ::= SEQUENCE {
		/// targetPosition    INTEGER (0 .. maxInt),
		/// contentCount     INTEGER (0 .. maxInt),
		/// virtualListViewResult ENUMERATED {
		/// success (0),
		/// operationsError (1),
		/// unwillingToPerform (53),
		/// insufficientAccessRights (50),
		/// busy (51),
		/// timeLimitExceeded (3),
		/// adminLimitExceeded (11),
		/// sortControlMissing (60),
		/// offsetRangeError (61),
		/// other (80) },
		/// contextID     OCTET STRING OPTIONAL }
		/// 
		/// 
		/// </summary>
		/// <param name="oid">    The OID of the control, as a dotted string.
		/// 
		/// </param>
		/// <param name="critical">  True if the Ldap operation should be discarded if
		/// the control is not supported. False if
		/// the operation can be processed without the control.
		/// 
		/// </param>
		/// <param name="values">    The control-specific data.
		/// </param>
		[CLSCompliantAttribute(false)]
		public LdapVirtualListResponse(System.String oid, bool critical, sbyte[] values):base(oid, critical, values)
		{
			
			/* Create a decoder object */
			LBERDecoder decoder = new LBERDecoder();
			if (decoder == null)
				throw new System.IO.IOException("Decoding error");
			
			/* We should get back an ASN.1 Sequence object */
			Asn1Object asnObj = decoder.decode(values);
			if ((asnObj == null) || (!(asnObj is Asn1Sequence)))
				throw new System.IO.IOException("Decoding error");
			
			/* Else we got back a ASN.1 sequence - print it if running debug code */
			
			/* Get the 1st element which should be an integer containing the
			* targetPosition (firstPosition)
			*/
			Asn1Object asn1firstPosition = ((Asn1Sequence) asnObj).get_Renamed(0);
			if ((asn1firstPosition != null) && (asn1firstPosition is Asn1Integer))
				m_firstPosition = ((Asn1Integer) asn1firstPosition).intValue();
			else
				throw new System.IO.IOException("Decoding error");
			
			/* Get the 2nd element which should be an integer containing the
			* current estimate of the contentCount
			*/
			Asn1Object asn1ContentCount = ((Asn1Sequence) asnObj).get_Renamed(1);
			if ((asn1ContentCount != null) && (asn1ContentCount is Asn1Integer))
				m_ContentCount = ((Asn1Integer) asn1ContentCount).intValue();
			else
				throw new System.IO.IOException("Decoding error");
			
			/* The 3rd element is an enum containing the errorcode */
			Asn1Object asn1Enum = ((Asn1Sequence) asnObj).get_Renamed(2);
			if ((asn1Enum != null) && (asn1Enum is Asn1Enumerated))
				m_resultCode = ((Asn1Enumerated) asn1Enum).intValue();
			else
				throw new System.IO.IOException("Decoding error");
			
			/* Optional 4th element could be the context string that the server
			* wants the client to send back with each subsequent VLV request
			*/
			if (((Asn1Sequence) asnObj).size() > 3)
			{
				Asn1Object asn1String = ((Asn1Sequence) asnObj).get_Renamed(3);
				if ((asn1String != null) && (asn1String is Asn1OctetString))
					m_context = ((Asn1OctetString) asn1String).stringValue();
			}
			return ;
		}
	}
}
