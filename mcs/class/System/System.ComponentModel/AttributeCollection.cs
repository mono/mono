//
// System.ComponentModel.AttributeCollection.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc.
//

using System;
using System.Collections;

namespace System.ComponentModel
{
	public class AttributeCollection : ICollection, IEnumerable
	{
		private ArrayList attrList = new ArrayList ();
		public static readonly AttributeCollection Empty = new AttributeCollection (null);
		
		public AttributeCollection (Attribute[] attributes) {
			for (int i = 0; i < attributes.Length; i++)
				attrList.Add (attributes[i]);
		}

		public bool Contains (Attribute attr) {
			for (int i = 0; i < attrList.Count; i++) {
				if (attrList[i] == attr)
					return true;
			}

			return false;
		}

		[MonoTODO]
		public bool Contains (Attribute[] attributes) {
			throw new NotImplementedException ();
		}

		public void CopyTo (Array array, int index) {
			attrList.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator () {
			return attrList.GetEnumerator ();
		}

		[MonoTODO]
		public bool Matches (Attribute attr) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Matches (Attribute[] attributes) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected Attribute GetDefaultAttribute (Type attributeType) {
			throw new NotImplementedException ();
		}

		public bool IsSynchronized {
			get {
				return attrList.IsSynchronized;
			}
		}

		public object SyncRoot {
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
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual Attribute this[int index] {
			get {
				return (Attribute) attrList[index];
			}
		}
	}
}
