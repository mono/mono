//
// System.Data.Sql.SqlDefinition
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System;

namespace System.Data.Sql {
	public sealed class SqlDefinition
	{
		#region Fields

		ISqlCommand cmd;
		string cmdText;
		CommandType cmdType;
		SqlMetaData[] metadata;
		ParameterDirection[] parmDirection;

		#endregion // Fields

		#region Constructors

		public SqlDefinition (ISqlCommand cmd)
		{
			this.cmd = cmd;
			this.cmdText = cmd.CommandText;
			this.cmdType = cmd.CommandType;
		}

		public SqlDefinition (string cmdText, CommandType cmdType, SqlMetaData[] metadata, ParameterDirection[] parmDirection)
		{
			this.cmd = null;
			this.cmdText = cmdText;
			this.cmdType = cmdType;
			this.metadata = metadata;
			this.parmDirection = parmDirection;
		}

		#endregion // Constructors

		#region Properties
			
		public string CommandText {
			get { 
				if (cmd == null)
					return cmdText;
				return cmd.CommandText;
			}
		}

		public CommandType CommandType {
			get { 
				if (cmd == null)
					return cmdType;
				return cmd.CommandType;
			}
		}

		[MonoTODO]
		public int ParameterCount {
			get { 
				if (cmd == null)
					throw new NotImplementedException ();
				return cmd.Parameters.Count;
			}
		}

		#endregion // Properties

		#region Methods

		public ParameterDirection GetParameterDirection (int i)
		{
			if (cmd == null)
				return parmDirection [i];

			return cmd.Parameters [i].Direction;

		}

		public SqlMetaData GetSqlMetaData (int i)
		{
			if (cmd == null)
				return metadata [i];

			return cmd.Parameters [i].MetaData;
		}

		#endregion // Methods
	}
}

#endif
