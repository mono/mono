//
// SoapAttributeOverrides.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;

namespace System.Xml.Serialization
{
	/// <summary>
	/// 
	/// </summary>
	public class SoapAttributeOverrides
	{
		/// <summary>
		/// This class requires to store SoapAttrributes indexed by a key containg
		/// both Type and Member Name. There are 3 approaches to this IMO.
		/// 1. Make the key as "FullTypeName..MemberName", with ".." seperating Type and Member.
		/// 2. Use a jagged 2D hashtable. The main hashtable is indexed by Type and each value 
		///    contains another hashtable which is indexed by member names. (Too many hashtables)
		/// 3. Use a new class which emcompasses the Type and MemberName. An implementation is there
		///    in TypeMember class in this namespace. (Too many instantiations of the class needed)
		///    
		/// Method 1 is the most elegent, but I am not sure if the seperator is language insensitive.
		/// What if someone writes a language which allows . in the member names.
		/// </summary>
		/// 
		private Hashtable overrides;

		public SoapAttributeOverrides ()
		{
			overrides = new Hashtable();
		}

		public SoapAttributes this [Type type] 
		{
			get { return this [type, string.Empty];	}
		}

		public SoapAttributes this [Type type, string member]
		{
			get 
			{
				return (SoapAttributes) overrides[GetKey(type,member)];
			}
		}

		public void Add (Type type, SoapAttributes attributes) 
		{
			Add(type, string.Empty, attributes);
		}

		public void Add (Type type, string member, SoapAttributes attributes) 
		{
			if(overrides[GetKey(type, member)] != null)
				throw new Exception("The attributes for the given type and Member already exist in the collection");
			
			overrides.Add(GetKey(type,member), attributes);
		}

		private TypeMember GetKey(Type type, string member)
		{
			return new TypeMember(type, member);
		}

		internal void AddKeyHash (System.Text.StringBuilder sb)
		{
			sb.Append ("SAO ");
			foreach (DictionaryEntry entry in overrides)
			{
				SoapAttributes val = (SoapAttributes) overrides [entry.Key];
				sb.Append (entry.Key.ToString()).Append(' ');
				val.AddKeyHash (sb);
			}
			sb.Append ("|");
		}
	}
}
