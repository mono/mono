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

using AbstractField = Mono.Lucene.Net.Documents.AbstractField;
using Document = Mono.Lucene.Net.Documents.Document;
using Directory = Mono.Lucene.Net.Store.Directory;
using FSDirectory = Mono.Lucene.Net.Store.FSDirectory;
using IndexInput = Mono.Lucene.Net.Store.IndexInput;

namespace Mono.Lucene.Net.Index
{
	
	/// <summary> Basic tool and API to check the health of an index and
	/// write a new segments file that removes reference to
	/// problematic segments.
	/// 
	/// <p/>As this tool checks every byte in the index, on a large
	/// index it can take quite a long time to run.
	/// 
	/// <p/><b>WARNING</b>: this tool and API is new and
	/// experimental and is subject to suddenly change in the
	/// next release.  Please make a complete backup of your
	/// index before using this to fix your index!
	/// </summary>
	public class CheckIndex
	{
		
		/// <summary>Default PrintStream for all CheckIndex instances.</summary>
		/// <deprecated> Use {@link #setInfoStream} per instance,
		/// instead. 
		/// </deprecated>
        [Obsolete("Use SetInfoStream per instance,instead.")]
		public static System.IO.StreamWriter out_Renamed = null;
		
		private System.IO.StreamWriter infoStream;
		private Directory dir;
		
		/// <summary> Returned from {@link #CheckIndex()} detailing the health and status of the index.
		/// 
		/// <p/><b>WARNING</b>: this API is new and experimental and is
		/// subject to suddenly change in the next release.
		/// 
		/// </summary>
		
		public class Status
		{
			
			/// <summary>True if no problems were found with the index. </summary>
			public bool clean;
			
			/// <summary>True if we were unable to locate and load the segments_N file. </summary>
			public bool missingSegments;
			
			/// <summary>True if we were unable to open the segments_N file. </summary>
			public bool cantOpenSegments;
			
			/// <summary>True if we were unable to read the version number from segments_N file. </summary>
			public bool missingSegmentVersion;
			
			/// <summary>Name of latest segments_N file in the index. </summary>
			public System.String segmentsFileName;
			
			/// <summary>Number of segments in the index. </summary>
			public int numSegments;
			
			/// <summary>String description of the version of the index. </summary>
			public System.String segmentFormat;
			
			/// <summary>Empty unless you passed specific segments list to check as optional 3rd argument.</summary>
			/// <seealso cref="CheckIndex.CheckIndex(List)">
			/// </seealso>
			public System.Collections.IList segmentsChecked = new System.Collections.ArrayList();
			
			/// <summary>True if the index was created with a newer version of Lucene than the CheckIndex tool. </summary>
			public bool toolOutOfDate;
			
			/// <summary>List of {@link SegmentInfoStatus} instances, detailing status of each segment. </summary>
			public System.Collections.IList segmentInfos = new System.Collections.ArrayList();
			
			/// <summary>Directory index is in. </summary>
			public Directory dir;
			
			/// <summary> SegmentInfos instance containing only segments that
			/// had no problems (this is used with the {@link CheckIndex#fixIndex} 
			/// method to repair the index. 
			/// </summary>
			internal SegmentInfos newSegments;
			
			/// <summary>How many documents will be lost to bad segments. </summary>
			public int totLoseDocCount;
			
			/// <summary>How many bad segments were found. </summary>
			public int numBadSegments;
			
			/// <summary>True if we checked only specific segments ({@link
			/// #CheckIndex(List)}) was called with non-null
			/// argument). 
			/// </summary>
			public bool partial;
			
			/// <summary>Holds the userData of the last commit in the index </summary>
            public System.Collections.Generic.IDictionary<string, string> userData;
			
			/// <summary>Holds the status of each segment in the index.
			/// See {@link #segmentInfos}.
			/// 
			/// <p/><b>WARNING</b>: this API is new and experimental and is
			/// subject to suddenly change in the next release.
			/// </summary>
			public class SegmentInfoStatus
			{
				/// <summary>Name of the segment. </summary>
				public System.String name;
				
				/// <summary>Document count (does not take deletions into account). </summary>
				public int docCount;
				
				/// <summary>True if segment is compound file format. </summary>
				public bool compound;
				
				/// <summary>Number of files referenced by this segment. </summary>
				public int numFiles;
				
				/// <summary>Net size (MB) of the files referenced by this
				/// segment. 
				/// </summary>
				public double sizeMB;
				
				/// <summary>Doc store offset, if this segment shares the doc
				/// store files (stored fields and term vectors) with
				/// other segments.  This is -1 if it does not share. 
				/// </summary>
				public int docStoreOffset = - 1;
				
				/// <summary>String of the shared doc store segment, or null if
				/// this segment does not share the doc store files. 
				/// </summary>
				public System.String docStoreSegment;
				
				/// <summary>True if the shared doc store files are compound file
				/// format. 
				/// </summary>
				public bool docStoreCompoundFile;
				
				/// <summary>True if this segment has pending deletions. </summary>
				public bool hasDeletions;
				
				/// <summary>Name of the current deletions file name. </summary>
				public System.String deletionsFileName;
				
				/// <summary>Number of deleted documents. </summary>
				public int numDeleted;
				
				/// <summary>True if we were able to open a SegmentReader on this
				/// segment. 
				/// </summary>
				public bool openReaderPassed;
				
				/// <summary>Number of fields in this segment. </summary>
				internal int numFields;
				
				/// <summary>True if at least one of the fields in this segment
				/// does not omitTermFreqAndPositions.
				/// </summary>
				/// <seealso cref="AbstractField.setOmitTermFreqAndPositions">
				/// </seealso>
				public bool hasProx;

                /// <summary>Map&lt;String, String&gt; that includes certain
				/// debugging details that IndexWriter records into
				/// each segment it creates 
				/// </summary>
                public System.Collections.Generic.IDictionary<string, string> diagnostics;
				
				/// <summary>Status for testing of field norms (null if field norms could not be tested). </summary>
				public FieldNormStatus fieldNormStatus;
				
				/// <summary>Status for testing of indexed terms (null if indexed terms could not be tested). </summary>
				public TermIndexStatus termIndexStatus;
				
				/// <summary>Status for testing of stored fields (null if stored fields could not be tested). </summary>
				public StoredFieldStatus storedFieldStatus;
				
				/// <summary>Status for testing of term vectors (null if term vectors could not be tested). </summary>
				public TermVectorStatus termVectorStatus;
			}
			
			/// <summary> Status from testing field norms.</summary>
			public sealed class FieldNormStatus
			{
				/// <summary>Number of fields successfully tested </summary>
				public long totFields = 0L;
				
				/// <summary>Exception thrown during term index test (null on success) </summary>
				public System.Exception error = null;
			}
			
			/// <summary> Status from testing term index.</summary>
			public sealed class TermIndexStatus
			{
				/// <summary>Total term count </summary>
				public long termCount = 0L;
				
				/// <summary>Total frequency across all terms. </summary>
				public long totFreq = 0L;
				
				/// <summary>Total number of positions. </summary>
				public long totPos = 0L;
				
				/// <summary>Exception thrown during term index test (null on success) </summary>
				public System.Exception error = null;
			}
			
			/// <summary> Status from testing stored fields.</summary>
			public sealed class StoredFieldStatus
			{
				
				/// <summary>Number of documents tested. </summary>
				public int docCount = 0;
				
				/// <summary>Total number of stored fields tested. </summary>
				public long totFields = 0;
				
				/// <summary>Exception thrown during stored fields test (null on success) </summary>
				public System.Exception error = null;
			}
			
			/// <summary> Status from testing stored fields.</summary>
			public sealed class TermVectorStatus
			{
				
				/// <summary>Number of documents tested. </summary>
				public int docCount = 0;
				
				/// <summary>Total number of term vectors tested. </summary>
				public long totVectors = 0;
				
				/// <summary>Exception thrown during term vector test (null on success) </summary>
				public System.Exception error = null;
			}
		}
		
		/// <summary>Create a new CheckIndex on the directory. </summary>
		public CheckIndex(Directory dir)
		{
			this.dir = dir;
			infoStream = out_Renamed;
		}
		
		/// <summary>Set infoStream where messages should go.  If null, no
		/// messages are printed 
		/// </summary>
		public virtual void  SetInfoStream(System.IO.StreamWriter out_Renamed)
		{
			infoStream = out_Renamed;
		}
		
		private void  Msg(System.String msg)
		{
			if (infoStream != null)
				infoStream.WriteLine(msg);
		}
		
		private class MySegmentTermDocs:SegmentTermDocs
		{
			
			internal int delCount;
			
			internal MySegmentTermDocs(SegmentReader p):base(p)
			{
			}
			
			public override void  Seek(Term term)
			{
				base.Seek(term);
				delCount = 0;
			}
			
			protected internal override void  SkippingDoc()
			{
				delCount++;
			}
		}
		
		/// <summary>Returns true if index is clean, else false. </summary>
		/// <deprecated> Please instantiate a CheckIndex and then use {@link #CheckIndex()} instead 
		/// </deprecated>
        [Obsolete("Please instantiate a CheckIndex and then use CheckIndex() instead")]
		public static bool Check(Directory dir, bool doFix)
		{
			return Check(dir, doFix, null);
		}
		
		/// <summary>Returns true if index is clean, else false.</summary>
		/// <deprecated> Please instantiate a CheckIndex and then use {@link #CheckIndex(List)} instead 
		/// </deprecated>
        [Obsolete("Please instantiate a CheckIndex and then use CheckIndex(List) instead")]
		public static bool Check(Directory dir, bool doFix, System.Collections.IList onlySegments)
		{
			CheckIndex checker = new CheckIndex(dir);
			Status status = checker.CheckIndex_Renamed_Method(onlySegments);
			if (doFix && !status.clean)
				checker.FixIndex(status);
			
			return status.clean;
		}
		
		/// <summary>Returns a {@link Status} instance detailing
		/// the state of the index.
		/// 
		/// <p/>As this method checks every byte in the index, on a large
		/// index it can take quite a long time to run.
		/// 
		/// <p/><b>WARNING</b>: make sure
		/// you only call this when the index is not opened by any
		/// writer. 
		/// </summary>
		public virtual Status CheckIndex_Renamed_Method()
		{
			return CheckIndex_Renamed_Method(null);
		}
		
		/// <summary>Returns a {@link Status} instance detailing
		/// the state of the index.
		/// 
		/// </summary>
		/// <param name="onlySegments">list of specific segment names to check
		/// 
		/// <p/>As this method checks every byte in the specified
		/// segments, on a large index it can take quite a long
		/// time to run.
		/// 
		/// <p/><b>WARNING</b>: make sure
		/// you only call this when the index is not opened by any
		/// writer. 
		/// </param>
		public virtual Status CheckIndex_Renamed_Method(System.Collections.IList onlySegments)
		{
            System.Globalization.NumberFormatInfo nf = System.Globalization.CultureInfo.CurrentCulture.NumberFormat;
			SegmentInfos sis = new SegmentInfos();
			Status result = new Status();
			result.dir = dir;
			try
			{
				sis.Read(dir);
			}
			catch (System.Exception t)
			{
				Msg("ERROR: could not read any segments file in directory");
				result.missingSegments = true;
				if (infoStream != null)
					infoStream.WriteLine(t.StackTrace);
				return result;
			}
			
			int numSegments = sis.Count;
			System.String segmentsFileName = sis.GetCurrentSegmentFileName();
			IndexInput input = null;
			try
			{
				input = dir.OpenInput(segmentsFileName);
			}
			catch (System.Exception t)
			{
				Msg("ERROR: could not open segments file in directory");
				if (infoStream != null)
					infoStream.WriteLine(t.StackTrace);
				result.cantOpenSegments = true;
				return result;
			}
			int format = 0;
			try
			{
				format = input.ReadInt();
			}
			catch (System.Exception t)
			{
				Msg("ERROR: could not read segment file version in directory");
				if (infoStream != null)
					infoStream.WriteLine(t.StackTrace);
				result.missingSegmentVersion = true;
				return result;
			}
			finally
			{
				if (input != null)
					input.Close();
			}
			
			System.String sFormat = "";
			bool skip = false;
			
			if (format == SegmentInfos.FORMAT)
				sFormat = "FORMAT [Lucene Pre-2.1]";
			if (format == SegmentInfos.FORMAT_LOCKLESS)
				sFormat = "FORMAT_LOCKLESS [Lucene 2.1]";
			else if (format == SegmentInfos.FORMAT_SINGLE_NORM_FILE)
				sFormat = "FORMAT_SINGLE_NORM_FILE [Lucene 2.2]";
			else if (format == SegmentInfos.FORMAT_SHARED_DOC_STORE)
				sFormat = "FORMAT_SHARED_DOC_STORE [Lucene 2.3]";
			else
			{
				if (format == SegmentInfos.FORMAT_CHECKSUM)
					sFormat = "FORMAT_CHECKSUM [Lucene 2.4]";
				else if (format == SegmentInfos.FORMAT_DEL_COUNT)
					sFormat = "FORMAT_DEL_COUNT [Lucene 2.4]";
				else if (format == SegmentInfos.FORMAT_HAS_PROX)
					sFormat = "FORMAT_HAS_PROX [Lucene 2.4]";
				else if (format == SegmentInfos.FORMAT_USER_DATA)
					sFormat = "FORMAT_USER_DATA [Lucene 2.9]";
				else if (format == SegmentInfos.FORMAT_DIAGNOSTICS)
					sFormat = "FORMAT_DIAGNOSTICS [Lucene 2.9]";
				else if (format < SegmentInfos.CURRENT_FORMAT)
				{
					sFormat = "int=" + format + " [newer version of Lucene than this tool]";
					skip = true;
				}
				else
				{
					sFormat = format + " [Lucene 1.3 or prior]";
				}
			}
			
			result.segmentsFileName = segmentsFileName;
			result.numSegments = numSegments;
			result.segmentFormat = sFormat;
			result.userData = sis.GetUserData();
			System.String userDataString;
			if (sis.GetUserData().Count > 0)
			{
				userDataString = " userData=" + SupportClass.CollectionsHelper.CollectionToString(sis.GetUserData());
			}
			else
			{
				userDataString = "";
			}
			
			Msg("Segments file=" + segmentsFileName + " numSegments=" + numSegments + " version=" + sFormat + userDataString);
			
			if (onlySegments != null)
			{
				result.partial = true;
				if (infoStream != null)
					infoStream.Write("\nChecking only these segments:");
				System.Collections.IEnumerator it = onlySegments.GetEnumerator();
				while (it.MoveNext())
				{
					if (infoStream != null)
					{
						infoStream.Write(" " + it.Current);
					}
				}
                System.Collections.IEnumerator e = onlySegments.GetEnumerator();
                while (e.MoveNext() == true)
                {
                    result.segmentsChecked.Add(e.Current);
                }
                Msg(":");
			}
			
			if (skip)
			{
				Msg("\nERROR: this index appears to be created by a newer version of Lucene than this tool was compiled on; please re-compile this tool on the matching version of Lucene; exiting");
				result.toolOutOfDate = true;
				return result;
			}
			
			
			result.newSegments = (SegmentInfos) sis.Clone();
			result.newSegments.Clear();
			
			for (int i = 0; i < numSegments; i++)
			{
				SegmentInfo info = sis.Info(i);
				if (onlySegments != null && !onlySegments.Contains(info.name))
					continue;
				Status.SegmentInfoStatus segInfoStat = new Status.SegmentInfoStatus();
				result.segmentInfos.Add(segInfoStat);
				Msg("  " + (1 + i) + " of " + numSegments + ": name=" + info.name + " docCount=" + info.docCount);
				segInfoStat.name = info.name;
				segInfoStat.docCount = info.docCount;
				
				int toLoseDocCount = info.docCount;
				
				SegmentReader reader = null;
				
				try
				{
					Msg("    compound=" + info.GetUseCompoundFile());
					segInfoStat.compound = info.GetUseCompoundFile();
					Msg("    hasProx=" + info.GetHasProx());
					segInfoStat.hasProx = info.GetHasProx();
					Msg("    numFiles=" + info.Files().Count);
					segInfoStat.numFiles = info.Files().Count;
					Msg(System.String.Format(nf, "    size (MB)={0:f}", new System.Object[] { (info.SizeInBytes() / (1024.0 * 1024.0)) }));
					segInfoStat.sizeMB = info.SizeInBytes() / (1024.0 * 1024.0);
                    System.Collections.Generic.IDictionary<string, string> diagnostics = info.GetDiagnostics();
					segInfoStat.diagnostics = diagnostics;
					if (diagnostics.Count > 0)
					{
						Msg("    diagnostics = " + SupportClass.CollectionsHelper.CollectionToString(diagnostics));
					}
					
					int docStoreOffset = info.GetDocStoreOffset();
					if (docStoreOffset != - 1)
					{
						Msg("    docStoreOffset=" + docStoreOffset);
						segInfoStat.docStoreOffset = docStoreOffset;
						Msg("    docStoreSegment=" + info.GetDocStoreSegment());
						segInfoStat.docStoreSegment = info.GetDocStoreSegment();
						Msg("    docStoreIsCompoundFile=" + info.GetDocStoreIsCompoundFile());
						segInfoStat.docStoreCompoundFile = info.GetDocStoreIsCompoundFile();
					}
					System.String delFileName = info.GetDelFileName();
					if (delFileName == null)
					{
						Msg("    no deletions");
						segInfoStat.hasDeletions = false;
					}
					else
					{
						Msg("    has deletions [delFileName=" + delFileName + "]");
						segInfoStat.hasDeletions = true;
						segInfoStat.deletionsFileName = delFileName;
					}
					if (infoStream != null)
						infoStream.Write("    test: open reader.........");
					reader = SegmentReader.Get(info);
					
					segInfoStat.openReaderPassed = true;
					
					int numDocs = reader.NumDocs();
					toLoseDocCount = numDocs;
					if (reader.HasDeletions())
					{
						if (reader.deletedDocs.Count() != info.GetDelCount())
						{
							throw new System.SystemException("delete count mismatch: info=" + info.GetDelCount() + " vs deletedDocs.count()=" + reader.deletedDocs.Count());
						}
						if (reader.deletedDocs.Count() > reader.MaxDoc())
						{
							throw new System.SystemException("too many deleted docs: maxDoc()=" + reader.MaxDoc() + " vs deletedDocs.count()=" + reader.deletedDocs.Count());
						}
						if (info.docCount - numDocs != info.GetDelCount())
						{
							throw new System.SystemException("delete count mismatch: info=" + info.GetDelCount() + " vs reader=" + (info.docCount - numDocs));
						}
						segInfoStat.numDeleted = info.docCount - numDocs;
						Msg("OK [" + (segInfoStat.numDeleted) + " deleted docs]");
					}
					else
					{
						if (info.GetDelCount() != 0)
						{
							throw new System.SystemException("delete count mismatch: info=" + info.GetDelCount() + " vs reader=" + (info.docCount - numDocs));
						}
						Msg("OK");
					}
					if (reader.MaxDoc() != info.docCount)
						throw new System.SystemException("SegmentReader.maxDoc() " + reader.MaxDoc() + " != SegmentInfos.docCount " + info.docCount);
					
					// Test getFieldNames()
					if (infoStream != null)
					{
						infoStream.Write("    test: fields..............");
					}
                    System.Collections.Generic.ICollection<string> fieldNames = reader.GetFieldNames(IndexReader.FieldOption.ALL);
					Msg("OK [" + fieldNames.Count + " fields]");
					segInfoStat.numFields = fieldNames.Count;
					
					// Test Field Norms
					segInfoStat.fieldNormStatus = TestFieldNorms(fieldNames, reader);
					
					// Test the Term Index
					segInfoStat.termIndexStatus = TestTermIndex(info, reader);
					
					// Test Stored Fields
					segInfoStat.storedFieldStatus = TestStoredFields(info, reader, nf);
					
					// Test Term Vectors
					segInfoStat.termVectorStatus = TestTermVectors(info, reader, nf);
					
					// Rethrow the first exception we encountered
					//  This will cause stats for failed segments to be incremented properly
					if (segInfoStat.fieldNormStatus.error != null)
					{
						throw new System.SystemException("Field Norm test failed");
					}
					else if (segInfoStat.termIndexStatus.error != null)
					{
						throw new System.SystemException("Term Index test failed");
					}
					else if (segInfoStat.storedFieldStatus.error != null)
					{
						throw new System.SystemException("Stored Field test failed");
					}
					else if (segInfoStat.termVectorStatus.error != null)
					{
						throw new System.SystemException("Term Vector test failed");
					}
					
					Msg("");
				}
				catch (System.Exception t)
				{
					Msg("FAILED");
					System.String comment;
					comment = "fixIndex() would remove reference to this segment";
					Msg("    WARNING: " + comment + "; full exception:");
					if (infoStream != null)
						infoStream.WriteLine(t.StackTrace);
					Msg("");
					result.totLoseDocCount += toLoseDocCount;
					result.numBadSegments++;
					continue;
				}
				finally
				{
					if (reader != null)
						reader.Close();
				}
				
				// Keeper
				result.newSegments.Add(info.Clone());
			}
			
			if (0 == result.numBadSegments)
			{
				result.clean = true;
				Msg("No problems were detected with this index.\n");
			}
			else
				Msg("WARNING: " + result.numBadSegments + " broken segments (containing " + result.totLoseDocCount + " documents) detected");
			
			return result;
		}
		
		/// <summary> Test field norms.</summary>
        private Status.FieldNormStatus TestFieldNorms(System.Collections.Generic.ICollection<string> fieldNames, SegmentReader reader)
		{
			Status.FieldNormStatus status = new Status.FieldNormStatus();
			
			try
			{
				// Test Field Norms
				if (infoStream != null)
				{
					infoStream.Write("    test: field norms.........");
				}
				System.Collections.IEnumerator it = fieldNames.GetEnumerator();
				byte[] b = new byte[reader.MaxDoc()];
				while (it.MoveNext())
				{
					System.String fieldName = (System.String) it.Current;
                    if (reader.HasNorms(fieldName))
                    {
                        reader.Norms(fieldName, b, 0);
                        ++status.totFields;
                    }
				}
				
				Msg("OK [" + status.totFields + " fields]");
			}
			catch (System.Exception e)
			{
				Msg("ERROR [" + System.Convert.ToString(e.Message) + "]");
				status.error = e;
				if (infoStream != null)
				{
					infoStream.WriteLine(e.StackTrace);
				}
			}
			
			return status;
		}
		
		/// <summary> Test the term index.</summary>
		private Status.TermIndexStatus TestTermIndex(SegmentInfo info, SegmentReader reader)
		{
			Status.TermIndexStatus status = new Status.TermIndexStatus();
			
			try
			{
				if (infoStream != null)
				{
					infoStream.Write("    test: terms, freq, prox...");
				}
				
				TermEnum termEnum = reader.Terms();
				TermPositions termPositions = reader.TermPositions();
				
				// Used only to count up # deleted docs for this term
				MySegmentTermDocs myTermDocs = new MySegmentTermDocs(reader);
				
				int maxDoc = reader.MaxDoc();
				
				while (termEnum.Next())
				{
					status.termCount++;
					Term term = termEnum.Term();
					int docFreq = termEnum.DocFreq();
					termPositions.Seek(term);
					int lastDoc = - 1;
					int freq0 = 0;
					status.totFreq += docFreq;
					while (termPositions.Next())
					{
						freq0++;
						int doc = termPositions.Doc();
						int freq = termPositions.Freq();
						if (doc <= lastDoc)
						{
							throw new System.SystemException("term " + term + ": doc " + doc + " <= lastDoc " + lastDoc);
						}
						if (doc >= maxDoc)
						{
							throw new System.SystemException("term " + term + ": doc " + doc + " >= maxDoc " + maxDoc);
						}
						
						lastDoc = doc;
						if (freq <= 0)
						{
							throw new System.SystemException("term " + term + ": doc " + doc + ": freq " + freq + " is out of bounds");
						}
						
						int lastPos = - 1;
						status.totPos += freq;
						for (int j = 0; j < freq; j++)
						{
							int pos = termPositions.NextPosition();
							if (pos < - 1)
							{
								throw new System.SystemException("term " + term + ": doc " + doc + ": pos " + pos + " is out of bounds");
							}
							if (pos < lastPos)
							{
								throw new System.SystemException("term " + term + ": doc " + doc + ": pos " + pos + " < lastPos " + lastPos);
							}
						}
					}
					
					// Now count how many deleted docs occurred in
					// this term:
					int delCount;
					if (reader.HasDeletions())
					{
						myTermDocs.Seek(term);
						while (myTermDocs.Next())
						{
						}
						delCount = myTermDocs.delCount;
					}
					else
					{
						delCount = 0;
					}
					
					if (freq0 + delCount != docFreq)
					{
						throw new System.SystemException("term " + term + " docFreq=" + docFreq + " != num docs seen " + freq0 + " + num docs deleted " + delCount);
					}
				}
				
				Msg("OK [" + status.termCount + " terms; " + status.totFreq + " terms/docs pairs; " + status.totPos + " tokens]");
			}
			catch (System.Exception e)
			{
				Msg("ERROR [" + System.Convert.ToString(e.Message) + "]");
				status.error = e;
				if (infoStream != null)
				{
					infoStream.WriteLine(e.StackTrace);
				}
			}
			
			return status;
		}
		
		/// <summary> Test stored fields for a segment.</summary>
		private Status.StoredFieldStatus TestStoredFields(SegmentInfo info, SegmentReader reader, System.Globalization.NumberFormatInfo format)
		{
			Status.StoredFieldStatus status = new Status.StoredFieldStatus();
			
			try
			{
				if (infoStream != null)
				{
					infoStream.Write("    test: stored fields.......");
				}
				
				// Scan stored fields for all documents
				for (int j = 0; j < info.docCount; ++j)
				{
					if (!reader.IsDeleted(j))
					{
						status.docCount++;
						Document doc = reader.Document(j);
						status.totFields += doc.GetFields().Count;
					}
				}
				
				// Validate docCount
				if (status.docCount != reader.NumDocs())
				{
					throw new System.SystemException("docCount=" + status.docCount + " but saw " + status.docCount + " undeleted docs");
				}
				
                Msg(string.Format(format, "OK [{0:d} total field count; avg {1:f} fields per doc]", new object[] { status.totFields, (((float) status.totFields) / status.docCount) }));
            }
			catch (System.Exception e)
			{
				Msg("ERROR [" + System.Convert.ToString(e.Message) + "]");
				status.error = e;
				if (infoStream != null)
				{
					infoStream.WriteLine(e.StackTrace);
				}
			}
			
			return status;
		}
		
		/// <summary> Test term vectors for a segment.</summary>
        private Status.TermVectorStatus TestTermVectors(SegmentInfo info, SegmentReader reader, System.Globalization.NumberFormatInfo format)
		{
			Status.TermVectorStatus status = new Status.TermVectorStatus();
			
			try
			{
				if (infoStream != null)
				{
					infoStream.Write("    test: term vectors........");
				}
				
				for (int j = 0; j < info.docCount; ++j)
				{
					if (!reader.IsDeleted(j))
					{
						status.docCount++;
						TermFreqVector[] tfv = reader.GetTermFreqVectors(j);
						if (tfv != null)
						{
							status.totVectors += tfv.Length;
						}
					}
				}
				
                Msg(System.String.Format(format, "OK [{0:d} total vector count; avg {1:f} term/freq vector fields per doc]", new object[] { status.totVectors, (((float) status.totVectors) / status.docCount) }));
            }
			catch (System.Exception e)
			{
				Msg("ERROR [" + System.Convert.ToString(e.Message) + "]");
				status.error = e;
				if (infoStream != null)
				{
					infoStream.WriteLine(e.StackTrace);
				}
			}
			
			return status;
		}
		
		/// <summary>Repairs the index using previously returned result
		/// from {@link #checkIndex}.  Note that this does not
		/// remove any of the unreferenced files after it's done;
		/// you must separately open an {@link IndexWriter}, which
		/// deletes unreferenced files when it's created.
		/// 
		/// <p/><b>WARNING</b>: this writes a
		/// new segments file into the index, effectively removing
		/// all documents in broken segments from the index.
		/// BE CAREFUL.
		/// 
		/// <p/><b>WARNING</b>: Make sure you only call this when the
		/// index is not opened  by any writer. 
		/// </summary>
		public virtual void  FixIndex(Status result)
		{
			if (result.partial)
				throw new System.ArgumentException("can only fix an index that was fully checked (this status checked a subset of segments)");
			result.newSegments.Commit(result.dir);
		}
		
		private static bool assertsOn;
		
		private static bool TestAsserts()
		{
			assertsOn = true;
			return true;
		}
		
		private static bool AssertsOn()
		{
			System.Diagnostics.Debug.Assert(TestAsserts());
			return assertsOn;
		}
		
		/// <summary>Command-line interface to check and fix an index.
		/// <p/>
		/// Run it like this:
		/// <pre>
		/// java -ea:Mono.Lucene.Net... Mono.Lucene.Net.Index.CheckIndex pathToIndex [-fix] [-segment X] [-segment Y]
		/// </pre>
		/// <ul>
		/// <li><code>-fix</code>: actually write a new segments_N file, removing any problematic segments</li>
		/// <li><code>-segment X</code>: only check the specified
		/// segment(s).  This can be specified multiple times,
		/// to check more than one segment, eg <code>-segment _2
		/// -segment _a</code>.  You can't use this with the -fix
		/// option.</li>
		/// </ul>
		/// <p/><b>WARNING</b>: <code>-fix</code> should only be used on an emergency basis as it will cause
		/// documents (perhaps many) to be permanently removed from the index.  Always make
		/// a backup copy of your index before running this!  Do not run this tool on an index
		/// that is actively being written to.  You have been warned!
		/// <p/>                Run without -fix, this tool will open the index, report version information
		/// and report any exceptions it hits and what action it would take if -fix were
		/// specified.  With -fix, this tool will remove any segments that have issues and
		/// write a new segments_N file.  This means all documents contained in the affected
		/// segments will be removed.
		/// <p/>
		/// This tool exits with exit code 1 if the index cannot be opened or has any
		/// corruption, else 0.
		/// </summary>
		[STAThread]
		public static void  Main(System.String[] args)
		{
			
			bool doFix = false;
			System.Collections.IList onlySegments = new System.Collections.ArrayList();
			System.String indexPath = null;
			int i = 0;
			while (i < args.Length)
			{
				if (args[i].Equals("-fix"))
				{
					doFix = true;
					i++;
				}
				else if (args[i].Equals("-segment"))
				{
					if (i == args.Length - 1)
					{
						System.Console.Out.WriteLine("ERROR: missing name for -segment option");
						System.Environment.Exit(1);
					}
					onlySegments.Add(args[i + 1]);
					i += 2;
				}
				else
				{
					if (indexPath != null)
					{
						System.Console.Out.WriteLine("ERROR: unexpected extra argument '" + args[i] + "'");
						System.Environment.Exit(1);
					}
					indexPath = args[i];
					i++;
				}
			}
			
			if (indexPath == null)
			{
				System.Console.Out.WriteLine("\nERROR: index path not specified");
				System.Console.Out.WriteLine("\nUsage: java Mono.Lucene.Net.Index.CheckIndex pathToIndex [-fix] [-segment X] [-segment Y]\n" + "\n" + "  -fix: actually write a new segments_N file, removing any problematic segments\n" + "  -segment X: only check the specified segments.  This can be specified multiple\n" + "              times, to check more than one segment, eg '-segment _2 -segment _a'.\n" + "              You can't use this with the -fix option\n" + "\n" + "**WARNING**: -fix should only be used on an emergency basis as it will cause\n" + "documents (perhaps many) to be permanently removed from the index.  Always make\n" + "a backup copy of your index before running this!  Do not run this tool on an index\n" + "that is actively being written to.  You have been warned!\n" + "\n" + "Run without -fix, this tool will open the index, report version information\n" + "and report any exceptions it hits and what action it would take if -fix were\n" + "specified.  With -fix, this tool will remove any segments that have issues and\n" + "write a new segments_N file.  This means all documents contained in the affected\n" + "segments will be removed.\n" + "\n" + "This tool exits with exit code 1 if the index cannot be opened or has any\n" + "corruption, else 0.\n");
				System.Environment.Exit(1);
			}
			
			if (!AssertsOn())
				System.Console.Out.WriteLine("\nNOTE: testing will be more thorough if you run java with '-ea:Mono.Lucene.Net...', so assertions are enabled");
			
			if (onlySegments.Count == 0)
				onlySegments = null;
			else if (doFix)
			{
				System.Console.Out.WriteLine("ERROR: cannot specify both -fix and -segment");
				System.Environment.Exit(1);
			}
			
			System.Console.Out.WriteLine("\nOpening index @ " + indexPath + "\n");
			Directory dir = null;
			try
			{
				dir = FSDirectory.Open(new System.IO.FileInfo(indexPath));
			}
			catch (System.Exception t)
			{
				System.Console.Out.WriteLine("ERROR: could not open directory \"" + indexPath + "\"; exiting");
				System.Console.Out.WriteLine(t.StackTrace);
				System.Environment.Exit(1);
			}
			
			CheckIndex checker = new CheckIndex(dir);
			System.IO.StreamWriter temp_writer;
			temp_writer = new System.IO.StreamWriter(System.Console.OpenStandardOutput(), System.Console.Out.Encoding);
			temp_writer.AutoFlush = true;
			checker.SetInfoStream(temp_writer);
			
			Status result = checker.CheckIndex_Renamed_Method(onlySegments);
			if (result.missingSegments)
			{
				System.Environment.Exit(1);
			}
			
			if (!result.clean)
			{
				if (!doFix)
				{
					System.Console.Out.WriteLine("WARNING: would write new segments file, and " + result.totLoseDocCount + " documents would be lost, if -fix were specified\n");
				}
				else
				{
					System.Console.Out.WriteLine("WARNING: " + result.totLoseDocCount + " documents will be lost\n");
					System.Console.Out.WriteLine("NOTE: will write new segments file in 5 seconds; this will remove " + result.totLoseDocCount + " docs from the index. THIS IS YOUR LAST CHANCE TO CTRL+C!");
					for (int s = 0; s < 5; s++)
					{
						System.Threading.Thread.Sleep(new System.TimeSpan((System.Int64) 10000 * 1000));
						System.Console.Out.WriteLine("  " + (5 - s) + "...");
					}
					System.Console.Out.WriteLine("Writing...");
					checker.FixIndex(result);
					System.Console.Out.WriteLine("OK");
					System.Console.Out.WriteLine("Wrote new segments file \"" + result.newSegments.GetCurrentSegmentFileName() + "\"");
				}
			}
			System.Console.Out.WriteLine("");
			
			int exitCode;
			if (result != null && result.clean == true)
				exitCode = 0;
			else
				exitCode = 1;
			System.Environment.Exit(exitCode);
		}
	}
}
