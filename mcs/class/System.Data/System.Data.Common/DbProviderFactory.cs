//
// System.Data.Common.DbProviderFactory.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Collections;
using System.Security;
using System.Security.Permissions;

namespace System.Data.Common {
	public abstract class DbProviderFactory
	{
		#region Constructors

		[MonoTODO]
		protected DbProviderFactory (DbProviderSupportedClasses supportedClasses)
		{
		}

		#endregion // Constructors

		#region Properties

		[MonoTODO]
		public DbProviderSupportedClasses SupportedClasses {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public virtual DbCommand CreateCommand ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual DbCommandBuilder CreateCommandBuilder ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual DbCommandSet BuildCommandSet ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual DbConnection CreateConnection ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual DbDataAdapter CreateDataAdapter ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual DbDataSourceEnumerator CreateDataSourceEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual DbTable CreateDbTable ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual DbParameter CreateParameter ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual CodeAccessPermission CreatePermission (PermissionState state)
		{
			throw new NotImplementedException ();
		}


		#endregion // Methods
	}
}

#endif // NET_1_2
