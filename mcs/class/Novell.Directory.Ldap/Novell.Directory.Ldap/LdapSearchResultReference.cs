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
// Novell.Directory.Ldap.LdapSearchResultReference.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap.Rfc2251;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap
{
	
	/// <summary> 
	/// Encapsulates a continuation reference from an asynchronous search operation.
	/// 
	/// </summary>
	public class LdapSearchResultReference:LdapMessage
	{
		/// <summary> Returns any URLs in the object.
		/// 
		/// </summary>
		/// <returns> The URLs.
		/// </returns>
		virtual public System.String[] Referrals
		{
			get
			{
				Asn1Object[] references = ((RfcSearchResultReference) message.Response).toArray();
				srefs = new System.String[references.Length];
				for (int i = 0; i < references.Length; i++)
				{
					srefs[i] = ((Asn1OctetString) (references[i])).stringValue();
				}
				return (srefs);
			}
			
		}
		
		private System.String[] srefs;
		private static System.Object nameLock; // protect agentNum
		private static int refNum = 0; // Debug, LdapConnection number
		private System.String name; // String name for debug
		/*package*/ /// <summary> Constructs an LdapSearchResultReference object.
		/// 
		/// </summary>
		/// <param name="message">The LdapMessage with a search reference.
		/// </param>
		internal LdapSearchResultReference(RfcLdapMessage message):base(message)
		{
			return ;
		}
		static LdapSearchResultReference()
		{
			nameLock = new System.Object();
		}
	}
}
