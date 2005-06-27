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
// Novell.Directory.Ldap.Controls.LdapVirtualListControl.cs
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
	
	/* The following is the ASN.1 of the VLV Request packet:
	*
	* VirtualListViewRequest ::= SEQUENCE {
	*      beforeCount    INTEGER (0..maxInt),
	*         afterCount     INTEGER (0..maxInt),
	*      CHOICE {
	*          byoffset [0] SEQUENCE {
	*              offset          INTEGER (0 .. maxInt),
	*              contentCount    INTEGER (0 .. maxInt) },
	*          greaterThanOrEqual [1] AssertionValue },
	*      contextID     OCTET STRING OPTIONAL }
	*
	*/
	
	/// <summary> LdapVirtualListControl is a Server Control used to specify
	/// that results from a search are to be returned in pages - which are
	/// subsets of the entire virtual result set.
	/// 
	/// On success, an updated LdapVirtualListResponse object is
	/// returned as a response Control, containing information on the virtual
	/// list size and the actual first index. This object can then be used
	/// by the client with a new requested position or length and sent to the
	/// server to obtain a different segment of the virtual list.
	/// 
	/// </summary>
	public class LdapVirtualListControl:LdapControl
	{
		/// <summary>    Returns the number of entries after the top/center one to return per
		/// page of results.
		/// </summary>
		virtual public int AfterCount
		{
			get
			{
				return m_afterCount;
			}
			
		}
		/// <summary>    Returns the number of entries before the top/center one to return per
		/// page of results.
		/// </summary>
		virtual public int BeforeCount
		{
			get
			{
				return m_beforeCount;
			}
			
		}
		/// <summary>    Returns the size of the virtual search results list. For a newly
		/// constructed control - one which is not the result of parseResponse on
		/// a control returned by a server - the method returns -1.
		/// </summary>
		/// <summary>    Sets the assumed size of the virtual search results list. This will
		/// typically be a number returned on a previous virtual list request in
		/// an LdapVirtualListResponse.
		/// </summary>
		virtual public int ListSize
		{
			get
			{
				return m_contentCount;
			}
			
			set
			{
				m_contentCount = value;
				
				/* since we just changed a field we need to rebuild the ber
				* encoded control
				*/
				BuildIndexedVLVRequest();
				
				/* Set the request data field in the in the parent LdapControl to
				* the ASN.1 encoded value of this control.  This encoding will be
				* appended to the search request when the control is sent.
				*/
				setValue(m_vlvRequest.getEncoding(new LBEREncoder()));
			}
			
		}
		/// <summary>   Returns the cookie used by some servers to optimize the processing of
		/// virtual list requests.
		/// </summary>
		/// <summary>    Sets the cookie used by some servers to optimize the processing of
		/// virtual list requests. It should be the context field returned in a
		/// virtual list response control for the same search.
		/// </summary>
		virtual public System.String Context
		{
			get
			{
				return m_context;
			}
			
			set
			{
				/* Index of the context field if one exists in the ber
				*/
				int CONTEXTIDINDEX = 3;
				
				/* Save off the new value in private variable
				*/
				m_context = value;
				
				/* Is there a context field that is already in the ber
				*/
				if (m_vlvRequest.size() == 4)
				{
					/* If YES then replace it */
					m_vlvRequest.set_Renamed(CONTEXTIDINDEX, new Asn1OctetString(m_context));
				}
				else if (m_vlvRequest.size() == 3)
				{
					/* If no then add a new one */
					m_vlvRequest.add(new Asn1OctetString(m_context));
				}
				
				/* Set the request data field in the in the parent LdapControl to
				* the ASN.1 encoded value of this control.  This encoding will be
				* appended to the search request when the control is sent.
				*/
				setValue(m_vlvRequest.getEncoding(new LBEREncoder()));
			}
			
		}
		
		/* The ASN.1 for the VLV Request has CHOICE field. These private
		* variables represent differnt ids for these different options
		*/
		private static int BYOFFSET = 0;
		private static int GREATERTHANOREQUAL = 1;
		
		
		/// <summary> The Request OID for a VLV Request</summary>
		private static System.String requestOID = "2.16.840.1.113730.3.4.9";
		
		/*
		* The Response stOID for a VLV Response
		*/
		private static System.String responseOID = "2.16.840.1.113730.3.4.10";
		
		/*
		* The encoded ASN.1 VLV Control is stored in this variable
		*/
		private Asn1Sequence m_vlvRequest;
		
		
		/* Private instance variables go here.
		* These variables are used to store copies of various fields
		* that can be set in a VLV control. One could have managed
		* without really defining these private variables by reverse
		* engineering each field from the ASN.1 encoded control.
		* However that would have complicated and slowed down the code.
		*/
		private int m_beforeCount;
		private int m_afterCount;
		private System.String m_jumpTo;
		private System.String m_context = null;
		private int m_startIndex = 0;
		private int m_contentCount = - 1;
		
		/// <summary> Constructs a virtual list control using the specified filter
		/// expression.
		/// 
		/// The expression specifies the first entry to be used for the
		/// virtual search results. The other two paramers are the number of
		/// entries before and after a located index to be returned.
		/// 
		/// </summary>
		/// <param name="jumpTo">           A search expression that defines the first
		/// element to be returned in the virtual search results. The filter
		/// expression in the search operation itself may be, for example,
		/// "objectclass=person" and the jumpTo expression in the virtual
		/// list control may be "cn=m*", to retrieve a subset of entries
		/// starting at or centered around those with a common name beginning
		/// with the letter "M". 
		/// 
		/// </param>
		/// <param name="beforeCount">   The number of entries before startIndex (the
		/// reference entry) to be returned. 
		/// 
		/// </param>
		/// <param name="afterCount">       The number of entries after startIndex to be
		/// returned. 
		/// </param>
		public LdapVirtualListControl(System.String jumpTo, int beforeCount, int afterCount):this(jumpTo, beforeCount, afterCount, null)
		{
			return ;
		}
		
		
		
		/// <summary> Constructs a virtual list control using the specified filter
		/// expression along with an optional server context.
		/// 
		/// The expression specifies the first entry to be used for the
		/// virtual search results. The other two paramers are the number of
		/// entries before and after a located index to be returned.
		/// 
		/// </summary>
		/// <param name="jumpTo">   A search expression that defines the first
		/// element to be returned in the virtual search results. The filter
		/// expression in the search operation itself may be, for example,
		/// "objectclass=person" and the jumpTo expression in the virtual
		/// list control may be "cn=m*", to retrieve a subset of entries
		/// starting at or centered around those with a common name beginning
		/// with the letter "M".
		/// 
		/// </param>
		/// <param name="beforeCount">The number of entries before startIndex (the
		/// reference entry) to be returned. 
		/// 
		/// </param>
		/// <param name="afterCount">The number of entries after startIndex to be
		/// returned. 
		/// 
		/// </param>
		/// <param name="context">Used by some implementations to process requests
		/// more efficiently. The context should be null on the first search,
		/// and thereafter it should be whatever was returned by the server in the
		/// virtual list response control.
		/// </param>
		public LdapVirtualListControl(System.String jumpTo, int beforeCount, int afterCount, System.String context):base(requestOID, true, null)
		{
			
			/* Save off the fields in local variables
			*/
			m_beforeCount = beforeCount;
			m_afterCount = afterCount;
			m_jumpTo = jumpTo;
			m_context = context;
			
			/* Call private method to build the ASN.1 encoded request packet.
			*/
			BuildTypedVLVRequest();
			
			/* Set the request data field in the in the parent LdapControl to
			* the ASN.1 encoded value of this control.  This encoding will be
			* appended to the search request when the control is sent.
			*/
			setValue(m_vlvRequest.getEncoding(new LBEREncoder()));
			return ;
		}
		
		/// <summary>Private method used to construct the ber encoded control
		/// Used only when using the typed mode of VLV Control.
		/// </summary>
		private void  BuildTypedVLVRequest()
		{
			/* Create a new Asn1Sequence object */
			m_vlvRequest = new Asn1Sequence(4);
			
			/* Add the beforeCount and afterCount fields to the sequence */
			m_vlvRequest.add(new Asn1Integer(m_beforeCount));
			m_vlvRequest.add(new Asn1Integer(m_afterCount));
			
			/* The next field is dependent on the type of indexing being used.
			* A "typed" VLV request uses a ASN.1 OCTET STRING to index to the
			* correct object in the list.  Encode the ASN.1 CHOICE corresponding
			* to this option (as indicated by the greaterthanOrEqual field)
			* in the ASN.1.
			*/
			m_vlvRequest.add(new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, GREATERTHANOREQUAL), new Asn1OctetString(m_jumpTo), false));
			
			/* Add the optional context string if one is available.
			*/
			if ((System.Object) m_context != null)
				m_vlvRequest.add(new Asn1OctetString(m_context));
			
			return ;
		}
		
		/// <summary> Use this constructor to fetch a subset when the size of the
		/// virtual list is known,
		/// 
		/// 
		/// </summary>
		/// <param name="beforeCount">The number of entries before startIndex (the
		/// reference entry) to be returned. 
		/// 
		/// </param>
		/// <param name="afterCount">   The number of entries after startIndex to be
		/// returned.
		/// 
		/// </param>
		/// <param name="startIndex">The index of the reference entry to be returned.
		/// 
		/// </param>
		/// <param name="contentCount">The total number of entries assumed to be in the
		/// list. This is a number returned on a previous search, in the
		/// LdapVirtualListResponse. The server may use this number to adjust
		/// the returned subset offset.
		/// </param>
		public LdapVirtualListControl(int startIndex, int beforeCount, int afterCount, int contentCount):this(startIndex, beforeCount, afterCount, contentCount, null)
		{
			return ;
		}
		
		
		
		/// <summary> Use this constructor to fetch a subset when the size of the
		/// virtual list is known,
		/// 
		/// 
		/// </summary>
		/// <param name="beforeCount">   The number of entries before startIndex (the
		/// reference entry) to be returned.
		/// 
		/// </param>
		/// <param name="afterCount">       The number of entries after startIndex to be
		/// returned.
		/// 
		/// </param>
		/// <param name="startIndex">    The index of the reference entry to be
		/// returned.
		/// 
		/// </param>
		/// <param name="contentCount">   The total number of entries assumed to be in the
		/// list. This is a number returned on a previous search, in the
		/// LdapVirtualListResponse. The server may use this number to adjust
		/// the returned subset offset.
		/// 
		/// </param>
		/// <param name="context">       Used by some implementations to process requests
		/// more efficiently. The context should be null on the first search,
		/// and thereafter it should be whatever was returned by the server in the
		/// virtual list response control.
		/// </param>
		public LdapVirtualListControl(int startIndex, int beforeCount, int afterCount, int contentCount, System.String context):base(requestOID, true, null)
		{
			
			
			/* Save off the fields in local variables
			*/
			m_beforeCount = beforeCount;
			m_afterCount = afterCount;
			m_startIndex = startIndex;
			m_contentCount = contentCount;
			m_context = context;
			
			/* Call private method to build the ASN.1 encoded request packet.
			*/
			BuildIndexedVLVRequest();
			
			/* Set the request data field in the in the parent LdapControl to
			* the ASN.1 encoded value of this control.  This encoding will be
			* appended to the search request when the control is sent.
			*/
			setValue(m_vlvRequest.getEncoding(new LBEREncoder()));
			return ;
		}
		
		/// <summary>Private method used to construct the ber encoded control
		/// Used only when using the Indexed mode of VLV Control
		/// </summary>
		private void  BuildIndexedVLVRequest()
		{
			/* Create a new Asn1Sequence object */
			m_vlvRequest = new Asn1Sequence(4);
			
			/* Add the beforeCount and afterCount fields to the sequence */
			m_vlvRequest.add(new Asn1Integer(m_beforeCount));
			m_vlvRequest.add(new Asn1Integer(m_afterCount));
			
			/* The next field is dependent on the type of indexing being used.
			* An "indexed" VLV request uses a ASN.1 SEQUENCE to index to the
			* correct object in the list.  Encode the ASN.1 CHOICE corresponding
			* to this option (as indicated by the byoffset fieldin the ASN.1.
			*/
			Asn1Sequence byoffset = new Asn1Sequence(2);
			byoffset.add(new Asn1Integer(m_startIndex));
			byoffset.add(new Asn1Integer(m_contentCount)); ;
			
			/* Add the ASN.1 sequence to the encoded data
			*/
			m_vlvRequest.add(new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, true, BYOFFSET), byoffset, false));
			
			/* Add the optional context string if one is available.
			*/
			if ((System.Object) m_context != null)
				m_vlvRequest.add(new Asn1OctetString(m_context));
			
			return ;
		}
		
		
		
		/// <summary> Sets the center or starting list index to return, and the number of
		/// results before and after.
		/// 
		/// 
		/// </summary>
		/// <param name="listIndex">       The center or starting list index to be
		/// returned. 
		/// 
		/// </param>
		/// <param name="beforeCount">       The number of entries before "listIndex" to be
		/// returned. 
		/// 
		/// </param>
		/// <param name="afterCount">       The number of entries after "listIndex" to be
		/// returned. 
		/// </param>
		public virtual void  setRange(int listIndex, int beforeCount, int afterCount)
		{
			
			/* Save off the fields in local variables
			*/
			m_beforeCount = beforeCount;
			m_afterCount = afterCount;
			m_startIndex = listIndex;
			
			/* since we just changed a field we need to rebuild the ber
			* encoded control
			*/
			BuildIndexedVLVRequest();
			
			/* Set the request data field in the in the parent LdapControl to
			* the ASN.1 encoded value of this control.  This encoding will be
			* appended to the search request when the control is sent.
			*/
			setValue(m_vlvRequest.getEncoding(new LBEREncoder()));
		}
		
		// PROPOSED ADDITION TO NEXT VERSION OF DRAFT (v7)
		/// <summary> Sets the center or starting list index to return, and the number of
		/// results before and after.
		/// 
		/// 
		/// </summary>
		/// <param name="jumpTo">A search expression that defines the first
		/// element to be returned in the virtual search results. The filter
		/// expression in the search operation itself may be, for example,
		/// "objectclass=person" and the jumpTo expression in the virtual
		/// list control may be "cn=m*", to retrieve a subset of entries
		/// starting at or centered around those with a common name
		/// beginning with the letter "M".
		/// 
		/// </param>
		/// <param name="beforeCount">   The number of entries before "listIndex" to be
		/// returned.
		/// 
		/// </param>
		/// <param name="afterCount">The number of entries after "listIndex" to be
		/// returned.
		/// </param>
		
		public virtual void  setRange(System.String jumpTo, int beforeCount, int afterCount)
		{
			/* Save off the fields in local variables
			*/
			m_beforeCount = beforeCount;
			m_afterCount = afterCount;
			m_jumpTo = jumpTo;
			
			/* since we just changed a field we need to rebuild the ber
			* encoded control
			*/
			BuildTypedVLVRequest();
			
			/* Set the request data field in the in the parent LdapControl to
			* the ASN.1 encoded value of this control.  This encoding will be
			* appended to the search request when the control is sent.
			*/
			setValue(m_vlvRequest.getEncoding(new LBEREncoder()));
		}
		static LdapVirtualListControl()
		{
			/*
			* This is where we register the control responses
			*/
			{
				/* Register the VLV Sort Control class which is returned by the server
				* in response to a VLV Sort Request
				*/
				try
				{
					LdapControl.register(responseOID, System.Type.GetType("Novell.Directory.Ldap.Controls.LdapVirtualListResponse"));
				}
				catch (System.Exception e)
				{
				}
			}
		}
	}
}
