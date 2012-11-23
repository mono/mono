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

using IndexFileNameFilter = Mono.Lucene.Net.Index.IndexFileNameFilter;

namespace Mono.Lucene.Net.Store
{
	
	/// <summary>A Directory is a flat list of files.  Files may be written once, when they
	/// are created.  Once a file is created it may only be opened for read, or
	/// deleted.  Random access is permitted both when reading and writing.
	/// 
	/// <p/> Java's i/o APIs not used directly, but rather all i/o is
	/// through this API.  This permits things such as: <ul>
	/// <li> implementation of RAM-based indices;</li>
	/// <li> implementation indices stored in a database, via JDBC;</li>
	/// <li> implementation of an index as a single file;</li>
	/// </ul>
	/// 
	/// Directory locking is implemented by an instance of {@link
	/// LockFactory}, and can be changed for each Directory
	/// instance using {@link #setLockFactory}.
	/// 
	/// </summary>
	[Serializable]
	public abstract class Directory : System.IDisposable
	{
		protected internal volatile bool isOpen = true;
		
		/// <summary>Holds the LockFactory instance (implements locking for
		/// this Directory instance). 
		/// </summary>
		[NonSerialized]
		protected internal LockFactory lockFactory;
		
		/// <deprecated> For some Directory implementations ({@link
		/// FSDirectory}, and its subclasses), this method
		/// silently filters its results to include only index
		/// files.  Please use {@link #listAll} instead, which
		/// does no filtering. 
		/// </deprecated>
        [Obsolete("For some Directory implementations (FSDirectory}, and its subclasses), this method silently filters its results to include only index files.  Please use ListAll instead, which does no filtering. ")]
		public abstract System.String[] List();
		
		/// <summary>Returns an array of strings, one for each file in the
		/// directory.  Unlike {@link #list} this method does no
		/// filtering of the contents in a directory, and it will
		/// never return null (throws IOException instead).
		/// 
		/// Currently this method simply fallsback to {@link
		/// #list} for Directory impls outside of Lucene's core &amp;
		/// contrib, but in 3.0 that method will be removed and
		/// this method will become abstract. 
		/// </summary>
		public virtual System.String[] ListAll()
		{
			return List();
		}
		
		/// <summary>Returns true iff a file with the given name exists. </summary>
		public abstract bool FileExists(System.String name);
		
		/// <summary>Returns the time the named file was last modified. </summary>
		public abstract long FileModified(System.String name);
		
		/// <summary>Set the modified time of an existing file to now. </summary>
		public abstract void  TouchFile(System.String name);
		
		/// <summary>Removes an existing file in the directory. </summary>
		public abstract void  DeleteFile(System.String name);
		
		/// <summary>Renames an existing file in the directory.
		/// If a file already exists with the new name, then it is replaced.
		/// This replacement is not guaranteed to be atomic.
		/// </summary>
		/// <deprecated> 
		/// </deprecated>
        [Obsolete]
		public abstract void  RenameFile(System.String from, System.String to);
		
		/// <summary>Returns the length of a file in the directory. </summary>
		public abstract long FileLength(System.String name);
		
		
		/// <summary>Creates a new, empty file in the directory with the given name.
		/// Returns a stream writing this file. 
		/// </summary>
		public abstract IndexOutput CreateOutput(System.String name);
		
		/// <summary>Ensure that any writes to this file are moved to
		/// stable storage.  Lucene uses this to properly commit
		/// changes to the index, to prevent a machine/OS crash
		/// from corrupting the index. 
		/// </summary>
		public virtual void  Sync(System.String name)
		{
		}
		
		/// <summary>Returns a stream reading an existing file. </summary>
		public abstract IndexInput OpenInput(System.String name);
		
		/// <summary>Returns a stream reading an existing file, with the
		/// specified read buffer size.  The particular Directory
		/// implementation may ignore the buffer size.  Currently
		/// the only Directory implementations that respect this
		/// parameter are {@link FSDirectory} and {@link
		/// Mono.Lucene.Net.Index.CompoundFileReader}.
		/// </summary>
		public virtual IndexInput OpenInput(System.String name, int bufferSize)
		{
			return OpenInput(name);
		}
		
		/// <summary>Construct a {@link Lock}.</summary>
		/// <param name="name">the name of the lock file
		/// </param>
		public virtual Lock MakeLock(System.String name)
		{
			return lockFactory.MakeLock(name);
		}
		/// <summary> Attempt to clear (forcefully unlock and remove) the
		/// specified lock.  Only call this at a time when you are
		/// certain this lock is no longer in use.
		/// </summary>
		/// <param name="name">name of the lock to be cleared.
		/// </param>
		public virtual void  ClearLock(System.String name)
		{
			if (lockFactory != null)
			{
				lockFactory.ClearLock(name);
			}
		}
		
		/// <summary>Closes the store. </summary>
		public abstract void  Close();

        public abstract void Dispose();
		
		/// <summary> Set the LockFactory that this Directory instance should
		/// use for its locking implementation.  Each * instance of
		/// LockFactory should only be used for one directory (ie,
		/// do not share a single instance across multiple
		/// Directories).
		/// 
		/// </summary>
		/// <param name="lockFactory">instance of {@link LockFactory}.
		/// </param>
		public virtual void  SetLockFactory(LockFactory lockFactory)
		{
			this.lockFactory = lockFactory;
			lockFactory.SetLockPrefix(this.GetLockID());
		}
		
		/// <summary> Get the LockFactory that this Directory instance is
		/// using for its locking implementation.  Note that this
		/// may be null for Directory implementations that provide
		/// their own locking implementation.
		/// </summary>
		public virtual LockFactory GetLockFactory()
		{
			return this.lockFactory;
		}
		
		/// <summary> Return a string identifier that uniquely differentiates
		/// this Directory instance from other Directory instances.
		/// This ID should be the same if two Directory instances
		/// (even in different JVMs and/or on different machines)
		/// are considered "the same index".  This is how locking
		/// "scopes" to the right index.
		/// </summary>
		public virtual System.String GetLockID()
		{
			return this.ToString();
		}

        public override string ToString()
        {
            return base.ToString() + " lockFactory=" + GetLockFactory();
        }
		
		/// <summary> Copy contents of a directory src to a directory dest.
		/// If a file in src already exists in dest then the
		/// one in dest will be blindly overwritten.
		/// 
		/// <p/><b>NOTE:</b> the source directory cannot change
		/// while this method is running.  Otherwise the results
		/// are undefined and you could easily hit a
		/// FileNotFoundException.
		/// 
		/// <p/><b>NOTE:</b> this method only copies files that look
		/// like index files (ie, have extensions matching the
		/// known extensions of index files).
		/// 
		/// </summary>
		/// <param name="src">source directory
		/// </param>
		/// <param name="dest">destination directory
		/// </param>
		/// <param name="closeDirSrc">if <code>true</code>, call {@link #Close()} method on source directory
		/// </param>
		/// <throws>  IOException </throws>
		public static void  Copy(Directory src, Directory dest, bool closeDirSrc)
		{
			System.String[] files = src.ListAll();
			
			IndexFileNameFilter filter = IndexFileNameFilter.GetFilter();
			
			byte[] buf = new byte[BufferedIndexOutput.BUFFER_SIZE];
			for (int i = 0; i < files.Length; i++)
			{
				
				if (!filter.Accept(null, files[i]))
					continue;
				
				IndexOutput os = null;
				IndexInput is_Renamed = null;
				try
				{
					// create file in dest directory
					os = dest.CreateOutput(files[i]);
					// read current file
					is_Renamed = src.OpenInput(files[i]);
					// and copy to dest directory
					long len = is_Renamed.Length();
					long readCount = 0;
					while (readCount < len)
					{
						int toRead = readCount + BufferedIndexOutput.BUFFER_SIZE > len?(int) (len - readCount):BufferedIndexOutput.BUFFER_SIZE;
						is_Renamed.ReadBytes(buf, 0, toRead);
						os.WriteBytes(buf, toRead);
						readCount += toRead;
					}
				}
				finally
				{
					// graceful cleanup
					try
					{
						if (os != null)
							os.Close();
					}
					finally
					{
						if (is_Renamed != null)
							is_Renamed.Close();
					}
				}
			}
			if (closeDirSrc)
				src.Close();
		}
		
		/// <throws>  AlreadyClosedException if this Directory is closed </throws>
		public /*protected internal*/ void  EnsureOpen()
		{
			if (!isOpen)
				throw new AlreadyClosedException("this Directory is closed");
		}

        public bool isOpen_ForNUnit
        {
            get { return isOpen; }
        }
	}
}
