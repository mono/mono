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
//	ConnectorPool.cs
// ------------------------------------------------------------------
//	Status
//		0.00.0000 - 06/17/2002 - ulrich sprick - creation

using System;
using Npgsql;

namespace Npgsql
{
	internal class ConnectorPool
	{
		/// <value>Unique static instance of the connector pool
		/// mamager.</value>
		internal static ConnectorPool ConnectorPoolMgr = new Npgsql.ConnectorPool();

		/// <value>List of unused, pooled connectors avaliable to the
		/// next RequestConnector() call.</value>
		/// <remarks>Points to the head of a double linked list</remarks>
		internal Npgsql.Connector PooledConnectors;

		/// <value>List of used, shared conncetors.</value>
		/// <remarks>Points to the head of a double linked list</remarks>
		private Npgsql.Connector SharedConnectors;

		/// <summary>
		/// Cuts out a connector from the the list it is in.
		/// </summary>
		/// <param name="Connector">The connector object to be cut out.</param>
		/// <remarks>Shall be replaced if the lists will be based on
		/// Collections.DictionaryBase classs </remarks>
		internal void CutOutConnector( Npgsql.Connector Connector )
		{
			if ( Connector.Prev != null ) Connector.Prev.Next = Connector.Next;
			if ( Connector.Next != null ) Connector.Next.Prev = Connector.Prev;
		}

		/// <summary>
		/// Inserts a connector at the head of a shared connector list.
		/// </summary>
		/// <param name="Connector">The connctor to be inserted</param>
		internal void InsertSharedConnector( Npgsql.Connector Connector )
		{
			if ( this.SharedConnectors == null ) // the list is empty
			{
				// make the connector the only member
				Connector.Prev = Connector.Next = null;
			}
			else // the list is not empty
			{
				// Make the connector the new list head
				Connector.Next = this.SharedConnectors;
				this.SharedConnectors.Prev = Connector;
				Connector.Prev = null;
			}
			// point the list to the new head
			this.SharedConnectors = Connector;
		}

		/// <summary>
		/// Inserts a connector at the head of a pooled connector list.
		/// </summary>
		/// <param name="Connector">The connctor to be inserted</param>
		internal void InsertPooledConnector( Npgsql.Connector Connector )
		{
			if ( this.PooledConnectors == null ) // the list is empty
			{
				// make the connector the only member
				Connector.Prev = Connector.Next = null;
			}
			else // the list is not empty
			{
				// Make the connector the new list head
				Connector.Next = this.PooledConnectors;
				this.PooledConnectors.Prev = Connector;
				Connector.Prev = null;
			}
			// point the list to the new head
			this.PooledConnectors = Connector;
		}

		/// <summary>
		/// Searches the shared and pooled connector lists for a
		/// matching connector object or creates a new one.
		/// </summary>
		/// <param name="ConnectString">used to connect to the
		/// database server</param>
		/// <param name="Shared">Allows multiple connections
		/// on a single connector. </param>
		/// <returns>A pooled connector object.</returns>
		internal Npgsql.Connector RequestConnector ( string ConnectString,
			bool Shared )
		{
			Npgsql.Connector Connector;

			if ( Shared )
			{
				// if a shared connector is requested then the
				// Shared Connector List is searched first

				for ( Connector = Npgsql.ConnectorPool.ConnectorPoolMgr.SharedConnectors;
					Connector != null; Connector = Connector.Next )
				{
					if ( Connector.ConnectString == ConnectString )
					{	// Bingo!
						// Return the shared connector to caller
						Connector.mShareCount++;
						return Connector;
					}
				}
			}
			else
			{
				// if a shared connector could not be found or a
				// nonshared connector is requested, then the pooled
				// (unused) connectors are beeing searched.

				for ( Connector = Npgsql.ConnectorPool.ConnectorPoolMgr.PooledConnectors;
					Connector != null; Connector = Connector.Next )
				{
					if ( Connector.ConnectString == ConnectString )
					{	// Bingo!
						// Remove the Connector from the pooled connectors list.
						this.CutOutConnector( Connector );
					}
					Connector.Shared = Shared;
					if ( Shared )
					{
						// Shared Connectors are then put in the shared
						// connectors list in order to be used by
						// additional clients.
						this.InsertSharedConnector( Connector );
						Connector.mShareCount++;
					}
					// done...
					return Connector;
				}
			}

			// No suitable connector could be found, so create new one
			Connector = new Npgsql.Connector( ConnectString, Shared );

			// Shared connections are added to the shared connectors list
			if ( Shared )
			{
				this.InsertSharedConnector( Connector );
				Connector.mShareCount++;
			}

			// and then returned to the caller
			return Connector;
		}
	}
}
