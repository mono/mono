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
	
	/// <summary> Implements {@link LockFactory} for a single in-process instance,
	/// meaning all locking will take place through this one instance.
	/// Only use this {@link LockFactory} when you are certain all
	/// IndexReaders and IndexWriters for a given index are running
	/// against a single shared in-process Directory instance.  This is
	/// currently the default locking for RAMDirectory.
	/// 
	/// </summary>
	/// <seealso cref="LockFactory">
	/// </seealso>
	
	public class SingleInstanceLockFactory:LockFactory
	{

        private System.Collections.Hashtable locks = new System.Collections.Hashtable();
		
		public override Lock MakeLock(System.String lockName)
		{
			// We do not use the LockPrefix at all, because the private
			// HashSet instance effectively scopes the locking to this
			// single Directory instance.
			return new SingleInstanceLock(locks, lockName);
		}
		
		public override void  ClearLock(System.String lockName)
		{
			lock (locks)
			{
				if (locks.Contains(lockName))
				{
					locks.Remove(lockName);
				}
			}
		}
	}
	
	
	class SingleInstanceLock:Lock
	{
		
		internal System.String lockName;
		private System.Collections.Hashtable locks;
		
		public SingleInstanceLock(System.Collections.Hashtable locks, System.String lockName)
		{
			this.locks = locks;
			this.lockName = lockName;
		}
		
		public override bool Obtain()
		{
			lock (locks)
			{
                if (locks.Contains(lockName) == false)
                {
                    locks.Add(lockName, lockName);
                    return true;
                }

                return false;
			}
		}
		
		public override void  Release()
		{
			lock (locks)
			{
				locks.Remove(lockName);
			}
		}
		
		public override bool IsLocked()
		{
			lock (locks)
			{
				return locks.Contains(lockName);
			}
		}
		
		public override System.String ToString()
		{
			return base.ToString() + ": " + lockName;
		}
	}
}
