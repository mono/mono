//
// System.Data.Common.DbParameter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data.Common {
	public abstract class DbParameter : MarshalByRefObject, IDbDataParameter, IDataParameter
	{
		#region Constructors

		[MonoTODO]
		protected DbParameter ()
		{
		}

		#endregion // Constructors

		#region Properties

		public abstract DbType DbType { get; set; }
		public abstract ParameterDirection Direction { get; set; }
		public abstract bool IsNullable { get; set; }
		public abstract int Offset { get; set; }
		public abstract string ParameterName { get; set; }
		public abstract byte Precision { get; set; }
		public abstract byte Scale { get; set; }
		public abstract int Size { get; set; }
		public abstract string SourceColumn { get; set; }
		public abstract DataRowVersion SourceVersion { get; set; }
		public abstract object Value { get; set; }

		#endregion // Properties

		#region Methods

		public abstract void CopyTo (DbParameter destination);
		public abstract void ResetDbType ();

		#endregion // Methods
	}
}

#endif // NET_1_2
