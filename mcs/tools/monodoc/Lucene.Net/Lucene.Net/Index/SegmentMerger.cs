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
using OutputStream = Monodoc.Lucene.Net.Store.OutputStream;
using RAMOutputStream = Monodoc.Lucene.Net.Store.RAMOutputStream;
namespace Monodoc.Lucene.Net.Index
{
	
	/// <summary> The SegmentMerger class combines two or more Segments, represented by an Monodoc.Lucene.Net.Index.IndexReader ({@link #add},
	/// into a single Segment.  After adding the appropriate readers, call the merge method to combine the 
	/// segments.
	/// <P> 
	/// If the compoundFile flag is set, then the segments will be merged into a compound file.
	/// 
	/// 
	/// </summary>
	/// <seealso cref="#merge">
	/// </seealso>
	/// <seealso cref="#add">
	/// </seealso>
	sealed public class SegmentMerger
	{
		private bool useCompoundFile;
		private Directory directory;
		private System.String segment;
		
		private System.Collections.ArrayList readers = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10));
		private FieldInfos fieldInfos;
		
		// File extensions of old-style index files
		private static readonly System.String[] COMPOUND_EXTENSIONS = new System.String[]{"fnm", "frq", "prx", "fdx", "fdt", "tii", "tis"};
		private static readonly System.String[] VECTOR_EXTENSIONS = new System.String[]{"tvx", "tvd", "tvf"};
		
		/// <summary> </summary>
		/// <param name="dir">The Directory to merge the other segments into
		/// </param>
		/// <param name="name">The name of the new segment
		/// </param>
		/// <param name="compoundFile">true if the new segment should use a compoundFile
		/// </param>
		public /*internal*/ SegmentMerger(Directory dir, System.String name, bool compoundFile)
		{
			directory = dir;
			segment = name;
			useCompoundFile = compoundFile;
		}
		
		/// <summary> Add an Monodoc.Lucene.Net.Index.IndexReader to the collection of readers that are to be merged</summary>
		/// <param name="">reader
		/// </param>
		public /*internal*/ void  Add(Monodoc.Lucene.Net.Index.IndexReader reader)
		{
			readers.Add(reader);
		}
		
		/// <summary> </summary>
		/// <param name="i">The index of the reader to return
		/// </param>
		/// <returns> The ith reader to be merged
		/// </returns>
		internal Monodoc.Lucene.Net.Index.IndexReader SegmentReader(int i)
		{
			return (Monodoc.Lucene.Net.Index.IndexReader) readers[i];
		}
		
		/// <summary> Merges the readers specified by the {@link #add} method into the directory passed to the constructor</summary>
		/// <returns> The number of documents that were merged
		/// </returns>
		/// <throws>  IOException </throws>
		public /*internal*/ int Merge()
		{
			int value_Renamed;
			
			value_Renamed = MergeFields();
			MergeTerms();
			MergeNorms();
			
			if (fieldInfos.HasVectors())
				MergeVectors();
			
			if (useCompoundFile)
				CreateCompoundFile();
			
			return value_Renamed;
		}
		
		/// <summary> close all Monodoc.Lucene.Net.Index.IndexReaders that have been added.
		/// Should not be called before merge().
		/// </summary>
		/// <throws>  IOException </throws>
		public /*internal*/ void  CloseReaders()
		{
			for (int i = 0; i < readers.Count; i++)
			{
				// close readers
				Monodoc.Lucene.Net.Index.IndexReader reader = (Monodoc.Lucene.Net.Index.IndexReader) readers[i];
				reader.Close();
			}
		}
		
		private void  CreateCompoundFile()
		{
			CompoundFileWriter cfsWriter = new CompoundFileWriter(directory, segment + ".cfs");
			
			System.Collections.ArrayList files = new System.Collections.ArrayList(COMPOUND_EXTENSIONS.Length + fieldInfos.Size());
			
			// Basic files
			for (int i = 0; i < COMPOUND_EXTENSIONS.Length; i++)
			{
				files.Add(segment + "." + COMPOUND_EXTENSIONS[i]);
			}
			
			// Field norm files
			for (int i = 0; i < fieldInfos.Size(); i++)
			{
				FieldInfo fi = fieldInfos.FieldInfo(i);
				if (fi.isIndexed)
				{
					files.Add(segment + ".f" + i);
				}
			}
			
			// Vector files
			if (fieldInfos.HasVectors())
			{
				for (int i = 0; i < VECTOR_EXTENSIONS.Length; i++)
				{
					files.Add(segment + "." + VECTOR_EXTENSIONS[i]);
				}
			}
			
			// Now merge all added files
			System.Collections.IEnumerator it = files.GetEnumerator();
			while (it.MoveNext())
			{
				cfsWriter.AddFile((System.String) it.Current);
			}
			
			// Perform the merge
			cfsWriter.Close();
			
			// Now delete the source files
			it = files.GetEnumerator();
			while (it.MoveNext())
			{
				directory.DeleteFile((System.String) it.Current);
			}
		}
		
		/// <summary> </summary>
		/// <returns> The number of documents in all of the readers
		/// </returns>
		/// <throws>  IOException </throws>
		private int MergeFields()
		{
			fieldInfos = new FieldInfos(); // merge Field names
			int docCount = 0;
			for (int i = 0; i < readers.Count; i++)
			{
				Monodoc.Lucene.Net.Index.IndexReader reader = (Monodoc.Lucene.Net.Index.IndexReader) readers[i];
				fieldInfos.AddIndexed(reader.GetIndexedFieldNames(true), true);
				fieldInfos.AddIndexed(reader.GetIndexedFieldNames(false), false);
				fieldInfos.Add(reader.GetFieldNames(false), false);
			}
			fieldInfos.Write(directory, segment + ".fnm");
			
			FieldsWriter fieldsWriter = new FieldsWriter(directory, segment, fieldInfos);
			try
			{
				for (int i = 0; i < readers.Count; i++)
				{
					Monodoc.Lucene.Net.Index.IndexReader reader = (Monodoc.Lucene.Net.Index.IndexReader) readers[i];
					int maxDoc = reader.MaxDoc();
					for (int j = 0; j < maxDoc; j++)
						if (!reader.IsDeleted(j))
						{
							// skip deleted docs
							fieldsWriter.AddDocument(reader.Document(j));
							docCount++;
						}
				}
			}
			finally
			{
				fieldsWriter.Close();
			}
			return docCount;
		}
		
		/// <summary> Merge the TermVectors from each of the segments into the new one.</summary>
		/// <throws>  IOException </throws>
		private void  MergeVectors()
		{
			TermVectorsWriter termVectorsWriter = new TermVectorsWriter(directory, segment, fieldInfos);
			
			try
			{
				for (int r = 0; r < readers.Count; r++)
				{
					Monodoc.Lucene.Net.Index.IndexReader reader = (Monodoc.Lucene.Net.Index.IndexReader) readers[r];
					int maxDoc = reader.MaxDoc();
					for (int docNum = 0; docNum < maxDoc; docNum++)
					{
						// skip deleted docs
						if (reader.IsDeleted(docNum))
						{
							continue;
						}
						termVectorsWriter.OpenDocument();
						
						// get all term vectors
						TermFreqVector[] sourceTermVector = reader.GetTermFreqVectors(docNum);
						
						if (sourceTermVector != null)
						{
							for (int f = 0; f < sourceTermVector.Length; f++)
							{
								// translate Field numbers
								TermFreqVector termVector = sourceTermVector[f];
								termVectorsWriter.OpenField(termVector.GetField());
								System.String[] terms = termVector.GetTerms();
								int[] freqs = termVector.GetTermFrequencies();
								
								for (int t = 0; t < terms.Length; t++)
								{
									termVectorsWriter.AddTerm(terms[t], freqs[t]);
								}
							}
							termVectorsWriter.CloseDocument();
						}
					}
				}
			}
			finally
			{
				termVectorsWriter.Close();
			}
		}
		
		private OutputStream freqOutput = null;
		private OutputStream proxOutput = null;
		private TermInfosWriter termInfosWriter = null;
		private int skipInterval;
		private SegmentMergeQueue queue = null;
		
		private void  MergeTerms()
		{
			try
			{
				freqOutput = directory.CreateFile(segment + ".frq");
				proxOutput = directory.CreateFile(segment + ".prx");
				termInfosWriter = new TermInfosWriter(directory, segment, fieldInfos);
				skipInterval = termInfosWriter.skipInterval;
				queue = new SegmentMergeQueue(readers.Count);
				
				MergeTermInfos();
			}
			finally
			{
				if (freqOutput != null)
					freqOutput.Close();
				if (proxOutput != null)
					proxOutput.Close();
				if (termInfosWriter != null)
					termInfosWriter.Close();
				if (queue != null)
					queue.Close();
			}
		}
		
		private void  MergeTermInfos()
		{
			int base_Renamed = 0;
			for (int i = 0; i < readers.Count; i++)
			{
				Monodoc.Lucene.Net.Index.IndexReader reader = (Monodoc.Lucene.Net.Index.IndexReader) readers[i];
				TermEnum termEnum = reader.Terms();
				SegmentMergeInfo smi = new SegmentMergeInfo(base_Renamed, termEnum, reader);
				base_Renamed += reader.NumDocs();
				if (smi.Next())
					queue.Put(smi);
				// initialize queue
				else
					smi.Close();
			}
			
			SegmentMergeInfo[] match = new SegmentMergeInfo[readers.Count];
			
			while (queue.Size() > 0)
			{
				int matchSize = 0; // pop matching terms
				match[matchSize++] = (SegmentMergeInfo) queue.Pop();
				Term term = match[0].term;
				SegmentMergeInfo top = (SegmentMergeInfo) queue.Top();
				
				while (top != null && term.CompareTo(top.term) == 0)
				{
					match[matchSize++] = (SegmentMergeInfo) queue.Pop();
					top = (SegmentMergeInfo) queue.Top();
				}
				
				MergeTermInfo(match, matchSize); // add new TermInfo
				
				while (matchSize > 0)
				{
					SegmentMergeInfo smi = match[--matchSize];
					if (smi.Next())
						queue.Put(smi);
					// restore queue
					else
						smi.Close(); // done with a segment
				}
			}
		}
		
		private TermInfo termInfo = new TermInfo(); // minimize consing
		
		/// <summary>Merge one term found in one or more segments. The array <code>smis</code>
		/// contains segments that are positioned at the same term. <code>N</code>
		/// is the number of cells in the array actually occupied.
		/// 
		/// </summary>
		/// <param name="smis">array of segments
		/// </param>
		/// <param name="n">number of cells in the array actually occupied
		/// </param>
		private void  MergeTermInfo(SegmentMergeInfo[] smis, int n)
		{
			long freqPointer = freqOutput.GetFilePointer();
			long proxPointer = proxOutput.GetFilePointer();
			
			int df = AppendPostings(smis, n); // append posting data
			
			long skipPointer = WriteSkip();
			
			if (df > 0)
			{
				// add an entry to the dictionary with pointers to prox and freq files
				termInfo.Set(df, freqPointer, proxPointer, (int) (skipPointer - freqPointer));
				termInfosWriter.Add(smis[0].term, termInfo);
			}
		}
		
		/// <summary>Process postings from multiple segments all positioned on the
		/// same term. Writes out merged entries into freqOutput and
		/// the proxOutput streams.
		/// 
		/// </summary>
		/// <param name="smis">array of segments
		/// </param>
		/// <param name="n">number of cells in the array actually occupied
		/// </param>
		/// <returns> number of documents across all segments where this term was found
		/// </returns>
		private int AppendPostings(SegmentMergeInfo[] smis, int n)
		{
			int lastDoc = 0;
			int df = 0; // number of docs w/ term
			ResetSkip();
			for (int i = 0; i < n; i++)
			{
				SegmentMergeInfo smi = smis[i];
				TermPositions postings = smi.postings;
				int base_Renamed = smi.base_Renamed;
				int[] docMap = smi.docMap;
				postings.Seek(smi.termEnum);
				while (postings.Next())
				{
					int doc = postings.Doc();
					if (docMap != null)
						doc = docMap[doc]; // map around deletions
					doc += base_Renamed; // convert to merged space
					
					if (doc < lastDoc)
						throw new System.SystemException("docs out of order");
					
					df++;
					
					if ((df % skipInterval) == 0)
					{
						BufferSkip(lastDoc);
					}
					
					int docCode = (doc - lastDoc) << 1; // use low bit to flag freq=1
					lastDoc = doc;
					
					int freq = postings.Freq();
					if (freq == 1)
					{
						freqOutput.WriteVInt(docCode | 1); // write doc & freq=1
					}
					else
					{
						freqOutput.WriteVInt(docCode); // write doc
						freqOutput.WriteVInt(freq); // write frequency in doc
					}
					
					int lastPosition = 0; // write position deltas
					for (int j = 0; j < freq; j++)
					{
						int position = postings.NextPosition();
						proxOutput.WriteVInt(position - lastPosition);
						lastPosition = position;
					}
				}
			}
			return df;
		}
		
		private RAMOutputStream skipBuffer = new RAMOutputStream();
		private int lastSkipDoc;
		private long lastSkipFreqPointer;
		private long lastSkipProxPointer;
		
		private void  ResetSkip()
		{
			skipBuffer.Leset();
			lastSkipDoc = 0;
			lastSkipFreqPointer = freqOutput.GetFilePointer();
			lastSkipProxPointer = proxOutput.GetFilePointer();
		}
		
		private void  BufferSkip(int doc)
		{
			long freqPointer = freqOutput.GetFilePointer();
			long proxPointer = proxOutput.GetFilePointer();
			
			skipBuffer.WriteVInt(doc - lastSkipDoc);
			skipBuffer.WriteVInt((int) (freqPointer - lastSkipFreqPointer));
			skipBuffer.WriteVInt((int) (proxPointer - lastSkipProxPointer));
			
			lastSkipDoc = doc;
			lastSkipFreqPointer = freqPointer;
			lastSkipProxPointer = proxPointer;
		}
		
		private long WriteSkip()
		{
			long skipPointer = freqOutput.GetFilePointer();
			skipBuffer.WriteTo(freqOutput);
			return skipPointer;
		}
		
		private void  MergeNorms()
		{
			for (int i = 0; i < fieldInfos.Size(); i++)
			{
				FieldInfo fi = fieldInfos.FieldInfo(i);
				if (fi.isIndexed)
				{
					OutputStream output = directory.CreateFile(segment + ".f" + i);
					try
					{
						for (int j = 0; j < readers.Count; j++)
						{
							Monodoc.Lucene.Net.Index.IndexReader reader = (Monodoc.Lucene.Net.Index.IndexReader) readers[j];
							byte[] input = reader.Norms(fi.name);
							int maxDoc = reader.MaxDoc();
							for (int k = 0; k < maxDoc; k++)
							{
								byte norm = input != null?input[k]:(byte) 0;
								if (!reader.IsDeleted(k))
								{
									output.WriteByte(norm);
								}
							}
						}
					}
					finally
					{
						output.Close();
					}
				}
			}
		}
	}
}