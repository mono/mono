//
// Mono.Data.TdsClient.TdsCommand.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using Mono.Data.TdsClient.Internal;
using System;
using System.ComponentModel;
using System.Data;

namespace Mono.Data.TdsClient {
        public class TdsCommand : Component, ICloneable, IDbCommand
	{
		#region Fields

		internal TdsCommandInternal command;

		#endregion // Fields

		#region Constructors

		public TdsCommand ()
		{
			command = new TdsCommandInternal ();
		}

		#endregion // Constructors

		#region Properties

		public string CommandText {
			get { return command.CommandText; }
			set { command.CommandText = value; }
		}

		public int CommandTimeout {
			get { return command.CommandTimeout; }
			set { command.CommandTimeout = value; }
		}

		public CommandType CommandType {
			get { return command.CommandType; }
			set { command.CommandType = value; }
		}

		IDbConnection IDbCommand.Connection {
			get { return ((IDbCommand) command).Connection; }
			set { ((IDbCommand) command).Connection = value; }
		}

		IDataParameterCollection IDbCommand.Parameters {
			get { return ((IDbCommand) command).Parameters; }
		}

		IDbTransaction IDbCommand.Transaction {
			get { return ((IDbCommand) command).Transaction; }
			set { ((IDbCommand) command).Transaction = value; }
		}

		public UpdateRowSource UpdatedRowSource {
			get { return command.UpdatedRowSource; }
			set { command.UpdatedRowSource = value; }
		}

		#endregion // Properties

                #region Methods

		public void Cancel ()
		{
			command.Cancel ();
		}

		IDbDataParameter IDbCommand.CreateParameter ()
		{
			return ((IDbCommand) command).CreateParameter ();
		}

		public int ExecuteNonQuery ()
		{
			return command.ExecuteNonQuery ();
		}

		IDataReader IDbCommand.ExecuteReader ()
		{
			return ((IDbCommand) command).ExecuteReader ();
		}

		IDataReader IDbCommand.ExecuteReader (CommandBehavior behavior)
		{
			return ((IDbCommand) command).ExecuteReader (behavior);
		}

		public object ExecuteScalar ()
		{
			return command.ExecuteScalar ();
		}

		[MonoTODO]
                object ICloneable.Clone()
                {
                        throw new NotImplementedException ();
                }

		public void Prepare ()
		{
			command.Prepare ();
		}

                #endregion // Methods
	}
}
