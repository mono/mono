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
	
	/// <summary> This class keeps track of closing the underlying directory. It is used to wrap
	/// DirectoryReaders, that are created using a String/File parameter
	/// in IndexReader.open() with FSDirectory.getDirectory().
	/// </summary>
	/// <deprecated> This helper class is removed with all String/File
	/// IndexReader.open() methods in Lucene 3.0
	/// </deprecated>
    [Obsolete("This helper class is removed with all String/File IndexReader.open() methods in Lucene 3.0")]
	sealed class DirectoryOwningReader:FilterIndexReader, System.ICloneable
	{
		
		internal DirectoryOwningReader(IndexReader in_Renamed):base(in_Renamed)
		{
			this.ref_Renamed = new SegmentReader.Ref();
			System.Diagnostics.Debug.Assert(this.ref_Renamed.RefCount() == 1);
		}
		
		private DirectoryOwningReader(IndexReader in_Renamed, SegmentReader.Ref ref_Renamed):base(in_Renamed)
		{
			this.ref_Renamed = ref_Renamed;
			ref_Renamed.IncRef();
		}
		
		public override IndexReader Reopen()
		{
			EnsureOpen();
			IndexReader r = in_Renamed.Reopen();
			if (r != in_Renamed)
				return new DirectoryOwningReader(r, ref_Renamed);
			return this;
		}
		
		public override IndexReader Reopen(bool openReadOnly)
		{
			EnsureOpen();
			IndexReader r = in_Renamed.Reopen(openReadOnly);
			if (r != in_Renamed)
				return new DirectoryOwningReader(r, ref_Renamed);
			return this;
		}
		
		public override IndexReader Reopen(IndexCommit commit)
		{
			EnsureOpen();
			IndexReader r = in_Renamed.Reopen(commit);
			if (r != in_Renamed)
				return new DirectoryOwningReader(r, ref_Renamed);
			return this;
		}
		
		public override System.Object Clone()
		{
			EnsureOpen();
			return new DirectoryOwningReader((IndexReader) in_Renamed.Clone(), ref_Renamed);
		}
		
		public override IndexReader Clone(bool openReadOnly)
		{
			EnsureOpen();
			return new DirectoryOwningReader(in_Renamed.Clone(openReadOnly), ref_Renamed);
		}
		
		protected internal override void  DoClose()
		{
			System.IO.IOException ioe = null;
			// close the reader, record exception
			try
			{
				base.DoClose();
			}
			catch (System.IO.IOException e)
			{
				ioe = e;
			}
			// close the directory, record exception
			if (ref_Renamed.DecRef() == 0)
			{
				try
				{
					in_Renamed.Directory().Close();
				}
				catch (System.IO.IOException e)
				{
					if (ioe == null)
						ioe = e;
				}
			}
			// throw the first exception
			if (ioe != null)
				throw ioe;
		}
		
		/// <summary> This member contains the ref counter, that is passed to each instance after cloning/reopening,
		/// and is global to all DirectoryOwningReader derived from the original one.
		/// This reuses the class {@link SegmentReader.Ref}
		/// </summary>
		private SegmentReader.Ref ref_Renamed;
	}
}
