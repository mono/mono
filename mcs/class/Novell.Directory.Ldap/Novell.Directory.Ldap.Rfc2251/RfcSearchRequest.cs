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
// Novell.Directory.Ldap.Rfc2251.RfcSearchRequest.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using LdapException = Novell.Directory.Ldap.LdapException;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Rfc2251
{
	
	/// <summary> Represents an Ldap Search Request.
	/// 
	/// <pre>
	/// SearchRequest ::= [APPLICATION 3] SEQUENCE {
	/// baseObject      LdapDN,
	/// scope           ENUMERATED {
	/// baseObject              (0),
	/// singleLevel             (1),
	/// wholeSubtree            (2) },
	/// derefAliases    ENUMERATED {
	/// neverDerefAliases       (0),
	/// derefInSearching        (1),
	/// derefFindingBaseObj     (2),
	/// derefAlways             (3) },
	/// sizeLimit       INTEGER (0 .. maxInt),
	/// timeLimit       INTEGER (0 .. maxInt),
	/// typesOnly       BOOLEAN,
	/// filter          Filter,
	/// attributes      AttributeDescriptionList }
	/// </pre>
	/// </summary>
	public class RfcSearchRequest:Asn1Sequence, RfcRequest
	{
		
		//*************************************************************************
		// Constructors for SearchRequest
		//*************************************************************************
		
		/*
		*
		*/
		public RfcSearchRequest(RfcLdapDN baseObject, Asn1Enumerated scope, Asn1Enumerated derefAliases, Asn1Integer sizeLimit, Asn1Integer timeLimit, Asn1Boolean typesOnly, RfcFilter filter, RfcAttributeDescriptionList attributes):base(8)
		{
			add(baseObject);
			add(scope);
			add(derefAliases);
			add(sizeLimit);
			add(timeLimit);
			add(typesOnly);
			add(filter);
			add(attributes);
			return ;
		}
		
		/// <summary> Constructs a new Search Request copying from an existing request.</summary>
		/* package */
		internal RfcSearchRequest(Asn1Object[] origRequest, System.String base_Renamed, System.String filter, bool request):base(origRequest, origRequest.Length)
		{
			
			// Replace the base if specified, otherwise keep original base
			if ((System.Object) base_Renamed != null)
			{
				set_Renamed(0, new RfcLdapDN(base_Renamed));
			}
			
			// If this is a reencode of a search continuation reference
			// and if original scope was one-level, we need to change the scope to
			// base so we don't return objects a level deeper than requested
			if (request)
			{
				int scope = ((Asn1Enumerated) origRequest[1]).intValue();
				if (scope == LdapConnection.SCOPE_ONE)
				{
					set_Renamed(1, new Asn1Enumerated(LdapConnection.SCOPE_BASE));
				}
			}
			// Replace the filter if specified, otherwise keep original filter
			if ((System.Object) filter != null)
			{
				set_Renamed(6, new RfcFilter(filter));
			}
			return ;
		}
		
		//*************************************************************************
		// Accessors
		//*************************************************************************
		
		/// <summary> Override getIdentifier to return an application-wide id.
		/// 
		/// <pre>
		/// ID = CLASS: APPLICATION, FORM: CONSTRUCTED, TAG: 3. (0x63)
		/// </pre>
		/// </summary>
		public override Asn1Identifier getIdentifier()
		{
			return new Asn1Identifier(Asn1Identifier.APPLICATION, true, LdapMessage.SEARCH_REQUEST);
		}
		
		public RfcRequest dupRequest(System.String base_Renamed, System.String filter, bool request)
		{
			return new RfcSearchRequest(toArray(), base_Renamed, filter, request);
		}
		
		public System.String getRequestDN()
		{
			return ((RfcLdapDN) get_Renamed(0)).stringValue();
		}
	}
}
