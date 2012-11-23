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
	
	/// <summary> The TermVectorMapper can be used to map Term Vectors into your own
	/// structure instead of the parallel array structure used by
	/// {@link Mono.Lucene.Net.Index.IndexReader#GetTermFreqVector(int,String)}.
	/// <p/>
	/// It is up to the implementation to make sure it is thread-safe.
	/// 
	/// 
	/// 
	/// </summary>
	public abstract class TermVectorMapper
	{
		
		private bool ignoringPositions;
		private bool ignoringOffsets;
		
		
		protected internal TermVectorMapper()
		{
		}
		
		/// <summary> </summary>
		/// <param name="ignoringPositions">true if this mapper should tell Lucene to ignore positions even if they are stored
		/// </param>
		/// <param name="ignoringOffsets">similar to ignoringPositions
		/// </param>
		protected internal TermVectorMapper(bool ignoringPositions, bool ignoringOffsets)
		{
			this.ignoringPositions = ignoringPositions;
			this.ignoringOffsets = ignoringOffsets;
		}
		
		/// <summary> Tell the mapper what to expect in regards to field, number of terms, offset and position storage.
		/// This method will be called once before retrieving the vector for a field.
		/// 
		/// This method will be called before {@link #Map(String,int,TermVectorOffsetInfo[],int[])}.
		/// </summary>
		/// <param name="field">The field the vector is for
		/// </param>
		/// <param name="numTerms">The number of terms that need to be mapped
		/// </param>
		/// <param name="storeOffsets">true if the mapper should expect offset information
		/// </param>
		/// <param name="storePositions">true if the mapper should expect positions info
		/// </param>
		public abstract void  SetExpectations(System.String field, int numTerms, bool storeOffsets, bool storePositions);
		/// <summary> Map the Term Vector information into your own structure</summary>
		/// <param name="term">The term to add to the vector
		/// </param>
		/// <param name="frequency">The frequency of the term in the document
		/// </param>
		/// <param name="offsets">null if the offset is not specified, otherwise the offset into the field of the term
		/// </param>
		/// <param name="positions">null if the position is not specified, otherwise the position in the field of the term
		/// </param>
		public abstract void  Map(System.String term, int frequency, TermVectorOffsetInfo[] offsets, int[] positions);
		
		/// <summary> Indicate to Lucene that even if there are positions stored, this mapper is not interested in them and they
		/// can be skipped over.  Derived classes should set this to true if they want to ignore positions.  The default
		/// is false, meaning positions will be loaded if they are stored.
		/// </summary>
		/// <returns> false
		/// </returns>
		public virtual bool IsIgnoringPositions()
		{
			return ignoringPositions;
		}
		
		/// <summary> </summary>
		/// <seealso cref="IsIgnoringPositions()"> Same principal as {@link #IsIgnoringPositions()}, but applied to offsets.  false by default.
		/// </seealso>
		/// <returns> false
		/// </returns>
		public virtual bool IsIgnoringOffsets()
		{
			return ignoringOffsets;
		}
		
		/// <summary> Passes down the index of the document whose term vector is currently being mapped,
		/// once for each top level call to a term vector reader.
		/// <p/>
		/// Default implementation IGNORES the document number.  Override if your implementation needs the document number.
		/// <p/> 
		/// NOTE: Document numbers are internal to Lucene and subject to change depending on indexing operations.
		/// 
		/// </summary>
		/// <param name="documentNumber">index of document currently being mapped
		/// </param>
		public virtual void  SetDocumentNumber(int documentNumber)
		{
		}
	}
}
