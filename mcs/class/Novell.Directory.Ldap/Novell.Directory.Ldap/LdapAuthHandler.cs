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
// Novell.Directory.Ldap.LdapAuthHandler.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
namespace Novell.Directory.Ldap
{
	
	/// <summary> 
	/// Used to provide credentials for authentication when processing a
	/// referral.
	/// 
	/// A programmer desiring to supply authentication credentials
	/// to the API when automatically following referrals MUST
	/// implement this interface. If LdapAuthHandler or LdapBindHandler are not
	/// implemented, automatically followed referrals will use anonymous
	/// authentication. Referral URLs of any type other than Ldap (i.e. a
	/// referral URL other than ldap://something) are not chased automatically
	/// by the API on automatic following.
	/// 
	/// 
	/// </summary>
	/// <seealso cref="LdapBindHandler">
	/// </seealso>
	/// <seealso cref="LdapConstraints.ReferralFollowing">
	/// </seealso>
	public interface LdapAuthHandler : LdapReferralHandler
		{
			
			/// <summary> Returns an object which can provide credentials for authenticating to
			/// a server at the specified host and port.
			/// 
			/// </summary>
			/// <param name="host">   Contains a host name or the IP address (in dotted string
			/// format) of a host running an Ldap server.
			/// 
			/// </param>
			/// <param name="port">   Contains the TCP or UDP port number of the host.
			/// 
			/// </param>
			/// <returns> An object with authentication credentials to the specified
			/// host and port.
			/// </returns>
			LdapAuthProvider getAuthProvider(System.String host, int port);
		}
}
