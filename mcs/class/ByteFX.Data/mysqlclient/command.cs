// ByteFX.Data data access components for .Net
// Copyright (C) 2002-2003  ByteFX, Inc.
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Data;
using System.ComponentModel;
using System.Collections;

namespace ByteFX.Data.MySqlClient
{
	/// <summary>
	/// Represents a SQL statement to execute against a MySQL database. This class cannot be inherited.
	/// </summary>
	/// <include file='docs/MySqlCommand.xml' path='MyDocs/MyMembers[@name="Class"]/*'/>
#if WINDOWS
	[System.Drawing.ToolboxBitmap( typeof(MySqlCommand), "MySqlClient.resources.command.bmp")]
#endif
	[System.ComponentModel.DesignerCategory("Code")]
	public sealed class MySqlCommand : Component, IDbCommand, ICloneable
	{
		MySqlConnection				connection;
		MySqlTransaction			curTransaction;
		string						cmdText;
		int							updateCount;
		UpdateRowSource				updatedRowSource = UpdateRowSource.Both;
		MySqlParameterCollection	parameters = new MySqlParameterCollection();
		private ArrayList			arraySql = new ArrayList();

		/// <summary>
		/// Overloaded. Initializes a new instance of the MySqlCommand class.
		/// </summary>
		public MySqlCommand()
		{
		}

		/// <summary>
		/// Overloaded. Initializes a new instance of the MySqlCommand class.
		/// </summary>
		public MySqlCommand(string cmdText)
		{
			this.cmdText = cmdText;
		}

		/// <summary>
		/// Overloaded. Initializes a new instance of the MySqlCommand class.
		/// </summary>
		public MySqlCommand(System.ComponentModel.IContainer container)
		{
			// Required for Windows.Forms Class Composition Designer support
			container.Add(this);
		}

		/// <summary>
		/// Overloaded. Initializes a new instance of the MySqlCommand class.
		/// </summary>
		public MySqlCommand(string cmdText, MySqlConnection connection)
		{
			this.cmdText    = cmdText;
			this.connection  = connection;
		}

		/// <summary>
		/// Disposes of this instance of MySqlCommand
		/// </summary>
		public new void Dispose() 
		{
			base.Dispose();
		}

		/// <summary>
		/// Overloaded. Initializes a new instance of the MySqlCommand class.
		/// </summary>
		public MySqlCommand(string cmdText, MySqlConnection connection, MySqlTransaction txn)
		{
			this.cmdText	= cmdText;
			this.connection	= connection;
			curTransaction	= txn;
		} 

		#region Properties

		/// <summary>
		/// Gets or sets the SQL statement to execute at the data source.
		/// </summary>
		[Category("Data")]
		[Description("Command text to execute")]
#if WINDOWS
		[Editor("ByteFX.Data.Common.Design.SqlCommandTextEditor,MySqlClient.Design", typeof(System.Drawing.Design.UITypeEditor))]
#endif
		public string CommandText
		{
			get { return cmdText;  }
			set  { cmdText = value;  }
		}

		internal int UpdateCount 
		{
			get { return updateCount; }
		}

		/// <summary>
		/// Gets or sets the wait time before terminating the attempt to execute a command and generating an error.
		/// </summary>
		[Category("Misc")]
		[Description("Time to wait for command to execute")]
		public int CommandTimeout
		{
			// TODO: support this
			get  { return 0; }
			set  { if (value != 0) throw new NotSupportedException(); }
		}

		/// <summary>
		/// Gets or sets a value indicating how the CommandText property is to be interpreted.  Only
		/// type Text is currently supported.
		/// </summary>
		[Category("Data")]
		public CommandType CommandType
		{
			get { return CommandType.Text; }
			set 
			{ 
				if (value != CommandType.Text) 
					throw new NotSupportedException("This version of the MySql provider only supports Text command types"); 
			}
		}

		/// <summary>
		/// Gets or sets the MySqlConnection used by this instance of the MySqlCommand.
		/// </summary>
		[Category("Behavior")]
		[Description("Connection used by the command")]
		public IDbConnection Connection
		{
			/*
			* The user should be able to set or change the connection at 
			* any time.
			*/
			get 
			{ 
				return connection;  
			}
			set
			{
				/*
				* The connection is associated with the transaction
				* so set the transaction object to return a null reference if the connection 
				* is reset.
				*/
				if (connection != value)
				this.Transaction = null;

				connection = (MySqlConnection)value;
			}
		}

		/// <summary>
		/// Gets the MySqlParameterCollection.
		/// </summary>
		[Category("Data")]
		[Description("The parameters collection")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public MySqlParameterCollection Parameters
		{
			get  { return parameters; }
		}

		IDataParameterCollection IDbCommand.Parameters
		{
			get  { return parameters; }
		}

		/// <summary>
		///	Gets or sets the MySqlTransaction within which the MySqlCommand executes.
		/// </summary>
		[Browsable(false)]
		public IDbTransaction Transaction
		{
			/*
			* Set the transaction. Consider additional steps to ensure that the transaction
			* is compatible with the connection, because the two are usually linked.
			*/
			get 
			{ 
				return curTransaction; 
			}
			set 
			{ 
				curTransaction = (MySqlTransaction)value; 
			}
		}

		/// <summary>
		/// Gets or sets how command results are applied to the DataRow when used by the Update method of the DbDataAdapter.
		/// </summary>
		[Category("Behavior")]
		public UpdateRowSource UpdatedRowSource
		{
			get 
			{ 
				return updatedRowSource;  
			}
			set 
			{ 
				updatedRowSource = value; 
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Attempts to cancel the execution of a MySqlCommand.  This operation is not supported.
		/// </summary>
		/// <exception cref="NotSupportedException">This operation is not supported.</exception>
		public void Cancel()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Creates a new instance of a MySqlParameter object.
		/// </summary>
		/// <returns></returns>
		public MySqlParameter CreateParameter()
		{
			return new MySqlParameter();
		}

		IDbDataParameter IDbCommand.CreateParameter()
		{
			return CreateParameter();
		}

		private ArrayList SplitSql(string sql)
		{
			ArrayList commands = new ArrayList();
			System.IO.MemoryStream ms = new System.IO.MemoryStream(sql.Length);

			// first we tack on a semi-colon, if not already there, to make our
			// sql processing code easier.  Then we ask our encoder to give us
			// the bytes for this sql string
			byte[] bytes = connection.Encoding.GetBytes(sql + ";");

			byte left_byte = 0;
			bool escaped = false;
			int  parm_start=-1;
			for (int x=0; x < bytes.Length; x++)
			{
				byte b = bytes[x];

				// if we see a quote marker, then check to see if we are opening
				// or closing a quote
				if ((b == '\'' || b == '\"') && ! escaped )
				{
					if (b == left_byte) left_byte = 0;
					else if (left_byte == 0) left_byte = b;
				}

				else if (b == '\\') 
				{
					escaped = !escaped;
				}

					// if we see the marker for a parameter, then save its position and
					// look for the end
				else if (b == '@' && left_byte == 0 && ! escaped && parm_start==-1) 
					parm_start = x;

					// if we see a space and we are tracking a parameter, then end the parameter and have
					// that parameter serialize itself to the memory stream
				else if (parm_start > -1 && (b != '@') && (b != '$') && (b != '_') && (b != '.') && ! Char.IsLetterOrDigit((char)b))
				{
					string parm_name = sql.Substring(parm_start, x-parm_start); 

					if (parameters.Contains( parm_name ))
					{
						MySqlParameter p = (parameters[parm_name] as MySqlParameter);
						p.SerializeToBytes(ms, connection );
					}
					else
					{
						// otherwise assume system param. just write it out
						byte[] buf = connection.Encoding.GetBytes(parm_name);
						ms.Write(buf, 0, buf.Length); 
					}
					parm_start=-1;
				}

				// if we are not in a string and we are not escaped and we are on a semi-colon,
				// then write out what we have as a command
				if (left_byte == 0 && ! escaped && b == ';' && ms.Length > 0)
				{
					bool goodcmd = false;
					byte[] byteArray = ms.ToArray();
					foreach (byte cmdByte in byteArray)
						if (cmdByte != ' ') { goodcmd = true; break; }

					if (goodcmd)
						commands.Add( byteArray );
					ms.SetLength(0);
				}
				else if (parm_start == -1)
					ms.WriteByte(b);


				// we want to write out the bytes in all cases except when we are parsing out a parameter
				if (escaped && b != '\\') escaped = false;
			}

			return commands;
		}

		private void ReadOffResultSet()
		{
			Driver driver = connection.InternalConnection.Driver;

			// first read off the schema
			Packet packet = driver.ReadPacket();
			while (! packet.IsLastPacket())
				packet = driver.ReadPacket();

			// now read off the data
			packet = driver.ReadPacket();
			while (! packet.IsLastPacket())
				packet = driver.ReadPacket();
		}

		/// <summary>
		/// Internal function to execute the next command in an array of commands
		/// </summary>
		internal CommandResult ExecuteBatch( bool stopAtResultSet )
		{
			Driver driver = connection.InternalConnection.Driver;

			while (arraySql.Count > 0)
			{
				byte[] sql = (byte[])arraySql[0];
				arraySql.RemoveAt(0);

				CommandResult result = driver.Send( DBCmd.QUERY, sql );
				
				if (result.IsResultSet)
				{
					if (stopAtResultSet) return result;
					result.Clear();
					continue;
				}

				// at this point, we know it is a zero field count
				if (updateCount == -1) updateCount = 0;
				updateCount += result.RowsAffected;
			}
			return null;
		}

		/// <summary>
		/// Executes a SQL statement against the connection and returns the number of rows affected.
		/// </summary>
		/// <returns>Number of rows affected</returns>
		public int ExecuteNonQuery()
		{
			// There must be a valid and open connection.
			if (connection == null || connection.State != ConnectionState.Open)
				throw new InvalidOperationException("Connection must be valid and open");

			// Data readers have to be closed first
			if (connection.Reader != null)
				throw new MySqlException("There is already an open DataReader associated with this Connection which must be closed first.");

			// execute any commands left in the queue from before.
			//ExecuteBatch(false);
			
			arraySql = SplitSql( cmdText );
			updateCount = 0;

			ExecuteBatch(false);

			return (int)updateCount;
		}

		IDataReader IDbCommand.ExecuteReader ()
		{
			return ExecuteReader ();
		}

		IDataReader IDbCommand.ExecuteReader (CommandBehavior behavior)
		{
			return ExecuteReader (behavior);
		}

		/// <summary>
		/// Overloaded. Sends the CommandText to the Connection and builds a MySqlDataReader.
		/// </summary>
		/// <returns></returns>
		public MySqlDataReader ExecuteReader()
		{
			return ExecuteReader(CommandBehavior.Default);
		}


		/// <summary>
		/// Overloaded. Sends the CommandText to the Connection and builds a MySqlDataReader.
		/// </summary>
		/// <returns></returns>
		public MySqlDataReader ExecuteReader(CommandBehavior behavior)
		{
			// There must be a valid and open connection.
			if (connection == null || connection.State != ConnectionState.Open)
				throw new InvalidOperationException("Connection must be valid and open");

			// make sure all readers on this connection are closed
			if (connection.Reader != null)
				throw new InvalidOperationException("There is already an open DataReader associated with this Connection which must be closed first.");

			string sql = cmdText;

			if (0 != (behavior & CommandBehavior.KeyInfo))
			{
			}

			if (0 != (behavior & CommandBehavior.SchemaOnly))
			{
			}

			if (0 != (behavior & CommandBehavior.SequentialAccess))
			{
			}

			if (0 != (behavior & CommandBehavior.SingleResult))
			{
			}

			if (0 != (behavior & CommandBehavior.SingleRow))
			{
				sql = String.Format("SET SQL_SELECT_LIMIT=1;{0};SET sql_select_limit=-1;", cmdText);
			}

			arraySql = SplitSql( sql );

			updateCount = -1;
			MySqlDataReader reader = new MySqlDataReader(this, behavior);

			// move to the first resultset
			reader.NextResult();
			connection.Reader = reader;
			return reader;
		}

		/// <summary>
		/// Executes the query, and returns the first column of the first row in the 
		/// result set returned by the query. Extra columns or rows are ignored.
		/// </summary>
		/// <returns></returns>
		public object ExecuteScalar()
		{
			// There must be a valid and open connection.
			if (connection == null || connection.State != ConnectionState.Open)
				throw new InvalidOperationException("Connection must be valid and open");

			// Data readers have to be closed first
			if (connection.Reader != null)
				throw new MySqlException("There is already an open DataReader associated with this Connection which must be closed first.");

			arraySql = SplitSql( cmdText );

			updateCount = -1;
			MySqlDataReader reader = new MySqlDataReader(this, 0);
			reader.NextResult();
			object val = null;
			if (reader.Read())
				val = reader.GetValue(0);
			reader.Close();
			return val;
		}

		/// <summary>
		/// Creates a prepared version of the command on an instance of MySQL Server. This
		/// is currently not supported.
		/// </summary>
		public void Prepare()
		{
		}
		#endregion

		#region ICloneable
		/// <summary>
		/// Creates a clone of this MySqlCommand object.  CommandText, Connection, and Transaction properties
		/// are included as well as the entire parameter list.
		/// </summary>
		/// <returns>The cloned MySqlCommand object</returns>
		public object Clone() 
		{
			MySqlCommand clone = new MySqlCommand(cmdText, connection, curTransaction);
			foreach (MySqlParameter p in parameters) 
			{
				clone.Parameters.Add((p as ICloneable).Clone());
			}
			return clone;
		}
		#endregion
  }
}
