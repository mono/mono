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
// Novell.Directory.Ldap.LdapAddRequest.cs
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
	
	/// <summary> Represents an Ldap Add Request.
	/// 
	/// </summary>
	/// <seealso cref="LdapConnection.SendRequest">
	/// </seealso>
   /*
	*       AddRequest ::= [APPLICATION 8] SEQUENCE {
	*               entry           LdapDN,
	*               attributes      AttributeList }
	*/
	public class LdapAddRequest:LdapMessage
	{
		/// <summary> Constructs an LdapEntry that represents the add request
		/// 
		/// </summary>
		/// <returns> an LdapEntry that represents the add request.
		/// </returns>
		virtual public LdapEntry Entry
		{
			get
			{
				RfcAddRequest addreq = (RfcAddRequest) Asn1Object.getRequest();
				
				LdapAttributeSet attrs = new LdapAttributeSet();
				
				// Build the list of attributes
				Asn1Object[] seqArray = addreq.Attributes.toArray();
				for (int i = 0; i < seqArray.Length; i++)
				{
					RfcAttributeTypeAndValues seq = (RfcAttributeTypeAndValues) seqArray[i];
					LdapAttribute attr = new LdapAttribute(((Asn1OctetString) seq.get_Renamed(0)).stringValue());
					
					// Add the values to the attribute
					Asn1SetOf set_Renamed = (Asn1SetOf) seq.get_Renamed(1);
					System.Object[] setArray = set_Renamed.toArray();
					for (int j = 0; j < setArray.Length; j++)
					{
						attr.addValue(((Asn1OctetString) setArray[j]).byteValue());
					}
					attrs.Add(attr);
				}
				
				return new LdapEntry(Asn1Object.RequestDN, attrs);
			}
			
		}
		/// <summary> Constructs a request to add an entry to the directory.
		/// 
		/// </summary>
		/// <param name="entry">The LdapEntry to add to the directory.
		/// 
		/// </param>
		/// <param name="cont">Any controls that apply to the add request,
		/// or null if none.
		/// </param>
		public LdapAddRequest(LdapEntry entry, LdapControl[] cont):base(LdapMessage.ADD_REQUEST, new RfcAddRequest(new RfcLdapDN(entry.DN), makeRfcAttrList(entry)), cont)
		{
			return ;
		}
		
		/// <summary> Build the attribuite list from an LdapEntry.
		/// 
		/// </summary>
		/// <param name="entry">The LdapEntry associated with this add request.
		/// </param>
		private static RfcAttributeList makeRfcAttrList(LdapEntry entry)
		{
			// convert Java-API LdapEntry to RFC2251 AttributeList
			LdapAttributeSet attrSet = entry.getAttributeSet();
			RfcAttributeList attrList = new RfcAttributeList(attrSet.Count);
			System.Collections.IEnumerator itr = attrSet.GetEnumerator();
			while (itr.MoveNext())
			{
				LdapAttribute attr = (LdapAttribute) itr.Current;
				Asn1SetOf vals = new Asn1SetOf(attr.size());
				System.Collections.IEnumerator attrEnum = attr.ByteValues;
				while (attrEnum.MoveNext())
				{
					vals.add(new RfcAttributeValue((sbyte[]) attrEnum.Current));
				}
				attrList.add(new RfcAttributeTypeAndValues(new RfcAttributeDescription(attr.Name), vals));
			}
			return attrList;
		}
		
		/// <summary> Return an Asn1 representation of this add request.
		/// 
		/// #return an Asn1 representation of this object.
		/// </summary>
		public override System.String ToString()
		{
			return Asn1Object.ToString();
		}
	}
}
