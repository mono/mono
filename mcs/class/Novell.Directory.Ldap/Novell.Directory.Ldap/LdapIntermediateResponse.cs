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
// Novell.Directory.Ldap.LdapIntermediateResponse.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.Rfc2251;
using Novell.Directory.Ldap.Utilclass;

namespace Novell.Directory.Ldap
{
  /**
   *
   *  Encapsulates the response returned by an LDAP server on an
   *  asynchronous extended operation request.  It extends LdapResponse.
   *
   *  The response can contain the OID of the extension, an octet string
   *  with the operation's data, both, or neither.
   */
  public class LdapIntermediateResponse : LdapResponse
  {
    private static RespExtensionSet registeredResponses = new RespExtensionSet();

	/**
	 * Registers a class to be instantiated on receipt of a extendedresponse
	 * with the given OID.
	 *
	 * <p>Any previous registration for the OID is overridden. The 
	 *  extendedResponseClass object MUST be an extension of 
	 *  LdapIntermediateResponse. </p>
	 *
	 * @param oid            The object identifier of the control.
	 * <br><br>
	 * @param extendedResponseClass  A class which can instantiate an 
	 *                                LdapIntermediateResponse.
	 */
	public static void register(String oid, Type extendedResponseClass) 
	{
		registeredResponses.registerResponseExtension(oid, extendedResponseClass);
		return;
	}

	/* package */
	public static RespExtensionSet getRegisteredResponses()
	{
		return registeredResponses;
	}


    /**
     * Creates an LdapIntermediateResponse object which encapsulates
     * a server response to an asynchronous extended operation request.
     *
     * @param message  The RfcLdapMessage to convert to an
     *                 LdapIntermediateResponse object.
     */
    public LdapIntermediateResponse(RfcLdapMessage message) : base(message)
    {
    }

    /**
     * Returns the message identifier of the response.
     *
     * @return OID of the response.
     */
    public String getID()
    {
        RfcLdapOID respOID =
            ((RfcIntermediateResponse)message.Response).getResponseName();
        if (respOID == null)
            return null;
        return respOID.stringValue();
    }

    /**
     * Returns the value part of the response in raw bytes.
     *
     * @return The value of the response.
     */
    [CLSCompliantAttribute(false)]
    public sbyte[] getValue()
    {
		Asn1OctetString tempString =
                ((RfcIntermediateResponse)message.Response).getResponse();
		if (tempString == null)
			return null;
		else
			return(tempString.byteValue());
    }
  }
}
