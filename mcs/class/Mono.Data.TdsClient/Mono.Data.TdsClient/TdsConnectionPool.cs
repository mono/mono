//
// Mono.Data.TdsClient.TdsConnectionPool.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using Mono.Data.TdsClient.Internal;
using System;
using System.Collections;

namespace Mono.Data.TdsClient {
        internal class TdsConnectionPool : MarshalByRefObject, IList, ICollection, IEnumerable
	{
		#region Fields

		ArrayList list = new ArrayList ();

		int maxSize;
		int minSize;
		int packetSize;
		int port;

		string dataSource;

		#endregion // Fields

		#region Constructors

		public TdsConnectionPool (string dataSource, int port, int packetSize, int minSize, int maxSize)
		{
			this.dataSource = dataSource;
			this.port = port;
			this.packetSize = packetSize;
			this.minSize = minSize;
			this.maxSize = maxSize;
		}

		#endregion // Constructors

		#region Properties

		public Tds this[int index] {
			get { return (Tds) list[index]; }
		}

		object IList.this[int index] {
			get { return this[index]; }
			set { throw new InvalidOperationException (); }
		}

		public int Count {
			get { return list.Count; }
		}

		public bool IsFixedSize {
			get { return false; }
		}

		public bool IsReadOnly {
			get { return true; }
		}

		public bool IsSynchronized {
			get { return false; }
		}

		public int MaxSize {
			get { return maxSize; }
		}

		public int MinSize {
			get { return minSize; }
		}

		public object SyncRoot {
			get { throw new InvalidOperationException (); }
		}

		#endregion // Properties

		#region Methods

		public int Add (object o)
		{
			return list.Add ((Tds) o);
		}

		public void Clear ()
		{
			list.Clear ();
		}

		public bool Contains (object o)
		{
			return list.Contains ((Tds) o);
		}

		public void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		[MonoTODO]
		public Tds FindAnAvailableTds ()
		{
			// make sure we have the minimum count (really only useful the first time)
			lock (list) {
				for (int i = Count; i < minSize; i += 1)
					Add (new Tds42 (dataSource, port, packetSize));

				// look for a tds that isn't in use
				foreach (object o in list) {
					if (!((Tds) o).InUse)
						return (Tds) o;
				}

				// otherwise, try to expand the list, if not at limits
				if (Count < maxSize) {
					Tds tds = new Tds42 (dataSource, port, packetSize);
					Add (tds);
					return tds;
				}
			}

			return null;
			// else we have to wait for one to be available
		}

		public int IndexOf (object o)
		{
			return list.IndexOf ((Tds) o);
		}

		public void Insert (int index, object o)
		{
			list.Insert (index, (Tds) o);
		}

		public void Remove (object o)
		{
			list.Remove ((Tds) o);
		}

		public void RemoveAt (int index)
		{
			list.RemoveAt (index);
		}

		#endregion // Methods
	}
}
