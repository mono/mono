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
// Novell.Directory.Ldap.Rfc2251.RfcModifyRequest.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap;

namespace Novell.Directory.Ldap.Rfc2251
{
	
	/// <summary> Represents an Ldap Modify Request.
	/// 
	/// <pre>
	/// ModifyRequest ::= [APPLICATION 6] SEQUENCE {
	/// object          LdapDN,
	/// modification    SEQUENCE OF SEQUENCE {
	/// operation       ENUMERATED {
	/// add     (0),
	/// delete  (1),
	/// replace (2) },
	/// modification    AttributeTypeAndValues } }
	/// </pre>
	/// </summary>
	public class RfcModifyRequest:Asn1Sequence, RfcRequest
	{
		/// <summary> Return the Modifications for this request
		/// 
		/// </summary>
		/// <returns> the modifications for this request.
		/// </returns>
		virtual public Asn1SequenceOf Modifications
		{
			get
			{
				return (Asn1SequenceOf) get_Renamed(1);
			}
			
		}
		
		//*************************************************************************
		// Constructor for ModifyRequest
		//*************************************************************************
		
		/// <summary> </summary>
		public RfcModifyRequest(RfcLdapDN object_Renamed, Asn1SequenceOf modification):base(2)
		{
			add(object_Renamed);
			add(modification);
			return ;
		}
		
		/// <summary> Constructs a new Modify Request copying from the ArrayList of
		/// an existing request.
		/// </summary>
		/* package */
		internal RfcModifyRequest(Asn1Object[] origRequest, System.String base_Renamed):base(origRequest, origRequest.Length)
		{
			// Replace the base if specified, otherwise keep original base
			if ((System.Object) base_Renamed != null)
			{
				set_Renamed(0, new RfcLdapDN(base_Renamed));
			}
			return ;
		}
		//*************************************************************************
		// Accessors
		//*************************************************************************
		
		/// <summary> Override getIdentifier to return an application-wide id.</summary>
		public override Asn1Identifier getIdentifier()
		{
			return new Asn1Identifier(Asn1Identifier.APPLICATION, true, LdapMessage.MODIFY_REQUEST);
		}
		
		public RfcRequest dupRequest(System.String base_Renamed, System.String filter, bool request)
		{
			return new RfcModifyRequest(toArray(), base_Renamed);
		}
		
		/// <summary> Return the String value of the DN associated with this request
		/// 
		/// </summary>
		/// <returns> the DN for this request.
		/// </returns>
		public System.String getRequestDN()
		{
			return ((RfcLdapDN) get_Renamed(0)).stringValue();
		}
	}
}
