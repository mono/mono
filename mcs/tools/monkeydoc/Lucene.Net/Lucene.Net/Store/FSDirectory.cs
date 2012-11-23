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
using System.Collections.Generic;

// Used only for WRITE_LOCK_NAME in deprecated create=true case:
using IndexFileNameFilter = Mono.Lucene.Net.Index.IndexFileNameFilter;
using IndexWriter = Mono.Lucene.Net.Index.IndexWriter;
using Constants = Mono.Lucene.Net.Util.Constants;

namespace Mono.Lucene.Net.Store
{
	
	/// <summary> <a name="subclasses"/>
	/// Base class for Directory implementations that store index
	/// files in the file system.  There are currently three core
	/// subclasses:
	/// 
	/// <ul>
	/// 
	/// <li> {@link SimpleFSDirectory} is a straightforward
	/// implementation using java.io.RandomAccessFile.
	/// However, it has poor concurrent performance
	/// (multiple threads will bottleneck) as it
	/// synchronizes when multiple threads read from the
	/// same file.</li>
	/// 
	/// <li> {@link NIOFSDirectory} uses java.nio's
	/// FileChannel's positional io when reading to avoid
	/// synchronization when reading from the same file.
	/// Unfortunately, due to a Windows-only <a
	/// href="http://bugs.sun.com/bugdatabase/view_bug.do?bug_id=6265734">Sun
	/// JRE bug</a> this is a poor choice for Windows, but
	/// on all other platforms this is the preferred
	/// choice. Applications using {@link Thread#interrupt()} or
    /// <code>Future#cancel(boolean)</code> (on Java 1.5) should use
    /// {@link SimpleFSDirectory} instead. See {@link NIOFSDirectory} java doc
    /// for details.
    ///        
    ///        
	/// 
	/// <li> {@link MMapDirectory} uses memory-mapped IO when
	/// reading. This is a good choice if you have plenty
	/// of virtual memory relative to your index size, eg
	/// if you are running on a 64 bit JRE, or you are
	/// running on a 32 bit JRE but your index sizes are
	/// small enough to fit into the virtual memory space.
	/// Java has currently the limitation of not being able to
	/// unmap files from user code. The files are unmapped, when GC
	/// releases the byte buffers. Due to
	/// <a href="http://bugs.sun.com/bugdatabase/view_bug.do?bug_id=4724038">
	/// this bug</a> in Sun's JRE, MMapDirectory's {@link IndexInput#close}
	/// is unable to close the underlying OS file handle. Only when
	/// GC finally collects the underlying objects, which could be
	/// quite some time later, will the file handle be closed.
	/// This will consume additional transient disk usage: on Windows,
	/// attempts to delete or overwrite the files will result in an
	/// exception; on other platforms, which typically have a &quot;delete on
	/// last close&quot; semantics, while such operations will succeed, the bytes
	/// are still consuming space on disk.  For many applications this
	/// limitation is not a problem (e.g. if you have plenty of disk space,
	/// and you don't rely on overwriting files on Windows) but it's still
	/// an important limitation to be aware of. This class supplies a
	/// (possibly dangerous) workaround mentioned in the bug report,
	/// which may fail on non-Sun JVMs.</li>
    ///       
    /// Applications using {@link Thread#interrupt()} or
    /// <code>Future#cancel(boolean)</code> (on Java 1.5) should use
    /// {@link SimpleFSDirectory} instead. See {@link MMapDirectory}
    /// java doc for details.
	/// </ul>
	/// 
	/// Unfortunately, because of system peculiarities, there is
	/// no single overall best implementation.  Therefore, we've
	/// added the {@link #open} method, to allow Lucene to choose
	/// the best FSDirectory implementation given your
	/// environment, and the known limitations of each
	/// implementation.  For users who have no reason to prefer a
	/// specific implementation, it's best to simply use {@link
	/// #open}.  For all others, you should instantiate the
	/// desired implementation directly.
	/// 
	/// <p/>The locking implementation is by default {@link
	/// NativeFSLockFactory}, but can be changed by
	/// passing in a custom {@link LockFactory} instance.
	/// The deprecated <code>getDirectory</code> methods default to use
	/// {@link SimpleFSLockFactory} for backwards compatibility.
	/// The system properties 
	/// <code>org.apache.lucene.store.FSDirectoryLockFactoryClass</code>
	/// and <code>org.apache.lucene.FSDirectory.class</code>
	/// are deprecated and only used by the deprecated
	/// <code>getDirectory</code> methods. The system property
	/// <code>org.apache.lucene.lockDir</code> is ignored completely,
	/// If you really want to store locks
	/// elsewhere, you can create your own {@link
	/// SimpleFSLockFactory} (or {@link NativeFSLockFactory},
	/// etc.) passing in your preferred lock directory.
	/// 
	/// <p/><em>In 3.0 this class will become abstract.</em>
	/// 
	/// </summary>
	/// <seealso cref="Directory">
	/// </seealso>
	// TODO: in 3.0 this will become an abstract base class
	public class FSDirectory:Directory
	{
		
		/// <summary>This cache of directories ensures that there is a unique Directory
		/// instance per path, so that synchronization on the Directory can be used to
		/// synchronize access between readers and writers.  We use
		/// refcounts to ensure when the last use of an FSDirectory
		/// instance for a given canonical path is closed, we remove the
		/// instance from the cache.  See LUCENE-776
		/// for some relevant discussion.
		/// </summary>
		/// <deprecated> Not used by any non-deprecated methods anymore
		/// </deprecated>
        [Obsolete("Not used by any non-deprecated methods anymore")]
        private static readonly Dictionary<string, FSDirectory> DIRECTORIES = new Dictionary<string, FSDirectory>();
		
		private static bool disableLocks = false;
		
		// TODO: should this move up to the Directory base class?  Also: should we
		// make a per-instance (in addition to the static "default") version?
		
		/// <summary> Set whether Lucene's use of lock files is disabled. By default, 
		/// lock files are enabled. They should only be disabled if the index
		/// is on a read-only medium like a CD-ROM.
		/// </summary>
		/// <deprecated> Use a {@link #open(File, LockFactory)} or a constructor
		/// that takes a {@link LockFactory} and supply
		/// {@link NoLockFactory#getNoLockFactory}. This setting does not work
		/// with {@link #open(File)} only the deprecated <code>getDirectory</code>
		/// respect this setting.   
		/// </deprecated>
        [Obsolete("Use a Open(File, LockFactory) or a constructor that takes a LockFactory and supply NoLockFactory.GetNoLockFactory. This setting does not work with Open(File) only the deprecated GetDirectory respect this setting.")]
		public static void  SetDisableLocks(bool doDisableLocks)
		{
			FSDirectory.disableLocks = doDisableLocks;
		}
		
		/// <summary> Returns whether Lucene's use of lock files is disabled.</summary>
		/// <returns> true if locks are disabled, false if locks are enabled.
		/// </returns>
		/// <seealso cref="setDisableLocks">
		/// </seealso>
		/// <deprecated> Use a constructor that takes a {@link LockFactory} and
		/// supply {@link NoLockFactory#getNoLockFactory}.
		/// </deprecated>
        [Obsolete("Use a constructor that takes a LockFactory and supply NoLockFactory.GetNoLockFactory.")]
		public static bool GetDisableLocks()
		{
			return FSDirectory.disableLocks;
		}
		
		/// <summary> Directory specified by <code>org.apache.lucene.lockDir</code>
		/// or <code>java.io.tmpdir</code> system property.
		/// </summary>
		/// <deprecated> As of 2.1, <code>LOCK_DIR</code> is unused
		/// because the write.lock is now stored by default in the
		/// index directory.  If you really want to store locks
		/// elsewhere, you can create your own {@link
		/// SimpleFSLockFactory} (or {@link NativeFSLockFactory},
		/// etc.) passing in your preferred lock directory.  Then,
		/// pass this <code>LockFactory</code> instance to one of
		/// the <code>open</code> methods that take a
		/// <code>lockFactory</code> (for example, {@link #open(File, LockFactory)}).
		/// </deprecated>
        //[Obsolete("As of 2.1, LOCK_DIR is unused because the write.lock is now stored by default in the index directory. ")]
		//public static readonly System.String LOCK_DIR = SupportClass.AppSettings.Get("Mono.Lucene.Net.lockDir", System.IO.Path.GetTempPath());
		
		/// <summary>The default class which implements filesystem-based directories. </summary>
		// deprecated
        [Obsolete]
        private static readonly System.Type IMPL = typeof(Mono.Lucene.Net.Store.SimpleFSDirectory);
		
		private static System.Security.Cryptography.HashAlgorithm DIGESTER;
		
		/// <summary>A buffer optionally used in renameTo method </summary>
		private byte[] buffer = null;
		
		
		/// <summary>Returns the directory instance for the named location.
		/// 
		/// </summary>
		/// <deprecated> Use {@link #Open(File)}
		/// 
		/// </deprecated>
		/// <param name="path">the path to the directory.
		/// </param>
		/// <returns> the FSDirectory for the named file.  
		/// </returns>
        [Obsolete("Use Open(File)")]
		public static FSDirectory GetDirectory(System.String path)
		{
			return GetDirectory(new System.IO.DirectoryInfo(path), null);
		}
		
		/// <summary>Returns the directory instance for the named location.
		/// 
		/// </summary>
		/// <deprecated> Use {@link #Open(File, LockFactory)}
		/// 
		/// </deprecated>
		/// <param name="path">the path to the directory.
		/// </param>
		/// <param name="lockFactory">instance of {@link LockFactory} providing the
		/// locking implementation.
		/// </param>
		/// <returns> the FSDirectory for the named file.  
		/// </returns>
        [Obsolete("Use Open(File, LockFactory)")]
		public static FSDirectory GetDirectory(System.String path, LockFactory lockFactory)
		{
			return GetDirectory(new System.IO.DirectoryInfo(path), lockFactory);
		}
		
		/// <summary>Returns the directory instance for the named location.
		/// 
		/// </summary>
		/// <deprecated> Use {@link #Open(File)}
		/// 
		/// </deprecated>
		/// <param name="file">the path to the directory.
		/// </param>
		/// <returns> the FSDirectory for the named file.  
		/// </returns>
        [Obsolete("Use Open(File)")]
		public static FSDirectory GetDirectory(System.IO.DirectoryInfo file)
		{
			return GetDirectory(file, null);
		}

        /// <summary>Returns the directory instance for the named location.
        /// 
        /// </summary>
        /// <deprecated> Use {@link #Open(File)}
        /// 
        /// </deprecated>
        /// <param name="file">the path to the directory.
        /// </param>
        /// <returns> the FSDirectory for the named file.  
        /// </returns>
        [System.Obsolete("Use the constructor that takes a DirectoryInfo, this will be removed in the 3.0 release")]
        public static FSDirectory GetDirectory(System.IO.FileInfo file)
        {
            return GetDirectory(new System.IO.DirectoryInfo(file.FullName), null);
        }
		
		/// <summary>Returns the directory instance for the named location.
		/// 
		/// </summary>
		/// <deprecated> Use {@link #Open(File, LockFactory)}
		/// 
		/// </deprecated>
		/// <param name="file">the path to the directory.
		/// </param>
		/// <param name="lockFactory">instance of {@link LockFactory} providing the
		/// locking implementation.
		/// </param>
		/// <returns> the FSDirectory for the named file.  
		/// </returns>
		[System.Obsolete("Use the constructor that takes a DirectoryInfo, this will be removed in the 3.0 release")]
		public static FSDirectory GetDirectory(System.IO.FileInfo file, LockFactory lockFactory)
		{
            return GetDirectory(new System.IO.DirectoryInfo(file.FullName), lockFactory);
		}

        /// <summary>Returns the directory instance for the named location.
        /// 
        /// </summary>
        /// <deprecated> Use {@link #Open(File, LockFactory)}
        /// 
        /// </deprecated>
        /// <param name="file">the path to the directory.
        /// </param>
        /// <param name="lockFactory">instance of {@link LockFactory} providing the
        /// locking implementation.
        /// </param>
        /// <returns> the FSDirectory for the named file.  
        /// </returns>
        [Obsolete("Use Open(File, LockFactory)")]
        public static FSDirectory GetDirectory(System.IO.DirectoryInfo file, LockFactory lockFactory)
        {
            FSDirectory dir;
            lock (DIRECTORIES)
            {
                if(!DIRECTORIES.TryGetValue(file.FullName, out dir))
                {
                    try
                    {
                        dir = (FSDirectory)System.Activator.CreateInstance(IMPL, true);
                    }
                    catch (System.Exception e)
                    {
                        throw new System.SystemException("cannot load FSDirectory class: " + e.ToString(), e);
                    }
                    dir.Init(file, lockFactory);
                    DIRECTORIES.Add(file.FullName, dir);
                }
                else
                {
                    // Catch the case where a Directory is pulled from the cache, but has a
                    // different LockFactory instance.
                    if (lockFactory != null && lockFactory != dir.GetLockFactory())
                    {
                        throw new System.IO.IOException("Directory was previously created with a different LockFactory instance; please pass null as the lockFactory instance and use setLockFactory to change it");
                    }
                    dir.checked_Renamed = false;
                }
            }
            lock (dir)
            {
                dir.refCount++;
            }
            return dir;
        }
		
		
		/// <summary>Returns the directory instance for the named location.
		/// 
		/// </summary>
		/// <deprecated> Use IndexWriter's create flag, instead, to
		/// create a new index.
		/// 
		/// </deprecated>
		/// <param name="path">the path to the directory.
		/// </param>
		/// <param name="create">if true, create, or erase any existing contents.
		/// </param>
		/// <returns> the FSDirectory for the named file.  
		/// </returns>
        [Obsolete("Use IndexWriter's create flag, instead, to create a new index.")]
		public static FSDirectory GetDirectory(System.String path, bool create)
		{
			return GetDirectory(new System.IO.DirectoryInfo(path), create);
		}
		
		/// <summary>Returns the directory instance for the named location.
		/// 
		/// </summary>
		/// <deprecated> Use IndexWriter's create flag, instead, to
		/// create a new index.
		/// 
		/// </deprecated>
		/// <param name="file">the path to the directory.
		/// </param>
		/// <param name="create">if true, create, or erase any existing contents.
		/// </param>
		/// <returns> the FSDirectory for the named file.  
		/// </returns>
		[System.Obsolete("Use the method that takes a DirectoryInfo, this will be removed in the 3.0 release")]
		public static FSDirectory GetDirectory(System.IO.FileInfo file, bool create)
		{
			return GetDirectory(new System.IO.DirectoryInfo(file.FullName), create);
		}

        /// <summary>Returns the directory instance for the named location.
        /// 
        /// </summary>
        /// <deprecated> Use IndexWriter's create flag, instead, to
        /// create a new index.
        /// 
        /// </deprecated>
        /// <param name="file">the path to the directory.
        /// </param>
        /// <param name="create">if true, create, or erase any existing contents.
        /// </param>
        /// <returns> the FSDirectory for the named file.  
        /// </returns>
        [Obsolete("Use IndexWriter's create flag, instead, to create a new index.")]
        public static FSDirectory GetDirectory(System.IO.DirectoryInfo file, bool create)
        {
            FSDirectory dir = GetDirectory(file, null);

            // This is now deprecated (creation should only be done
            // by IndexWriter):
            if (create)
            {
                dir.Create();
            }

            return dir;
        }
		
		/// <deprecated> 
		/// </deprecated>
        [Obsolete]
		private void  Create()
		{
			if (directory.Exists)
 			{
 				System.String[] files = SupportClass.FileSupport.GetLuceneIndexFiles(directory.FullName, IndexFileNameFilter.GetFilter()); // clear old files
 				if (files == null)
 					throw new System.IO.IOException("cannot read directory " + directory.FullName + ": list() returned null");
 				for (int i = 0; i < files.Length; i++)
 				{
                    System.String fileOrDir = System.IO.Path.Combine(directory.FullName, files[i]);
                    if (System.IO.File.Exists(fileOrDir))
 					{
                        System.IO.File.Delete(fileOrDir);
 					}
                    else if (System.IO.Directory.Exists(fileOrDir))
 					{
                        System.IO.Directory.Delete(fileOrDir);
 					}
                    // no need to throw anything - if a delete fails the exc will propogate to the caller
 				}
 			}
 			lockFactory.ClearLock(IndexWriter.WRITE_LOCK_NAME);
		}
		
		private bool checked_Renamed;
		
		internal void  CreateDir()
		{
			if (!checked_Renamed)
			{
                if (!this.directory.Exists)
                {
                    try
                    {
                        this.directory.Create();
                    }
                    catch (Exception)
                    {
                        throw new System.IO.IOException("Cannot create directory: " + directory);
                    }
                    this.directory.Refresh(); // need to see the creation
                }
				
				checked_Renamed = true;
			}
		}
		
		/// <summary>Initializes the directory to create a new file with the given name.
		/// This method should be used in {@link #createOutput}. 
		/// </summary>
		protected internal void  InitOutput(System.String name)
		{
			EnsureOpen();
			CreateDir();
			System.IO.FileInfo file = new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, name));
            if (file.Exists) // delete existing, if any
            {
                try
                {
                    file.Delete();
                }
                catch (Exception)
                {
                    throw new System.IO.IOException("Cannot overwrite: " + file);
                }
            }
		}
		
		/// <summary>The underlying filesystem directory </summary>
		protected internal System.IO.DirectoryInfo directory = null;
		
		/// <deprecated> 
		/// </deprecated>
        [Obsolete]
		private int refCount = 0;
		
		/// <deprecated> 
		/// </deprecated>
        [Obsolete]
		protected internal FSDirectory()
		{
		}
		 // permit subclassing
		
		/// <summary>Create a new FSDirectory for the named location (ctor for subclasses).</summary>
		/// <param name="path">the path of the directory
		/// </param>
		/// <param name="lockFactory">the lock factory to use, or null for the default
		/// ({@link NativeFSLockFactory});
		/// </param>
		/// <throws>  IOException </throws>
		protected internal FSDirectory(System.IO.DirectoryInfo path, LockFactory lockFactory)
		{
			// new ctors use always NativeFSLockFactory as default:
			if (lockFactory == null)
			{
				lockFactory = new NativeFSLockFactory();
			}
			Init(path, lockFactory);
			refCount = 1;
		}
		
		/// <summary>Creates an FSDirectory instance, trying to pick the
		/// best implementation given the current environment.
		/// The directory returned uses the {@link NativeFSLockFactory}.
        /// 
        /// <p/>Currently this returns {@link SimpleFSDirectory} as
        /// NIOFSDirectory is currently not supported.
		/// 
		/// <p/>Currently this returns {@link SimpleFSDirectory} as
		/// NIOFSDirectory is currently not supported.
		/// 
		/// <p/><b>NOTE</b>: this method may suddenly change which
		/// implementation is returned from release to release, in
		/// the event that higher performance defaults become
		/// possible; if the precise implementation is important to
		/// your application, please instantiate it directly,
		/// instead. On 64 bit systems, it may also good to
		/// return {@link MMapDirectory}, but this is disabled
		/// because of officially missing unmap support in Java.
		/// For optimal performance you should consider using
		/// this implementation on 64 bit JVMs.
		/// 
		/// <p/>See <a href="#subclasses">above</a> 
		/// </summary>
		[System.Obsolete("Use the method that takes a DirectoryInfo, this will be removed in the 3.0 release")]
		public static FSDirectory Open(System.IO.FileInfo path)
		{
			System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(path.FullName);
			return Open(dir, null);
		}
		
		/// <summary>Creates an FSDirectory instance, trying to pick the
		/// best implementation given the current environment.
		/// The directory returned uses the {@link NativeFSLockFactory}.
		/// 
		/// <p/>Currently this returns {@link SimpleFSDirectory} as
		/// NIOFSDirectory is currently not supported.
		/// 
		/// <p/><b>NOTE</b>: this method may suddenly change which
		/// implementation is returned from release to release, in
		/// the event that higher performance defaults become
		/// possible; if the precise implementation is important to
		/// your application, please instantiate it directly,
		/// instead. On 64 bit systems, it may also good to
		/// return {@link MMapDirectory}, but this is disabled
		/// because of officially missing unmap support in Java.
		/// For optimal performance you should consider using
		/// this implementation on 64 bit JVMs.
		/// 
		/// <p/>See <a href="#subclasses">above</a> 
		/// </summary>
		public static FSDirectory Open(System.IO.DirectoryInfo path)
		{
			return Open(path, null);
		}
		
		/// <summary>Just like {@link #Open(File)}, but allows you to
		/// also specify a custom {@link LockFactory}. 
		/// </summary>
		public static FSDirectory Open(System.IO.DirectoryInfo path, LockFactory lockFactory)
		{
			/* For testing:
			MMapDirectory dir=new MMapDirectory(path, lockFactory);
			dir.setUseUnmap(true);
			return dir;
			*/
			
			if (Constants.WINDOWS)
			{
				return new SimpleFSDirectory(path, lockFactory);
			}
			else
			{
                //NIOFSDirectory is not implemented in Mono.Lucene.Net
				//return new NIOFSDirectory(path, lockFactory);
                return new SimpleFSDirectory(path, lockFactory);
			}
        }
		
		/* will move to ctor, when reflection is removed in 3.0 */
		private void  Init(System.IO.DirectoryInfo path, LockFactory lockFactory)
		{
			
			// Set up lockFactory with cascaded defaults: if an instance was passed in,
			// use that; else if locks are disabled, use NoLockFactory; else if the
			// system property Mono.Lucene.Net.Store.FSDirectoryLockFactoryClass is set,
			// instantiate that; else, use SimpleFSLockFactory:
			
			directory = path;
			
            // due to differences in how Java & .NET refer to files, the checks are a bit different
            if (!directory.Exists && System.IO.File.Exists(directory.FullName))
            {
                throw new NoSuchDirectoryException("file '" + directory.FullName + "' exists but is not a directory");
            }
			
			if (lockFactory == null)
			{
				
				if (disableLocks)
				{
					// Locks are disabled:
					lockFactory = NoLockFactory.GetNoLockFactory();
				}
				else
				{
					System.String lockClassName = SupportClass.AppSettings.Get("Mono.Lucene.Net.Store.FSDirectoryLockFactoryClass", "");
					
					if (lockClassName != null && !lockClassName.Equals(""))
					{
						System.Type c;
						
						try
						{
							c = System.Type.GetType(lockClassName);
						}
						catch (System.Exception e)
						{
							throw new System.IO.IOException("unable to find LockClass " + lockClassName);
						}
						
						try
						{
							lockFactory = (LockFactory) System.Activator.CreateInstance(c, true);
						}
						catch (System.UnauthorizedAccessException e)
						{
							throw new System.IO.IOException("IllegalAccessException when instantiating LockClass " + lockClassName);
						}
						catch (System.InvalidCastException e)
						{
							throw new System.IO.IOException("unable to cast LockClass " + lockClassName + " instance to a LockFactory");
						}
						catch (System.Exception e)
						{
							throw new System.IO.IOException("InstantiationException when instantiating LockClass " + lockClassName);
						}
					}
					else
					{
						// Our default lock is SimpleFSLockFactory;
						// default lockDir is our index directory:
						lockFactory = new SimpleFSLockFactory();
					}
				}
			}
			
			SetLockFactory(lockFactory);
			
			// for filesystem based LockFactory, delete the lockPrefix, if the locks are placed
			// in index dir. If no index dir is given, set ourselves
			if (lockFactory is FSLockFactory)
			{
				FSLockFactory lf = (FSLockFactory) lockFactory;
				System.IO.DirectoryInfo dir = lf.GetLockDir();
				// if the lock factory has no lockDir set, use the this directory as lockDir
				if (dir == null)
				{
					lf.SetLockDir(this.directory);
					lf.SetLockPrefix(null);
				}
				else if (dir.FullName.Equals(this.directory.FullName))
				{
					lf.SetLockPrefix(null);
				}
			}
		}
		
		/// <summary>Lists all files (not subdirectories) in the
		/// directory.  This method never returns null (throws
		/// {@link IOException} instead).
		/// 
		/// </summary>
		/// <throws>  NoSuchDirectoryException if the directory </throws>
		/// <summary>   does not exist, or does exist but is not a
		/// directory.
		/// </summary>
		/// <throws>  IOException if list() returns null  </throws>
		[System.Obsolete("Use the method that takes a DirectoryInfo, this will be removed in the 3.0 release")]
		public static System.String[] ListAll(System.IO.FileInfo dir)
		{
			return ListAll(new System.IO.DirectoryInfo(dir.FullName));
		}
		
        /// <summary>Lists all files (not subdirectories) in the
        /// directory.  This method never returns null (throws
        /// {@link IOException} instead).
        /// 
        /// </summary>
        /// <throws>  NoSuchDirectoryException if the directory </throws>
        /// <summary>   does not exist, or does exist but is not a
        /// directory.
        /// </summary>
        /// <throws>  IOException if list() returns null  </throws>
        public static System.String[] ListAll(System.IO.DirectoryInfo dir)
        {
            if (!dir.Exists)
            {
                throw new NoSuchDirectoryException("directory '" + dir.FullName + "' does not exist");
            }
            // Exclude subdirs, only the file names, not the paths
            System.IO.FileInfo[] files = dir.GetFiles();
            System.String[] result = new System.String[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                result[i] = files[i].Name;
            }

            // no reason to return null, if the directory cannot be listed, an exception 
            // will be thrown on the above call to dir.GetFiles()
            // use of LINQ to create the return value array may be a bit more efficient

            return result;
        }
		
        [Obsolete("Mono.Lucene.Net-2.9.1. This method overrides obsolete member Mono.Lucene.Net.Store.Directory.List()")]
		public override System.String[] List()
		{
			EnsureOpen();
			return SupportClass.FileSupport.GetLuceneIndexFiles(directory.FullName, IndexFileNameFilter.GetFilter());
		}
		
		/// <summary>Lists all files (not subdirectories) in the
		/// directory.
		/// </summary>
		/// <seealso cref="ListAll(File)">
		/// </seealso>
		public override System.String[] ListAll()
		{
			EnsureOpen();
			return ListAll(directory);
		}
		
		/// <summary>Returns true iff a file with the given name exists. </summary>
		public override bool FileExists(System.String name)
		{
			EnsureOpen();
			System.IO.FileInfo file = new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, name));
            return file.Exists;
		}
		
		/// <summary>Returns the time the named file was last modified. </summary>
		public override long FileModified(System.String name)
		{
			EnsureOpen();
			System.IO.FileInfo file = new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, name));
            return (long)file.LastWriteTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds; //{{LUCENENET-353}}
		}
		
		/// <summary>Returns the time the named file was last modified. </summary>
		public static long FileModified(System.IO.FileInfo directory, System.String name)
		{
			System.IO.FileInfo file = new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, name));
            return (long)file.LastWriteTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds; //{{LUCENENET-353}}
		}
		
		/// <summary>Set the modified time of an existing file to now. </summary>
		public override void  TouchFile(System.String name)
		{
			EnsureOpen();
			System.IO.FileInfo file = new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, name));
			file.LastWriteTime = System.DateTime.Now;
		}
		
		/// <summary>Returns the length in bytes of a file in the directory. </summary>
		public override long FileLength(System.String name)
		{
			EnsureOpen();
			System.IO.FileInfo file = new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, name));
			return file.Exists ? file.Length : 0;
		}
		
		/// <summary>Removes an existing file in the directory. </summary>
		public override void  DeleteFile(System.String name)
		{
			EnsureOpen();
			System.IO.FileInfo file = new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, name));
            try
            {
                file.Delete();
            }
            catch (Exception)
            {
                throw new System.IO.IOException("Cannot delete " + file);
            }
		}
		
		/// <summary>Renames an existing file in the directory. 
		/// Warning: This is not atomic.
		/// </summary>
		/// <deprecated> 
		/// </deprecated>
        [Obsolete]
		public override void  RenameFile(System.String from, System.String to)
		{
			lock (this)
			{
				EnsureOpen();
                System.IO.FileInfo old = new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, from));
                try
                {
                    old.MoveTo(System.IO.Path.Combine(directory.FullName, to));
                }
                catch (System.IO.IOException ioe)
                {
                    System.IO.IOException newExc = new System.IO.IOException("Cannot rename " + old + " to " + directory, ioe);
                    throw newExc;
                }
			}
		}
		
		/// <summary>Creates an IndexOutput for the file with the given name.
		/// <em>In 3.0 this method will become abstract.</em> 
		/// </summary>
		public override IndexOutput CreateOutput(System.String name)
		{
			InitOutput(name);
			return new FSIndexOutput(new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, name)));
		}
		
		public override void  Sync(System.String name)
		{
			EnsureOpen();
			System.IO.FileInfo fullFile = new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, name));
			bool success = false;
			int retryCount = 0;
			System.IO.IOException exc = null;
			while (!success && retryCount < 5)
			{
				retryCount++;
				System.IO.FileStream file = null;
				try
				{
					try
					{
                        file = new System.IO.FileStream(fullFile.FullName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write, System.IO.FileShare.ReadWrite);
                        SupportClass.FileSupport.Sync(file);
                        success = true;
					}
					finally
					{
						if (file != null)
							file.Close();
					}
				}
				catch (System.IO.IOException ioe)
				{
					if (exc == null)
						exc = ioe;
					try
					{
						// Pause 5 msec
						System.Threading.Thread.Sleep(new System.TimeSpan((System.Int64) 10000 * 5));
					}
					catch (System.Threading.ThreadInterruptedException ie)
					{
						// In 3.0 we will change this to throw
						// InterruptedException instead
						SupportClass.ThreadClass.Current().Interrupt();
                        throw new System.SystemException(ie.ToString(), ie);
					}
				}
			}
			if (!success)
			// Throw original exception
				throw exc;
		}
		
		// Inherit javadoc
		public override IndexInput OpenInput(System.String name)
		{
			EnsureOpen();
			return OpenInput(name, BufferedIndexInput.BUFFER_SIZE);
		}
		
		/// <summary>Creates an IndexInput for the file with the given name.
		/// <em>In 3.0 this method will become abstract.</em> 
		/// </summary>
		public override IndexInput OpenInput(System.String name, int bufferSize)
		{
			EnsureOpen();
			return new FSIndexInput(new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, name)), bufferSize);
		}
		
		/// <summary> So we can do some byte-to-hexchar conversion below</summary>
		private static readonly char[] HEX_DIGITS = new char[]{'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'};
		
		
		public override System.String GetLockID()
		{
			EnsureOpen();
			System.String dirName; // name to be hashed
			try
			{
				dirName = directory.FullName;
			}
			catch (System.IO.IOException e)
			{
				throw new System.SystemException(e.ToString(), e);
			}
			
			byte[] digest;
			lock (DIGESTER)
			{
				digest = DIGESTER.ComputeHash(System.Text.Encoding.UTF8.GetBytes(dirName));
			}
			System.Text.StringBuilder buf = new System.Text.StringBuilder();
			buf.Append("lucene-");
			for (int i = 0; i < digest.Length; i++)
			{
				int b = digest[i];
				buf.Append(HEX_DIGITS[(b >> 4) & 0xf]);
				buf.Append(HEX_DIGITS[b & 0xf]);
			}
			
			return buf.ToString();
		}
		
		/// <summary>Closes the store to future operations. </summary>
		public override void  Close()
		{
			lock (this)
			{
				if (isOpen && --refCount <= 0)
				{
					isOpen = false;
					lock (DIRECTORIES)
					{
						DIRECTORIES.Remove(directory.FullName);
					}
				}
			}
		}

        /// <summary>
        /// .NET
        /// </summary>
        public override void Dispose()
        {
            Close();
        }

        [System.Obsolete("A DirectoryInfo is more appropriate, however this is here for backwards compatibility. This will be removed in the 3.0 release")]
		public virtual System.IO.FileInfo GetFile()
		{
			EnsureOpen();
			return new System.IO.FileInfo(directory.FullName);
		}


        // Java Lucene implements GetFile() which returns a FileInfo.
        // For Mono.Lucene.Net, GetDirectory() is more appropriate
        public virtual System.IO.DirectoryInfo GetDirectory()
        {
            EnsureOpen();
            return directory;
        }
		
		/// <summary>For debug output. </summary>
		public override System.String ToString()
		{
            return this.GetType().FullName + "@" + directory + " lockFactory=" + GetLockFactory();
		}
		
		/// <summary> Default read chunk size.  This is a conditional
		/// default: on 32bit JVMs, it defaults to 100 MB.  On
		/// 64bit JVMs, it's <code>Integer.MAX_VALUE</code>.
		/// </summary>
		/// <seealso cref="setReadChunkSize">
		/// </seealso>
		public static readonly int DEFAULT_READ_CHUNK_SIZE;
		
		// LUCENE-1566
		private int chunkSize = DEFAULT_READ_CHUNK_SIZE;
		
		/// <summary> Sets the maximum number of bytes read at once from the
		/// underlying file during {@link IndexInput#readBytes}.
		/// The default value is {@link #DEFAULT_READ_CHUNK_SIZE};
		/// 
		/// <p/> This was introduced due to <a
		/// href="http://bugs.sun.com/bugdatabase/view_bug.do?bug_id=6478546">Sun
		/// JVM Bug 6478546</a>, which throws an incorrect
		/// OutOfMemoryError when attempting to read too many bytes
		/// at once.  It only happens on 32bit JVMs with a large
		/// maximum heap size.<p/>
		/// 
		/// <p/>Changes to this value will not impact any
		/// already-opened {@link IndexInput}s.  You should call
		/// this before attempting to open an index on the
		/// directory.<p/>
		/// 
		/// <p/> <b>NOTE</b>: This value should be as large as
		/// possible to reduce any possible performance impact.  If
		/// you still encounter an incorrect OutOfMemoryError,
		/// trying lowering the chunk size.<p/>
		/// </summary>
		public void  SetReadChunkSize(int chunkSize)
		{
			// LUCENE-1566
			if (chunkSize <= 0)
			{
				throw new System.ArgumentException("chunkSize must be positive");
			}
			if (!Constants.JRE_IS_64BIT)
			{
				this.chunkSize = chunkSize;
			}
		}
		
		/// <summary> The maximum number of bytes to read at once from the
		/// underlying file during {@link IndexInput#readBytes}.
		/// </summary>
		/// <seealso cref="setReadChunkSize">
		/// </seealso>
		public int GetReadChunkSize()
		{
			// LUCENE-1566
			return chunkSize;
		}
		
		
		/// <deprecated> Use SimpleFSDirectory.SimpleFSIndexInput instead 
		/// </deprecated>
        [Obsolete("Use SimpleFSDirectory.SimpleFSIndexInput instead ")]
		public /*protected internal*/ class FSIndexInput:SimpleFSDirectory.SimpleFSIndexInput
		{
			
			/// <deprecated> 
			/// </deprecated>
            [Obsolete]
			new protected internal class Descriptor:SimpleFSDirectory.SimpleFSIndexInput.Descriptor
			{
				/// <deprecated> 
				/// </deprecated>
                [Obsolete]
				public Descriptor(/*FSIndexInput enclosingInstance,*/ System.IO.FileInfo file, System.IO.FileAccess mode) : base(file, mode)
				{
				}
			}
			
			/// <deprecated> 
			/// </deprecated>
            [Obsolete]
			public FSIndexInput(System.IO.FileInfo path):base(path)
			{
			}
			
			/// <deprecated> 
			/// </deprecated>
            [Obsolete]
			public FSIndexInput(System.IO.FileInfo path, int bufferSize):base(path, bufferSize)
			{
			}
		}
		
		/// <deprecated> Use SimpleFSDirectory.SimpleFSIndexOutput instead 
		/// </deprecated>
        [Obsolete("Use SimpleFSDirectory.SimpleFSIndexOutput instead ")]
		protected internal class FSIndexOutput:SimpleFSDirectory.SimpleFSIndexOutput
		{
			
			/// <deprecated> 
			/// </deprecated>
            [Obsolete]
			public FSIndexOutput(System.IO.FileInfo path):base(path)
			{
			}
		}
		static FSDirectory()
		{
			{
				try
				{
					System.String name = SupportClass.AppSettings.Get("Mono.Lucene.Net.FSDirectory.class", typeof(SimpleFSDirectory).FullName);
					if (typeof(FSDirectory).FullName.Equals(name))
					{
						// FSDirectory will be abstract, so we replace it by the correct class
						IMPL = typeof(SimpleFSDirectory);
					}
					else
					{
						IMPL = System.Type.GetType(name);
					}
				}
				catch (System.Security.SecurityException se)
				{
					IMPL = typeof(SimpleFSDirectory);
				}
				catch (System.Exception e)
				{
					throw new System.SystemException("cannot load FSDirectory class: " + e.ToString(), e);
				}
			}
			{
				try
				{
					DIGESTER = SupportClass.Cryptography.GetHashAlgorithm();
				}
				catch (System.Exception e)
				{
					throw new System.SystemException(e.ToString(), e);
				}
			}
			DEFAULT_READ_CHUNK_SIZE = Constants.JRE_IS_64BIT?System.Int32.MaxValue:100 * 1024 * 1024;
		}
	}
}
