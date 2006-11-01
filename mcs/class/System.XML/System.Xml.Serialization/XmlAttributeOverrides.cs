//
// XmlAttributeOverrides.cs: 
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
using System.Globalization;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for XmlAttributeOverrides.
	/// </summary>
	public class XmlAttributeOverrides
	{
		
		private Hashtable overrides;

		public XmlAttributeOverrides ()
		{
			overrides = new Hashtable();
		}

		public XmlAttributes this [Type type] 
		{
			get { return this [type, string.Empty];	}
		}

		public XmlAttributes this [Type type, string member]
		{
			get 
			{
				return (XmlAttributes) overrides[GetKey(type,member)];
			}
		}

		public void Add (Type type, XmlAttributes attributes) 
		{
			Add(type, string.Empty, attributes);
		}

		public void Add (Type type, string member, XmlAttributes attributes) 
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
			sb.Append ("XAO ");
			foreach (DictionaryEntry entry in overrides)
			{
				XmlAttributes val = (XmlAttributes) entry.Value;
				IFormattable keyFormattable = entry.Key as IFormattable;
				sb.Append (keyFormattable != null ? keyFormattable.ToString (null, CultureInfo.InvariantCulture) : entry.Key.ToString()).Append(' ');
				val.AddKeyHash (sb);
			}
			sb.Append ("|");
		}
	}
}
