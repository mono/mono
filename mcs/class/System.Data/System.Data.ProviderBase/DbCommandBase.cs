//
// System.Data.ProviderBase.DbCommandBase
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.Common;

namespace System.Data.ProviderBase {
	public abstract class DbCommandBase : DbCommand
	{
		#region Fields
		
		string commandText;
		int commandTimeout;
		CommandType commandType;
		bool designTimeVisible;
		UpdateRowSource updatedRowSource;

		#endregion // Fields

		#region Constructors
		
		protected DbCommandBase ()
		{
			CommandText = String.Empty;
			CommandTimeout = 30;
			CommandType = CommandType.Text;
			DesignTimeVisible = true;
			UpdatedRowSource = UpdateRowSource.Both;
		}

		protected DbCommandBase (DbCommandBase from)
		{
		}

		#endregion // Constructors

		#region Properties

		public override string CommandText {
			get { return commandText; }
			set { commandText = value; }
		}

		public override int CommandTimeout {
			get { return commandTimeout; }
			set { commandTimeout = value; }
		}

		public override CommandType CommandType {
			get { return commandType; }
			set { commandType = value; }
		}

		public override bool DesignTimeVisible {
			get { return designTimeVisible; }
			set { designTimeVisible = value; }
		}

		public override UpdateRowSource UpdatedRowSource {
			get { return updatedRowSource; }
			set { updatedRowSource = value; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override void Cancel ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int ExecuteNonQuery ()
		{
			DbDataReader reader = ExecuteReader ();
			reader.Close ();
			return reader.RecordsAffected;
		}

		[MonoTODO]
		public override object ExecuteScalar ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Prepare ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void PropertyChanging ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void ResetCommandTimeout ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal static void SetInputParameterValues (DbCommand command, object[] inputParameterValues)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif
