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

using IndexOutput = Mono.Lucene.Net.Store.IndexOutput;
using Similarity = Mono.Lucene.Net.Search.Similarity;

namespace Mono.Lucene.Net.Index
{
	
	// TODO FI: norms could actually be stored as doc store
	
	/// <summary>Writes norms.  Each thread X field accumulates the norms
	/// for the doc/fields it saw, then the flush method below
	/// merges all of these together into a single _X.nrm file.
	/// </summary>
	
	sealed class NormsWriter:InvertedDocEndConsumer
	{
		
		private static readonly byte defaultNorm;
		private FieldInfos fieldInfos;
		public override InvertedDocEndConsumerPerThread AddThread(DocInverterPerThread docInverterPerThread)
		{
			return new NormsWriterPerThread(docInverterPerThread, this);
		}
		
		public override void  Abort()
		{
		}
		
		// We only write the _X.nrm file at flush
		internal void  Files(System.Collections.ICollection files)
		{
		}
		
		internal override void  SetFieldInfos(FieldInfos fieldInfos)
		{
			this.fieldInfos = fieldInfos;
		}
		
		/// <summary>Produce _X.nrm if any document had a field with norms
		/// not disabled 
		/// </summary>
		public override void  Flush(System.Collections.IDictionary threadsAndFields, SegmentWriteState state)
		{
			
			System.Collections.IDictionary byField = new System.Collections.Hashtable();
			
			// Typically, each thread will have encountered the same
			// field.  So first we collate by field, ie, all
			// per-thread field instances that correspond to the
			// same FieldInfo
			System.Collections.IEnumerator it = new System.Collections.Hashtable(threadsAndFields).GetEnumerator();
			while (it.MoveNext())
			{
				System.Collections.DictionaryEntry entry = (System.Collections.DictionaryEntry) it.Current;
				
				System.Collections.ICollection fields = (System.Collections.ICollection) entry.Value;
				System.Collections.IEnumerator fieldsIt = fields.GetEnumerator();
                System.Collections.ArrayList fieldsToRemove = new System.Collections.ArrayList();
				
				while (fieldsIt.MoveNext())
				{
					NormsWriterPerField perField = (NormsWriterPerField) ((System.Collections.DictionaryEntry) fieldsIt.Current).Key;
					
					if (perField.upto > 0)
					{
						// It has some norms
						System.Collections.IList l = (System.Collections.IList) byField[perField.fieldInfo];
						if (l == null)
						{
							l = new System.Collections.ArrayList();
							byField[perField.fieldInfo] = l;
						}
						l.Add(perField);
					}
					// Remove this field since we haven't seen it
					// since the previous flush
					else
					{
                        fieldsToRemove.Add(perField);
					}
				}

                System.Collections.Hashtable fieldsHT = (System.Collections.Hashtable)fields;
                for (int i = 0; i < fieldsToRemove.Count; i++)
                {
                    fieldsHT.Remove(fieldsToRemove[i]);    
                }
			}
			
			System.String normsFileName = state.segmentName + "." + IndexFileNames.NORMS_EXTENSION;
			state.flushedFiles[normsFileName] = normsFileName;
			IndexOutput normsOut = state.directory.CreateOutput(normsFileName);
			
			try
			{
				normsOut.WriteBytes(SegmentMerger.NORMS_HEADER, 0, SegmentMerger.NORMS_HEADER.Length);
				
				int numField = fieldInfos.Size();
				
				int normCount = 0;
				
				for (int fieldNumber = 0; fieldNumber < numField; fieldNumber++)
				{
					
					FieldInfo fieldInfo = fieldInfos.FieldInfo(fieldNumber);
					
					System.Collections.IList toMerge = (System.Collections.IList) byField[fieldInfo];
					int upto = 0;
					if (toMerge != null)
					{
						
						int numFields = toMerge.Count;
						
						normCount++;
						
						NormsWriterPerField[] fields = new NormsWriterPerField[numFields];
						int[] uptos = new int[numFields];
						
						for (int j = 0; j < numFields; j++)
							fields[j] = (NormsWriterPerField) toMerge[j];
						
						int numLeft = numFields;
						
						while (numLeft > 0)
						{
							
							System.Diagnostics.Debug.Assert(uptos [0] < fields [0].docIDs.Length, " uptos[0]=" + uptos [0] + " len=" +(fields [0].docIDs.Length));
							
							int minLoc = 0;
							int minDocID = fields[0].docIDs[uptos[0]];
							
							for (int j = 1; j < numLeft; j++)
							{
								int docID = fields[j].docIDs[uptos[j]];
								if (docID < minDocID)
								{
									minDocID = docID;
									minLoc = j;
								}
							}
							
							System.Diagnostics.Debug.Assert(minDocID < state.numDocs);
							
							// Fill hole
							for (; upto < minDocID; upto++)
								normsOut.WriteByte(defaultNorm);
							
							normsOut.WriteByte(fields[minLoc].norms[uptos[minLoc]]);
							(uptos[minLoc])++;
							upto++;
							
							if (uptos[minLoc] == fields[minLoc].upto)
							{
								fields[minLoc].Reset();
								if (minLoc != numLeft - 1)
								{
									fields[minLoc] = fields[numLeft - 1];
									uptos[minLoc] = uptos[numLeft - 1];
								}
								numLeft--;
							}
						}
						
						// Fill final hole with defaultNorm
						for (; upto < state.numDocs; upto++)
							normsOut.WriteByte(defaultNorm);
					}
					else if (fieldInfo.isIndexed && !fieldInfo.omitNorms)
					{
						normCount++;
						// Fill entire field with default norm:
						for (; upto < state.numDocs; upto++)
							normsOut.WriteByte(defaultNorm);
					}
					
					System.Diagnostics.Debug.Assert(4 + normCount * state.numDocs == normsOut.GetFilePointer(), ".nrm file size mismatch: expected=" +(4 + normCount * state.numDocs) + " actual=" + normsOut.GetFilePointer());
				}
			}
			finally
			{
				normsOut.Close();
			}
		}
		
		internal override void  CloseDocStore(SegmentWriteState state)
		{
		}
		static NormsWriter()
		{
			defaultNorm = Similarity.EncodeNorm(1.0f);
		}
	}
}
