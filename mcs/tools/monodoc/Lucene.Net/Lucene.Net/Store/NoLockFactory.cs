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
	
	/// <summary> Use this {@link LockFactory} to disable locking entirely.
	/// This LockFactory is used when you call {@link FSDirectory#setDisableLocks}.
	/// Only one instance of this lock is created.  You should call {@link
	/// #GetNoLockFactory()} to get the instance.
	/// 
	/// </summary>
	/// <seealso cref="LockFactory">
	/// </seealso>
	
	public class NoLockFactory:LockFactory
	{
		
		// Single instance returned whenever makeLock is called.
		private static NoLock singletonLock = new NoLock();
		private static NoLockFactory singleton = new NoLockFactory();
		
		public static NoLockFactory GetNoLockFactory()
		{
			return singleton;
		}
		
		public override Lock MakeLock(System.String lockName)
		{
			return singletonLock;
		}
		
		public override void  ClearLock(System.String lockName)
		{
		}
		
	}
	
	
	class NoLock:Lock
	{
		public override bool Obtain()
		{
			return true;
		}
		
		public override void  Release()
		{
		}
		
		public override bool IsLocked()
		{
			return false;
		}
		
		public override System.String ToString()
		{
			return "NoLock";
		}
	}
}
