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
// Novell.Directory.Ldap.Rfc2251.RfcSaslCredentials.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Rfc2251
{
	
	/// <summary> Represents Ldap Sasl Credentials.
	/// 
	/// <pre>
	/// SaslCredentials ::= SEQUENCE {
	/// mechanism               LdapString,
	/// credentials             OCTET STRING OPTIONAL }
	/// </pre>
	/// </summary>
	public class RfcSaslCredentials:Asn1Sequence
	{
		
		//*************************************************************************
		// Constructors for SaslCredentials
		//*************************************************************************
		
		/// <summary> </summary>
		public RfcSaslCredentials(RfcLdapString mechanism):this(mechanism, null)
		{
		}
		
		/// <summary> </summary>
		public RfcSaslCredentials(RfcLdapString mechanism, Asn1OctetString credentials):base(2)
		{
			add(mechanism);
			if (credentials != null)
				add(credentials);
		}
	}
}
