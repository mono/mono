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
// Novell.Directory.Ldap.Utilclass.RDN.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap.Utilclass
{
	
	/// <summary> A RDN encapsulates a single object's name of a Distinguished Name(DN).
	/// The object name represented by this class contains no context.  Thus a
	/// Relative Distinguished Name (RDN) could be relative to anywhere in the
	/// Directories tree.
	/// 
	/// For example, of following DN, 'cn=admin, ou=marketing, o=corporation', all
	/// possible RDNs are 'cn=admin', 'ou=marketing', and 'o=corporation'.
	/// 
	/// Multivalued attributes are encapsulated in this class.  For example the
	/// following could be represented by an RDN: 'cn=john + l=US', or
	/// 'cn=juan + l=ES' 
	/// 
	/// </summary>
	/// <seealso cref="DN">
	/// </seealso>
	
	
	public class RDN:System.Object
	{
		/// <summary> Returns the actually Raw String before Normalization
		/// 
		/// </summary>
		/// <returns> The raw string
		/// </returns>
		virtual protected internal System.String RawValue
		{
			get
			{
				return rawValue;
			}
			
		}
		/// <summary> Returns the type of this RDN.  This method assumes that only one value
		/// is used, If multivalues attributes are used only the first Type is
		/// returned.  Use GetTypes.
		/// </summary>
		/// <returns> Type of attribute
		/// </returns>
		virtual public System.String Type
		{
			get
			{
				return (System.String) types[0];
			}
			
		}
		/// <summary> Returns all the types for this RDN.</summary>
		/// <returns> list of types
		/// </returns>
		virtual public System.String[] Types
		{
			get
			{
				System.String[] toReturn = new System.String[types.Count];
				for (int i = 0; i < types.Count; i++)
					toReturn[i] = ((System.String) types[i]);
				return toReturn;
			}
			
		}
		/// <summary> Returns the values of this RDN.  If multivalues attributes are used only
		/// the first Type is returned.  Use GetTypes.
		/// 
		/// </summary>
		/// <returns> Type of attribute
		/// </returns>
		virtual public System.String Value
		{
			get
			{
				return (System.String) values[0];
			}
			
		}
		/// <summary> Returns all the types for this RDN.</summary>
		/// <returns> list of types
		/// </returns>
		virtual public System.String[] Values
		{
			get
			{
				System.String[] toReturn = new System.String[values.Count];
				for (int i = 0; i < values.Count; i++)
					toReturn[i] = ((System.String) values[i]);
				return toReturn;
			}
			
		}
		/// <summary> Determines if this RDN is multivalued or not</summary>
		/// <returns> true if this RDN is multivalued
		/// </returns>
		virtual public bool Multivalued
		{
			get
			{
				return (values.Count > 1)?true:false;
			}
			
		}
		private System.Collections.ArrayList types; //list of Type strings
		private System.Collections.ArrayList values; //list of Value strings
		private System.String rawValue; //the unnormalized value
		
		/// <summary> Creates an RDN object from the DN component specified in the string RDN
		/// 
		/// </summary>
		/// <param name="rdn">the DN component
		/// </param>
		public RDN(System.String rdn)
		{
			rawValue = rdn;
			DN dn = new DN(rdn);
			System.Collections.ArrayList rdns = dn.RDNs;
			//there should only be one rdn
			if (rdns.Count != 1)
				throw new System.ArgumentException("Invalid RDN: see API " + "documentation");
			RDN thisRDN = (RDN) (rdns[0]);
			this.types = thisRDN.types;
			this.values = thisRDN.values;
			this.rawValue = thisRDN.rawValue;
			return ;
		}
		
		public RDN()
		{
			types = new System.Collections.ArrayList();
			values = new System.Collections.ArrayList();
			rawValue = "";
			return ;
		}
		
		/// <summary> Compares the RDN to the rdn passed.  Note: If an there exist any
		/// mulivalues in one RDN they must all be present in the other.
		/// 
		/// </summary>
		/// <param name="rdn">the RDN to compare to
		/// 
		/// @throws IllegalArgumentException if the application compares a name
		/// with an OID.
		/// </param>
		[CLSCompliantAttribute(false)]
		public virtual bool equals(RDN rdn)
		{
			if (this.values.Count != rdn.values.Count)
			{
				return false;
			}
			int j, i;
			for (i = 0; i < this.values.Count; i++)
			{
				//verify that the current value and type exists in the other list
				j = 0;
				//May need a more intellegent compare
				while (j < values.Count && (!((System.String) this.values[i]).ToUpper().Equals(((System.String) rdn.values[j]).ToUpper()) || !equalAttrType((System.String) this.types[i], (System.String) rdn.types[j])))
				{
					j++;
				}
				if (j >= rdn.values.Count)
				//couldn't find first value
					return false;
			}
			return true;
		}
		
		/// <summary> Internal function used by equal to compare Attribute types.  Because
		/// attribute types could either be an OID or a name.  There needs to be a
		/// Translation mechanism.  This function will absract this functionality.
		/// 
		/// Currently if types differ (Oid and number) then UnsupportedOperation is
		/// thrown, either one or the other must used.  In the future an OID to name
		/// translation can be used.
		/// 
		/// 
		/// </summary>
		private bool equalAttrType(System.String attr1, System.String attr2)
		{
			if (System.Char.IsDigit(attr1[0]) ^ System.Char.IsDigit(attr2[0]))
			//isDigit tests if it is an OID
				throw new System.ArgumentException("OID numbers are not " + "currently compared to attribute names");
			
			return attr1.ToUpper().Equals(attr2.ToUpper());
		}
		
		/// <summary> Adds another value to the RDN.  Only one attribute type is allowed for
		/// the RDN.
		/// </summary>
		/// <param name="attrType">Attribute type, could be an OID or String
		/// </param>
		/// <param name="attrValue">Attribute Value, must be normalized and escaped
		/// </param>
		/// <param name="rawValue">or text before normalization, can be Null
		/// </param>
		public virtual void  add(System.String attrType, System.String attrValue, System.String rawValue)
		{
			types.Add(attrType);
			values.Add(attrValue);
			this.rawValue += rawValue;
		}
		
		/// <summary> Creates a string that represents this RDN, according to RFC 2253
		/// 
		/// </summary>
		/// <returns> An RDN string
		/// </returns>
		public override System.String ToString()
		{
			return toString(false);
		}
		
		/// <summary> Creates a string that represents this RDN.
		/// 
		/// If noTypes is true then Atribute types will be ommited.
		/// 
		/// </summary>
		/// <param name="noTypes">true if attribute types will be omitted.
		/// 
		/// </param>
		/// <returns> An RDN string
		/// </returns>
		[CLSCompliantAttribute(false)]
		public virtual System.String toString(bool noTypes)
		{
			int length = types.Count;
			System.String toReturn = "";
			if (length < 1)
				return null;
			if (!noTypes)
			{
				toReturn = types[0] + "=";
			}
			toReturn += values[0];
			
			for (int i = 1; i < length; i++)
			{
				toReturn += "+";
				if (!noTypes)
				{
					toReturn += (types[i] + "=");
				}
				toReturn += values[i];
			}
			return toReturn;
		}
		
		/// <summary> Returns each multivalued name in the current RDN as an array of Strings.
		/// 
		/// </summary>
		/// <param name="noTypes">Specifies whether Attribute types are included. The attribute
		/// type names will be ommitted if the parameter noTypes is true.
		/// 
		/// </param>
		/// <returns> List of multivalued Attributes
		/// </returns>
		public virtual System.String[] explodeRDN(bool noTypes)
		{
			int length = types.Count;
			if (length < 1)
				return null;
			System.String[] toReturn = new System.String[types.Count];
			
			if (!noTypes)
			{
				toReturn[0] = types[0] + "=";
			}
			toReturn[0] += values[0];
			
			for (int i = 1; i < length; i++)
			{
				if (!noTypes)
				{
					toReturn[i] += (types[i] + "=");
				}
				toReturn[i] += values[i];
			}
			
			return toReturn;
		}
	} //end class RDN
}
