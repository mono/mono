//
// System.Data.ProviderBase.DbParameterCollectionBase
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Collections;
using System.Data.Common;

namespace System.Data.ProviderBase {
	public abstract class DbParameterCollectionBase : DbParameterCollection
	{
		#region Fields

		ArrayList list;

		#endregion // Fields

		#region Constructors
	
		[MonoTODO]
		protected DbParameterCollectionBase ()
		{
			list = new ArrayList ();
		}

		#endregion // Constructors

		#region Properties

		public override int Count {
			get { return list.Count; }
		}

		public override bool IsFixedSize {
			get { return list.IsFixedSize; }
		}

		public override bool IsReadOnly {
			get { return list.IsReadOnly; }
		}

		public override bool IsSynchronized {
			get { return list.IsSynchronized; }
		}

		protected abstract Type ItemType { get; }

		[MonoTODO]
		protected virtual string ParameterNamePrefix {
			get { throw new NotImplementedException (); }
		}

		public override object SyncRoot {
			get { return list.SyncRoot; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override int Add (object value)
		{
			ValidateType (value);
			throw new NotImplementedException ();
		}

		public override void AddRange (Array values)
		{
			foreach (object value in values)
				Add (value);
		}

		[MonoTODO]
		protected override int CheckName (string parameterName)
		{
			throw new NotImplementedException ();
		}

		public override void Clear ()
		{
			list.Clear ();
		}

		public override bool Contains (object value)
		{
			return list.Contains (value);
		}

		[MonoTODO]
		public override bool Contains (string value)
		{
			throw new NotImplementedException ();
		}

		public override void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

		public override IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		protected override DbParameter GetParameter (int index)
		{
			return (DbParameter) list [index];
		}

		public override int IndexOf (object value)
		{
			return list.IndexOf (value);
		}

		[MonoTODO]
		public override int IndexOf (string parameterName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal static int IndexOf (IEnumerable items, string parameterName)
		{
			throw new NotImplementedException ();
		}

		public override void Insert (int index, object value)
		{
			list.Insert (index, value);
		}

		[MonoTODO]
		protected virtual void OnChange ()
		{
			throw new NotImplementedException ();
		}

		public override void Remove (object value)
		{
			list.Remove (value);
		}

		public override void RemoveAt (int index)
		{
			list.RemoveAt (index);
		}

		[MonoTODO]
		public override void RemoveAt (string parameterName)
		{
			throw new NotImplementedException ();
		}

		protected override void SetParameter (int index, DbParameter value)
		{
			list [index] = value;
		}

		[MonoTODO]
		protected virtual void Validate (int index, object value)
		{
			throw new NotImplementedException ();
		}

		protected virtual void ValidateType (object value)
		{
			Type objectType = value.GetType ();
			Type itemType = ItemType;

			if (objectType != itemType)
			{
				Type thisType = this.GetType ();
				string err = String.Format ("The {0} only accepts non-null {1} type objects, not {2} objects.", thisType.Name, itemType.Name, objectType.Name);
				throw new InvalidCastException (err);
			}
		}

		#endregion // Methods
	}
}

#endif
