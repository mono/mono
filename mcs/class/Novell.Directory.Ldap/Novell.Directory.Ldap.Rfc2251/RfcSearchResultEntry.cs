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
// Novell.Directory.Ldap.Rfc2251.RfcSearchResultEntry.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Rfc2251
{
	
	/// <summary> Represents an Ldap Search Result Entry.
	/// 
	/// <pre>
	/// SearchResultEntry ::= [APPLICATION 4] SEQUENCE {
	/// objectName      LdapDN,
	/// attributes      PartialAttributeList }
	/// </pre>
	/// </summary>
	public class RfcSearchResultEntry:Asn1Sequence
	{
		/// <summary> </summary>
		virtual public Asn1OctetString ObjectName
		{
			get
			{
				return (Asn1OctetString) get_Renamed(0);
			}
			
		}
		/// <summary> </summary>
		virtual public Asn1Sequence Attributes
		{
			get
			{
				return (Asn1Sequence) get_Renamed(1);
			}
			
		}
		
		//*************************************************************************
		// Constructors for SearchResultEntry
		//*************************************************************************
		
		/// <summary> The only time a client will create a SearchResultEntry is when it is
		/// decoding it from an InputStream
		/// </summary>
		[CLSCompliantAttribute(false)]
		public RfcSearchResultEntry(Asn1Decoder dec, System.IO.Stream in_Renamed, int len):base(dec, in_Renamed, len)
		{
			
			// Decode objectName
			//      set(0, new RfcLdapDN(((Asn1OctetString)get(0)).stringValue()));
			
			// Create PartitalAttributeList. This does not need to be decoded, only
			// typecast.
			//      set(1, new PartitalAttributeList());
			return ;
		}
		
		//*************************************************************************
		// Accessors
		//*************************************************************************
		
		/// <summary> Override getIdentifier to return an application-wide id.</summary>
		public override Asn1Identifier getIdentifier()
		{
			return new Asn1Identifier(Asn1Identifier.APPLICATION, true, LdapMessage.SEARCH_RESPONSE);
		}
	}
}
