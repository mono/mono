//
// System.Data.DbMetaData.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data {
	public class DbMetaData 
	{
		#region Fields

		DbType dbType;
		bool isNullable;
		long maxLength;
		string name;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public DbMetaData ()
		{
		}

		[MonoTODO]
		public DbMetaData (DbMetaData source)
		{
		}

		#endregion // Constructors

		#region Properties

		public virtual DbType DbType {
			get { return dbType; }
			set { dbType = value; }
		}

		public virtual bool IsNullable {
			get { return isNullable; }
			set { isNullable = value; }
		}

		public virtual long MaxLength {
			get { return maxLength; }
			set { maxLength = value; }
		}

		public string Name {
			get { return name; } 
			set { name = value; }
		}

		#endregion // Properties
	}
}

#endif // NET_1_2
