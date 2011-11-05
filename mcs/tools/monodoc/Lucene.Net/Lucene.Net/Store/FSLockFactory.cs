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
	
	/// <summary> Base class for file system based locking implementation.</summary>
	
	public abstract class FSLockFactory:LockFactory
	{
		
		/// <summary> Directory for the lock files.</summary>
		protected internal System.IO.DirectoryInfo lockDir = null;
		
		/// <summary> Set the lock directory. This method can be only called
		/// once to initialize the lock directory. It is used by {@link FSDirectory}
		/// to set the lock directory to itsself.
		/// Subclasses can also use this method to set the directory
		/// in the constructor.
		/// </summary>
		protected internal virtual void  SetLockDir(System.IO.DirectoryInfo lockDir)
		{
			if (this.lockDir != null)
				throw new System.SystemException("You can set the lock directory for this factory only once.");
			this.lockDir = lockDir;
		}
		
		/// <summary> Retrieve the lock directory.</summary>
		public virtual System.IO.DirectoryInfo GetLockDir()
		{
			return lockDir;
		}
	}
}
