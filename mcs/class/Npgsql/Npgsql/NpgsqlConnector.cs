//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
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
//
//	Connector.cs
// ------------------------------------------------------------------
//	Project
//		Npgsql
//	Status
//		0.00.0000 - 06/17/2002 - ulrich sprick - created

using System;
using System.Net.Sockets;

namespace Npgsql
{
	/// <summary>
	/// !!! Helper class, for compilation only. 
	/// </summary>
	internal class Socket 
	{
		internal void Open() { return; }
		internal void Close() { return; }
	}
	
	/// <summary>
	/// Connector implements the logic for the Connection Objects to 
	/// access the physical connection to the database, and isolate 
	/// the application developer from connection pooling internals.
	/// </summary>
	internal class Connector
	{	
		/// <value>Buffer for the public Pooled property</value>
		private bool mPooled;

		/// <value>Chain references for implementing a double linked 
		/// list</value>
		/// <remarks>!!! This is a quick hack in order to get things
		/// going faster. A connector list should better be based on  
		/// System.Collections.DictionaryBase...</remarks>
		internal Connector Next;
		internal Connector Prev;		
		
		/// <value>Controls the pooling of the connector.</value>
		/// <remarks>It this is reset, then the physical connection is
		/// closed and the connector is <b>not</b> added to the
		/// pooled connectors list upon Release(). Can only be cleared
		/// if connector is not shared.</remarks>
		internal bool Pooled 
		{
			get { return this.mPooled; }
			set 
			{
				if ( this.mShared && ! value) return;
				this.mPooled = value;
			}
		}

		/// <value>Buffer for the public Shared property</value>
		private bool mShared;

		/// <value>Controls the physical connection sharing.</value>
		/// <remarks>Set true if this connector is shared among multiple
		/// connections. Can only be set if the connector is pooled
		/// and not yet opened.</remarks>
		internal bool Shared 
		{
			get { return this.mShared; }
			set 
			{
				if ( ! this.mPooled && value && ! mOpen ) return;
				mShared = value;
			}
		}

		/// <value>Counts the numbers of Connections that share
		/// this Connector. Used in Release() to decide wether this
		/// connector is to be moved to the PooledConnectors list.</value>
		internal int mShareCount;

		/// <value>Private Buffer for the connection string property.</value>
		/// <remarks>Compared to the requested connection string in the 
		/// ConnectorPool.RequestConnector() function.
		/// Should not be modified if physical connection is open.</remarks>
		private string mConnectString;
		
		/// <summary>Used to connect to the database server. </summary>
		public string ConnectString 
		{
			get { return mConnectString; }
			set 
			{
				if ( this.mOpen ) // uuuuugh, bad habits...
				{
					throw new Npgsql.NpgsqlException( "Connection strings "
					+ " cannot be modified if connection is open." );
				}
				mConnectString = value;
			}
		}

		/// <value>Provides physical access to the server</value>
		// !!! to be fixed
		private Npgsql.Socket Socket;
		
		/// <value>True if the physical connection is open.</value>
		private bool mOpen;
		
		/// <summary>
		/// Default constructor. Creates a pooled Connector by default.
		/// </summary>
		public Connector()
		{
			this.Pooled = true;
		}
		
		/// <summary>
		/// Construcor, initializes the Connector object.
		/// </summary>
		internal Connector( string ConnectString, bool Shared )
		{
			this.ConnectString = ConnectString;
			this.Shared = Shared;
			this.Pooled = true;
		}
		
		/// <summary>
		/// Opens the physical connection to the server. 
		/// </summary>
		/// <remarks>Usually called by the RequestConnector
		/// Method of the connection pool manager.</remarks>
		internal void Open()
		{
			this.Socket = new Npgsql.Socket();
			this.Socket.Open(); // !!! to be fixed
			this.mOpen = true;
		}
		
		/// <summary>
		/// Releases a connector back to the pool manager's garding. Or to the 
		/// garbage collection.
		/// </summary>
		/// <remarks>The Shared and Pooled properties are no longer needed after 
		/// evaluation inside this method, so they are left in their current state. 
		///	They get new meaning again when the connector is requested from the 
		/// pool manager later. </remarks>
		public void Release()
		{
			if ( this.mShared )
			{
				// A shared connector is returned to the pooled connectors
				// list only if it is not used by any Connection object.
				// Otherwise the job is done by simply decrementing the
				// usage counter:
				if ( --this.mShareCount == 0 )
				{
					Npgsql.ConnectorPool.ConnectorPoolMgr.CutOutConnector( this );
					// Shared connectors are *always* pooled after usage.
					// Depending on the Pooled property at this point
					// might introduce a lot of trouble into an application...
					Npgsql.ConnectorPool.ConnectorPoolMgr.InsertPooledConnector( this );
				}
			}
			else // it is a nonshared connector
			{
				if ( this.Pooled )
				{
					// Pooled connectors are simply put in the
					// PooledConnectors list for later recycling
					Npgsql.ConnectorPool.ConnectorPoolMgr.InsertPooledConnector( this );
				}
				else
				{
					// Unpooled private connectors get the physical
					// connection closed, they are *not* recyled later.
					// Instead they are (implicitly) handed over to the
					// garbage collection.
					// !!! to be fixed
					this.Socket.Close();
				}
			}
		}
	}
}
