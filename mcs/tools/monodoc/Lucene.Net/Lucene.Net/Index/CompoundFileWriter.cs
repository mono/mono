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
using Directory = Monodoc.Lucene.Net.Store.Directory;
using InputStream = Monodoc.Lucene.Net.Store.InputStream;
using OutputStream = Monodoc.Lucene.Net.Store.OutputStream;
namespace Monodoc.Lucene.Net.Index
{
	
	
	/// <summary> Combines multiple files into a single compound file.
	/// The file format:<br>
	/// <ul>
	/// <li>VInt fileCount</li>
	/// <li>{Directory}
	/// fileCount entries with the following structure:</li>
	/// <ul>
	/// <li>long dataOffset</li>
	/// <li>UTFString extension</li>
	/// </ul>
	/// <li>{File Data}
	/// fileCount entries with the raw data of the corresponding file</li>
	/// </ul>
	/// 
	/// The fileCount integer indicates how many files are contained in this compound
	/// file. The {directory} that follows has that many entries. Each directory entry
	/// contains an encoding identifier, an long pointer to the start of this file's
	/// data section, and a UTF String with that file's extension.
	/// 
	/// </summary>
	/// <author>  Dmitry Serebrennikov
	/// </author>
	/// <version>  $Id: CompoundFileWriter.java,v 1.3 2004/03/29 22:48:02 cutting Exp $
	/// </version>
	sealed public class CompoundFileWriter
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
		
		
		/// <summary>Create the compound stream in the specified file. The file name is the
		/// entire name (no extensions are added).
		/// </summary>
		public CompoundFileWriter(Directory dir, System.String name)
		{
			if (dir == null)
				throw new System.ArgumentException("Missing directory");
			if (name == null)
				throw new System.ArgumentException("Missing name");
			
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
		
		/// <summary>Add a source stream. If sourceDir is null, it is set to the
		/// same value as the directory where this compound stream exists.
		/// The id is the string by which the sub-stream will be know in the
		/// compound stream. The caller must ensure that the ID is unique. If the
		/// id is null, it is set to the name of the source file.
		/// </summary>
		public void  AddFile(System.String file)
		{
			if (merged)
				throw new System.SystemException("Can't add extensions after merge has been called");
			
			if (file == null)
				throw new System.ArgumentException("Missing source file");
			
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
		public void  Close()
		{
			if (merged)
				throw new System.SystemException("Merge already performed");
			
			if ((entries.Count == 0))
				throw new System.SystemException("No entries to merge have been defined");
			
			merged = true;
			
			// open the compound stream
			OutputStream os = null;
			try
			{
				os = directory.CreateFile(fileName);
				
				// Write the number of entries
				os.WriteVInt(entries.Count);
				
				// Write the directory with all offsets at 0.
				// Remember the positions of directory entries so that we can
				// adjust the offsets later
				System.Collections.IEnumerator it = entries.GetEnumerator();
				while (it.MoveNext())
				{
					FileEntry fe = (FileEntry) it.Current;
					fe.directoryOffset = os.GetFilePointer();
					os.WriteLong(0); // for now
					os.WriteString(fe.file);
				}
				
				// Open the files and copy their data into the stream.
				// Remeber the locations of each file's data section.
				byte[] buffer = new byte[1024];
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
				
				// Close the output stream. Set the os to null before trying to
				// close so that if an exception occurs during the close, the
				// finally clause below will not attempt to close the stream
				// the second time.
				OutputStream tmp = os;
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
					catch (System.IO.IOException)
					{
					}
			}
		}
		
		/// <summary>Copy the contents of the file with specified extension into the
		/// provided output stream. Use the provided buffer for moving data
		/// to reduce memory allocation.
		/// </summary>
		private void  CopyFile(FileEntry source, OutputStream os, byte[] buffer)
		{
			InputStream is_Renamed = null;
			try
			{
				long startPtr = os.GetFilePointer();
				
				is_Renamed = directory.OpenFile(source.file);
				long length = is_Renamed.Length();
				long remainder = length;
				int chunk = buffer.Length;
				
				while (remainder > 0)
				{
					int len = (int) System.Math.Min(chunk, remainder);
					is_Renamed.ReadBytes(buffer, 0, len);
					os.WriteBytes(buffer, len);
					remainder -= len;
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
