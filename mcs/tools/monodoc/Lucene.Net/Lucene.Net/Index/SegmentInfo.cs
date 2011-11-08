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
using BitVector = Mono.Lucene.Net.Util.BitVector;

namespace Mono.Lucene.Net.Index
{
	
	/// <summary> Information about a segment such as it's name, directory, and files related
	/// to the segment.
	/// 
	/// * <p/><b>NOTE:</b> This API is new and still experimental
	/// (subject to change suddenly in the next release)<p/>
	/// </summary>
	public sealed class SegmentInfo : System.ICloneable
	{
		
		internal const int NO = - 1; // e.g. no norms; no deletes;
		internal const int YES = 1; // e.g. have norms; have deletes;
		internal const int CHECK_DIR = 0; // e.g. must check dir to see if there are norms/deletions
		internal const int WITHOUT_GEN = 0; // a file name that has no GEN in it. 
		
		public System.String name; // unique name in dir
		public int docCount; // number of docs in seg
		public Directory dir; // where segment resides
		
		private bool preLockless; // true if this is a segments file written before
		// lock-less commits (2.1)
		
		private long delGen; // current generation of del file; NO if there
		// are no deletes; CHECK_DIR if it's a pre-2.1 segment
		// (and we must check filesystem); YES or higher if
		// there are deletes at generation N
		
		private long[] normGen; // current generation of each field's norm file.
		// If this array is null, for lockLess this means no 
		// separate norms.  For preLockLess this means we must 
		// check filesystem. If this array is not null, its 
		// values mean: NO says this field has no separate  
		// norms; CHECK_DIR says it is a preLockLess segment and    
		// filesystem must be checked; >= YES says this field  
		// has separate norms with the specified generation
		
		private sbyte isCompoundFile; // NO if it is not; YES if it is; CHECK_DIR if it's
		// pre-2.1 (ie, must check file system to see
		// if <name>.cfs and <name>.nrm exist)         
		
		private bool hasSingleNormFile; // true if this segment maintains norms in a single file; 
		// false otherwise
		// this is currently false for segments populated by DocumentWriter
		// and true for newly created merged segments (both
		// compound and non compound).
		
		private System.Collections.Generic.IList<string> files; // cached list of files that this segment uses
		// in the Directory
		
		internal long sizeInBytes = - 1; // total byte size of all of our files (computed on demand)
		
		private int docStoreOffset; // if this segment shares stored fields & vectors, this
		// offset is where in that file this segment's docs begin
		private System.String docStoreSegment; // name used to derive fields/vectors file we share with
		// other segments
		private bool docStoreIsCompoundFile; // whether doc store files are stored in compound file (*.cfx)
		
		private int delCount; // How many deleted docs in this segment, or -1 if not yet known
		// (if it's an older index)
		
		private bool hasProx; // True if this segment has any fields with omitTermFreqAndPositions==false

        private System.Collections.Generic.IDictionary<string, string> diagnostics;
		
		public override System.String ToString()
		{
			return "si: " + dir.ToString() + " " + name + " docCount: " + docCount + " delCount: " + delCount + " delFileName: " + GetDelFileName();
		}
		
		public SegmentInfo(System.String name, int docCount, Directory dir)
		{
			this.name = name;
			this.docCount = docCount;
			this.dir = dir;
			delGen = NO;
			isCompoundFile = (sbyte) (CHECK_DIR);
			preLockless = true;
			hasSingleNormFile = false;
			docStoreOffset = - 1;
			docStoreSegment = name;
			docStoreIsCompoundFile = false;
			delCount = 0;
			hasProx = true;
		}
		
		public SegmentInfo(System.String name, int docCount, Directory dir, bool isCompoundFile, bool hasSingleNormFile):this(name, docCount, dir, isCompoundFile, hasSingleNormFile, - 1, null, false, true)
		{
		}
		
		public SegmentInfo(System.String name, int docCount, Directory dir, bool isCompoundFile, bool hasSingleNormFile, int docStoreOffset, System.String docStoreSegment, bool docStoreIsCompoundFile, bool hasProx):this(name, docCount, dir)
		{
			this.isCompoundFile = (sbyte) (isCompoundFile?YES:NO);
			this.hasSingleNormFile = hasSingleNormFile;
			preLockless = false;
			this.docStoreOffset = docStoreOffset;
			this.docStoreSegment = docStoreSegment;
			this.docStoreIsCompoundFile = docStoreIsCompoundFile;
			this.hasProx = hasProx;
			delCount = 0;
			System.Diagnostics.Debug.Assert(docStoreOffset == - 1 || docStoreSegment != null, "dso=" + docStoreOffset + " dss=" + docStoreSegment + " docCount=" + docCount);
		}
		
		/// <summary> Copy everything from src SegmentInfo into our instance.</summary>
		internal void  Reset(SegmentInfo src)
		{
			ClearFiles();
			name = src.name;
			docCount = src.docCount;
			dir = src.dir;
			preLockless = src.preLockless;
			delGen = src.delGen;
			docStoreOffset = src.docStoreOffset;
			docStoreIsCompoundFile = src.docStoreIsCompoundFile;
			if (src.normGen == null)
			{
				normGen = null;
			}
			else
			{
				normGen = new long[src.normGen.Length];
				Array.Copy(src.normGen, 0, normGen, 0, src.normGen.Length);
			}
			isCompoundFile = src.isCompoundFile;
			hasSingleNormFile = src.hasSingleNormFile;
			delCount = src.delCount;
		}
		
		// must be Map<String, String>
        internal void SetDiagnostics(System.Collections.Generic.IDictionary<string, string> diagnostics)
		{
			this.diagnostics = diagnostics;
		}
		
		// returns Map<String, String>
        public System.Collections.Generic.IDictionary<string, string> GetDiagnostics()
		{
			return diagnostics;
		}
		
		/// <summary> Construct a new SegmentInfo instance by reading a
		/// previously saved SegmentInfo from input.
		/// 
		/// </summary>
		/// <param name="dir">directory to load from
		/// </param>
		/// <param name="format">format of the segments info file
		/// </param>
		/// <param name="input">input handle to read segment info from
		/// </param>
		internal SegmentInfo(Directory dir, int format, IndexInput input)
		{
			this.dir = dir;
			name = input.ReadString();
			docCount = input.ReadInt();
			if (format <= SegmentInfos.FORMAT_LOCKLESS)
			{
				delGen = input.ReadLong();
				if (format <= SegmentInfos.FORMAT_SHARED_DOC_STORE)
				{
					docStoreOffset = input.ReadInt();
					if (docStoreOffset != - 1)
					{
						docStoreSegment = input.ReadString();
						docStoreIsCompoundFile = (1 == input.ReadByte());
					}
					else
					{
						docStoreSegment = name;
						docStoreIsCompoundFile = false;
					}
				}
				else
				{
					docStoreOffset = - 1;
					docStoreSegment = name;
					docStoreIsCompoundFile = false;
				}
				if (format <= SegmentInfos.FORMAT_SINGLE_NORM_FILE)
				{
					hasSingleNormFile = (1 == input.ReadByte());
				}
				else
				{
					hasSingleNormFile = false;
				}
				int numNormGen = input.ReadInt();
				if (numNormGen == NO)
				{
					normGen = null;
				}
				else
				{
					normGen = new long[numNormGen];
					for (int j = 0; j < numNormGen; j++)
					{
						normGen[j] = input.ReadLong();
					}
				}
				isCompoundFile = (sbyte) input.ReadByte();
				preLockless = (isCompoundFile == CHECK_DIR);
				if (format <= SegmentInfos.FORMAT_DEL_COUNT)
				{
					delCount = input.ReadInt();
					System.Diagnostics.Debug.Assert(delCount <= docCount);
				}
				else
					delCount = - 1;
				if (format <= SegmentInfos.FORMAT_HAS_PROX)
					hasProx = input.ReadByte() == 1;
				else
					hasProx = true;
				
				if (format <= SegmentInfos.FORMAT_DIAGNOSTICS)
				{
					diagnostics = input.ReadStringStringMap();
				}
				else
				{
					diagnostics = new System.Collections.Generic.Dictionary<string,string>();
				}
			}
			else
			{
				delGen = CHECK_DIR;
				normGen = null;
				isCompoundFile = (sbyte) (CHECK_DIR);
				preLockless = true;
				hasSingleNormFile = false;
				docStoreOffset = - 1;
				docStoreIsCompoundFile = false;
				docStoreSegment = null;
				delCount = - 1;
				hasProx = true;
				diagnostics = new System.Collections.Generic.Dictionary<string,string>();
			}
		}
		
		internal void  SetNumFields(int numFields)
		{
			if (normGen == null)
			{
				// normGen is null if we loaded a pre-2.1 segment
				// file, or, if this segments file hasn't had any
				// norms set against it yet:
				normGen = new long[numFields];
				
				if (preLockless)
				{
					// Do nothing: thus leaving normGen[k]==CHECK_DIR (==0), so that later we know  
					// we have to check filesystem for norm files, because this is prelockless.
				}
				else
				{
					// This is a FORMAT_LOCKLESS segment, which means
					// there are no separate norms:
					for (int i = 0; i < numFields; i++)
					{
						normGen[i] = NO;
					}
				}
			}
		}
		
		/// <summary>Returns total size in bytes of all of files used by
		/// this segment. 
		/// </summary>
		public long SizeInBytes()
		{
			if (sizeInBytes == - 1)
			{
				System.Collections.Generic.IList<string> files = Files();
				int size = files.Count;
				sizeInBytes = 0;
				for (int i = 0; i < size; i++)
				{
					System.String fileName = (System.String) files[i];
					// We don't count bytes used by a shared doc store
					// against this segment:
					if (docStoreOffset == - 1 || !IndexFileNames.IsDocStoreFile(fileName))
						sizeInBytes += dir.FileLength(fileName);
				}
			}
			return sizeInBytes;
		}
		
		public bool HasDeletions()
		{
			// Cases:
			//
			//   delGen == NO: this means this segment was written
			//     by the LOCKLESS code and for certain does not have
			//     deletions yet
			//
			//   delGen == CHECK_DIR: this means this segment was written by
			//     pre-LOCKLESS code which means we must check
			//     directory to see if .del file exists
			//
			//   delGen >= YES: this means this segment was written by
			//     the LOCKLESS code and for certain has
			//     deletions
			//
			if (delGen == NO)
			{
				return false;
			}
			else if (delGen >= YES)
			{
				return true;
			}
			else
			{
				return dir.FileExists(GetDelFileName());
			}
		}
		
		internal void  AdvanceDelGen()
		{
			// delGen 0 is reserved for pre-LOCKLESS format
			if (delGen == NO)
			{
				delGen = YES;
			}
			else
			{
				delGen++;
			}
			ClearFiles();
		}
		
		internal void  ClearDelGen()
		{
			delGen = NO;
			ClearFiles();
		}
		
		public System.Object Clone()
		{
			SegmentInfo si = new SegmentInfo(name, docCount, dir);
			si.isCompoundFile = isCompoundFile;
			si.delGen = delGen;
			si.delCount = delCount;
			si.hasProx = hasProx;
			si.preLockless = preLockless;
			si.hasSingleNormFile = hasSingleNormFile;
            if (this.diagnostics != null)
            {
                si.diagnostics = new System.Collections.Generic.Dictionary<string, string>();
                foreach (string o in diagnostics.Keys)
                {
                    si.diagnostics.Add(o,diagnostics[o]);
                }
            }
			if (normGen != null)
			{
				si.normGen = new long[normGen.Length];
				normGen.CopyTo(si.normGen, 0);
			}
			si.docStoreOffset = docStoreOffset;
			si.docStoreSegment = docStoreSegment;
			si.docStoreIsCompoundFile = docStoreIsCompoundFile;
            if (this.files != null)
            {
                si.files = new System.Collections.Generic.List<string>();
                foreach (string file in files)
                {
                    si.files.Add(file);
                }
            }
            
			return si;
		}
		
		public System.String GetDelFileName()
		{
			if (delGen == NO)
			{
				// In this case we know there is no deletion filename
				// against this segment
				return null;
			}
			else
			{
				// If delGen is CHECK_DIR, it's the pre-lockless-commit file format
				return IndexFileNames.FileNameFromGeneration(name, "." + IndexFileNames.DELETES_EXTENSION, delGen);
			}
		}

        /// <summary> Returns true if this field for this segment has saved a separate norms file (_&lt;segment&gt;_N.sX).
		/// 
		/// </summary>
		/// <param name="fieldNumber">the field index to check
		/// </param>
		public bool HasSeparateNorms(int fieldNumber)
		{
			if ((normGen == null && preLockless) || (normGen != null && normGen[fieldNumber] == CHECK_DIR))
			{
				// Must fallback to directory file exists check:
				System.String fileName = name + ".s" + fieldNumber;
				return dir.FileExists(fileName);
			}
			else if (normGen == null || normGen[fieldNumber] == NO)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
		
		/// <summary> Returns true if any fields in this segment have separate norms.</summary>
		public bool HasSeparateNorms()
		{
			if (normGen == null)
			{
				if (!preLockless)
				{
					// This means we were created w/ LOCKLESS code and no
					// norms are written yet:
					return false;
				}
				else
				{
					// This means this segment was saved with pre-LOCKLESS
					// code.  So we must fallback to the original
					// directory list check:
					System.String[] result = dir.List();
					if (result == null)
					{
						throw new System.IO.IOException("cannot read directory " + dir + ": list() returned null");
					}
					
					System.String pattern;
					pattern = name + ".s";
					int patternLength = pattern.Length;
					for (int i = 0; i < result.Length; i++)
					{
						if (result[i].StartsWith(pattern) && System.Char.IsDigit(result[i][patternLength]))
							return true;
					}
					return false;
				}
			}
			else
			{
				// This means this segment was saved with LOCKLESS
				// code so we first check whether any normGen's are >= 1
				// (meaning they definitely have separate norms):
				for (int i = 0; i < normGen.Length; i++)
				{
					if (normGen[i] >= YES)
					{
						return true;
					}
				}
				// Next we look for any == 0.  These cases were
				// pre-LOCKLESS and must be checked in directory:
				for (int i = 0; i < normGen.Length; i++)
				{
					if (normGen[i] == CHECK_DIR)
					{
						if (HasSeparateNorms(i))
						{
							return true;
						}
					}
				}
			}
			
			return false;
		}
		
		/// <summary> Increment the generation count for the norms file for
		/// this field.
		/// 
		/// </summary>
		/// <param name="fieldIndex">field whose norm file will be rewritten
		/// </param>
		internal void  AdvanceNormGen(int fieldIndex)
		{
			if (normGen[fieldIndex] == NO)
			{
				normGen[fieldIndex] = YES;
			}
			else
			{
				normGen[fieldIndex]++;
			}
			ClearFiles();
		}
		
		/// <summary> Get the file name for the norms file for this field.
		/// 
		/// </summary>
		/// <param name="number">field index
		/// </param>
		public System.String GetNormFileName(int number)
		{
			System.String prefix;
			
			long gen;
			if (normGen == null)
			{
				gen = CHECK_DIR;
			}
			else
			{
				gen = normGen[number];
			}
			
			if (HasSeparateNorms(number))
			{
				// case 1: separate norm
				prefix = ".s";
				return IndexFileNames.FileNameFromGeneration(name, prefix + number, gen);
			}
			
			if (hasSingleNormFile)
			{
				// case 2: lockless (or nrm file exists) - single file for all norms 
				prefix = "." + IndexFileNames.NORMS_EXTENSION;
				return IndexFileNames.FileNameFromGeneration(name, prefix, WITHOUT_GEN);
			}
			
			// case 3: norm file for each field
			prefix = ".f";
			return IndexFileNames.FileNameFromGeneration(name, prefix + number, WITHOUT_GEN);
		}
		
		/// <summary> Mark whether this segment is stored as a compound file.
		/// 
		/// </summary>
		/// <param name="isCompoundFile">true if this is a compound file;
		/// else, false
		/// </param>
		internal void  SetUseCompoundFile(bool isCompoundFile)
		{
			if (isCompoundFile)
			{
				this.isCompoundFile = (sbyte) (YES);
			}
			else
			{
				this.isCompoundFile = (sbyte) (NO);
			}
			ClearFiles();
		}
		
		/// <summary> Returns true if this segment is stored as a compound
		/// file; else, false.
		/// </summary>
		public bool GetUseCompoundFile()
		{
			if (isCompoundFile == NO)
			{
				return false;
			}
			else if (isCompoundFile == YES)
			{
				return true;
			}
			else
			{
				return dir.FileExists(name + "." + IndexFileNames.COMPOUND_FILE_EXTENSION);
			}
		}
		
		public int GetDelCount()
		{
			if (delCount == - 1)
			{
				if (HasDeletions())
				{
					System.String delFileName = GetDelFileName();
					delCount = new BitVector(dir, delFileName).Count();
				}
				else
					delCount = 0;
			}
			System.Diagnostics.Debug.Assert(delCount <= docCount);
			return delCount;
		}
		
		internal void  SetDelCount(int delCount)
		{
			this.delCount = delCount;
			System.Diagnostics.Debug.Assert(delCount <= docCount);
		}
		
		public int GetDocStoreOffset()
		{
			return docStoreOffset;
		}
		
		public bool GetDocStoreIsCompoundFile()
		{
			return docStoreIsCompoundFile;
		}
		
		internal void  SetDocStoreIsCompoundFile(bool v)
		{
			docStoreIsCompoundFile = v;
			ClearFiles();
		}
		
		public System.String GetDocStoreSegment()
		{
			return docStoreSegment;
		}
		
		internal void  SetDocStoreOffset(int offset)
		{
			docStoreOffset = offset;
			ClearFiles();
		}
		
		internal void  SetDocStore(int offset, System.String segment, bool isCompoundFile)
		{
			docStoreOffset = offset;
			docStoreSegment = segment;
			docStoreIsCompoundFile = isCompoundFile;
		}
		
		/// <summary> Save this segment's info.</summary>
		internal void  Write(IndexOutput output)
		{
			output.WriteString(name);
			output.WriteInt(docCount);
			output.WriteLong(delGen);
			output.WriteInt(docStoreOffset);
			if (docStoreOffset != - 1)
			{
				output.WriteString(docStoreSegment);
				output.WriteByte((byte) (docStoreIsCompoundFile?1:0));
			}
			
			output.WriteByte((byte) (hasSingleNormFile?1:0));
			if (normGen == null)
			{
				output.WriteInt(NO);
			}
			else
			{
				output.WriteInt(normGen.Length);
				for (int j = 0; j < normGen.Length; j++)
				{
					output.WriteLong(normGen[j]);
				}
			}
			output.WriteByte((byte) isCompoundFile);
			output.WriteInt(delCount);
			output.WriteByte((byte) (hasProx?1:0));
			output.WriteStringStringMap(diagnostics);
		}
		
		internal void  SetHasProx(bool hasProx)
		{
			this.hasProx = hasProx;
			ClearFiles();
		}
		
		public bool GetHasProx()
		{
			return hasProx;
		}
		
		private void  AddIfExists(System.Collections.Generic.IList<string> files, System.String fileName)
		{
			if (dir.FileExists(fileName))
				files.Add(fileName);
		}
		
		/*
		* Return all files referenced by this SegmentInfo.  The
		* returns List is a locally cached List so you should not
		* modify it.
		*/
		
		public System.Collections.Generic.IList<string> Files()
		{
			
			if (files != null)
			{
				// Already cached:
				return files;
			}

            System.Collections.Generic.List<string> fileList = new System.Collections.Generic.List<string>();
			
			bool useCompoundFile = GetUseCompoundFile();
			
			if (useCompoundFile)
			{
                fileList.Add(name + "." + IndexFileNames.COMPOUND_FILE_EXTENSION);
			}
			else
			{
				System.String[] exts = IndexFileNames.NON_STORE_INDEX_EXTENSIONS;
				for (int i = 0; i < exts.Length; i++)
                    AddIfExists(fileList, name + "." + exts[i]);
			}
			
			if (docStoreOffset != - 1)
			{
				// We are sharing doc stores (stored fields, term
				// vectors) with other segments
				System.Diagnostics.Debug.Assert(docStoreSegment != null);
				if (docStoreIsCompoundFile)
				{
                    fileList.Add(docStoreSegment + "." + IndexFileNames.COMPOUND_FILE_STORE_EXTENSION);
				}
				else
				{
					System.String[] exts = IndexFileNames.STORE_INDEX_EXTENSIONS;
					for (int i = 0; i < exts.Length; i++)
                        AddIfExists(fileList, docStoreSegment + "." + exts[i]);
				}
			}
			else if (!useCompoundFile)
			{
				// We are not sharing, and, these files were not
				// included in the compound file
				System.String[] exts = IndexFileNames.STORE_INDEX_EXTENSIONS;
				for (int i = 0; i < exts.Length; i++)
                    AddIfExists(fileList, name + "." + exts[i]);
			}
			
			System.String delFileName = IndexFileNames.FileNameFromGeneration(name, "." + IndexFileNames.DELETES_EXTENSION, delGen);
			if (delFileName != null && (delGen >= YES || dir.FileExists(delFileName)))
			{
                fileList.Add(delFileName);
			}
			
			// Careful logic for norms files    
			if (normGen != null)
			{
				for (int i = 0; i < normGen.Length; i++)
				{
					long gen = normGen[i];
					if (gen >= YES)
					{
						// Definitely a separate norm file, with generation:
                        fileList.Add(IndexFileNames.FileNameFromGeneration(name, "." + IndexFileNames.SEPARATE_NORMS_EXTENSION + i, gen));
					}
					else if (NO == gen)
					{
						// No separate norms but maybe plain norms
						// in the non compound file case:
						if (!hasSingleNormFile && !useCompoundFile)
						{
							System.String fileName = name + "." + IndexFileNames.PLAIN_NORMS_EXTENSION + i;
							if (dir.FileExists(fileName))
							{
                                fileList.Add(fileName);
							}
						}
					}
					else if (CHECK_DIR == gen)
					{
						// Pre-2.1: we have to check file existence
						System.String fileName = null;
						if (useCompoundFile)
						{
							fileName = name + "." + IndexFileNames.SEPARATE_NORMS_EXTENSION + i;
						}
						else if (!hasSingleNormFile)
						{
							fileName = name + "." + IndexFileNames.PLAIN_NORMS_EXTENSION + i;
						}
						if (fileName != null && dir.FileExists(fileName))
						{
                            fileList.Add(fileName);
						}
					}
				}
			}
			else if (preLockless || (!hasSingleNormFile && !useCompoundFile))
			{
				// Pre-2.1: we have to scan the dir to find all
				// matching _X.sN/_X.fN files for our segment:
				System.String prefix;
				if (useCompoundFile)
					prefix = name + "." + IndexFileNames.SEPARATE_NORMS_EXTENSION;
				else
					prefix = name + "." + IndexFileNames.PLAIN_NORMS_EXTENSION;
				int prefixLength = prefix.Length;
				System.String[] allFiles = dir.ListAll();
				IndexFileNameFilter filter = IndexFileNameFilter.GetFilter();
				for (int i = 0; i < allFiles.Length; i++)
				{
					System.String fileName = allFiles[i];
					if (filter.Accept(null, fileName) && fileName.Length > prefixLength && System.Char.IsDigit(fileName[prefixLength]) && fileName.StartsWith(prefix))
					{
						fileList.Add(fileName);
					}
				}
			}
            //System.Diagnostics.Debug.Assert();
            files = fileList;
			return files;
		}
		
		/* Called whenever any change is made that affects which
		* files this segment has. */
		private void  ClearFiles()
		{
			files = null;
			sizeInBytes = - 1;
		}
		
		/// <summary>Used for debugging </summary>
		public System.String SegString(Directory dir)
		{
			System.String cfs;
			try
			{
				if (GetUseCompoundFile())
					cfs = "c";
				else
					cfs = "C";
			}
			catch (System.IO.IOException ioe)
			{
				cfs = "?";
			}
			
			System.String docStore;
			
			if (docStoreOffset != - 1)
				docStore = "->" + docStoreSegment;
			else
				docStore = "";
			
			return name + ":" + cfs + (this.dir == dir?"":"x") + docCount + docStore;
		}
		
		/// <summary>We consider another SegmentInfo instance equal if it
		/// has the same dir and same name. 
		/// </summary>
		public  override bool Equals(System.Object obj)
		{
			SegmentInfo other;
			try
			{
				other = (SegmentInfo) obj;
			}
			catch (System.InvalidCastException cce)
			{
				return false;
			}
			return other.dir == dir && other.name.Equals(name);
		}
		
		public override int GetHashCode()
		{
			return dir.GetHashCode() + name.GetHashCode();
		}
	}
}
