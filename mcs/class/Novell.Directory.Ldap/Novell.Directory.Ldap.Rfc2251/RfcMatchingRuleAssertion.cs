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
// Novell.Directory.Ldap.Rfc2251.RfcMatchingRuleAssertion.cs
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
	
	/// <summary> Represents an Ldap Matching Rule Assertion.
	/// 
	/// <pre>
	/// MatchingRuleAssertion ::= SEQUENCE {
	/// matchingRule    [1] MatchingRuleId OPTIONAL,
	/// type            [2] AttributeDescription OPTIONAL,
	/// matchValue      [3] AssertionValue,
	/// dnAttributes    [4] BOOLEAN DEFAULT FALSE }
	/// </pre>
	/// </summary>
	public class RfcMatchingRuleAssertion:Asn1Sequence
	{
		
		//*************************************************************************
		// Constructors for MatchingRuleAssertion
		//*************************************************************************
		
		/// <summary> Creates a MatchingRuleAssertion with the only required parameter.
		/// 
		/// </summary>
		/// <param name="matchValue">The assertion value.
		/// </param>
		public RfcMatchingRuleAssertion(RfcAssertionValue matchValue):this(null, null, matchValue, null)
		{
		}
		
		/// <summary> Creates a MatchingRuleAssertion.
		/// 
		/// The value null may be passed for an optional value that is not used.
		/// 
		/// </summary>
		/// <param name="matchValue">The assertion value.
		/// </param>
		/// <param name="matchingRule">Optional matching rule.
		/// </param>
		/// <param name="type">Optional attribute description.
		/// </param>
		/// <param name="dnAttributes">Asn1Boolean value. (default false)
		/// </param>
		public RfcMatchingRuleAssertion(RfcMatchingRuleId matchingRule, RfcAttributeDescription type, RfcAssertionValue matchValue, Asn1Boolean dnAttributes):base(4)
		{
			if (matchingRule != null)
				add(new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, 1), matchingRule, false));
			
			if (type != null)
				add(new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, 2), type, false));
			
			add(new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, 3), matchValue, false));
			
			// if dnAttributes if false, that is the default value and we must not
			// encode it. (See RFC 2251 5.1 number 4)
			if (dnAttributes != null && dnAttributes.booleanValue())
				add(new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, 4), dnAttributes, false));
			return ;
		}
	}
}
