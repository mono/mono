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
// Novell.Directory.Ldap.LdapModifyRequest.cs
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
	
	/// <summary> Modification Request.
	/// 
	/// </summary>
	/// <seealso cref="LdapConnection.SendRequest">
	/// </seealso>
   /*
	*       ModifyRequest ::= [APPLICATION 6] SEQUENCE {
	*               object          LdapDN,
	*               modification    SEQUENCE OF SEQUENCE {
	*                       operation       ENUMERATED {
	*                                               add     (0),
	*                                               delete  (1),
	*                                               replace (2) },
	*                       modification    AttributeTypeAndValues } }
	*/
	public class LdapModifyRequest:LdapMessage
	{
		/// <summary> Returns of the dn of the entry to modify in the directory
		/// 
		/// </summary>
		/// <returns> the dn of the entry to modify
		/// </returns>
		virtual public System.String DN
		{
			get
			{
				return Asn1Object.RequestDN;
			}
			
		}
		/// <summary> Constructs the modifications associated with this request
		/// 
		/// </summary>
		/// <returns> an array of LdapModification objects
		/// </returns>
		virtual public LdapModification[] Modifications
		{
			get
			{
				// Get the RFC request object for this request
				RfcModifyRequest req = (RfcModifyRequest) Asn1Object.getRequest();
				// get beginning sequenceOf modifications
				Asn1SequenceOf seqof = req.Modifications;
				Asn1Object[] mods = seqof.toArray();
				LdapModification[] modifications = new LdapModification[mods.Length];
				// Process each modification
				for (int m = 0; m < mods.Length; m++)
				{
					// Each modification consists of a mod type and a sequence
					// containing the attr name and a set of values
					Asn1Sequence opSeq = (Asn1Sequence) mods[m];
					if (opSeq.size() != 2)
					{
						throw new System.SystemException("LdapModifyRequest: modification " + m + " is wrong size: " + opSeq.size());
					}
					// Contains operation and sequence for the attribute
					Asn1Object[] opArray = opSeq.toArray();
					Asn1Enumerated asn1op = (Asn1Enumerated) opArray[0];
					// get the operation
					int op = asn1op.intValue();
					Asn1Sequence attrSeq = (Asn1Sequence) opArray[1];
					Asn1Object[] attrArray = attrSeq.toArray();
					RfcAttributeDescription aname = (RfcAttributeDescription) attrArray[0];
					System.String name = aname.stringValue();
					Asn1SetOf avalue = (Asn1SetOf) attrArray[1];
					Asn1Object[] valueArray = avalue.toArray();
					LdapAttribute attr = new LdapAttribute(name);
					
					for (int v = 0; v < valueArray.Length; v++)
					{
						RfcAttributeValue rfcV = (RfcAttributeValue) valueArray[v];
						attr.addValue(rfcV.byteValue());
					}
					
					modifications[m] = new LdapModification(op, attr);
				}
				return modifications;
			}
			
		}
		/// <summary> Constructs an Ldap Modify request.
		/// 
		/// </summary>
		/// <param name="dn">        The distinguished name of the entry to modify.
		/// 
		/// </param>
		/// <param name="mods">      The changes to be made to the entry.
		/// 
		/// </param>
		/// <param name="cont">       Any controls that apply to the modify request,
		/// or null if none.
		/// </param>
		public LdapModifyRequest(System.String dn, LdapModification[] mods, LdapControl[] cont):base(LdapMessage.MODIFY_REQUEST, new RfcModifyRequest(new RfcLdapDN(dn), encodeModifications(mods)), cont)
		{
			return ;
		}
		
		/// <summary> Encode an array of LdapModifications to ASN.1.
		/// 
		/// </summary>
		/// <param name="mods">an array of LdapModification objects
		/// 
		/// </param>
		/// <returns> an Asn1SequenceOf object containing the modifications.
		/// </returns>
		static private Asn1SequenceOf encodeModifications(LdapModification[] mods)
		{
			// Convert Java-API LdapModification[] to RFC2251 SEQUENCE OF SEQUENCE
			Asn1SequenceOf rfcMods = new Asn1SequenceOf(mods.Length);
			for (int i = 0; i < mods.Length; i++)
			{
				LdapAttribute attr = mods[i].Attribute;
				
				// place modification attribute values in Asn1SetOf
				Asn1SetOf vals = new Asn1SetOf(attr.size());
				if (attr.size() > 0)
				{
					System.Collections.IEnumerator attrEnum = attr.ByteValues;
					while (attrEnum.MoveNext())
					{
						vals.add(new RfcAttributeValue((sbyte[]) attrEnum.Current));
					}
				}
				
				// create SEQUENCE containing mod operation and attr type and vals
				Asn1Sequence rfcMod = new Asn1Sequence(2);
				rfcMod.add(new Asn1Enumerated(mods[i].Op));
				rfcMod.add(new RfcAttributeTypeAndValues(new RfcAttributeDescription(attr.Name), vals));
				
				// place SEQUENCE into SEQUENCE OF
				rfcMods.add(rfcMod);
			}
			return rfcMods;
		}
		
		/// <summary> Return an Asn1 representation of this modify request
		/// 
		/// #return an Asn1 representation of this object
		/// </summary>
		public override System.String ToString()
		{
			return Asn1Object.ToString();
		}
	}
}
