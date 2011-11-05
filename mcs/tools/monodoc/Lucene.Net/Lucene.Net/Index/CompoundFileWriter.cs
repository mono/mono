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

using Directory = Mono.Lucene.Net.Store.Directory;
using IndexInput = Mono.Lucene.Net.Store.IndexInput;
using IndexOutput = Mono.Lucene.Net.Store.IndexOutput;

namespace Mono.Lucene.Net.Index
{
	
	
	/// <summary> Combines multiple files into a single compound file.
	/// The file format:<br/>
	/// <ul>
	/// <li>VInt fileCount</li>
	/// <li>{Directory}
	/// fileCount entries with the following structure:</li>
	/// <ul>
	/// <li>long dataOffset</li>
	/// <li>String fileName</li>
	/// </ul>
	/// <li>{File Data}
	/// fileCount entries with the raw data of the corresponding file</li>
	/// </ul>
	/// 
	/// The fileCount integer indicates how many files are contained in this compound
	/// file. The {directory} that follows has that many entries. Each directory entry
	/// contains a long pointer to the start of this file's data section, and a String
	/// with that file's name.
	/// 
	/// 
	/// </summary>
	/// <version>  $Id: CompoundFileWriter.java 690539 2008-08-30 17:33:06Z mikemccand $
	/// </version>
	public sealed class CompoundFileWriter
	{
		
		private sealed class FileEntry
		{
			/// <summary>source file </summary>
			internal System.String file;
			
			/// <summary>temporary holder for the start of directory entry for this file </summary>
			internal long directoryOffset;
			
			/// <summary>temporary holder for the start of this file's data section </summary>
			internal long dataOffset;
		}
		
		
		private Directory directory;
		private System.String fileName;
        private System.Collections.Hashtable ids;
		private System.Collections.ArrayList entries;
		private bool merged = false;
		private SegmentMerger.CheckAbort checkAbort;
		
		/// <summary>Create the compound stream in the specified file. The file name is the
		/// entire name (no extensions are added).
		/// </summary>
		/// <throws>  NullPointerException if <code>dir</code> or <code>name</code> is null </throws>
		public CompoundFileWriter(Directory dir, System.String name):this(dir, name, null)
		{
		}
		
		internal CompoundFileWriter(Directory dir, System.String name, SegmentMerger.CheckAbort checkAbort)
		{
			if (dir == null)
				throw new System.NullReferenceException("directory cannot be null");
			if (name == null)
				throw new System.NullReferenceException("name cannot be null");
			this.checkAbort = checkAbort;
			directory = dir;
			fileName = name;
            ids = new System.Collections.Hashtable();
			entries = new System.Collections.ArrayList();
		}
		
		/// <summary>Returns the directory of the compound file. </summary>
		public Directory GetDirectory()
		{
			return directory;
		}
		
		/// <summary>Returns the name of the compound file. </summary>
		public System.String GetName()
		{
			return fileName;
		}
		
		/// <summary>Add a source stream. <code>file</code> is the string by which the 
		/// sub-stream will be known in the compound stream.
		/// 
		/// </summary>
		/// <throws>  IllegalStateException if this writer is closed </throws>
		/// <throws>  NullPointerException if <code>file</code> is null </throws>
		/// <throws>  IllegalArgumentException if a file with the same name </throws>
		/// <summary>   has been added already
		/// </summary>
		public void  AddFile(System.String file)
		{
			if (merged)
				throw new System.SystemException("Can't add extensions after merge has been called");
			
			if (file == null)
				throw new System.NullReferenceException("file cannot be null");
			
            try
            {
                ids.Add(file, file);
            }
            catch (Exception)
            {
				throw new System.ArgumentException("File " + file + " already added");
            }
			
			FileEntry entry = new FileEntry();
			entry.file = file;
			entries.Add(entry);
		}
		
		/// <summary>Merge files with the extensions added up to now.
		/// All files with these extensions are combined sequentially into the
		/// compound stream. After successful merge, the source files
		/// are deleted.
		/// </summary>
		/// <throws>  IllegalStateException if close() had been called before or </throws>
		/// <summary>   if no file has been added to this object
		/// </summary>
		public void  Close()
		{
			if (merged)
				throw new System.SystemException("Merge already performed");
			
			if ((entries.Count == 0))
				throw new System.SystemException("No entries to merge have been defined");
			
			merged = true;
			
			// open the compound stream
			IndexOutput os = null;
			try
			{
				os = directory.CreateOutput(fileName);
				
				// Write the number of entries
				os.WriteVInt(entries.Count);
				
				// Write the directory with all offsets at 0.
				// Remember the positions of directory entries so that we can
				// adjust the offsets later
				System.Collections.IEnumerator it = entries.GetEnumerator();
				long totalSize = 0;
				while (it.MoveNext())
				{
					FileEntry fe = (FileEntry) it.Current;
					fe.directoryOffset = os.GetFilePointer();
					os.WriteLong(0); // for now
					os.WriteString(fe.file);
					totalSize += directory.FileLength(fe.file);
				}
				
				// Pre-allocate size of file as optimization --
				// this can potentially help IO performance as
				// we write the file and also later during
				// searching.  It also uncovers a disk-full
				// situation earlier and hopefully without
				// actually filling disk to 100%:
				long finalLength = totalSize + os.GetFilePointer();
				os.SetLength(finalLength);
				
				// Open the files and copy their data into the stream.
				// Remember the locations of each file's data section.
				byte[] buffer = new byte[16384];
				it = entries.GetEnumerator();
				while (it.MoveNext())
				{
					FileEntry fe = (FileEntry) it.Current;
					fe.dataOffset = os.GetFilePointer();
					CopyFile(fe, os, buffer);
				}
				
				// Write the data offsets into the directory of the compound stream
				it = entries.GetEnumerator();
				while (it.MoveNext())
				{
					FileEntry fe = (FileEntry) it.Current;
					os.Seek(fe.directoryOffset);
					os.WriteLong(fe.dataOffset);
				}
				
				System.Diagnostics.Debug.Assert(finalLength == os.Length());
				
				// Close the output stream. Set the os to null before trying to
				// close so that if an exception occurs during the close, the
				// finally clause below will not attempt to close the stream
				// the second time.
				IndexOutput tmp = os;
				os = null;
				tmp.Close();
			}
			finally
			{
				if (os != null)
					try
					{
						os.Close();
					}
					catch (System.IO.IOException e)
					{
					}
			}
		}
		
		/// <summary>Copy the contents of the file with specified extension into the
		/// provided output stream. Use the provided buffer for moving data
		/// to reduce memory allocation.
		/// </summary>
		private void  CopyFile(FileEntry source, IndexOutput os, byte[] buffer)
		{
			IndexInput is_Renamed = null;
			try
			{
				long startPtr = os.GetFilePointer();
				
				is_Renamed = directory.OpenInput(source.file);
				long length = is_Renamed.Length();
				long remainder = length;
				int chunk = buffer.Length;
				
				while (remainder > 0)
				{
					int len = (int) System.Math.Min(chunk, remainder);
					is_Renamed.ReadBytes(buffer, 0, len, false);
					os.WriteBytes(buffer, len);
					remainder -= len;
					if (checkAbort != null)
					// Roughly every 2 MB we will check if
					// it's time to abort
						checkAbort.Work(80);
				}
				
				// Verify that remainder is 0
				if (remainder != 0)
					throw new System.IO.IOException("Non-zero remainder length after copying: " + remainder + " (id: " + source.file + ", length: " + length + ", buffer size: " + chunk + ")");
				
				// Verify that the output length diff is equal to original file
				long endPtr = os.GetFilePointer();
				long diff = endPtr - startPtr;
				if (diff != length)
					throw new System.IO.IOException("Difference in the output file offsets " + diff + " does not match the original file length " + length);
			}
			finally
			{
				if (is_Renamed != null)
					is_Renamed.Close();
			}
		}
	}
}
