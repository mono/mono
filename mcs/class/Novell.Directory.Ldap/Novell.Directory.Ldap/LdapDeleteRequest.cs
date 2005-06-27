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
// Novell.Directory.Ldap.LdapDeleteRequest.cs
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
	
	/// <summary> Represents a request to delete an entry.
	/// 
	/// </summary>
	/// <seealso cref="LdapConnection.SendRequest">
	/// </seealso>
   /*
	*       DelRequest ::= [APPLICATION 10] LdapDN
	*/
	public class LdapDeleteRequest:LdapMessage
	{
		/// <summary> Returns of the dn of the entry to delete from the directory
		/// 
		/// </summary>
		/// <returns> the dn of the entry to delete
		/// </returns>
		virtual public System.String DN
		{
			get
			{
				return Asn1Object.RequestDN;
			}
			
		}
		/// <summary> Constructs a request to delete an entry from the directory
		/// 
		/// </summary>
		/// <param name="dn">the dn of the entry to delete.
		/// 
		/// </param>
		/// <param name="cont">Any controls that apply to the abandon request
		/// or null if none.
		/// </param>
		public LdapDeleteRequest(System.String dn, LdapControl[] cont):base(LdapMessage.DEL_REQUEST, new RfcDelRequest(dn), cont)
		{
			return ;
		}
		
		/// <summary> Return an Asn1 representation of this delete request
		/// 
		/// #return an Asn1 representation of this object
		/// </summary>
		public override System.String ToString()
		{
			return Asn1Object.ToString();
		}
	}
}
