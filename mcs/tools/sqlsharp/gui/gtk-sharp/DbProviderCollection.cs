//
// DbProviderCollection.cs
//
// Author:
//     Daniel Morgan <danmorg@sc.rr.com>
//
// (C)Copyright 2002 by Daniel Morgan
//
// To be included with Mono as a SQL query tool licensed under the GPL license.
//

namespace Mono.Data.SqlSharp.Gui.GtkSharp 
{
	using System;
	using System.Data;
	using System.Collections;

	public class DbProviderCollection : MarshalByRefObject, IList, ICollection, IEnumerable 
	{	
		#region Fields

		ArrayList list = new ArrayList ();

		#endregion // Fields

		#region Constructors

		public DbProviderCollection () 
		{
		}

		#endregion // Constructors

		#region Properties

		public DbProvider this[int index] {
			get { 
				return (DbProvider) list[index]; 
			}
		}

		public DbProvider this[string key] {
			get {
				DbProvider p = null;
				foreach(object o in list) {
					p = (DbProvider) o;
					if(p.Key.ToUpper().Equals(key.ToUpper())) {
						return p;
					}
				}
				throw new Exception("DbProvider not found");
			}
		}

		object IList.this[int index] {
			get { 
				return list[index]; 
			}			

			set {
				list[index] = value;
			}
		}

		public int Count {
			get { 
				return list.Count; 
			}
		}

		public bool IsFixedSize {
			get { 
				return false; 
			}
		}

		public bool IsReadOnly {
			get { 
				return true; 
			}
		}

		public bool IsSynchronized {
			get { 
				return false; 
			}
		}

		public object SyncRoot {
			get { 
				throw new InvalidOperationException (); 
			}
		}

		#endregion // Properties

		#region Methods

		public int Add (object o) 
		{
			return list.Add ((DbProvider) o);
		}

		public void Clear () 
		{
			list.Clear ();
		}

		public bool Contains (object o) 
		{
			return list.Contains ((DbProvider) o);
		}

		public void CopyTo (Array array, int index) 
		{
			list.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator () 
		{
			return list.GetEnumerator ();
		}

		public int IndexOf (object o) 
		{
			return list.IndexOf ((DbProvider) o);
		}

		public void Insert (int index, object o) 
		{
			list.Insert (index, (DbProvider) o);
		}

		public void Remove (object o) 
		{
			list.Remove ((DbProvider) o);
		}

		public void RemoveAt (int index) 
		{
			list.RemoveAt (index);
		}

		#endregion // Methods
		
	}
}
