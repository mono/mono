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
	
	/// <summary> A memory-resident {@link Directory} implementation.  Locking
	/// implementation is by default the {@link SingleInstanceLockFactory}
	/// but can be changed with {@link #setLockFactory}.
	/// 
	/// </summary>
	/// <version>  $Id: RAMDirectory.java 781333 2009-06-03 10:38:57Z mikemccand $
	/// </version>
	[Serializable]
	public class RAMDirectory:Directory
	{
		
		private const long serialVersionUID = 1L;
		
		internal protected System.Collections.Hashtable fileMap = new System.Collections.Hashtable();
		internal protected long sizeInBytes = 0;
		
		// *****
		// Lock acquisition sequence:  RAMDirectory, then RAMFile
		// *****
		
		/// <summary>Constructs an empty {@link Directory}. </summary>
		public RAMDirectory()
		{
			SetLockFactory(new SingleInstanceLockFactory());
		}
		
		/// <summary> Creates a new <code>RAMDirectory</code> instance from a different
		/// <code>Directory</code> implementation.  This can be used to load
		/// a disk-based index into memory.
		/// <p/>
		/// This should be used only with indices that can fit into memory.
		/// <p/>
		/// Note that the resulting <code>RAMDirectory</code> instance is fully
		/// independent from the original <code>Directory</code> (it is a
		/// complete copy).  Any subsequent changes to the
		/// original <code>Directory</code> will not be visible in the
		/// <code>RAMDirectory</code> instance.
		/// 
		/// </summary>
		/// <param name="dir">a <code>Directory</code> value
		/// </param>
		/// <exception cref="IOException">if an error occurs
		/// </exception>
		public RAMDirectory(Directory dir):this(dir, false)
		{
		}
		
		private RAMDirectory(Directory dir, bool closeDir):this()
		{
			Directory.Copy(dir, this, closeDir);
		}
		
		/// <summary> Creates a new <code>RAMDirectory</code> instance from the {@link FSDirectory}.
		/// 
		/// </summary>
		/// <param name="dir">a <code>File</code> specifying the index directory
		/// 
		/// </param>
		/// <seealso cref="RAMDirectory(Directory)">
		/// </seealso>
		/// <deprecated> Use {@link #RAMDirectory(Directory)} instead
		/// </deprecated>
        [Obsolete("Use RAMDirectory(Directory) instead")]
		public RAMDirectory(System.IO.FileInfo dir):this(FSDirectory.GetDirectory(dir), true)
		{
		}
		
		/// <summary> Creates a new <code>RAMDirectory</code> instance from the {@link FSDirectory}.
		/// 
		/// </summary>
		/// <param name="dir">a <code>String</code> specifying the full index directory path
		/// 
		/// </param>
		/// <seealso cref="RAMDirectory(Directory)">
		/// </seealso>
		/// <deprecated> Use {@link #RAMDirectory(Directory)} instead
		/// </deprecated>
        [Obsolete("Use RAMDirectory(Directory) instead")]
		public RAMDirectory(System.String dir):this(FSDirectory.GetDirectory(dir), true)
		{
		}

         //https://issues.apache.org/jira/browse/LUCENENET-174
        [System.Runtime.Serialization.OnDeserialized]
        void OnDeserialized(System.Runtime.Serialization.StreamingContext context)
        {
            if (lockFactory == null)
            {
                SetLockFactory(new SingleInstanceLockFactory());
            }
        }

        [Obsolete("Mono.Lucene.Net-2.9.1. This method overrides obsolete member Mono.Lucene.Net.Store.Directory.List()")]
		public override System.String[] List()
		{
			lock (this)
			{
				return ListAll();
			}
		}
		
		public override System.String[] ListAll()
		{
			lock (this)
			{
				EnsureOpen();
				System.Collections.ICollection fileNames = fileMap.Keys;
				System.String[] result = new System.String[fileNames.Count];
				int i = 0;
				System.Collections.IEnumerator it = fileNames.GetEnumerator();
				while (it.MoveNext())
				{
					result[i++] = ((System.String) it.Current);
				}
				return result;
			}
		}
		
		/// <summary>Returns true iff the named file exists in this directory. </summary>
		public override bool FileExists(System.String name)
		{
			EnsureOpen();
			RAMFile file;
			lock (this)
			{
				file = (RAMFile) fileMap[name];
			}
			return file != null;
		}
		
		/// <summary>Returns the time the named file was last modified.</summary>
		/// <throws>  IOException if the file does not exist </throws>
		public override long FileModified(System.String name)
		{
			EnsureOpen();
			RAMFile file;
			lock (this)
			{
				file = (RAMFile) fileMap[name];
			}
			if (file == null)
				throw new System.IO.FileNotFoundException(name);
			return file.GetLastModified();
		}
		
		/// <summary>Set the modified time of an existing file to now.</summary>
		/// <throws>  IOException if the file does not exist </throws>
		public override void  TouchFile(System.String name)
		{
			EnsureOpen();
			RAMFile file;
			lock (this)
			{
				file = (RAMFile) fileMap[name];
			}
			if (file == null)
				throw new System.IO.FileNotFoundException(name);
			
			long ts2, ts1 = System.DateTime.Now.Ticks;
			do 
			{
				try
				{
					System.Threading.Thread.Sleep(new System.TimeSpan((System.Int64) 10000 * 0 + 100 * 1));
				}
				catch (System.Threading.ThreadInterruptedException ie)
				{
					// In 3.0 we will change this to throw
					// InterruptedException instead
					SupportClass.ThreadClass.Current().Interrupt();
					throw new System.SystemException(ie.Message, ie);
				}
				ts2 = System.DateTime.Now.Ticks;
			}
			while (ts1 == ts2);
			
			file.SetLastModified(ts2);
		}
		
		/// <summary>Returns the length in bytes of a file in the directory.</summary>
		/// <throws>  IOException if the file does not exist </throws>
		public override long FileLength(System.String name)
		{
			EnsureOpen();
			RAMFile file;
			lock (this)
			{
				file = (RAMFile) fileMap[name];
			}
			if (file == null)
				throw new System.IO.FileNotFoundException(name);
			return file.GetLength();
		}
		
		/// <summary>Return total size in bytes of all files in this
		/// directory.  This is currently quantized to
		/// RAMOutputStream.BUFFER_SIZE. 
		/// </summary>
		public long SizeInBytes()
		{
			lock (this)
			{
				EnsureOpen();
				return sizeInBytes;
			}
		}
		
		/// <summary>Removes an existing file in the directory.</summary>
		/// <throws>  IOException if the file does not exist </throws>
		public override void  DeleteFile(System.String name)
		{
			lock (this)
			{
				EnsureOpen();
				RAMFile file = (RAMFile) fileMap[name];
				if (file != null)
				{
					fileMap.Remove(name);
					file.directory = null;
					sizeInBytes -= file.sizeInBytes; 
				}
				else
					throw new System.IO.FileNotFoundException(name);
			}
		}
		
		/// <summary>Renames an existing file in the directory.</summary>
		/// <throws>  FileNotFoundException if from does not exist </throws>
		/// <deprecated>
		/// </deprecated>
        [Obsolete]
		public override void  RenameFile(System.String from, System.String to)
		{
			lock (this)
			{
				EnsureOpen();
				RAMFile fromFile = (RAMFile) fileMap[from];
				if (fromFile == null)
					throw new System.IO.FileNotFoundException(from);
				RAMFile toFile = (RAMFile) fileMap[to];
				if (toFile != null)
				{
					sizeInBytes -= toFile.sizeInBytes; // updates to RAMFile.sizeInBytes synchronized on directory
					toFile.directory = null;
				}
				fileMap.Remove(from);
				fileMap[to] = fromFile;
			}
		}
		
		/// <summary>Creates a new, empty file in the directory with the given name. Returns a stream writing this file. </summary>
		public override IndexOutput CreateOutput(System.String name)
		{
			EnsureOpen();
			RAMFile file = new RAMFile(this);
			lock (this)
			{
				RAMFile existing = (RAMFile) fileMap[name];
				if (existing != null)
				{
					sizeInBytes -= existing.sizeInBytes;
					existing.directory = null;
				}
				fileMap[name] = file;
			}
			return new RAMOutputStream(file);
		}
		
		/// <summary>Returns a stream reading an existing file. </summary>
		public override IndexInput OpenInput(System.String name)
		{
			EnsureOpen();
			RAMFile file;
			lock (this)
			{
				file = (RAMFile) fileMap[name];
			}
			if (file == null)
				throw new System.IO.FileNotFoundException(name);
			return new RAMInputStream(file);
		}
		
		/// <summary>Closes the store to future operations, releasing associated memory. </summary>
		public override void  Close()
		{
			isOpen = false;
			fileMap = null;
		}

        /// <summary>
        /// .NET
        /// </summary>
        public override void Dispose()
        {
            Close();
        }

        public System.Collections.Hashtable fileMap_ForNUnit
        {
            get { return fileMap; }
        }

        public long sizeInBytes_ForNUnitTest
        {
            get { return sizeInBytes; }
            set { sizeInBytes = value; }
        }
	}
}
