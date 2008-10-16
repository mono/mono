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
namespace Monodoc.Lucene.Net.Store
{
	
	/// <summary> A memory-resident {@link Directory} implementation.
	/// 
	/// </summary>
	/// <version>  $Id: RAMDirectory.java,v 1.15 2004/05/09 12:41:47 ehatcher Exp $
	/// </version>
	public sealed class RAMDirectory : Directory
	{
		private class AnonymousClassLock:Lock
		{
			public AnonymousClassLock(System.String name, RAMDirectory enclosingInstance)
			{
				InitBlock(name, enclosingInstance);
			}
			private void  InitBlock(System.String name, RAMDirectory enclosingInstance)
			{
				this.name = name;
				this.enclosingInstance = enclosingInstance;
			}
			private System.String name;
			private RAMDirectory enclosingInstance;
			public RAMDirectory Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public override bool Obtain()
			{
				lock (Enclosing_Instance.files.SyncRoot)
				{
					if (!Enclosing_Instance.FileExists(name))
					{
						Enclosing_Instance.CreateFile(name).Close();
						return true;
					}
					return false;
				}
			}
			public override void  Release()
			{
				Enclosing_Instance.DeleteFile(name);
			}
			public override bool IsLocked()
			{
				return Enclosing_Instance.FileExists(name);
			}
		}
		internal System.Collections.Hashtable files = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());
		
		/// <summary>Constructs an empty {@link Directory}. </summary>
		public RAMDirectory()
		{
		}
		
		/// <summary> Creates a new <code>RAMDirectory</code> instance from a different
		/// <code>Directory</code> implementation.  This can be used to load
		/// a disk-based index into memory.
		/// <P>
		/// This should be used only with indices that can fit into memory.
		/// 
		/// </summary>
		/// <param name="dir">a <code>Directory</code> value
		/// </param>
		/// <exception cref=""> IOException if an error occurs
		/// </exception>
		public RAMDirectory(Directory dir) : this(dir, false)
		{
		}
		
		private RAMDirectory(Directory dir, bool closeDir)
		{
			System.String[] files = dir.List();
			for (int i = 0; i < files.Length; i++)
			{
				// make place on ram disk
				OutputStream os = CreateFile(System.IO.Path.GetFileName(files[i]));
				// read current file
				InputStream is_Renamed = dir.OpenFile(files[i]);
				// and copy to ram disk
				int len = (int) is_Renamed.Length();
				byte[] buf = new byte[len];
				is_Renamed.ReadBytes(buf, 0, len);
				os.WriteBytes(buf, len);
				// graceful cleanup
				is_Renamed.Close();
				os.Close();
			}
			if (closeDir)
				dir.Close();
		}
		
		/// <summary> Creates a new <code>RAMDirectory</code> instance from the {@link FSDirectory}.
		/// 
		/// </summary>
		/// <param name="dir">a <code>File</code> specifying the index directory
		/// </param>
		public RAMDirectory(System.IO.FileInfo dir) : this(FSDirectory.GetDirectory(dir, false), true)
		{
		}
		
		/// <summary> Creates a new <code>RAMDirectory</code> instance from the {@link FSDirectory}.
		/// 
		/// </summary>
		/// <param name="dir">a <code>String</code> specifying the full index directory path
		/// </param>
		public RAMDirectory(System.String dir) : this(FSDirectory.GetDirectory(dir, false), true)
		{
		}
		
		/// <summary>Returns an array of strings, one for each file in the directory. </summary>
		public override System.String[] List()
		{
			System.String[] result = new System.String[files.Count];
			int i = 0;
			System.Collections.IEnumerator names = files.Keys.GetEnumerator();
			while (names.MoveNext())
			{
				result[i++] = ((System.String) names.Current);
			}
			return result;
		}
		
		/// <summary>Returns true iff the named file exists in this directory. </summary>
		public override bool FileExists(System.String name)
		{
			RAMFile file = (RAMFile) files[name];
			return file != null;
		}
		
		/// <summary>Returns the time the named file was last modified. </summary>
		public override long FileModified(System.String name)
		{
			RAMFile file = (RAMFile) files[name];
			return file.lastModified;
		}
		
		/// <summary>Set the modified time of an existing file to now. </summary>
		public override void  TouchFile(System.String name)
		{
			//     final boolean MONITOR = false;
			
			RAMFile file = (RAMFile) files[name];
			long ts2, ts1 = (System.DateTime.Now.Ticks - 621355968000000000) / 10000;
			do 
			{
				try
				{
					System.Threading.Thread.Sleep(new System.TimeSpan((System.Int64) 10000 * 0 + 100 * 1));
				}
				catch (System.Threading.ThreadInterruptedException)
				{
				}
				ts2 = (System.DateTime.Now.Ticks - 621355968000000000) / 10000;
				//       if (MONITOR) {
				//         count++;
				//       }
			}
			while (ts1 == ts2);
			
			file.lastModified = ts2;
			
			//     if (MONITOR)
			//         System.out.println("SLEEP COUNT: " + count);
		}
		
		/// <summary>Returns the length in bytes of a file in the directory. </summary>
		public override long FileLength(System.String name)
		{
			RAMFile file = (RAMFile) files[name];
			return file.length;
		}
		
		/// <summary>Removes an existing file in the directory. </summary>
		public override void  DeleteFile(System.String name)
		{
			files.Remove(name);
		}
		
		/// <summary>Removes an existing file in the directory. </summary>
		public override void  RenameFile(System.String from, System.String to)
		{
			RAMFile file = (RAMFile) files[from];
			files.Remove(from);
			files[to] = file;
		}
		
		/// <summary>Creates a new, empty file in the directory with the given name.
		/// Returns a stream writing this file. 
		/// </summary>
		public override OutputStream CreateFile(System.String name)
		{
			RAMFile file = new RAMFile();
			files[name] = file;
			return new RAMOutputStream(file);
		}
		
		/// <summary>Returns a stream reading an existing file. </summary>
		public override InputStream OpenFile(System.String name)
		{
			RAMFile file = (RAMFile) files[name];
			return new RAMInputStream(file);
		}
		
		/// <summary>Construct a {@link Lock}.</summary>
		/// <param name="name">the name of the lock file
		/// </param>
		public override Lock MakeLock(System.String name)
		{
			return new AnonymousClassLock(name, this);
		}
		
		/// <summary>Closes the store to future operations. </summary>
		public override void  Close()
		{
		}
	}
}