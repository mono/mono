//
// System.ComponentModel.AttributeCollection.cs
//
// Authors:
// 	Rodrigo Moya (rodrigo@ximian.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.ComponentModel
{
	[ComVisible (true)]
	public class AttributeCollection : ICollection, IEnumerable
	{
		private ArrayList attrList = new ArrayList ();
		public static readonly AttributeCollection Empty = new AttributeCollection (null);
		
		public AttributeCollection (Attribute[] attributes)
		{
			if (attributes != null)
				for (int i = 0; i < attributes.Length; i++)
					attrList.Add (attributes[i]);
		}

		public bool Contains (Attribute attr)
		{
			return attrList.Contains (attr);
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
			Attribute attr;
			BindingFlags bf = BindingFlags.Public | BindingFlags.Static;

			FieldInfo def = attributeType.GetField ("Default", bf);
			if (def == null) {
				attr = Activator.CreateInstance (attributeType) as Attribute;
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
		
		public int Count {
			get {
				return attrList.Count;
			}
		}

		public virtual Attribute this[Type type] {
			get {
				Attribute attr = null;
				foreach (Attribute a in attrList) {
					if (a.GetType () == type){
						attr = a;
						break;
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
	}
}
