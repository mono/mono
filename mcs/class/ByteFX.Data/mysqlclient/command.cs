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

namespace ByteFX.Data.MySQLClient
{
#if WINDOWS
	[System.Drawing.ToolboxBitmap( typeof(MySQLCommand), "Designers.command.bmp")]
#endif
	public sealed class MySQLCommand : Component, IDbCommand, ICloneable
	{
		MySQLConnection				m_connection;
		MySQLTransaction			m_txn;
		string						m_sCmdText;
		int							m_UpdateCount;
		UpdateRowSource				m_updatedRowSource = UpdateRowSource.Both;
		MySQLParameterCollection	m_parameters = new MySQLParameterCollection();

		// Implement the default constructor here.
		public MySQLCommand()
		{
		}

		// Implement other constructors here.
		public MySQLCommand(string cmdText)
		{
			m_sCmdText = cmdText;
		}

		public MySQLCommand(System.ComponentModel.IContainer container)
		{
			/// <summary>
			/// Required for Windows.Forms Class Composition Designer support
			/// </summary>
			container.Add(this);
		}

		public MySQLCommand(string cmdText, MySQLConnection connection)
		{
			m_sCmdText    = cmdText;
			m_connection  = connection;
		}

		public new void Dispose() 
		{
			base.Dispose();
		}

		public MySQLCommand(string cmdText, MySQLConnection connection, MySQLTransaction txn)
		{
			m_sCmdText		= cmdText;
			m_connection	= connection;
			m_txn			= txn;
		} 

		/****
		* IMPLEMENT THE REQUIRED PROPERTIES.
		****/
		public string CommandText
		{
			get { return m_sCmdText;  }
			set  { m_sCmdText = value;  }
		}

		public int CommandTimeout
		{
			/*
			* The sample does not support a command time-out. As a result,
			* for the get, zero is returned because zero indicates an indefinite
			* time-out period. For the set, throw an exception.
			*/
			get  { return 0; }
			set  { if (value != 0) throw new NotSupportedException(); }
		}

		public CommandType CommandType
		{
			/*
			* The sample only supports CommandType.Text.
			*/
			get { return CommandType.Text; }
			set { if (value != CommandType.Text) throw new NotSupportedException(); }
		}

		public IDbConnection Connection
		{
			/*
			* The user should be able to set or change the connection at 
			* any time.
			*/
			get 
			{ 
				return m_connection;  
			}
			set
			{
				/*
				* The connection is associated with the transaction
				* so set the transaction object to return a null reference if the connection 
				* is reset.
				*/
				if (m_connection != value)
				this.Transaction = null;

				m_connection = (MySQLConnection)value;
			}
		}

		public MySQLParameterCollection Parameters
		{
			get  { return m_parameters; }
		}

		IDataParameterCollection IDbCommand.Parameters
		{
			get  { return m_parameters; }
		}

		public IDbTransaction Transaction
		{
			/*
			* Set the transaction. Consider additional steps to ensure that the transaction
			* is compatible with the connection, because the two are usually linked.
			*/
			get 
			{ 
				return m_txn; 
			}
			set 
			{ 
				m_txn = (MySQLTransaction)value; 
			}
		}

		public UpdateRowSource UpdatedRowSource
		{
			get 
			{ 
				return m_updatedRowSource;  
			}
			set 
			{ 
				m_updatedRowSource = value; 
			}
		}

		/****
			* IMPLEMENT THE REQUIRED METHODS.
			****/
		public void Cancel()
		{
			// The sample does not support canceling a command
			// once it has been initiated.
			throw new NotSupportedException();
		}

		public IDbDataParameter CreateParameter()
		{
			return new MySQLParameter();
		}

		/// <summary>
		/// Convert the SQL command into a series of ASCII bytes streaming
		/// each of the parameters into the proper place
		/// </summary>
		/// <param name="sql">Source SQL command with parameter markers</param>
		/// <returns>Byte array with all parameters included</returns>
		private ArrayList ConvertSQLToBytes(string sql)
		{
			ArrayList	byteArrays = new ArrayList();
			System.IO.MemoryStream ms = new System.IO.MemoryStream();
			
			if (sql[ sql.Length-1 ] != ';')
				sql += ';';
			byte[] bytes = System.Text.Encoding.ASCII.GetBytes(sql);

			byte left_byte = 0;
			int  parm_start=-1, parm_end = -1;
			for (int x=0; x < bytes.Length; x++)
			{
				byte b = bytes[x];
				// if we see a quote marker, then check to see if we are opening
				// or closing a quote
				if (b == '\'' || b == '\"')
				{
					if (b == left_byte)
					{
						left_byte = 0;
					}
					else
					{
						if (left_byte == 0)
							left_byte = b;
					}
					ms.WriteByte(b);
				}

					// if we find a ; not part of a quoted string, then take the parsed portion
					// as a sql command and add it to the array
				else if (b == ';' && left_byte == 0)
				{
					byte[] sqlBytes = ms.ToArray();
					byteArrays.Add( sqlBytes );
					ms = new System.IO.MemoryStream();
				}

					// if we see the marker for a parameter, then save its position and
					// look for the end
				else if (b == '@' && left_byte == 0) 
				{
					parm_start = x;
					left_byte = b;
				}

					// if we see a space and we are tracking a parameter, then end the parameter and have
					// that parameter serialize itself to the memory streams
				else if ((b == ' ' || b == ',' || b == ';' || b == ')') && left_byte == '@')
				{
					parm_end = x-1;
					string parm_name = sql.Substring(parm_start, parm_end-parm_start+1);
					MySQLParameter p = (m_parameters[parm_name] as MySQLParameter);
					p.SerializeToBytes(ms);
					ms.WriteByte(b);

					if (b == ';') 
					{
						byte[] sqlBytes = ms.ToArray();
						byteArrays.Add( sqlBytes );
						ms = new System.IO.MemoryStream();
					}

					left_byte = 0;
				}

					// we want to write out the bytes in all cases except when we are parsing out a parameter
				else if (left_byte != '@')
					ms.WriteByte( b );
			}

			// if we have any left, then add it at the end
			if (ms.Length > 0) 
			{
				byte[] newbytes = ms.ToArray();
				byteArrays.Add( newbytes );
			}

/*			string s = new string('c', 0);
			byte[] bites = (byte[])byteArrays[0];
			for (int zt=0; zt < bites.Length; zt++)
				s += Convert.ToChar(bites[zt]);
			System.Windows.Forms.MessageBox.Show(s);
*/
			return byteArrays;
		}

		/// <summary>
		/// Executes a single non-select SQL statement.  Examples of this are update,
		/// insert, etc.
		/// </summary>
		/// <returns>Number of rows affected</returns>
		public int ExecuteNonQuery()
		{
			/*
			* ExecuteNonQuery is intended for commands that do
			* not return results, instead returning only the number
			* of records affected.
			*/

			// There must be a valid and open connection.
			if (m_connection == null || m_connection.State != ConnectionState.Open)
			throw new InvalidOperationException("Connection must valid and open");

			ArrayList list = ConvertSQLToBytes( m_sCmdText );
			m_UpdateCount = 0;

			// Execute the command.
			Driver d = m_connection.Driver;
			try 
			{
				for (int x=0; x < list.Count; x++)
				{
					d.SendQuery( (byte[])list[x] );	
					if (d.LastResult == 0)
						m_UpdateCount += d.ReadLength();
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}

			return m_UpdateCount;
		}

		IDataReader IDbCommand.ExecuteReader ()
		{
			return ExecuteReader ();
		}

		IDataReader IDbCommand.ExecuteReader (CommandBehavior behavior)
		{
			return ExecuteReader (behavior);
		}

		public MySQLDataReader ExecuteReader()
		{
			return ExecuteReader(CommandBehavior.Default);
		}

		public MySQLDataReader ExecuteReader(CommandBehavior behavior)
		{
			/*
			* ExecuteReader should retrieve results from the data source
			* and return a DataReader that allows the user to process 
			* the results.
			*/

			// There must be a valid and open connection.
			if (m_connection == null || m_connection.State != ConnectionState.Open)
				throw new InvalidOperationException("Connection must valid and open");

			if (0 != (behavior & CommandBehavior.CloseConnection))
			{
			}

			if (0 != (behavior & CommandBehavior.Default))
			{
			}

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
			}


			/*
			* ExecuteReader should retrieve results from the data source
			* and return a DataReader that allows the user to process 
			* the results.
			*/
			ArrayList cmds = ConvertSQLToBytes( m_sCmdText );
			m_UpdateCount = 0;

			MySQLDataReader reader = new MySQLDataReader(m_connection, behavior == CommandBehavior.SequentialAccess);

			// Execute the command.
			Driver d = m_connection.Driver;
			try 
			{
				for (int x=0; x < cmds.Count; x++)
				{
/*					string st = new string('c',0);
					for (int z=0; z < ((byte[])cmds[x]).Length; z++)
					{
						st += Convert.ToChar( ((byte[])cmds[x])[z] );
					}
					System.Windows.Forms.MessageBox.Show(st); */

/*					System.IO.FileStream fs = new System.IO.FileStream("c:\\cmd.sql",  System.IO.FileMode.OpenOrCreate);
					byte[] bites = (byte[])(cmds[0]);
					fs.Write(bites, 0, bites.Length);
					fs.Close();
*/
					d.SendQuery( (byte[])cmds[x] );
					if (d.LastResult == 0)
						m_UpdateCount += d.ReadLength();
					else
						reader.LoadResults();
				}
			}
			catch (Exception ex) 
			{
				throw ex;
			}

			return reader;

			//TODO implement rest of command behaviors on ExecuteReader
			/*
			* The only CommandBehavior option supported by this
			* sample is the automatic closing of the connection
			* when the user is done with the reader.
			*/
		//      if (behavior == CommandBehavior.CloseConnection)
		//        return new TemplateDataReader(resultset, m_connection);
		//      else
		//        return new TemplateDataReader(resultset);
		}

		/// <summary>
		/// ExecuteScalar executes a single SQL command that will return
		/// a single row with a single column, or if more rows/columns are
		/// returned it will return the first column of the first row.
		/// </summary>
		/// <returns></returns>
		public object ExecuteScalar()
		{
			// There must be a valid and open connection.
			if (m_connection == null || m_connection.State != ConnectionState.Open)
				throw new InvalidOperationException("Connection must valid and open");

			MySQLDataReader reader = new MySQLDataReader(m_connection, false);
			ArrayList cmds = ConvertSQLToBytes( m_sCmdText );
			m_UpdateCount = 0;

			// Execute the command.
			Driver d = m_connection.Driver;

			try 
			{
				for (int x=0; x < cmds.Count; x++)
				{
					d.SendQuery( (byte[])cmds[x] );
					if (d.LastResult == 0)
						m_UpdateCount += d.ReadLength();
					else
						reader.LoadResults();
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}

			if (! reader.Read()) return null;
			return reader.GetValue(0);
		}

		public void Prepare()
		{
		}

		#region ICloneable
		public object Clone() 
		{
			MySQLCommand clone = new MySQLCommand(m_sCmdText, m_connection, m_txn);
			foreach (MySQLParameter p in m_parameters) 
			{
				clone.Parameters.Add(p.Clone());
			}
			return clone;
		}
		#endregion
  }
}
