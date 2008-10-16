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
	
	/// <summary>A Directory is a flat list of files.  Files may be written once, when they
	/// are created.  Once a file is created it may only be opened for read, or
	/// deleted.  Random access is permitted both when reading and writing.
	/// 
	/// <p> Java's i/o APIs not used directly, but rather all i/o is
	/// through this API.  This permits things such as: <ul>
	/// <li> implementation of RAM-based indices;
	/// <li> implementation indices stored in a database, via JDBC;
	/// <li> implementation of an index as a single file;
	/// </ul>
	/// 
	/// </summary>
	/// <author>  Doug Cutting
	/// </author>
	public abstract class Directory
	{
		/// <summary>Returns an array of strings, one for each file in the directory. </summary>
		public abstract System.String[] List();
		
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
		/// This replacement should be atomic. 
		/// </summary>
		public abstract void  RenameFile(System.String from, System.String to);
		
		/// <summary>Returns the length of a file in the directory. </summary>
		public abstract long FileLength(System.String name);
		
		/// <summary>Creates a new, empty file in the directory with the given name.
		/// Returns a stream writing this file. 
		/// </summary>
		public abstract OutputStream CreateFile(System.String name);
		
		/// <summary>Returns a stream reading an existing file. </summary>
		public abstract InputStream OpenFile(System.String name);
		
		/// <summary>Construct a {@link Lock}.</summary>
		/// <param name="name">the name of the lock file
		/// </param>
		public abstract Lock MakeLock(System.String name);
		
		/// <summary>Closes the store. </summary>
		public abstract void  Close();
	}
}