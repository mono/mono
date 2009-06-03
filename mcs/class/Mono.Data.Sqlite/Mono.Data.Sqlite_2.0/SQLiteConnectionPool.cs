/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace Mono.Data.Sqlite
{
  using System;
  using System.Collections.Generic;

  internal static class SqliteConnectionPool
  {
    /// <summary>
    /// Keeps track of connections made on a specified file.  The PoolVersion dictates whether old objects get
    /// returned to the pool or discarded when no longer in use.
    /// </summary>
    internal class Pool
    {
      internal readonly Queue<WeakReference> Queue = new Queue<WeakReference>();
      internal int PoolVersion;
      internal int MaxPoolSize;

      internal Pool(int version, int maxSize)
      {
        PoolVersion = version;
        MaxPoolSize = maxSize;
      }
    }

    /// <summary>
    /// The connection pool object
    /// </summary>
    private static SortedList<string, Pool> _connections = new SortedList<string, Pool>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The default version number new pools will get
    /// </summary>
    private static int _poolVersion = 1;

    /// <summary>
    /// Attempt to pull a pooled connection out of the queue for active duty
    /// </summary>
    /// <param name="fileName">The filename for a desired connection</param>
    /// <param name="maxPoolSize">The maximum size the connection pool for the filename can be</param>
    /// <param name="version">The pool version the returned connection will belong to</param>
    /// <returns>Returns NULL if no connections were available.  Even if none are, the poolversion will still be a valid pool version</returns>
    internal static SqliteConnectionHandle Remove(string fileName, int maxPoolSize, out int version)
    {
      lock (_connections)
      {
        Pool queue;

        // Default to the highest pool version
        version = _poolVersion;

        // If we didn't find a pool for this file, create one even though it will be empty.
        // We have to do this here because otherwise calling ClearPool() on the file will not work for active connections
        // that have never seen the pool yet.
        if (_connections.TryGetValue(fileName, out queue) == false)
        {
          queue = new Pool(_poolVersion, maxPoolSize);
          _connections.Add(fileName, queue);

          return null;
        }

        // We found a pool for this file, so use its version number
        version = queue.PoolVersion;
        queue.MaxPoolSize = maxPoolSize;

        ResizePool(queue, false);

        // Try and get a pooled connection from the queue
        while (queue.Queue.Count > 0)
        {
          WeakReference cnn = queue.Queue.Dequeue();
          SqliteConnectionHandle hdl = cnn.Target as SqliteConnectionHandle;
          if (hdl != null)
          {
            return hdl;
          }
        }
        return null;
      }
    }

    /// <summary>
    /// Clears out all pooled connections and rev's up the default pool version to force all old active objects
    /// not in the pool to get discarded rather than returned to their pools.
    /// </summary>
    internal static void ClearAllPools()
    {
      lock (_connections)
      {
        foreach (KeyValuePair<string, Pool> pair in _connections)
        {
          while (pair.Value.Queue.Count > 0)
          {
            WeakReference cnn = pair.Value.Queue.Dequeue();
            SqliteConnectionHandle hdl = cnn.Target as SqliteConnectionHandle;
            if (hdl != null)
            {
              hdl.Dispose();
            }
          }
          
          // Keep track of the highest revision so we can go one higher when we're finished
          if (_poolVersion <= pair.Value.PoolVersion)
            _poolVersion = pair.Value.PoolVersion + 1;
        }
        // All pools are cleared and we have a new highest version number to force all old version active items to get discarded
        // instead of going back to the queue when they are closed.
        // We can get away with this because we're pumped up the _poolVersion out of range of all active connections, so they
        // will all get discarded when they try to put themselves back in their pool.
        _connections.Clear();
      }
    }

    /// <summary>
    /// Clear a given pool for a given filename.  Discards anything in the pool for the given file, and revs the pool
    /// version so current active objects on the old version of the pool will get discarded rather than be returned to the pool.
    /// </summary>
    /// <param name="fileName">The filename of the pool to clear</param>
    internal static void ClearPool(string fileName)
    {
      lock (_connections)
      {
        Pool queue;
        if (_connections.TryGetValue(fileName, out queue) == true)
        {
          queue.PoolVersion++;
          while (queue.Queue.Count > 0)
          {
            WeakReference cnn = queue.Queue.Dequeue();
            SqliteConnectionHandle hdl = cnn.Target as SqliteConnectionHandle;
            if (hdl != null)
            {
              hdl.Dispose();
            }
          }
        }
      }
    }

    /// <summary>
    /// Return a connection to the pool for someone else to use.
    /// </summary>
    /// <param name="fileName">The filename of the pool to use</param>
    /// <param name="hdl">The connection handle to pool</param>
    /// <param name="version">The pool version the handle was created under</param>
    /// <remarks>
    /// If the version numbers don't match between the connection and the pool, then the handle is discarded.
    /// </remarks>
    internal static void Add(string fileName, SqliteConnectionHandle hdl, int version)
    {
      lock (_connections)
      {
        // If the queue doesn't exist in the pool, then it must've been cleared sometime after the connection was created.
        Pool queue;
        if (_connections.TryGetValue(fileName, out queue) == true && version == queue.PoolVersion)
        {
          ResizePool(queue, true);
          queue.Queue.Enqueue(new WeakReference(hdl, false));
          GC.KeepAlive(hdl);
        }
        else
        {
          hdl.Close();
        }
      }
    }

    private static void ResizePool(Pool queue, bool forAdding)
    {
      int target = queue.MaxPoolSize;

      if (forAdding && target > 0) target--;

      while (queue.Queue.Count > target)
      {
        WeakReference cnn = queue.Queue.Dequeue();
        SqliteConnectionHandle hdl = cnn.Target as SqliteConnectionHandle;
        if (hdl != null)
        {
          hdl.Dispose();
        }
      }
    }
  }
}
