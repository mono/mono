/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.Data;
using System.Collections;
using System.Threading;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Firebird
{
	internal sealed class FbPoolManager
	{
		#region Static fields

		public static readonly FbPoolManager Instance = new FbPoolManager();

		#endregion

		#region Fields

		private Hashtable	pools;
		private Hashtable	handlers;
		private object		syncObject;

		#endregion

		#region Properties

		public int PoolsCount
		{
			get
			{
				if (this.pools != null)
				{
					return this.pools.Count;
				}
				return 0;
			}
		}

		#endregion

		#region Constructors

		private FbPoolManager()
		{
			this.pools		= Hashtable.Synchronized(new Hashtable());
			this.handlers	= Hashtable.Synchronized(new Hashtable());
			this.syncObject	= new object();
		}

		#endregion

		#region Methods

		public FbConnectionPool FindPool(string connectionString)
		{
			FbConnectionPool pool = null;

			lock (this.syncObject)
			{
				if (this.pools.ContainsKey(connectionString.GetHashCode()))
				{
					pool = (FbConnectionPool)pools[connectionString.GetHashCode()];
				}
			}

			return pool;
		}

		public FbConnectionPool CreatePool(string connectionString)
		{
			FbConnectionPool pool = null;

			lock (this.syncObject)
			{
				pool = this.FindPool(connectionString);

				if (pool == null)
				{
					lock (this.pools.SyncRoot)
					{
						int hashcode = connectionString.GetHashCode();

						// Create an empty pool	handler
						EmptyPoolEventHandler handler = new EmptyPoolEventHandler(this.OnEmptyPool);

						this.handlers.Add(hashcode, handler);

						// Create the new connection pool
						pool = new FbConnectionPool(connectionString);

						this.pools.Add(hashcode, pool);

						pool.EmptyPool += handler;
					}
				}
			}

			return pool;
		}

		public void ClearAllPools()
		{
			lock (this.syncObject)
			{
				lock (this.pools.SyncRoot)
				{
					FbConnectionPool[] tempPools = new FbConnectionPool[this.pools.Count];

					this.pools.Values.CopyTo(tempPools, 0);

					foreach (FbConnectionPool pool in tempPools)
					{
						// Clear pool
						pool.Clear();
					}

					// Clear Hashtables
					this.pools.Clear();
					this.handlers.Clear();
				}
			}
		}

		public void ClearPool(string connectionString)
		{
			lock (this.syncObject)
			{
				lock (this.pools.SyncRoot)
				{
					int hashCode = connectionString.GetHashCode();

					if (this.pools.ContainsKey(hashCode))
					{
						FbConnectionPool pool = (FbConnectionPool)this.pools[hashCode];

						// Clear pool
						pool.Clear();
					}
				}
			}
		}

		#endregion

		#region Private	Methods

		private void OnEmptyPool(object sender, EventArgs e)
		{
			lock (this.pools.SyncRoot)
			{
				int hashCode = (int)sender;

				if (this.pools.ContainsKey(hashCode))
				{
					FbConnectionPool pool = (FbConnectionPool)this.pools[hashCode];
					EmptyPoolEventHandler handler = (EmptyPoolEventHandler)this.handlers[hashCode];

					pool.EmptyPool -= handler;

					this.pools.Remove(hashCode);
					this.handlers.Remove(hashCode);

					pool = null;
					handler = null;
				}
			}
		}

		#endregion
	}

	internal delegate void EmptyPoolEventHandler(object sender, EventArgs e);

	internal class FbConnectionPool : MarshalByRefObject
	{
		#region Fields

		private FbConnectionString	options;
		private ArrayList			locked;
		private ArrayList			unlocked;
		private Thread				cleanUpThread;
		private string				connectionString;
		private bool				isRunning;
		private long				lifeTime;
		private	object				syncObject;

		#endregion

		#region Events

		public event EmptyPoolEventHandler EmptyPool;

		#endregion

		#region Properties

		public int Count
		{
			get { return this.unlocked.Count + this.locked.Count; }
		}

		#endregion

		#region Constructors

		public FbConnectionPool(string connectionString)
		{
			this.syncObject			= new object();
			this.connectionString	= connectionString;
			this.options			= new FbConnectionString(connectionString);
			this.lifeTime			= this.options.ConnectionLifeTime * TimeSpan.TicksPerSecond;

			if (this.options.MaxPoolSize == 0)
			{
				this.locked = ArrayList.Synchronized(new ArrayList());
				this.unlocked = ArrayList.Synchronized(new ArrayList());
			}
			else
			{
				this.locked = ArrayList.Synchronized(new ArrayList(this.options.MaxPoolSize));
				this.unlocked = ArrayList.Synchronized(new ArrayList(this.options.MaxPoolSize));
			}

			// If a	minimun	number of connections is requested
			// initialize the pool
			this.Initialize();

			// Start the cleanup thread	only if	needed
			if (this.lifeTime != 0)
			{
				this.isRunning = true;

				this.cleanUpThread = new Thread(new ThreadStart(this.RunCleanup));
				this.cleanUpThread.Name = "Cleanup Thread";
				this.cleanUpThread.Start();
				this.cleanUpThread.IsBackground = true;
			}
		}

		#endregion

		#region Methods

		public void CheckIn(FbConnectionInternal connection)
		{
			connection.OwningConnection = null;
			connection.Created = System.DateTime.Now.Ticks;

			this.locked.Remove(connection);
			this.unlocked.Add(connection);
		}

		public FbConnectionInternal CheckOut()
		{
			FbConnectionInternal newConnection = null;

			lock (this.syncObject)
			{
				this.CheckMaxPoolSize();

				lock (this.unlocked.SyncRoot)
				{
					newConnection = this.GetConnection();
					if (newConnection != null)
					{
						return newConnection;
					}
				}

				newConnection = this.Create();

				// Set connection pooling settings to the new connection
				newConnection.Lifetime = this.options.ConnectionLifeTime;
				newConnection.Pooled = true;

				// Added to	the	locked connections list.
				this.locked.Add(newConnection);
			}

			return newConnection;
		}

		public void Clear()
		{
			lock (this.syncObject)
			{
				// Stop	cleanup	thread
				if (this.cleanUpThread != null)
				{
					this.cleanUpThread.Abort();
					this.cleanUpThread.Join();
				}

				// Close all unlocked connections
				FbConnectionInternal[] list = (FbConnectionInternal[])this.unlocked.ToArray(typeof(FbConnectionInternal));

				foreach (FbConnectionInternal connection in list)
				{
					connection.Disconnect();
				}

				// Close all locked	connections
				list = (FbConnectionInternal[])this.locked.ToArray(typeof(FbConnectionInternal));

				foreach (FbConnectionInternal connection in list)
				{
					connection.Disconnect();
				}

				// Clear lists
				this.unlocked.Clear();
				this.locked.Clear();

				// Raise EmptyPool event
				if (this.EmptyPool != null)
				{
					this.EmptyPool(this.connectionString.GetHashCode(), null);
				}

				// Reset fields
				this.unlocked			= null;
				this.locked				= null;
				this.connectionString	= null;
				this.cleanUpThread		= null;
				this.EmptyPool			= null;
			}
		}

		#endregion

		#region Private	Methods

		private bool CheckMinPoolSize()
		{
			if (this.options.MinPoolSize > 0 && this.Count == this.options.MinPoolSize)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		private void CheckMaxPoolSize()
		{
			lock (this.syncObject)
			{
				if (this.options.MaxPoolSize > 0 &&
					(this.Count + 1) >= this.options.MaxPoolSize)
				{
					long timeout = this.options.ConnectionTimeout * TimeSpan.TicksPerSecond;
					long start = DateTime.Now.Ticks;

					while (true)
					{
						if ((this.Count + 1) >= this.options.MaxPoolSize)
						{
							if ((DateTime.Now.Ticks - start) > timeout)
							{
								throw new SystemException("Timeout exceeded.");
							}

							Thread.Sleep(100);
						}
						else
						{
							break;
						}
					}
				}
			}
		}

		private void Initialize()
		{
			lock (this.syncObject)
			{
				for (int i = 0; i < this.options.MinPoolSize; i++)
				{
					this.unlocked.Add(this.Create());
				}
			}
		}

		private FbConnectionInternal Create()
		{
			FbConnectionInternal connection = new FbConnectionInternal(this.options);
			connection.Connect();

			connection.Pooled = true;
			connection.Created = DateTime.Now.Ticks;

			return connection;
		}

		private FbConnectionInternal GetConnection()
		{
			FbConnectionInternal[] list = (FbConnectionInternal[])this.unlocked.ToArray(typeof(FbConnectionInternal));
			FbConnectionInternal result = null;
			long check = -1;

			Array.Reverse(list);

			foreach (FbConnectionInternal connection in list)
			{
				if (connection.Verify())
				{
					if (this.lifeTime != 0)
					{
						long now = DateTime.Now.Ticks;
						long expire = connection.Created + this.lifeTime;

						if (now >= expire)
						{
							if (this.CheckMinPoolSize())
							{
								this.unlocked.Remove(connection);
								this.Expire(connection);
							}
						}
						else
						{
							if (expire > check)
							{
								check = expire;
								result = connection;
							}
						}
					}
					else
					{
						result = connection;
						break;
					}
				}
				else
				{
					this.unlocked.Remove(connection);
					this.Expire(connection);
				}
			}

			if (result != null)
			{
				this.unlocked.Remove(result);
				this.locked.Add(result);
			}

			return result;
		}

		private void RunCleanup()
		{
			int interval = Convert.ToInt32(TimeSpan.FromTicks(this.lifeTime).TotalMilliseconds);

			if (interval > 60000)
			{
				interval = 60000;
			}

			try
			{
				while (this.isRunning)
				{
					Thread.Sleep(interval);

					this.Cleanup();

					if (this.Count == 0)
					{
						lock (this.syncObject)
						{
							// Empty pool
							if (this.EmptyPool != null)
							{
								this.EmptyPool(this.connectionString.GetHashCode(), null);
							}

							// Stop	running
							this.isRunning = false;
						}
					}
				}
			}
			catch (ThreadAbortException)
			{
				this.isRunning = false;
			}
		}

		private void Expire(FbConnectionInternal connection)
		{
			try
			{
				if (connection.Verify())
				{
					connection.Disconnect();
				}
			}
			catch (Exception)
			{
				throw new FbException("Error closing database connection.");
			}
		}

		private void Cleanup()
		{
			lock (this.unlocked.SyncRoot)
			{
				if (this.unlocked.Count > 0 && this.lifeTime != 0)
				{
					FbConnectionInternal[] list = (FbConnectionInternal[])this.unlocked.ToArray(typeof(FbConnectionInternal));

					foreach (FbConnectionInternal connection in list)
					{
						long now = DateTime.Now.Ticks;
						long expire = connection.Created + this.lifeTime;

						if (now >= expire)
						{
							if (this.CheckMinPoolSize())
							{
								this.unlocked.Remove(connection);
								this.Expire(connection);
							}
						}
					}
				}
			}
		}

		#endregion
	}
}
