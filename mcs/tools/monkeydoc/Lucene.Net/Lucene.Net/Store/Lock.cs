/* 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
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

namespace Mono.Lucene.Net.Store
{
	
	/// <summary>An interprocess mutex lock.
	/// <p/>Typical use might look like:<pre>
	/// new Lock.With(directory.makeLock("my.lock")) {
	/// public Object doBody() {
	/// <i>... code to execute while locked ...</i>
	/// }
	/// }.run();
	/// </pre>
	/// 
	/// 
	/// </summary>
	/// <version>  $Id: Lock.java 769409 2009-04-28 14:05:43Z mikemccand $
	/// </version>
	/// <seealso cref="Directory.MakeLock(String)">
	/// </seealso>
	public abstract class Lock
	{
		
		/// <summary>How long {@link #Obtain(long)} waits, in milliseconds,
		/// in between attempts to acquire the lock. 
		/// </summary>
		public static long LOCK_POLL_INTERVAL = 1000;
		
		/// <summary>Pass this value to {@link #Obtain(long)} to try
		/// forever to obtain the lock. 
		/// </summary>
		public const long LOCK_OBTAIN_WAIT_FOREVER = - 1;
		
		/// <summary>Attempts to obtain exclusive access and immediately return
		/// upon success or failure.
		/// </summary>
		/// <returns> true iff exclusive access is obtained
		/// </returns>
		public abstract bool Obtain();
		
		/// <summary> If a lock obtain called, this failureReason may be set
		/// with the "root cause" Exception as to why the lock was
		/// not obtained.
		/// </summary>
		protected internal System.Exception failureReason;
		
		/// <summary>Attempts to obtain an exclusive lock within amount of
		/// time given. Polls once per {@link #LOCK_POLL_INTERVAL}
		/// (currently 1000) milliseconds until lockWaitTimeout is
		/// passed.
		/// </summary>
		/// <param name="lockWaitTimeout">length of time to wait in
		/// milliseconds or {@link
		/// #LOCK_OBTAIN_WAIT_FOREVER} to retry forever
		/// </param>
		/// <returns> true if lock was obtained
		/// </returns>
		/// <throws>  LockObtainFailedException if lock wait times out </throws>
		/// <throws>  IllegalArgumentException if lockWaitTimeout is </throws>
		/// <summary>         out of bounds
		/// </summary>
		/// <throws>  IOException if obtain() throws IOException </throws>
		public virtual bool Obtain(long lockWaitTimeout)
		{
			failureReason = null;
			bool locked = Obtain();
			if (lockWaitTimeout < 0 && lockWaitTimeout != LOCK_OBTAIN_WAIT_FOREVER)
				throw new System.ArgumentException("lockWaitTimeout should be LOCK_OBTAIN_WAIT_FOREVER or a non-negative number (got " + lockWaitTimeout + ")");
			
			long maxSleepCount = lockWaitTimeout / LOCK_POLL_INTERVAL;
			long sleepCount = 0;
			while (!locked)
			{
				if (lockWaitTimeout != LOCK_OBTAIN_WAIT_FOREVER && sleepCount++ >= maxSleepCount)
				{
					System.String reason = "Lock obtain timed out: " + this.ToString();
					if (failureReason != null)
					{
						reason += (": " + failureReason);
					}
                    LockObtainFailedException e;
                    if (failureReason != null)
                    {
                        e = new LockObtainFailedException(reason, failureReason);
                    }
                    else
                    {
                        e = new LockObtainFailedException(reason);
                    }
                    throw e;
				}
				try
				{
					System.Threading.Thread.Sleep(new System.TimeSpan((System.Int64) 10000 * LOCK_POLL_INTERVAL));
				}
				catch (System.Threading.ThreadInterruptedException e)
				{
					// In 3.0 we will change this to throw
					// InterruptedException instead
					SupportClass.ThreadClass.Current().Interrupt();
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
			
			
			/// <summary>Constructs an executor that will grab the named lock. </summary>
			public With(Lock lock_Renamed, long lockWaitTimeout)
			{
				this.lock_Renamed = lock_Renamed;
				this.lockWaitTimeout = lockWaitTimeout;
			}
			
			/// <summary>Code to execute with exclusive access. </summary>
			protected internal abstract System.Object DoBody();
			
			/// <summary>Calls {@link #doBody} while <i>lock</i> is obtained.  Blocks if lock
			/// cannot be obtained immediately.  Retries to obtain lock once per second
			/// until it is obtained, or until it has tried ten times. Lock is released when
			/// {@link #doBody} exits.
			/// </summary>
			/// <throws>  LockObtainFailedException if lock could not </throws>
			/// <summary> be obtained
			/// </summary>
			/// <throws>  IOException if {@link Lock#obtain} throws IOException </throws>
			public virtual System.Object run()
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
