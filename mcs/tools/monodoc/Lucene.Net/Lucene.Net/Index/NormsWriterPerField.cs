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

using ArrayUtil = Mono.Lucene.Net.Util.ArrayUtil;
using Similarity = Mono.Lucene.Net.Search.Similarity;

namespace Mono.Lucene.Net.Index
{
	
	/// <summary>Taps into DocInverter, as an InvertedDocEndConsumer,
	/// which is called at the end of inverting each field.  We
	/// just look at the length for the field (docState.length)
	/// and record the norm. 
	/// </summary>
	
	sealed class NormsWriterPerField:InvertedDocEndConsumerPerField, System.IComparable
	{
		
		internal NormsWriterPerThread perThread;
		internal FieldInfo fieldInfo;
		internal DocumentsWriter.DocState docState;
		
		// Holds all docID/norm pairs we've seen
		internal int[] docIDs = new int[1];
		internal byte[] norms = new byte[1];
		internal int upto;
		
		internal FieldInvertState fieldState;
		
		public void  Reset()
		{
			// Shrink back if we are overallocated now:
			docIDs = ArrayUtil.Shrink(docIDs, upto);
			norms = ArrayUtil.Shrink(norms, upto);
			upto = 0;
		}
		
		public NormsWriterPerField(DocInverterPerField docInverterPerField, NormsWriterPerThread perThread, FieldInfo fieldInfo)
		{
			this.perThread = perThread;
			this.fieldInfo = fieldInfo;
			docState = perThread.docState;
			fieldState = docInverterPerField.fieldState;
		}
		
		internal override void  Abort()
		{
			upto = 0;
		}
		
		public int CompareTo(System.Object other)
		{
			return String.CompareOrdinal(fieldInfo.name, ((NormsWriterPerField) other).fieldInfo.name);
		}
		
		internal override void  Finish()
		{
			System.Diagnostics.Debug.Assert(docIDs.Length == norms.Length);
			if (fieldInfo.isIndexed && !fieldInfo.omitNorms)
			{
				if (docIDs.Length <= upto)
				{
					System.Diagnostics.Debug.Assert(docIDs.Length == upto);
					docIDs = ArrayUtil.Grow(docIDs, 1 + upto);
					norms = ArrayUtil.Grow(norms, 1 + upto);
				}
				float norm = docState.similarity.ComputeNorm(fieldInfo.name, fieldState);
				norms[upto] = Similarity.EncodeNorm(norm);
				docIDs[upto] = docState.docID;
				upto++;
			}
		}
	}
}
