//
// Microsoft.Web.Services.Addressing.AddressList.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Collections;

namespace Microsoft.Web.Services.Addressing
{

	public class AddressList : ICollection, IEnumerable
	{

		private ArrayList _list;

		public AddressList ()
		{
			_list = new ArrayList ();
		}

		public int Add (Address address)
		{
			if(address == null) {
				throw new ArgumentNullException ("address");
			}
			return _list.Add (address);
		}

		public bool Contains (Address address)
		{
			if(address == null) {
				throw new ArgumentNullException ("address");
			}
			return _list.Contains (address);
		}

		public void CopyTo (Array array, int index)
		{
			if(array == null) {
				throw new ArgumentNullException ("array");
			}
			if(index < 0 || index > _list.Count) {
				throw new ArgumentOutOfRangeException ("index");
			}
			_list.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator ()
		{
			return _list.GetEnumerator ();
		}

		public int IndexOf (Address address)
		{
			if(address == null) {
				throw new ArgumentNullException ("address");
			}
			return _list.IndexOf (address);
		}

		public void Insert (int index, Address address)
		{
			if(index < 0 || index > _list.Count) {
				throw new ArgumentOutOfRangeException ("index");
			}
			if(address == null) {
				throw new ArgumentNullException ("address");
			}
			_list.Insert (index, address);
		}

		public void Remove (Address address)
		{
			_list.Remove (address);
		}

		public void RemoveAt (int index)
		{
			if (index < 0 || index > _list.Count) {
				throw new ArgumentOutOfRangeException ("index");
			}
			_list.RemoveAt (index);
		}

		public int Count {
			get { return _list.Count; }
		}

		public bool IsFixedSize {
			get { return _list.IsFixedSize; }
		}

		public bool IsReadOnly {
			get { return _list.IsReadOnly; }
		}

		public bool IsSynchronized {
			get { return _list.IsSynchronized; }
		}

		public Address this[int index] {
			get {
				if(index < 0 || index > _list.Count) {
					throw new ArgumentOutOfRangeException ("index");
				}
				return (Address) _list[index];
			}
			set {
				if(index < 0 || index > _list.Count) {
					throw new ArgumentOutOfRangeException ("index");
				}
				_list[index] = value;
			}
		}

		public object SyncRoot {
			get { return _list.SyncRoot; }
		}
	}
}
