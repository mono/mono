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
using System.Collections.Specialized;
using System.ComponentModel;

namespace ByteFX.Data.MySQLClient
{
	/// <summary>
	/// Summary description for MySQLConnection.
	/// </summary>
#if WINDOWS
	[System.Drawing.ToolboxBitmap( typeof(MySQLConnection), "Designers.connection.bmp")]
#endif
	[ToolboxItem(true)]
	public sealed class MySQLConnection : Common.Connection, IDbConnection, ICloneable
	{
		Driver				m_Driver;

		// Always have a default constructor.
		public MySQLConnection()
		{
			ConnectionString = "data source=localhost;user id=root;pwd=;database=mysql";
		}

		public MySQLConnection(System.ComponentModel.IContainer container)
		{
			ConnectionString = "data source=localhost;user id=root;pwd=;database=mysql";
		}
    

		// Have a constructor that takes a connection string.
		public MySQLConnection(string sConnString)
		{
			ConnectionString = sConnString;
			Init();
		}

		public new void Dispose()
		{
			base.Dispose();
			if (m_State == ConnectionState.Open)
				Close();
		}

		internal Driver Driver
		{
			get { return m_Driver; }
		}

		#region Properties
		/// <summary>
		/// Gets a string containing the version of the of the server to which the client is connected.
		/// </summary>
		[Browsable(false)]
		public string ServerVersion 
		{
			get
			{
				return m_Driver.ServerVersion;
			}
		}

		[Browsable(false)]
		public bool UseCompression
		{
			get 
			{
				String s = m_ConnSettings["use compression"];
				if (s == null) return false;
				return s.ToLower() == "true" || s.ToLower() == "yes";
			}
		}
		
		[Browsable(false)]
		public int Port
		{
			get
			{
				if (m_ConnSettings["port"] == null)
					return 3306;
				else
					return Convert.ToInt32(m_ConnSettings["port"]);
			}
		}

#if WINDOWS
		[Editor(typeof(Designers.ConnectionStringEditor), typeof(System.Drawing.Design.UITypeEditor))]
#endif
		[Browsable(true)]
		[Category("Data")]
		public string ConnectionString
		{
			get
			{
				// Always return exactly what the user set.
				// Security-sensitive information may be removed.
				return m_ConnString;
			}
			set
			{
				m_ConnString = value;
				m_ConnSettings = Common.ConnectionString.ParseConnectString( m_ConnString );
			}
		}

		#endregion

		#region Transactions
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public MySQLTransaction BeginTransaction()
		{
			MySQLTransaction t = new MySQLTransaction();
			t.Connection = this;
			m_Driver.SendCommand( DBCmd.QUERY, "BEGIN");
			return t;
		}

		/// <summary>
		/// 
		/// </summary>
		IDbTransaction IDbConnection.BeginTransaction()
		{
			return BeginTransaction();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="level"></param>
		/// <returns></returns>
		public MySQLTransaction BeginTransaction(IsolationLevel level)
		{
			MySQLTransaction t = new MySQLTransaction();
			t.Connection = this;
			t.IsolationLevel = level;
			string cmd = "SET SESSION TRANSACTION ISOLATION LEVEL ";
			switch (level) 
			{
				case IsolationLevel.ReadCommitted:
					cmd += "READ COMMITTED"; break;
				case IsolationLevel.ReadUncommitted:
					cmd += "READ UNCOMMITTED"; break;
				case IsolationLevel.RepeatableRead:
					cmd += "REPEATABLE READ"; break;
				case IsolationLevel.Serializable:
					cmd += "SERIALIZABLE"; break;
				case IsolationLevel.Chaos:
					throw new NotSupportedException("Chaos isolation level is not supported");
			}
			m_Driver.SendCommand( DBCmd.QUERY, cmd );
			m_Driver.SendCommand( DBCmd.QUERY, "BEGIN");
			return t;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="level"></param>
		/// <returns></returns>
		IDbTransaction IDbConnection.BeginTransaction(IsolationLevel level)
		{
			return BeginTransaction(level);
		}
		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dbName"></param>
		public void ChangeDatabase(string dbName)
		{
			/*
			* Change the database setting on the back-end. Note that it is a method
			* and not a property because the operation requires an expensive
			* round trip.
			*/
			m_ConnSettings["database"] = dbName;

			m_Driver.SendCommand( DBCmd.INIT_DB, m_ConnSettings["database"] );
		}

		public void Open()
		{
			/*
			* Open the database connection and set the ConnectionState
			* property. If the underlying connection to the server is 
			* expensive to obtain, the implementation should provide
			* implicit pooling of that connection.
			* 
			* If the provider also supports automatic enlistment in 
			* distributed transactions, it should enlist during Open().
			*/
			m_State = ConnectionState.Connecting;
			if (m_Driver == null)
				m_Driver = new Driver(ConnectionTimeout);
			m_Driver.Open( DataSource, Port, User, Password, UseCompression );

			//m_Driver.SendCommand( DBCmd.QUERY, "use " + m_Settings["database"] );
			m_Driver.SendCommand( DBCmd.INIT_DB, m_ConnSettings["database"] );
			m_State = ConnectionState.Open;
		}


		public void Close()
		{
			// this shouldn't happen, but it is!
			if (m_State == ConnectionState.Closed) return;

			m_Driver.SendCommand( DBCmd.QUIT, null );
			m_Driver.Close();
			/*
			* Close the database connection and set the ConnectionState
			* property. If the underlying connection to the server is
			* being pooled, Close() will release it back to the pool.
			*/
			m_State = ConnectionState.Closed;
		}

		public IDbCommand CreateCommand()
		{
			// Return a new instance of a command object.
			MySQLCommand c = new MySQLCommand();
			c.Connection = this;
			return c;
		}

		#region ICloneable
		public object Clone()
		{
			MySQLConnection clone = new MySQLConnection();
			clone.ConnectionString = this.ConnectionString;
			//TODO:  how deep should this go?
			return clone;
		}
		#endregion
  }
}
