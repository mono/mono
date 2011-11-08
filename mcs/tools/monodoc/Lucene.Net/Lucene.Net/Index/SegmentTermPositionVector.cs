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
	
	class SegmentTermPositionVector:SegmentTermVector, TermPositionVector
	{
		protected internal int[][] positions;
		protected internal TermVectorOffsetInfo[][] offsets;
		public static readonly int[] EMPTY_TERM_POS = new int[0];
		
		public SegmentTermPositionVector(System.String field, System.String[] terms, int[] termFreqs, int[][] positions, TermVectorOffsetInfo[][] offsets):base(field, terms, termFreqs)
		{
			this.offsets = offsets;
			this.positions = positions;
		}
		
		/// <summary> Returns an array of TermVectorOffsetInfo in which the term is found.
		/// 
		/// </summary>
		/// <param name="index">The position in the array to get the offsets from
		/// </param>
		/// <returns> An array of TermVectorOffsetInfo objects or the empty list
		/// </returns>
		/// <seealso cref="Mono.Lucene.Net.Analysis.Token">
		/// </seealso>
		public virtual TermVectorOffsetInfo[] GetOffsets(int index)
		{
			TermVectorOffsetInfo[] result = TermVectorOffsetInfo.EMPTY_OFFSET_INFO;
			if (offsets == null)
				return null;
			if (index >= 0 && index < offsets.Length)
			{
				result = offsets[index];
			}
			return result;
		}
		
		/// <summary> Returns an array of positions in which the term is found.
		/// Terms are identified by the index at which its number appears in the
		/// term String array obtained from the <code>indexOf</code> method.
		/// </summary>
		public virtual int[] GetTermPositions(int index)
		{
			int[] result = EMPTY_TERM_POS;
			if (positions == null)
				return null;
			if (index >= 0 && index < positions.Length)
			{
				result = positions[index];
			}
			
			return result;
		}
	}
}
