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
using Constants = Monodoc.Lucene.Net.Util.Constants;
namespace Monodoc.Lucene.Net.Store
{
	
	/// <summary> Straightforward implementation of {@link Directory} as a directory of files.
	/// <p>If the system property 'disableLuceneLocks' has the String value of
	/// "true", lock creation will be disabled.
	/// 
	/// </summary>
	/// <seealso cref="Directory">
	/// </seealso>
	/// <author>  Doug Cutting
	/// </author>
	public sealed class FSDirectory : Directory
	{
		private class AnonymousClassLock : Lock
		{
			public AnonymousClassLock(System.IO.FileInfo lockFile, FSDirectory enclosingInstance)
			{
				InitBlock(lockFile, enclosingInstance);
			}
			private void  InitBlock(System.IO.FileInfo lockFile, FSDirectory enclosingInstance)
			{
				this.lockFile = lockFile;
				this.enclosingInstance = enclosingInstance;
			}
			private System.IO.FileInfo lockFile;
			private FSDirectory enclosingInstance;
			public FSDirectory Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public override bool Obtain()
			{
				if (Monodoc.Lucene.Net.Store.FSDirectory.DISABLE_LOCKS)
					return true;
				
				bool tmpBool;
				if (System.IO.File.Exists(Enclosing_Instance.lockDir.FullName))
					tmpBool = true;
				else
					tmpBool = System.IO.Directory.Exists(Enclosing_Instance.lockDir.FullName);
				if (!tmpBool)
				{
                    try
                    {
                        System.IO.Directory.CreateDirectory(Enclosing_Instance.lockDir.FullName);
                    }
                    catch (Exception)
					{
						throw new System.IO.IOException("Cannot create lock directory: " + Enclosing_Instance.lockDir);
					}
				}
				
                try
                {
                    System.IO.FileStream createdFile = lockFile.Create();
                    createdFile.Close();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
			}
			public override void  Release()
			{
				if (Monodoc.Lucene.Net.Store.FSDirectory.DISABLE_LOCKS)
					return ;
				bool tmpBool;
				if (System.IO.File.Exists(lockFile.FullName))
				{
					System.IO.File.Delete(lockFile.FullName);
					tmpBool = true;
				}
				else if (System.IO.Directory.Exists(lockFile.FullName))
				{
					System.IO.Directory.Delete(lockFile.FullName);
					tmpBool = true;
				}
				else
					tmpBool = false;
				bool generatedAux = tmpBool;
			}
			public override bool IsLocked()
			{
				if (Monodoc.Lucene.Net.Store.FSDirectory.DISABLE_LOCKS)
					return false;
				bool tmpBool;
				if (System.IO.File.Exists(lockFile.FullName))
					tmpBool = true;
				else
					tmpBool = System.IO.Directory.Exists(lockFile.FullName);
				return tmpBool;
			}
			
			public override System.String ToString()
			{
				return "Lock@" + lockFile;
			}
		}
		/// <summary>This cache of directories ensures that there is a unique Directory
		/// instance per path, so that synchronization on the Directory can be used to
		/// synchronize access between readers and writers.
		/// 
		/// This should be a WeakHashMap, so that entries can be GC'd, but that would
		/// require Java 1.2.  Instead we use refcounts...
		/// </summary>
		private static readonly System.Collections.Hashtable DIRECTORIES = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());
		
		private static readonly bool DISABLE_LOCKS;
		
		/// <summary> Directory specified by <code>Monodoc.Lucene.Net.lockdir</code>
		/// or <code>java.io.tmpdir</code> system property
		/// </summary>
		public static readonly System.String LOCK_DIR = SupportClass.AppSettings.Get("Monodoc.Lucene.Net.lockdir", System.IO.Path.GetTempPath());
		
        private static System.Security.Cryptography.MD5 DIGESTER;
		
		/// <summary>A buffer optionally used in renameTo method </summary>
		private byte[] buffer = null;
		
		/// <summary>Returns the directory instance for the named location.
		/// 
		/// <p>Directories are cached, so that, for a given canonical path, the same
		/// FSDirectory instance will always be returned.  This permits
		/// synchronization on directories.
		/// 
		/// </summary>
		/// <param name="path">the path to the directory.
		/// </param>
		/// <param name="create">if true, create, or erase any existing contents.
		/// </param>
		/// <returns> the FSDirectory for the named file.  
		/// </returns>
		public static FSDirectory GetDirectory(System.String path, bool create)
		{
			return GetDirectory(new System.IO.FileInfo(path), create);
		}
		
		/// <summary>Returns the directory instance for the named location.
		/// 
		/// <p>Directories are cached, so that, for a given canonical path, the same
		/// FSDirectory instance will always be returned.  This permits
		/// synchronization on directories.
		/// 
		/// </summary>
		/// <param name="file">the path to the directory.
		/// </param>
		/// <param name="create">if true, create, or erase any existing contents.
		/// </param>
		/// <returns> the FSDirectory for the named file.  
		/// </returns>
		public static FSDirectory GetDirectory(System.IO.FileInfo file, bool create)
		{
			file = new System.IO.FileInfo(file.FullName);
			FSDirectory dir;
			lock (DIRECTORIES.SyncRoot)
			{
				dir = (FSDirectory) DIRECTORIES[file];
				if (dir == null)
				{
					dir = new FSDirectory(file, create);
					DIRECTORIES[file] = dir;
				}
				else if (create)
				{
					dir.Create();
				}
			}
			lock (dir)
			{
				dir.refCount++;
			}
			return dir;
		}
		
		private System.IO.FileInfo directory = null;
		private int refCount;
		private System.IO.FileInfo lockDir;
		
		private FSDirectory(System.IO.FileInfo path, bool create)
		{
			directory = path;
			
			if (LOCK_DIR == null)
			{
				lockDir = directory;
			}
			else
			{
				lockDir = new System.IO.FileInfo(LOCK_DIR);
			}
			if (create)
			{
				Create();
			}
			
			if (!System.IO.Directory.Exists(directory.FullName))
				throw new System.IO.IOException(path + " not a directory");
		}
		
		private void  Create()
		{
			lock (this)
			{
				bool tmpBool;
				if (System.IO.File.Exists(directory.FullName))
					tmpBool = true;
				else
					tmpBool = System.IO.Directory.Exists(directory.FullName);
				if (!tmpBool)
				{
                    try
                    {
                        System.IO.Directory.CreateDirectory(directory.FullName);
                    }
                    catch (Exception)
                    {
                        throw new System.IO.IOException("Cannot create directory: " + directory);
                    }
				}
				
				System.String[] files = System.IO.Directory.GetFileSystemEntries(directory.FullName); // clear old files
				for (int i = 0; i < files.Length; i++)
				{
					System.IO.FileInfo file = new System.IO.FileInfo(files[i]);
					bool tmpBool2;
					if (System.IO.File.Exists(file.FullName))
					{
						System.IO.File.Delete(file.FullName);
						tmpBool2 = true;
					}
					else if (System.IO.Directory.Exists(file.FullName))
					{
						System.IO.Directory.Delete(file.FullName);
						tmpBool2 = true;
					}
					else
						tmpBool2 = false;
					if (!tmpBool2)
						throw new System.IO.IOException("Cannot delete " + files[i]);
				}
				
				System.String lockPrefix = GetLockPrefix().ToString(); // clear old locks
				files = System.IO.Directory.GetFileSystemEntries(lockDir.FullName);
				for (int i = 0; i < files.Length; i++)
				{
					if (!files[i].StartsWith(lockPrefix))
						continue;
					System.IO.FileInfo lockFile = new System.IO.FileInfo(System.IO.Path.Combine(lockDir.FullName, files[i]));
					bool tmpBool3;
					if (System.IO.File.Exists(lockFile.FullName))
					{
						System.IO.File.Delete(lockFile.FullName);
						tmpBool3 = true;
					}
					else if (System.IO.Directory.Exists(lockFile.FullName))
					{
						System.IO.Directory.Delete(lockFile.FullName);
						tmpBool3 = true;
					}
					else
						tmpBool3 = false;
					if (!tmpBool3)
						throw new System.IO.IOException("Cannot delete " + files[i]);
				}
			}
		}
		
		/// <summary>Returns an array of strings, one for each file in the directory. </summary>
		public override System.String[] List()
		{
			return System.IO.Directory.GetFileSystemEntries(directory.FullName);
		}
		
		/// <summary>Returns true iff a file with the given name exists. </summary>
		public override bool FileExists(System.String name)
		{
			System.IO.FileInfo file = new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, name));
			bool tmpBool;
			if (System.IO.File.Exists(file.FullName))
				tmpBool = true;
			else
				tmpBool = System.IO.Directory.Exists(file.FullName);
			return tmpBool;
		}
		
		/// <summary>Returns the time the named file was last modified. </summary>
		public override long FileModified(System.String name)
		{
			System.IO.FileInfo file = new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, name));
			return ((file.LastWriteTime.Ticks - 621355968000000000) / 10000);
		}
		
		/// <summary>Returns the time the named file was last modified. </summary>
		public static long FileModified(System.IO.FileInfo directory, System.String name)
		{
			System.IO.FileInfo file = new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, name));
			return ((file.LastWriteTime.Ticks - 621355968000000000) / 10000);
		}
		
		/// <summary>Set the modified time of an existing file to now. </summary>
		public override void  TouchFile(System.String name)
		{
			System.IO.FileInfo file = new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, name));
            file.LastWriteTime = System.DateTime.Now;
		}
		
		/// <summary>Returns the length in bytes of a file in the directory. </summary>
		public override long FileLength(System.String name)
		{
			System.IO.FileInfo file = new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, name));
            return file.Exists ? file.Length : 0;
		}
		
		/// <summary>Removes an existing file in the directory. </summary>
		public override void  DeleteFile(System.String name)
		{
			System.IO.FileInfo file = new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, name));
			bool tmpBool;
			if (System.IO.File.Exists(file.FullName))
			{
				System.IO.File.Delete(file.FullName);
				tmpBool = true;
			}
			else if (System.IO.Directory.Exists(file.FullName))
			{
				System.IO.Directory.Delete(file.FullName);
				tmpBool = true;
			}
			else
				tmpBool = false;
			if (!tmpBool)
				throw new System.IO.IOException("Cannot delete " + name);
		}
		
		/// <summary>Renames an existing file in the directory. </summary>
		public override void  RenameFile(System.String from, System.String to)
		{
			lock (this)
			{
				System.IO.FileInfo old = new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, from));
				System.IO.FileInfo nu = new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, to));
				
				/* This is not atomic.  If the program crashes between the call to
				delete() and the call to renameTo() then we're screwed, but I've
				been unable to figure out how else to do this... */
				
				bool tmpBool;
				if (System.IO.File.Exists(nu.FullName))
					tmpBool = true;
				else
					tmpBool = System.IO.Directory.Exists(nu.FullName);
				if (tmpBool)
				{
					bool tmpBool2;
					if (System.IO.File.Exists(nu.FullName))
					{
						System.IO.File.Delete(nu.FullName);
						tmpBool2 = true;
					}
					else if (System.IO.Directory.Exists(nu.FullName))
					{
						System.IO.Directory.Delete(nu.FullName);
						tmpBool2 = true;
					}
					else
						tmpBool2 = false;
					if (!tmpBool2)
						throw new System.IO.IOException("Cannot delete " + to);
				}
				
				// Rename the old file to the new one. Unfortunately, the renameTo()
				// method does not work reliably under some JVMs.  Therefore, if the
				// rename fails, we manually rename by copying the old file to the new one
                try
                {
                    old.MoveTo(nu.FullName);
                }
                catch (System.Exception ex)
                {
                    System.IO.BinaryReader in_Renamed = null;
                    System.IO.Stream out_Renamed = null;
                    try
                    {
                        in_Renamed = new System.IO.BinaryReader(System.IO.File.Open(old.FullName, System.IO.FileMode.Open, System.IO.FileAccess.Read));
                        out_Renamed = new System.IO.FileStream(nu.FullName, System.IO.FileMode.Create);
                        // see if the buffer needs to be initialized. Initialization is
                        // only done on-demand since many VM's will never run into the renameTo
                        // bug and hence shouldn't waste 1K of mem for no reason.
                        if (buffer == null)
                        {
                            buffer = new byte[1024];
                        }
                        int len;
                        len = in_Renamed.Read(buffer, 0, buffer.Length);
                        out_Renamed.Write(buffer, 0, len);
						
                        // delete the old file.
                        bool tmpBool3;
                        if (System.IO.File.Exists(old.FullName))
                        {
                            System.IO.File.Delete(old.FullName);
                            tmpBool3 = true;
                        }
                        else if (System.IO.Directory.Exists(old.FullName))
                        {
                            System.IO.Directory.Delete(old.FullName);
                            tmpBool3 = true;
                        }
                        else
                            tmpBool3 = false;
                        bool generatedAux = tmpBool3;
                    }
                    catch (System.IO.IOException e)
                    {
                        throw new System.IO.IOException("Cannot rename " + from + " to " + to);
                    }
                    finally
                    {
                        if (in_Renamed != null)
                        {
                            try
                            {
                                in_Renamed.Close();
                            }
                            catch (System.IO.IOException e)
                            {
                                throw new System.SystemException("Cannot close input stream: " + e.Message);
                            }
                        }
                        if (out_Renamed != null)
                        {
                            try
                            {
                                out_Renamed.Close();
                            }
                            catch (System.IO.IOException e)
                            {
                                throw new System.SystemException("Cannot close output stream: " + e.Message);
                            }
                        }
                    }
                }
			}
		}
		
		/// <summary>Creates a new, empty file in the directory with the given name.
		/// Returns a stream writing this file. 
		/// </summary>
		public override OutputStream CreateFile(System.String name)
		{
			return new FSOutputStream(new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, name)));
		}
		
		/// <summary>Returns a stream reading an existing file. </summary>
		public override InputStream OpenFile(System.String name)
		{
			return new FSInputStream(new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, name)));
		}
		
		/// <summary> So we can do some byte-to-hexchar conversion below</summary>
		private static readonly char[] HEX_DIGITS = new char[]{'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'};
		
		/// <summary>Constructs a {@link Lock} with the specified name.  Locks are implemented
		/// with {@link File#createNewFile() }.
		/// 
		/// <p>In JDK 1.1 or if system property <I>disableLuceneLocks</I> is the
		/// string "true", locks are disabled.  Assigning this property any other
		/// string will <B>not</B> prevent creation of lock files.  This is useful for
		/// using Lucene on read-only medium, such as CD-ROM.
		/// 
		/// </summary>
		/// <param name="name">the name of the lock file
		/// </param>
		/// <returns> an instance of <code>Lock</code> holding the lock
		/// </returns>
		public override Lock MakeLock(System.String name)
		{
			System.Text.StringBuilder buf = GetLockPrefix();
			buf.Append("-");
			buf.Append(name);
			
			// create a lock file
			System.IO.FileInfo lockFile = new System.IO.FileInfo(System.IO.Path.Combine(lockDir.FullName, buf.ToString()));
			
			return new AnonymousClassLock(lockFile, this);
		}
		
		private System.Text.StringBuilder GetLockPrefix()
		{
			System.String dirName; // name to be hashed
			try
			{
				dirName = directory.FullName;
			}
			catch (System.IO.IOException e)
			{
				throw new System.SystemException(e.ToString());
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
			
			return buf;
		}
		
		/// <summary>Closes the store to future operations. </summary>
		public override void  Close()
		{
			lock (this)
			{
				if (--refCount <= 0)
				{
					lock (DIRECTORIES.SyncRoot)
					{
						DIRECTORIES.Remove(directory);
					}
				}
			}
		}
		
		public System.IO.FileInfo GetFile()
		{
			return directory;
		}
		
		/// <summary>For debug output. </summary>
		public override System.String ToString()
		{
			return "FSDirectory@" + directory;
		}
		static FSDirectory()
		{
			DISABLE_LOCKS = System.Configuration.ConfigurationSettings.AppSettings.Get("disableLuceneLocks") != null;
			{
				try
				{
					DIGESTER = System.Security.Cryptography.MD5.Create();
				}
				catch (System.Exception e)
				{
					throw new System.SystemException(e.ToString());
				}
			}
		}
	}
	
	
	sealed public class FSInputStream : InputStream, System.ICloneable
	{
		internal class Descriptor : System.IO.BinaryReader
		{
			private void  InitBlock(FSInputStream enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private FSInputStream enclosingInstance;
			public FSInputStream Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			/* DEBUG */
			//private String name;
			/* DEBUG */
            // {{Aroush
			public long position;
            public Descriptor(FSInputStream enclosingInstance, System.IO.FileInfo file, System.IO.FileAccess fileAccess) 
                : base(new System.IO.FileStream(file.FullName, System.IO.FileMode.Open, fileAccess, System.IO.FileShare.ReadWrite))
            {
            }

			//{{}}// public Descriptor(FSInputStream enclosingInstance, System.IO.FileInfo file, System.String mode) : base(file, mode)
			//{{}}// {
			//{{}}// 	InitBlock(enclosingInstance);
			//{{}}// 	/* DEBUG */
			//{{}}// 	//name = file.ToString();
			//{{}}// 	//debug_printInfo("OPEN");
			//{{}}// 	/* DEBUG */
			//{{}}// }
            // Aroush}}
			
			/* DEBUG */
			//public void close() throws IOException {
			//  debug_printInfo("CLOSE");
			//    super.close();
			//}
			//
			//private void debug_printInfo(String op) {
			//  try { throw new Exception(op + " <" + name + ">");
			//  } catch (Exception e) {
			//    java.io.StringWriter sw = new java.io.StringWriter();
			//    java.io.PrintWriter pw = new java.io.PrintWriter(sw);
			//    e.printStackTrace(pw);
			//    System.out.println(sw.getBuffer().ToString());
			//  }
			//}
			/* DEBUG */
		}
		
		internal Descriptor file = null;
		public /*internal*/ bool isClone;
		
		public FSInputStream(System.IO.FileInfo path)
		{
			file = new Descriptor(this, path, System.IO.FileAccess.Read);
			length = file.BaseStream.Length;
		}
		
		/// <summary>InputStream methods </summary>
		public override void  ReadInternal(byte[] b, int offset, int len)
		{
			lock (file)
			{
				long position = GetFilePointer();
				if (position != file.position)
				{
					file.BaseStream.Seek(position, System.IO.SeekOrigin.Begin);
					file.position = position;
				}
				int total = 0;
				do 
				{
                    int i = file.Read(b, offset + total, len - total);
					if (i <= 0)
						throw new System.IO.IOException("read past EOF");
					file.position += i;
					total += i;
				}
				while (total < len);
			}
		}
		
		public override void  Close()
		{
			if (!isClone && file != null)
				file.Close();
			System.GC.SuppressFinalize(this);
		}
		
		/// <summary>Random-access methods </summary>
		public override void  SeekInternal(long position)
		{
		}
		
		~FSInputStream()
		{
			Close(); // close the file
		}
		
		public override System.Object Clone()
		{
			FSInputStream clone = (FSInputStream) base.Clone();
			clone.isClone = true;
			return clone;
		}
		
		/// <summary>Method used for testing. Returns true if the underlying
		/// file descriptor is valid.
		/// </summary>
		public /*internal*/ bool IsFDValid()
		{
			return file.BaseStream.CanRead;
		}
	}
	
	
	sealed class FSOutputStream : OutputStream
	{
		internal System.IO.BinaryWriter file = null;
		
		public FSOutputStream(System.IO.FileInfo path)
		{
			file = new System.IO.BinaryWriter(new System.IO.FileStream(path.FullName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write, System.IO.FileShare.ReadWrite));
		}
		
		/// <summary>output methods: </summary>
		public override void  FlushBuffer(byte[] b, int size)
		{
            file.Write(b, 0, size);
		}
		public override void  Close()
		{
			base.Close();
			file.Close();
			System.GC.SuppressFinalize(this);
		}
		
		/// <summary>Random-access methods </summary>
		public override void  Seek(long pos)
		{
			base.Seek(pos);
			file.BaseStream.Seek(pos, System.IO.SeekOrigin.Begin);
		}
		public override long Length()
		{
			return file.BaseStream.Length;
		}
		
		~FSOutputStream()
		{
			file.Close(); // close the file
		}
	}
}