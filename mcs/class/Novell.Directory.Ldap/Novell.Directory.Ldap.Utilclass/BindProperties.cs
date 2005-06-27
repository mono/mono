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
// Novell.Directory.Ldap.Utilclass.BindProperties.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap.Utilclass
{
	
	/// <summary> Encapsulates an Ldap Bind properties</summary>
	public class BindProperties
	{
		/// <summary> gets the protocol version</summary>
		virtual public int ProtocolVersion
		{
			get
			{
				return version;
			}
			
		}
		/// <summary> Gets the authentication dn
		/// 
		/// </summary>
		/// <returns> the authentication dn for this connection
		/// </returns>
		virtual public System.String AuthenticationDN
		{
			get
			{
				return dn;
			}
			
		}
		/// <summary> Gets the authentication method
		/// 
		/// </summary>
		/// <returns> the authentication method for this connection
		/// </returns>
		virtual public System.String AuthenticationMethod
		{
			get
			{
				return method;
			}
			
		}
		/// <summary> Gets the SASL Bind properties
		/// 
		/// </summary>
		/// <returns> the sasl bind properties for this connection
		/// </returns>
		virtual public System.Collections.Hashtable SaslBindProperties
		{
			get
			{
				return bindProperties;
			}
			
		}
		
		/// <summary> Gets the SASL callback handler
		/// 
		/// </summary>
		/// <returns> the sasl callback handler for this connection
		/// </returns>
		virtual public System.Object SaslCallbackHandler
		{
			get
			{
				return bindCallbackHandler;
			}
			
		}
		
		/// <summary> Indicates whether or not the bind properties specify an anonymous bind
		/// 
		/// </summary>
		/// <returns> true if the bind properties specify an anonymous bind
		/// </returns>
		virtual public bool Anonymous
		{
			get
			{
				return anonymous;
			}
			
		}
		
		private int version = 3;
		private System.String dn = null;
		private System.String method = null;
		private bool anonymous;
		private System.Collections.Hashtable bindProperties = null;
		private System.Object bindCallbackHandler = null;
		
		public BindProperties(int version, System.String dn, System.String method, bool anonymous, System.Collections.Hashtable bindProperties, System.Object bindCallbackHandler)
		{
			this.version = version;
			this.dn = dn;
			this.method = method;
			this.anonymous = anonymous;
			this.bindProperties = bindProperties;
			this.bindCallbackHandler = bindCallbackHandler;
		}
	}
}
