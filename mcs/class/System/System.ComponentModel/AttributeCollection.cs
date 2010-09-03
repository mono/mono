//
// System.ComponentModel.AttributeCollection.cs
//
// Authors:
// 	Rodrigo Moya (rodrigo@ximian.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com)
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
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.ComponentModel
{
	[ComVisible (true)]
	public class AttributeCollection : ICollection
	{
		private ArrayList attrList = new ArrayList ();
		public static readonly AttributeCollection Empty = new AttributeCollection ((ArrayList)null);
		
		internal AttributeCollection (ArrayList attributes)
		{
			if (attributes != null)
				attrList = attributes;
		}
		
		public AttributeCollection (params Attribute[] attributes)
		{
			if (attributes != null)
				for (int i = 0; i < attributes.Length; i++)
					attrList.Add (attributes[i]);
		}

#if NET_4_0
		protected AttributeCollection ()
		{
		}
#endif

		public static AttributeCollection FromExisting (AttributeCollection existing, params Attribute [] newAttributes)
		{
			if (existing == null)
				throw new ArgumentNullException ("existing");
			AttributeCollection ret = new AttributeCollection ();
			ret.attrList.AddRange (existing.attrList);
			if (newAttributes != null)
				ret.attrList.AddRange (newAttributes);
			return ret;
		}

		public bool Contains (Attribute attr)
		{
			Attribute at = this [attr.GetType ()];
			if (at != null)
				return attr.Equals (at);
			else
				return false;
		}

		public bool Contains (Attribute [] attributes)
		{
			if (attributes == null)
				return true;

			foreach (Attribute attr in attributes)
				if (!Contains (attr))
					return false;

			return true;
		}

		public void CopyTo (Array array, int index)
		{
			attrList.CopyTo (array, index);
		}

		IEnumerator IEnumerable.GetEnumerator () {
			return GetEnumerator ();
		}

		public IEnumerator GetEnumerator ()
		{
			return attrList.GetEnumerator ();
		}

		public bool Matches (Attribute attr)
		{
			foreach (Attribute a in attrList)
				if (a.Match (attr))
					return true;
			return false;
		}

		public bool Matches (Attribute [] attributes)
		{
			foreach (Attribute a in attributes)
				if (!(Matches (a)))
					return false;
			return true;
		}

		protected Attribute GetDefaultAttribute (Type attributeType)
		{
			Attribute attr = null;
			BindingFlags bf = BindingFlags.Public | BindingFlags.Static;

			FieldInfo def = attributeType.GetField ("Default", bf);
			if (def == null) {
				ConstructorInfo constructorInfo = attributeType.GetConstructor (Type.EmptyTypes);
				if (constructorInfo != null)
					attr = constructorInfo.Invoke (null) as Attribute;
				if (attr != null && !attr.IsDefaultAttribute ())
					attr = null;
			} else {
				attr = (Attribute) def.GetValue (null);
			}

			return attr;
		}

		bool ICollection.IsSynchronized {
			get {
				return attrList.IsSynchronized;
			}
		}

		object ICollection.SyncRoot {
			get {
				return attrList.SyncRoot;
			}
		}
		
		int ICollection.Count {
			get {
				return Count;
			}
		}

		public int Count {
			get {
				return attrList != null ? attrList.Count : 0;
			}
		}

		public virtual Attribute this[Type type] {
			get {
				Attribute attr = null;
				if (attrList != null) {
					foreach (Attribute a in attrList) {
						if (type.IsAssignableFrom (a.GetType ())) {
							attr = a;
							break;
						}
					}
				}
				
				if (attr == null)
					attr = GetDefaultAttribute (type);

				return attr;
			}
		}

		public virtual Attribute this[int index] {
			get {
				return (Attribute) attrList [index];
			}
		}

#if NET_4_0
		Attribute [] attributes_arr;

		// MSDN doesn't mention it, but this property is returning the same instance always.
		protected virtual Attribute [] Attributes {
			get {
				if (attrList == null || attrList.Count == 0)
					return null;

				if (attributes_arr == null)
					attributes_arr = (Attribute[]) attrList.ToArray (typeof (Attribute));

				return attributes_arr;
			}
		}
#endif
	}
}
