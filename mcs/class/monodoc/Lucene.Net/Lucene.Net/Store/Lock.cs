/*
 * Copyright 2004 The Apache Software Foundation
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using IndexWriter = Monodoc.Lucene.Net.Index.IndexWriter;
namespace Monodoc.Lucene.Net.Store
{
	
	/// <summary>An interprocess mutex lock.
	/// <p>Typical use might look like:<pre>
	/// new Lock.With(directory.makeLock("my.lock")) {
	/// public Object doBody() {
	/// <i>... code to execute while locked ...</i>
	/// }
	/// }.Run();
	/// </pre>
	/// 
	/// </summary>
	/// <author>  Doug Cutting
	/// </author>
	/// <version>  $Id: Lock.java,v 1.12 2004/05/11 17:43:28 cutting Exp $
	/// </version>
	/// <seealso cref="Directory#MakeLock(String)">
	/// </seealso>
	public abstract class Lock
	{
		public static long LOCK_POLL_INTERVAL = 1000;
		
		/// <summary>Attempts to obtain exclusive access and immediately return
		/// upon success or failure.
		/// </summary>
		/// <returns> true iff exclusive access is obtained
		/// </returns>
		public abstract bool Obtain();
		
		/// <summary>Attempts to obtain an exclusive lock within amount
		/// of time given. Currently polls once per second until
		/// lockWaitTimeout is passed.
		/// </summary>
		/// <param name="lockWaitTimeout">length of time to wait in ms
		/// </param>
		/// <returns> true if lock was obtained
		/// </returns>
		/// <throws>  IOException if lock wait times out or obtain() throws an IOException </throws>
		public virtual bool Obtain(long lockWaitTimeout)
		{
			bool locked = Obtain();
			int maxSleepCount = (int) (lockWaitTimeout / LOCK_POLL_INTERVAL);
			int sleepCount = 0;
			while (!locked)
			{
				if (++sleepCount == maxSleepCount)
				{
					throw new System.IO.IOException("Lock obtain timed out: " + this.ToString());
				}
				try
				{
					System.Threading.Thread.Sleep(new System.TimeSpan((System.Int64) 10000 * LOCK_POLL_INTERVAL));
				}
				catch (System.Threading.ThreadInterruptedException e)
				{
					throw new System.IO.IOException(e.ToString());
				}
				locked = Obtain();
			}
			return locked;
		}
		
		/// <summary>Releases exclusive access. </summary>
		public abstract void  Release();
		
		/// <summary>Returns true if the resource is currently locked.  Note that one must
		/// still call {@link #Obtain()} before using the resource. 
		/// </summary>
		public abstract bool IsLocked();
		
		
		/// <summary>Utility class for executing code with exclusive access. </summary>
		public abstract class With
		{
			private Lock lock_Renamed;
			private long lockWaitTimeout;
			
			/// <summary>Constructs an executor that will grab the named lock.
			/// Defaults lockWaitTimeout to Lock.COMMIT_LOCK_TIMEOUT.
			/// </summary>
			/// <deprecated> Kept only to avoid breaking existing code.
			/// </deprecated>
			public With(Lock lock_Renamed):this(lock_Renamed, IndexWriter.COMMIT_LOCK_TIMEOUT)
			{
			}
			
			/// <summary>Constructs an executor that will grab the named lock. </summary>
			public With(Lock lock_Renamed, long lockWaitTimeout)
			{
				this.lock_Renamed = lock_Renamed;
				this.lockWaitTimeout = lockWaitTimeout;
			}
			
			/// <summary>Code to execute with exclusive access. </summary>
			public abstract System.Object DoBody();
			
			/// <summary>Calls {@link #doBody} while <i>lock</i> is obtained.  Blocks if lock
			/// cannot be obtained immediately.  Retries to obtain lock once per second
			/// until it is obtained, or until it has tried ten times. Lock is released when
			/// {@link #doBody} exits. 
			/// </summary>
			public virtual System.Object Run()
			{
				bool locked = false;
				try
				{
					locked = lock_Renamed.Obtain(lockWaitTimeout);
					return DoBody();
				}
				finally
				{
					if (locked)
						lock_Renamed.Release();
				}
			}
		}
	}
}