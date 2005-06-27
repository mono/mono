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
// Novell.Directory.Ldap.Rfc2251.RfcDelRequest.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap;

namespace Novell.Directory.Ldap.Rfc2251
{
	
	/// <summary> Represents an Ldap Delete Request.
	/// 
	/// <pre>
	/// DelRequest ::= [APPLICATION 10] LdapDN
	/// </pre>
	/// </summary>
	public class RfcDelRequest:RfcLdapDN, RfcRequest
	{
		
		//*************************************************************************
		// Constructor for DelRequest
		//*************************************************************************
		
		/// <summary> Constructs an Ldapv3 delete request protocol operation.
		/// 
		/// </summary>
		/// <param name="dn">The Distinguished Name of the entry to delete.
		/// </param>
		public RfcDelRequest(System.String dn):base(dn)
		{
		}
		
		/// <summary> Constructs an Ldapv3 delete request protocol operation.
		/// 
		/// </summary>
		/// <param name="dn">The Distinguished Name of the entry to delete.
		/// </param>
		[CLSCompliantAttribute(false)]
		public RfcDelRequest(sbyte[] dn):base(dn)
		{
		}
		
		/// <summary> Override getIdentifier() to return the appropriate application-wide id
		/// representing this delete request. The getIdentifier() method is called
		/// when this object is encoded.
		/// 
		/// Identifier = CLASS: APPLICATION, FORM: CONSTRUCTED, TAG: 10
		/// </summary>
		public override Asn1Identifier getIdentifier()
		{
			return new Asn1Identifier(Asn1Identifier.APPLICATION, false, LdapMessage.DEL_REQUEST);
		}
		
		public RfcRequest dupRequest(System.String base_Renamed, System.String filter, bool request)
		{
			if ((System.Object) base_Renamed == null)
			{
				return new RfcDelRequest(byteValue());
			}
			else
			{
				return new RfcDelRequest(base_Renamed);
			}
		}
		public System.String getRequestDN()
		{
			return base.stringValue();
		}
	}
}
