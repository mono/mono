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
// Novell.Directory.Ldap.LdapCompareRequest.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap.Rfc2251;

namespace Novell.Directory.Ldap
{
	
	/// <summary> Represents an Ldap Compare Request.
	/// 
	/// </summary>
	/// <seealso cref="LdapConnection.SendRequest">
	/// </seealso>
   /*
	*       CompareRequest ::= [APPLICATION 14] SEQUENCE {
	*               entry           LdapDN,
	*               ava             AttributeValueAssertion }
	*/
	public class LdapCompareRequest:LdapMessage
	{
		/// <summary> Returns the LdapAttribute associated with this request.
		/// 
		/// </summary>
		/// <returns> the LdapAttribute
		/// </returns>
		virtual public System.String AttributeDescription
		{
			get
			{
				RfcCompareRequest req = (RfcCompareRequest) Asn1Object.getRequest();
				return req.AttributeValueAssertion.AttributeDescription;
			}
			
		}
		/// <summary> Returns the LdapAttribute associated with this request.
		/// 
		/// </summary>
		/// <returns> the LdapAttribute
		/// </returns>
		[CLSCompliantAttribute(false)]
		virtual public sbyte[] AssertionValue
		{
			get
			{
				RfcCompareRequest req = (RfcCompareRequest) Asn1Object.getRequest();
				return req.AttributeValueAssertion.AssertionValue;
			}
			
		}
		/// <summary> Returns of the dn of the entry to compare in the directory
		/// 
		/// </summary>
		/// <returns> the dn of the entry to compare
		/// </returns>
		virtual public System.String DN
		{
			get
			{
				return Asn1Object.RequestDN;
			}
			
		}
		/// <summary> Constructs an LdapCompareRequest Object.
		/// 
		/// </summary>
		/// <param name="dn">     The distinguished name of the entry containing an
		/// attribute to compare.
		/// 
		/// </param>
		/// <param name="name">   The name of the attribute to compare.
		/// 
		/// </param>
		/// <param name="value">   The value of the attribute to compare.
		/// 
		/// 
		/// </param>
		/// <param name="cont">Any controls that apply to the compare request,
		/// or null if none.
		/// </param>
		[CLSCompliantAttribute(false)]
		public LdapCompareRequest(System.String dn, System.String name, sbyte[] value_Renamed, LdapControl[] cont):base(LdapMessage.COMPARE_REQUEST, new RfcCompareRequest(new RfcLdapDN(dn), new RfcAttributeValueAssertion(new RfcAttributeDescription(name), new RfcAssertionValue(value_Renamed))), cont)
		{
			return ;
		}
	}
}
