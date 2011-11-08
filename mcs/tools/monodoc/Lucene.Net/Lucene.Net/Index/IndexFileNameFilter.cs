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

namespace Mono.Lucene.Net.Index
{
	
	/// <summary> Filename filter that accept filenames and extensions only created by Lucene.
	/// 
	/// </summary>
	/// <version>  $rcs = ' $Id: Exp $ ' ;
	/// </version>
	public class IndexFileNameFilter
	{
		
		private static IndexFileNameFilter singleton = new IndexFileNameFilter();
        private System.Collections.Hashtable extensions;
        private System.Collections.Hashtable extensionsInCFS;
		
		// Prevent instantiation.
		private IndexFileNameFilter()
		{
            extensions = new System.Collections.Hashtable();
			for (int i = 0; i < IndexFileNames.INDEX_EXTENSIONS.Length; i++)
			{
				extensions.Add(IndexFileNames.INDEX_EXTENSIONS[i], IndexFileNames.INDEX_EXTENSIONS[i]);
			}
            extensionsInCFS = new System.Collections.Hashtable();
			for (int i = 0; i < IndexFileNames.INDEX_EXTENSIONS_IN_COMPOUND_FILE.Length; i++)
			{
				extensionsInCFS.Add(IndexFileNames.INDEX_EXTENSIONS_IN_COMPOUND_FILE[i], IndexFileNames.INDEX_EXTENSIONS_IN_COMPOUND_FILE[i]);
			}
		}
		
		/* (non-Javadoc)
		* @see java.io.FilenameFilter#accept(java.io.File, java.lang.String)
		*/
		public virtual bool Accept(System.IO.FileInfo dir, System.String name)
		{
			int i = name.LastIndexOf((System.Char) '.');
			if (i != - 1)
			{
				System.String extension = name.Substring(1 + i);
				if (extensions.Contains(extension))
				{
					return true;
				}
				else if (extension.StartsWith("f") && (new System.Text.RegularExpressions.Regex("f\\d+")).Match(extension).Success)
				{
					return true;
				}
				else if (extension.StartsWith("s") && (new System.Text.RegularExpressions.Regex("s\\d+")).Match(extension).Success)
				{
					return true;
				}
			}
			else
			{
				if (name.Equals(IndexFileNames.DELETABLE))
					return true;
				else if (name.StartsWith(IndexFileNames.SEGMENTS))
					return true;
			}
			return false;
		}
		
		/// <summary> Returns true if this is a file that would be contained
		/// in a CFS file.  This function should only be called on
		/// files that pass the above "accept" (ie, are already
		/// known to be a Lucene index file).
		/// </summary>
		public virtual bool IsCFSFile(System.String name)
		{
			int i = name.LastIndexOf((System.Char) '.');
			if (i != - 1)
			{
				System.String extension = name.Substring(1 + i);
				if (extensionsInCFS.Contains(extension))
				{
					return true;
				}
				if (extension.StartsWith("f") && (new System.Text.RegularExpressions.Regex("f\\d+")).Match(extension).Success)
				{
					return true;
				}
			}
			return false;
		}
		
		public static IndexFileNameFilter GetFilter()
		{
			return singleton;
		}
	}
}
