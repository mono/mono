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
// Novell.Directory.Ldap.LdapAbandonRequest.cs
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
	
	/// <summary> Represents an Ldap Abandon Request
	/// 
	/// </summary>
	/// <seealso cref="LdapConnection.SendRequest">
	/// </seealso> 
   /*
	*       AbandonRequest ::= [APPLICATION 16] MessageID
	*/
	public class LdapAbandonRequest:LdapMessage
	{
		/// <summary> Construct an Ldap Abandon Request.
		/// 
		/// </summary>
		/// <param name="id">The ID of the operation to abandon.
		/// 
		/// </param>
		/// <param name="cont">Any controls that apply to the abandon request
		/// or null if none.
		/// </param>
		public LdapAbandonRequest(int id, LdapControl[] cont):base(LdapMessage.ABANDON_REQUEST, new RfcAbandonRequest(id), cont)
		{
			return ;
		}
	}
}
