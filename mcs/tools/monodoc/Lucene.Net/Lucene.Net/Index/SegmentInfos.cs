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
	[Serializable]
	sealed public class SegmentInfos : System.Collections.ArrayList
	{
		
		/// <summary>The file format version, a negative number. </summary>
		/* Works since counter, the old 1st entry, is always >= 0 */
		public const int FORMAT = - 1;
		
		public int counter = 0; // used to name new segments
		private long version = 0; //counts how often the index has been changed by adding or deleting docs
		
		public SegmentInfo Info(int i)
		{
			return (SegmentInfo) this[i];
		}
		
		public void  Read(Directory directory)
		{
			
			InputStream input = directory.OpenFile("segments");
			try
			{
				int format = input.ReadInt();
				if (format < 0)
				{
					// file contains explicit format info
					// check that it is a format we can understand
					if (format < FORMAT)
						throw new System.IO.IOException("Unknown format version: " + format);
					version = input.ReadLong(); // read version
					counter = input.ReadInt(); // read counter
				}
				else
				{
					// file is in old format without explicit format info
					counter = format;
				}
				
				for (int i = input.ReadInt(); i > 0; i--)
				{
					// read segmentInfos
					SegmentInfo si = new SegmentInfo(input.ReadString(), input.ReadInt(), directory);
					Add(si);
				}
				
				if (format >= 0)
				{
					// in old format the version number may be at the end of the file
					if (input.GetFilePointer() >= input.Length())
						version = 0;
					// old file format without version number
					else
						version = input.ReadLong(); // read version
				}
			}
			finally
			{
				input.Close();
			}
		}
		
		public void  Write(Directory directory)
		{
			OutputStream output = directory.CreateFile("segments.new");
			try
			{
				output.WriteInt(FORMAT); // write FORMAT
				output.WriteLong(++version); // every write changes the index
				output.WriteInt(counter); // write counter
				output.WriteInt(Count); // write infos
				for (int i = 0; i < Count; i++)
				{
					SegmentInfo si = Info(i);
					output.WriteString(si.name);
					output.WriteInt(si.docCount);
				}
			}
			finally
			{
				output.Close();
			}
			
			// install new segment info
			directory.RenameFile("segments.new", "segments");
		}
		
		/// <summary> version number when this SegmentInfos was generated.</summary>
		public long GetVersion()
		{
			return version;
		}
		
		/// <summary> Current version number from segments file.</summary>
		public static long ReadCurrentVersion(Directory directory)
		{
			
			InputStream input = directory.OpenFile("segments");
			int format = 0;
			long version = 0;
			try
			{
				format = input.ReadInt();
				if (format < 0)
				{
					if (format < FORMAT)
						throw new System.IO.IOException("Unknown format version: " + format);
					version = input.ReadLong(); // read version
				}
			}
			finally
			{
				input.Close();
			}
			
			if (format < 0)
				return version;
			
			// We cannot be sure about the format of the file.
			// Therefore we have to read the whole file and cannot simply seek to the version entry.
			
			SegmentInfos sis = new SegmentInfos();
			sis.Read(directory);
			return sis.GetVersion();
		}
	}
}