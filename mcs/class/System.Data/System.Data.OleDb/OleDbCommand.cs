//
// System.Data.OleDb.OleDbCommand
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// Copyright (C) Rodrigo Moya, 2002
//

using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.OleDb
{
	/// <summary>
	/// Represents an SQL statement or stored procedure to execute against a data source.
	/// </summary>
	public sealed class OleDbCommand : Component, ICloneable, IDbCommand
	{
		private string m_command_string = null;
		private OleDbConnection m_connection = null;
		private OleDbTransaction m_transaction = null;
		private int m_timeout = 30; // 30 is the default, as per .NET docs
		private CommandType m_type = CommandType.Text;
		private OleDbParameterCollection m_parameters;

		public OleDbCommand ()
	        {
			m_parameters = new OleDbParameterCollection ();
		}

		public OleDbCommand (string s) : this ()
		{
			m_command_string = s;
		}

		public OleDbCommand (string s, OleDbConnection cnc) : this ()
		{
			m_command_string = s;
			m_connection = cnc;
		}

		public OleDbCommand (string s,
				     OleDbConnection cnc,
				     OleDbTransaction xtrans) : this ()
		{
			m_command_string = s;
			m_connection = cnc;
			m_transaction = xtrans;
		}

		string IDbCommand.CommandText
		{
			get {
				return m_command_string;
			}
			set {
				m_command_string = value;
			}
		}

		int IDbCommand.CommandTimeout
		{
			get {
				return m_timeout;
			}
			set {
				m_timeout = value;
			}
		}

		CommandType IDbCommand.CommandType
		{
			get {
				return m_type;
			}
			set {
				m_type = value;
			}
		}

		public OleDbConnection Connection
		{
			get {
				return m_connection;
			}
			set {
				m_connection = value;
			}
		}

		public bool DesignTimeVisible
		{
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		public OleDbParameterCollection Parameters
		{
			get {
				return m_parameters;
			}
		}

		public OleDbTransaction Transaction
		{
			get {
				return m_transaction;
			}
			set {
				m_transaction = value;
			}
		}

		UpdateRowSource IDbCommand.UpdatedRowSource
		{
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		void IDbCommand.Cancel ()
		{
			throw new NotImplementedException ();
		}

		IDbDataParameter IDbCommand.CreateParameter ()
		{
			return new OleDbParameter ();
		}

		
		int IDbCommand.ExecuteNonQuery ()
		{
			if (m_command_string == null)
				return -1;

			// FIXME
			return 0;
		}

		[MonoTODO]
		IDataReader IDbCommand.ExecuteReader ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		IDataReader IDbCommand.ExecuteReader (CommandBehavior behavior)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		object IDbCommand.ExecuteScalar ()
		{
			throw new NotImplementedException ();
		}

		void IDbCommand.Prepare ()
		{
			// FIXME: prepare string with parameters
		}

		public void ResetCommandTimeout ()
		{
			m_timeout = 30;
		}
	}
}
