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
	
	/// <summary> A {@link LockFactory} that wraps another {@link
	/// LockFactory} and verifies that each lock obtain/release
	/// is "correct" (never results in two processes holding the
	/// lock at the same time).  It does this by contacting an
	/// external server ({@link LockVerifyServer}) to assert that
	/// at most one process holds the lock at a time.  To use
	/// this, you should also run {@link LockVerifyServer} on the
	/// host &amp; port matching what you pass to the constructor.
	/// 
	/// </summary>
	/// <seealso cref="LockVerifyServer">
	/// </seealso>
	/// <seealso cref="LockStressTest">
	/// </seealso>
	
	public class VerifyingLockFactory:LockFactory
	{
		
		internal LockFactory lf;
		internal sbyte id;
		internal System.String host;
		internal int port;
		
		private class CheckedLock:Lock
		{
			private void  InitBlock(VerifyingLockFactory enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private VerifyingLockFactory enclosingInstance;
			public VerifyingLockFactory Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			private Lock lock_Renamed;
			
			public CheckedLock(VerifyingLockFactory enclosingInstance, Lock lock_Renamed)
			{
				InitBlock(enclosingInstance);
				this.lock_Renamed = lock_Renamed;
			}
			
			private void  Verify(sbyte message)
			{
				try
				{
					System.Net.Sockets.TcpClient s = new System.Net.Sockets.TcpClient(Enclosing_Instance.host, Enclosing_Instance.port);
					System.IO.Stream out_Renamed = s.GetStream();
					out_Renamed.WriteByte((byte) Enclosing_Instance.id);
					out_Renamed.WriteByte((byte) message);
					System.IO.Stream in_Renamed = s.GetStream();
					int result = in_Renamed.ReadByte();
					in_Renamed.Close();
					out_Renamed.Close();
					s.Close();
					if (result != 0)
						throw new System.SystemException("lock was double acquired");
				}
				catch (System.Exception e)
				{
					throw new System.SystemException(e.Message, e);
				}
			}
			
			public override bool Obtain(long lockWaitTimeout)
			{
				lock (this)
				{
					bool obtained = lock_Renamed.Obtain(lockWaitTimeout);
					if (obtained)
						Verify((sbyte) 1);
					return obtained;
				}
			}
			
			public override bool Obtain()
			{
				lock (this)
				{
					return lock_Renamed.Obtain();
				}
			}
			
			public override bool IsLocked()
			{
				lock (this)
				{
					return lock_Renamed.IsLocked();
				}
			}
			
			public override void  Release()
			{
				lock (this)
				{
					if (IsLocked())
					{
						Verify((sbyte) 0);
						lock_Renamed.Release();
					}
				}
			}
		}
		
		/// <param name="id">should be a unique id across all clients
		/// </param>
		/// <param name="lf">the LockFactory that we are testing
		/// </param>
		/// <param name="host">host or IP where {@link LockVerifyServer}
		/// is running
		/// </param>
		/// <param name="port">the port {@link LockVerifyServer} is
		/// listening on
		/// </param>
		public VerifyingLockFactory(sbyte id, LockFactory lf, System.String host, int port)
		{
			this.id = id;
			this.lf = lf;
			this.host = host;
			this.port = port;
		}
		
		public override Lock MakeLock(System.String lockName)
		{
			lock (this)
			{
				return new CheckedLock(this, lf.MakeLock(lockName));
			}
		}
		
		public override void  ClearLock(System.String lockName)
		{
			lock (this)
			{
				lf.ClearLock(lockName);
			}
		}
	}
}
