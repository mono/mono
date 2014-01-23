//
// XmlArrayItemAttributes.cs: 
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

using System.Collections;
using System.Collections.Generic;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for XmlArrayItemAttributes.
	/// </summary>

	public class XmlArrayItemAttributes : CollectionBase {

		public XmlArrayItemAttribute this [int index] {
			get {
				return (XmlArrayItemAttribute)List [index];
			}
			set {
				List [index] = value;
			}	
		}

		public int Add (XmlArrayItemAttribute attribute)
		{
			return (List as IList).Add (attribute);
		}

		public bool Contains(XmlArrayItemAttribute attribute)
		{
			return List.Contains(attribute);
		}

		public void CopyTo(XmlArrayItemAttribute[] array, int index)
		{
			List.CopyTo(array, index);
		}

		public int IndexOf(XmlArrayItemAttribute attribute)
		{
			return List.IndexOf(attribute);
		}

		public void Insert(int index, XmlArrayItemAttribute attribute)
		{
			List.Insert(index, attribute);
		}

		public void Remove(XmlArrayItemAttribute attribute)
		{
			List.Remove(attribute);
		}
		
		internal void AddKeyHash (System.Text.StringBuilder sb)
		{
			if (Count == 0) return;
			
			sb.Append ("XAIAS ");
			for (int n=0; n<Count; n++)
				this[n].AddKeyHash (sb);
			sb.Append ('|');
		}
	}
}
