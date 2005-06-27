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
// Novell.Directory.Ldap.Rfc2251.RfcLdapMessage.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using LdapException = Novell.Directory.Ldap.LdapException;
using LdapMessage = Novell.Directory.Ldap.LdapMessage;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Rfc2251
{
	
	/// <summary> Represents an Ldap Message.
	/// 
	/// <pre>
	/// LdapMessage ::= SEQUENCE {
	/// messageID       MessageID,
	/// protocolOp      CHOICE {
	/// bindRequest     BindRequest,
	/// bindResponse    BindResponse,
	/// unbindRequest   UnbindRequest,
	/// searchRequest   SearchRequest,
	/// searchResEntry  SearchResultEntry,
	/// searchResDone   SearchResultDone,
	/// searchResRef    SearchResultReference,
	/// modifyRequest   ModifyRequest,
	/// modifyResponse  ModifyResponse,
	/// addRequest      AddRequest,
	/// addResponse     AddResponse,
	/// delRequest      DelRequest,
	/// delResponse     DelResponse,
	/// modDNRequest    ModifyDNRequest,
	/// modDNResponse   ModifyDNResponse,
	/// compareRequest  CompareRequest,
	/// compareResponse CompareResponse,
	/// abandonRequest  AbandonRequest,
	/// extendedReq     ExtendedRequest,
	/// extendedResp    ExtendedResponse },
	/// controls       [0] Controls OPTIONAL }
	/// </pre>
	/// 
	/// 
	/// Note: The creation of a MessageID should be hidden within the creation of
	/// an RfcLdapMessage. The MessageID needs to be in sequence, and has an
	/// upper and lower limit. There is never a case when a user should be
	/// able to specify the MessageID for an RfcLdapMessage. The MessageID()
	/// constructor should be package protected. (So the MessageID value
	/// isn't arbitrarily run up.)
	/// </summary>
	public class RfcLdapMessage:Asn1Sequence
	{
		/// <summary> Returns this RfcLdapMessage's messageID as an int.</summary>
		virtual public int MessageID
		{
			get
			{
				return ((Asn1Integer) get_Renamed(0)).intValue();
			}
			
		}
		/// <summary> Returns this RfcLdapMessage's message type</summary>
		virtual public int Type
		{
			get
			{
				return get_Renamed(1).getIdentifier().Tag;
			}
			
		}
		/// <summary> Returns the response associated with this RfcLdapMessage.
		/// Can be one of RfcLdapResult, RfcBindResponse, RfcExtendedResponse
		/// all which extend RfcResponse. It can also be
		/// RfcSearchResultEntry, or RfcSearchResultReference
		/// </summary>
		virtual public Asn1Object Response
		{
			get
			{
				return get_Renamed(1);
			}
			
		}
		/// <summary> Returns the optional Controls for this RfcLdapMessage.</summary>
		virtual public RfcControls Controls
		{
			get
			{
				if (size() > 2)
					return (RfcControls) get_Renamed(2);
				return null;
			}
			
		}
		/// <summary> Returns the dn of the request, may be null</summary>
		virtual public System.String RequestDN
		{
			get
			{
				return ((RfcRequest) op).getRequestDN();
			}
			
		}
		/// <summary> returns the original request in this message
		/// 
		/// </summary>
		/// <returns> the original msg request for this response
		/// </returns>
		/// <summary> sets the original request in this message
		/// 
		/// </summary>
		/// <param name="msg">the original request for this response
		/// </param>
		virtual public LdapMessage RequestingMessage
		{
			get
			{
				return requestMessage;
			}
			
			set
			{
				requestMessage = value;
				return ;
			}
			
		}
		
		private Asn1Object op;
		private RfcControls controls;
		private LdapMessage requestMessage = null;
		
		/// <summary> Create an RfcLdapMessage by copying the content array
		/// 
		/// </summary>
		/// <param name="origContent">the array list to copy
		/// </param>
		/* package */
		internal RfcLdapMessage(Asn1Object[] origContent, RfcRequest origRequest, System.String dn, System.String filter, bool reference):base(origContent, origContent.Length)
		{
			
			set_Renamed(0, new RfcMessageID()); // MessageID has static counter
			
			RfcRequest req = (RfcRequest) origContent[1];
			RfcRequest newreq = req.dupRequest(dn, filter, reference);
			op = (Asn1Object) newreq;
			set_Renamed(1, (Asn1Object) newreq);
			
			return ;
		}
		
		/// <summary> Create an RfcLdapMessage using the specified Ldap Request.</summary>
		public RfcLdapMessage(RfcRequest op):this(op, null)
		{
			return ;
		}
		
		/// <summary> Create an RfcLdapMessage request from input parameters.</summary>
		public RfcLdapMessage(RfcRequest op, RfcControls controls):base(3)
		{
			
			this.op = (Asn1Object) op;
			this.controls = controls;
			
			add(new RfcMessageID()); // MessageID has static counter
			add((Asn1Object) op);
			if (controls != null)
			{
				add(controls);
			}
			return ;
		}
		
		/// <summary> Create an RfcLdapMessage using the specified Ldap Response.</summary>
		public RfcLdapMessage(Asn1Sequence op):this(op, null)
		{
			return ;
		}
		
		/// <summary> Create an RfcLdapMessage response from input parameters.</summary>
		public RfcLdapMessage(Asn1Sequence op, RfcControls controls):base(3)
		{
			
			this.op = op;
			this.controls = controls;
			
			add(new RfcMessageID()); // MessageID has static counter
			add(op);
			if (controls != null)
			{
				add(controls);
			}
			return ;
		}
		
		/// <summary> Will decode an RfcLdapMessage directly from an InputStream.</summary>
		[CLSCompliantAttribute(false)]
		public RfcLdapMessage(Asn1Decoder dec, System.IO.Stream in_Renamed, int len):base(dec, in_Renamed, len)
		{
			
			sbyte[] content;
			System.IO.MemoryStream bais;
			
			// Decode implicitly tagged protocol operation from an Asn1Tagged type
			// to its appropriate application type.
			Asn1Tagged protocolOp = (Asn1Tagged) get_Renamed(1);
			Asn1Identifier protocolOpId = protocolOp.getIdentifier();
			content = ((Asn1OctetString) protocolOp.taggedValue()).byteValue();
			bais = new System.IO.MemoryStream(SupportClass.ToByteArray(content));
			
			switch (protocolOpId.Tag)
			{
				
				case LdapMessage.SEARCH_RESPONSE: 
					set_Renamed(1, new RfcSearchResultEntry(dec, bais, content.Length));
					break;
				
				case LdapMessage.SEARCH_RESULT: 
					set_Renamed(1, new RfcSearchResultDone(dec, bais, content.Length));
					break;
				
				case LdapMessage.SEARCH_RESULT_REFERENCE: 
					set_Renamed(1, new RfcSearchResultReference(dec, bais, content.Length));
					break;
				
				case LdapMessage.ADD_RESPONSE: 
					set_Renamed(1, new RfcAddResponse(dec, bais, content.Length));
					break;
				
				case LdapMessage.BIND_RESPONSE: 
					set_Renamed(1, new RfcBindResponse(dec, bais, content.Length));
					break;
				
				case LdapMessage.COMPARE_RESPONSE: 
					set_Renamed(1, new RfcCompareResponse(dec, bais, content.Length));
					break;
				
				case LdapMessage.DEL_RESPONSE: 
					set_Renamed(1, new RfcDelResponse(dec, bais, content.Length));
					break;
				
				case LdapMessage.EXTENDED_RESPONSE: 
					set_Renamed(1, new RfcExtendedResponse(dec, bais, content.Length));
					break;
				
				case LdapMessage.INTERMEDIATE_RESPONSE:
					set_Renamed(1, new RfcIntermediateResponse(dec, bais, content.Length));
					break;

				case LdapMessage.MODIFY_RESPONSE: 
					set_Renamed(1, new RfcModifyResponse(dec, bais, content.Length));
					break;
				
				case LdapMessage.MODIFY_RDN_RESPONSE: 
					set_Renamed(1, new RfcModifyDNResponse(dec, bais, content.Length));
					break;
				
				default: 
					throw new System.SystemException("RfcLdapMessage: Invalid tag: " + protocolOpId.Tag);
				
			}
			
			// decode optional implicitly tagged controls from Asn1Tagged type to
			// to RFC 2251 types.
			if (size() > 2)
			{
				Asn1Tagged controls = (Asn1Tagged) get_Renamed(2);
				//   Asn1Identifier controlsId = protocolOp.getIdentifier();
				// we could check to make sure we have controls here....
				
				content = ((Asn1OctetString) controls.taggedValue()).byteValue();
				bais = new System.IO.MemoryStream(SupportClass.ToByteArray(content));
				set_Renamed(2, new RfcControls(dec, bais, content.Length));
			}
			return ;
		}
		
		//*************************************************************************
		// Accessors
		//*************************************************************************
		
		/// <summary> Returns the request associated with this RfcLdapMessage.
		/// Throws a class cast exception if the RfcLdapMessage is not a request.
		/// </summary>
		public RfcRequest getRequest()
		{
			return (RfcRequest) get_Renamed(1);
		}
		
		public virtual bool isRequest()
		{
			return get_Renamed(1) is RfcRequest;
		}
		
		/// <summary> Duplicate this message, replacing base dn, filter, and scope if supplied
		/// 
		/// </summary>
		/// <param name="dn">the base dn
		/// 
		/// </param>
		/// <param name="filter">the filter
		/// 
		/// </param>
		/// <param name="reference">true if a search reference
		/// 
		/// </param>
		/// <returns> the object representing the new message
		/// </returns>
		public System.Object dupMessage(System.String dn, System.String filter, bool reference)
		{
			if ((op == null))
			{
				throw new LdapException("DUP_ERROR", LdapException.LOCAL_ERROR, (System.String) null);
			}
			
			RfcLdapMessage newMsg = new RfcLdapMessage(toArray(), (RfcRequest) get_Renamed(1), dn, filter, reference);
			return newMsg;
		}
	}
}
