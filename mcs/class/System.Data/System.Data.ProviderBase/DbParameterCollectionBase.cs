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
		#region Constructors
	
		[MonoTODO]
		protected DbParameterCollectionBase ()
		{
		}

		#endregion // Constructors

		#region Properties

		[MonoTODO]
		public override int Count {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override bool IsFixedSize {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override bool IsReadOnly {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override bool IsSynchronized {
			get { throw new NotImplementedException (); }
		}

		protected abstract Type ItemType { get; }

		[MonoTODO]
		protected virtual string ParameterNamePrefix {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override object SyncRoot {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override int Add (object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void AddRange (Array values)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override int CheckName (string parameterName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Clear ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool Contains (object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool Contains (string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void CopyTo (Array array, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override DbParameter GetParameter (int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int IndexOf (object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int IndexOf (string parameterName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal static int IndexOf (IEnumerable itmes, string parameterName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Insert (int index, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnChange ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Remove (object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void RemoveAt (int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void RemoveAt (string parameterName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void SetParameter (int index, DbParameter value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void Validate (int index, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void ValidateType (object value)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif
