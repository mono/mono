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
// Novell.Directory.Ldap.Rfc2251.RfcAuthenticationChoice.cs
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
	
	/// <summary> Represents an Ldap Authentication Choice.
	/// 
	/// <pre>
	/// AuthenticationChoice ::= CHOICE {
	/// simple                  [0] OCTET STRING,
	/// -- 1 and 2 reserved
	/// sasl                    [3] SaslCredentials }
	/// </pre>
	/// </summary>
	public class RfcAuthenticationChoice:Asn1Choice
	{
		
		//*************************************************************************
		// Constructors for AuthenticationChoice
		//*************************************************************************
		
		/// <summary> </summary>
		public RfcAuthenticationChoice(Asn1Tagged choice):base(choice)
		{
		}

		[CLSCompliantAttribute(false)]		
		public RfcAuthenticationChoice(System.String mechanism, sbyte[] credentials):base(new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, true, 3), new RfcSaslCredentials(new RfcLdapString(mechanism), credentials != null?new Asn1OctetString(credentials):null), false))
		{ // implicit tagging
		}
		
		//*************************************************************************
		// Mutators
		//*************************************************************************
		
		//*************************************************************************
		// Accessors
		//*************************************************************************
	}
}
