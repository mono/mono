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
// Novell.Directory.Ldap.LdapBindHandler.cs
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
	/// Used to do explicit bind processing on a referral.
	/// 
	/// This interface allows a programmer to override the default
	/// authentication and reauthentication behavior when automatically
	/// following referrals and search references. It is used to control the
	/// authentication mechanism used on automatic referral following.
	/// 
	/// A client can specify an instance of this class to be used
	/// on a single operation (through the LdapConstraints object)
	/// or for all operations (through the LdapContraints object
	/// associated with the connection).
	/// 
	/// 
	/// </summary>
	/// <seealso cref="LdapAuthHandler">
	/// </seealso>
	/// <seealso cref="LdapConstraints.ReferralFollowing">
	/// </seealso>
	public interface LdapBindHandler : LdapReferralHandler
		{
			
			/// <summary> Called by LdapConnection when a referral is received.
			/// 
			/// This method has the responsibility to bind to one of the
			/// hosts in the list specified by the ldaprul parameter which corresponds
			/// exactly to the list of hosts returned in a single referral response.
			/// An implementation may access the host, port, credentials, and other
			/// information in the original LdapConnection object to decide on an
			/// appropriate authentication mechanism, and/or interact with a user or
			/// external module. The object implementing LdapBind creates a new
			/// LdapConnection object to perform its connect and bind calls.  It
			/// returns the new connection when both the connect and bind operations
			/// succeed on one host from the list.  The LdapConnection object referral
			/// following code uses the new LdapConnection object when it resends the
			/// search request, updated with the new search base and possible search
			/// filter. An LdapException is thrown on failure, as in the
			/// LdapConnection.bind method. 
			/// 
			/// </summary>
			/// <param name="ldapurl">The list of servers contained in a referral response.
			/// </param>
			/// <param name="conn">   An established connection to an Ldap server.
			/// 
			/// </param>
			/// <returns>       An established connection to one of the ldap servers
			/// in the referral list.
			/// 
			/// </returns>
			/// <exception>  LdapReferralException An LdapreferralException is thrown
			/// with appropriate fields set to give the reason for the failure.
			/// </exception>
			LdapConnection Bind(System.String[] ldapurl, LdapConnection conn);
		}
}
