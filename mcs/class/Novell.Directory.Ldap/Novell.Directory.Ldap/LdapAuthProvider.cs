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
// Novell.Directory.Ldap.LdapAuthProvider.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
namespace Novell.Directory.Ldap
{
	
	/// <summary>  An implementation of LdapAuthHandler must be able to provide an
	/// LdapAuthProvider object at the time of a referral.  The class
	/// encapsulates information that is used by the client for authentication
	/// when following referrals automatically.
	/// 
	/// </summary>
	/// <seealso cref="LdapAuthHandler">
	/// </seealso>
	/// <seealso cref="LdapBindHandler">
	/// </seealso>
	public class LdapAuthProvider
	{
		/// <summary> Returns the distinguished name to be used for authentication on
		/// automatic referral following.
		/// 
		/// </summary>
		/// <returns> The distinguished name from the object.
		/// </returns>
		virtual public System.String DN
		{
			get
			{
				return dn;
			}
			
		}
		/// <summary> Returns the password to be used for authentication on automatic
		/// referral following.
		/// 
		/// </summary>
		/// <returns> The byte[] value (UTF-8) of the password from the object.
		/// </returns>
		[CLSCompliantAttribute(false)]
		virtual public sbyte[] Password
		{
			get
			{
				return password;
			}
			
		}
		
		private System.String dn;
		private sbyte[] password;
		
		/// <summary> Constructs information that is used by the client for authentication
		/// when following referrals automatically.
		/// 
		/// </summary>
		/// <param name="dn">          The distinguished name to use when authenticating to
		/// a server.
		/// 
		/// </param>
		/// <param name="password">    The password to use when authenticating to a server.
		/// </param>
		[CLSCompliantAttribute(false)]
		public LdapAuthProvider(System.String dn, sbyte[] password)
		{
			this.dn = dn;
			this.password = password;
			return ;
		}
	}
}
