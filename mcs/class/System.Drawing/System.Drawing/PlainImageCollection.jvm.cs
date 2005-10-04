using System;
using System.Collections;

namespace Mainsoft.Drawing.Imaging
{
	/// <summary>
	/// Summary description for PlainImageCollection.
	/// </summary>
	public class PlainImageCollection : ICollection, IEnumerable
	{
		ArrayList collection = new ArrayList();
		int _position = 0;

		public PlainImageCollection()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		#region ICollection members
		
		public bool IsSynchronized {
			get {
				return collection.IsSynchronized;
			}
		}

		public int Count {
			get {
				return collection.Count;
			}
		}

		public void CopyTo(Array array, int index) {
			collection.CopyTo(array, index);
		}

		public object SyncRoot {
			get {
				return collection.SyncRoot;
			}
		}

		#endregion

		#region IEnumerable members

		public IEnumerator GetEnumerator() {
			return collection.GetEnumerator();
		}

		#endregion

		#region Collection members

		public int Add(PlainImage plainImage) {
			return collection.Add( plainImage );
		}

		public void Clear() {
			collection.Clear();
		}

		public bool Contains(PlainImage plainImage) {
			return collection.Contains(plainImage);
		}

		public int IndexOf(PlainImage plainImage) {
			return collection.IndexOf( plainImage );
		}

		public void Insert(int index, PlainImage value) {
			collection.Insert( index, value );
		}

		public void Remove(PlainImage value) {
			collection.Remove( value );
		}

		public void RemoveAt(int index) {
			collection.RemoveAt( index );
		}

		public PlainImage this[int index] {
			get { return (PlainImage) collection[ index ]; }
		}

		public PlainImage CurrentImage {
			get { return (PlainImage) collection[ _position ]; }
			set { collection[ _position ] = value; }
		}

		public int CurrentImageIndex {
			get { return _position; }
			set { _position = value; }
		}

		#endregion
	}
}
