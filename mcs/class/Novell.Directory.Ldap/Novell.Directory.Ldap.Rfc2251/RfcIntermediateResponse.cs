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
// Novell.Directory.Ldap.Rfc2251.RfcIntermediateResponse.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using LdapException = Novell.Directory.Ldap.LdapException;
using LdapMessage = Novell.Directory.Ldap.LdapMessage;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Rfc2251
{
  /// <summary> 
  /// Represents an LDAP Intermediate Response.
  ///
  ///
  ///  IntermediateResponse ::= [APPLICATION 25] SEQUENCE {
  ///         COMPONENTS OF LDAPResult, note: only present on incorrectly 
  ///  encoded response from 
  ///                                               pre Falcon-sp1 server 
  ///               responseName     [10] LDAPOID OPTIONAL,
  ///               responseValue    [11] OCTET STRING OPTIONAL }
  /// </summary>
  ///
  public class RfcIntermediateResponse : Asn1Sequence,  RfcResponse 
  {
    /**
     * Context-specific TAG for optional responseName.
     */
    public const int TAG_RESPONSE_NAME = 0;
    /**
     * Context-specific TAG for optional response.
     */
    public const int TAG_RESPONSE      = 1;

    private int m_referralIndex;
    private int m_responseNameIndex;
    private int m_responseValueIndex;
	

    //*************************************************************************
    // Constructors for ExtendedResponse
    //*************************************************************************

    /**
     * The only time a client will create a IntermediateResponse is when it is
     * decoding it from an InputStream. The stream contains the intermediate
     * response sequence that follows the msgID in the PDU. The intermediate
     * response draft defines this as:
     *      IntermediateResponse ::= [APPLICATION 25] SEQUENCE {
     *             responseName     [0] LDAPOID OPTIONAL,
     *             responseValue    [1] OCTET STRING OPTIONAL }
     *
     * Until post Falcon sp1, the LDAP server was incorrectly encoding
     * intermediate response as:
     *      IntermediateResponse ::= [APPLICATION 25] SEQUENCE {
     *             Components of LDAPResult,
     *             responseName     [0] LDAPOID OPTIONAL,
     *             responseValue    [1] OCTET STRING OPTIONAL }
     *
     * where the Components of LDAPResult are
     *               resultCode      ENUMERATED {...}
     *               matchedDN       LDAPDN,
     *               errorMessage    LDAPString,
     *               referral        [3] Referral OPTIONAL }
     *
     *
     * (The components of LDAPResult never have the optional referral.)
     * This constructor is written to handle both cases.
     *
     * The sequence of this intermediate response will have the element
     * at index m_responseNameIndex set to an RfcLDAPOID containing the
     * oid of the response. The element at m_responseValueIndex will be set
     * to an ASN1OctetString containing the value bytes.
     */
    [CLSCompliantAttribute(false)]
    public RfcIntermediateResponse(Asn1Decoder dec, System.IO.Stream in_Renamed, int len) : base(dec, in_Renamed, len)
      //		throws IOException
    {
      //		super(dec, in, len);
        
      int i=0;		
      m_responseNameIndex = m_responseValueIndex = 0;

      // decode optional tagged elements. The parent class constructor will
      // have decoded these elements as ASN1Tagged objects with the value
      // stored as an ASN1OctectString object.

      if(size() >= 3) //the incorrectly encoded case, LDAPResult contains 
	i = 3;        //at least 3 components
      else
	i = 0; //correctly encoded case, can have zero components

      for(; i<size(); i++) 
      {		
	Asn1Tagged obj = (Asn1Tagged)get_Renamed(i);
	Asn1Identifier id = obj.getIdentifier();
	switch(id.Tag) 
	{
	case TAG_RESPONSE_NAME:
	  set_Renamed(i, new RfcLdapOID(
					((Asn1OctetString)obj.taggedValue()).byteValue()));
	  m_responseNameIndex = i;
	  break;

	case TAG_RESPONSE:
	  set_Renamed(i, obj.taggedValue());
	  m_responseValueIndex = i;
	  break;
	}
      }
    }

    public Asn1Enumerated getResultCode()
    {
        if ( size() > 3)    
            return (Asn1Enumerated)get_Renamed(0);
        else
            return null;    
    }

    public RfcLdapDN getMatchedDN()
    {
        if ( size() > 3)
            return new RfcLdapDN(((Asn1OctetString)get_Renamed(1)).byteValue());
        else
            return null;    
    }

    public RfcLdapString getErrorMessage()
    {
        if ( size() > 3)
            return new RfcLdapString(((Asn1OctetString)get_Renamed(2)).byteValue());
        else
            return null;
    }

    public RfcReferral getReferral()
    {
      return (size() > 3) ? (RfcReferral)get_Renamed(3) : null;
    }

    public RfcLdapOID getResponseName()
    {
        return (m_responseNameIndex != 0) ? (RfcLdapOID)get_Renamed(m_responseNameIndex)
                                        : null;
    }

    public Asn1OctetString getResponse()
    {
        return (m_responseValueIndex != 0) ? (Asn1OctetString)get_Renamed(m_responseValueIndex)
                                    : null;
    }

    /**
     * Override getIdentifier to return an application-wide id.
     */
    public override Asn1Identifier getIdentifier()
    {
        return new Asn1Identifier(Asn1Identifier.APPLICATION, true,
                                   LdapMessage.INTERMEDIATE_RESPONSE);
    }
  }
}
