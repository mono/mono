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
	
	/// <summary> Expert: A Directory instance that switches files between
	/// two other Directory instances.
	/// <p/>Files with the specified extensions are placed in the
	/// primary directory; others are placed in the secondary
	/// directory.  The provided Set must not change once passed
	/// to this class, and must allow multiple threads to call
	/// contains at once.<p/>
	/// 
	/// <p/><b>NOTE</b>: this API is new and experimental and is
	/// subject to suddenly change in the next release.
	/// </summary>
	
	public class FileSwitchDirectory:Directory
	{
		private Directory secondaryDir;
		private Directory primaryDir;
		private System.Collections.Hashtable primaryExtensions;
		private bool doClose;
		
		public FileSwitchDirectory(System.Collections.Hashtable primaryExtensions, Directory primaryDir, Directory secondaryDir, bool doClose)
		{
			this.primaryExtensions = primaryExtensions;
			this.primaryDir = primaryDir;
			this.secondaryDir = secondaryDir;
			this.doClose = doClose;
			this.lockFactory = primaryDir.GetLockFactory();
		}
		
		/// <summary>Return the primary directory </summary>
		public virtual Directory GetPrimaryDir()
		{
			return primaryDir;
		}
		
		/// <summary>Return the secondary directory </summary>
		public virtual Directory GetSecondaryDir()
		{
			return secondaryDir;
		}
		
		public override void  Close()
		{
			if (doClose)
			{
				try
				{
					secondaryDir.Close();
				}
				finally
				{
					primaryDir.Close();
				}
				doClose = false;
			}
		}

        /// <summary>
        /// .NET
        /// </summary>
        public override void Dispose()
        {
            Close();
        }
		
		public override System.String[] ListAll()
		{
            System.Collections.Generic.List<string> files = new System.Collections.Generic.List<string>();
            files.AddRange(primaryDir.ListAll());
            files.AddRange(secondaryDir.ListAll());
            return files.ToArray();
		}

        [Obsolete("Mono.Lucene.Net-2.9.1. This method overrides obsolete member Mono.Lucene.Net.Store.Directory.List()")]
		public override System.String[] List()
		{
			return ListAll();
		}
		
		/// <summary>Utility method to return a file's extension. </summary>
		public static System.String GetExtension(System.String name)
		{
			int i = name.LastIndexOf('.');
			if (i == - 1)
			{
				return "";
			}
			return name.Substring(i + 1, (name.Length) - (i + 1));
		}
		
		private Directory GetDirectory(System.String name)
		{
			System.String ext = GetExtension(name);
			if (primaryExtensions.Contains(ext))
			{
				return primaryDir;
			}
			else
			{
				return secondaryDir;
			}
		}
		
		public override bool FileExists(System.String name)
		{
			return GetDirectory(name).FileExists(name);
		}
		
		public override long FileModified(System.String name)
		{
			return GetDirectory(name).FileModified(name);
		}
		
		public override void  TouchFile(System.String name)
		{
			GetDirectory(name).TouchFile(name);
		}
		
		public override void  DeleteFile(System.String name)
		{
			GetDirectory(name).DeleteFile(name);
		}

        [Obsolete("Mono.Lucene.Net-2.9.1. This method overrides obsolete member Mono.Lucene.Net.Store.Directory.RenameFile(string, string)")]
		public override void  RenameFile(System.String from, System.String to)
		{
			GetDirectory(from).RenameFile(from, to);
		}
		
		public override long FileLength(System.String name)
		{
			return GetDirectory(name).FileLength(name);
		}
		
		public override IndexOutput CreateOutput(System.String name)
		{
			return GetDirectory(name).CreateOutput(name);
		}
		
		public override void  Sync(System.String name)
		{
			GetDirectory(name).Sync(name);
		}
		
		public override IndexInput OpenInput(System.String name)
		{
			return GetDirectory(name).OpenInput(name);
		}
	}
}
