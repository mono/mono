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
// Novell.Directory.Ldap.LdapModifyDNRequest.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.Rfc2251;

namespace Novell.Directory.Ldap
{
	
	/// <summary> Represents an Ldap ModifyDN request
	/// 
	/// </summary>
	/// <seealso cref="LdapConnection.SendRequest">
	/// </seealso> 
   /*
	*       ModifyDNRequest ::= [APPLICATION 12] SEQUENCE {
	*               entry           LdapDN,
	*               newrdn          RelativeLdapDN,
	*               deleteoldrdn    BOOLEAN,
	*               newSuperior     [0] LdapDN OPTIONAL }
	*/
	public class LdapModifyDNRequest:LdapMessage
	{
		/// <summary> Returns the dn of the entry to rename or move in the directory
		/// 
		/// </summary>
		/// <returns> the dn of the entry to rename or move
		/// </returns>
		virtual public System.String DN
		{
			get
			{
				return Asn1Object.RequestDN;
			}
			
		}
		/// <summary> Returns the newRDN of the entry to rename or move in the directory
		/// 
		/// </summary>
		/// <returns> the newRDN of the entry to rename or move
		/// </returns>
		virtual public System.String NewRDN
		{
			get
			{
				// Get the RFC request object for this request
				RfcModifyDNRequest req = (RfcModifyDNRequest) Asn1Object.getRequest();
				RfcRelativeLdapDN relDN = (RfcRelativeLdapDN) req.toArray()[1];
				return relDN.stringValue();
			}
			
		}
		/// <summary> Returns the DeleteOldRDN flag that applies to the entry to rename or
		/// move in the directory
		/// 
		/// </summary>
		/// <returns> the DeleteOldRDN flag for the entry to rename or move
		/// </returns>
		virtual public bool DeleteOldRDN
		{
			get
			{
				// Get the RFC request object for this request
				RfcModifyDNRequest req = (RfcModifyDNRequest) Asn1Object.getRequest();
				Asn1Boolean delOld = (Asn1Boolean) req.toArray()[2];
				return delOld.booleanValue();
			}
			
		}
		/// <summary> Returns the ParentDN for the entry move in the directory
		/// 
		/// </summary>
		/// <returns> the ParentDN for the entry to move, or <dd>null</dd>
		/// if the request is not a move.
		/// </returns>
		virtual public System.String ParentDN
		{
			get
			{
				// Get the RFC request object for this request
				RfcModifyDNRequest req = (RfcModifyDNRequest) Asn1Object.getRequest();
				Asn1Object[] seq = req.toArray();
				if ((seq.Length < 4) || (seq[3] == null))
				{
					return null;
				}
				RfcLdapDN parentDN = (RfcLdapDN) req.toArray()[3];
				return parentDN.stringValue();
			}
			
		}
		/// <summary> Constructs a ModifyDN (rename) Request.
		/// 
		/// </summary>
		/// <param name="dn">            The current distinguished name of the entry.
		/// 
		/// </param>
		/// <param name="newRdn">        The new relative distinguished name for the entry.
		/// 
		/// </param>
		/// <param name="newParentdn">   The distinguished name of an existing entry which
		/// is to be the new parent of the entry.
		/// 
		/// </param>
		/// <param name="deleteOldRdn">  If true, the old name is not retained as an
		/// attribute value. If false, the old name is
		/// retained as an attribute value.
		/// 
		/// </param>
		/// <param name="cont">           Any controls that apply to the modifyDN request,
		/// or null if none.
		/// </param>
		public LdapModifyDNRequest(System.String dn, System.String newRdn, System.String newParentdn, bool deleteOldRdn, LdapControl[] cont):base(LdapMessage.MODIFY_RDN_REQUEST, new RfcModifyDNRequest(new RfcLdapDN(dn), new RfcRelativeLdapDN(newRdn), new Asn1Boolean(deleteOldRdn), ((System.Object) newParentdn != null)?new RfcLdapSuperDN(newParentdn):null), cont)
		{
			return ;
		}
		
		/// <summary> Return an Asn1 representation of this mod DN request
		/// 
		/// #return an Asn1 representation of this object
		/// </summary>
		public override System.String ToString()
		{
			return Asn1Object.ToString();
		}
	}
}
