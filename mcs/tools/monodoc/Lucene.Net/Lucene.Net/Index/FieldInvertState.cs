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

using AttributeSource = Mono.Lucene.Net.Util.AttributeSource;

namespace Mono.Lucene.Net.Index
{
	
	/// <summary> This class tracks the number and position / offset parameters of terms
	/// being added to the index. The information collected in this class is
	/// also used to calculate the normalization factor for a field.
	/// 
	/// <p/><b>WARNING</b>: This API is new and experimental, and may suddenly
	/// change.<p/>
	/// </summary>
	public sealed class FieldInvertState
	{
		internal int position;
		internal int length;
		internal int numOverlap;
		internal int offset;
		internal float boost;
		internal AttributeSource attributeSource;
		
		public FieldInvertState()
		{
		}
		
		public FieldInvertState(int position, int length, int numOverlap, int offset, float boost)
		{
			this.position = position;
			this.length = length;
			this.numOverlap = numOverlap;
			this.offset = offset;
			this.boost = boost;
		}
		
		/// <summary> Re-initialize the state, using this boost value.</summary>
		/// <param name="docBoost">boost value to use.
		/// </param>
		internal void  Reset(float docBoost)
		{
			position = 0;
			length = 0;
			numOverlap = 0;
			offset = 0;
			boost = docBoost;
			attributeSource = null;
		}
		
		/// <summary> Get the last processed term position.</summary>
		/// <returns> the position
		/// </returns>
		public int GetPosition()
		{
			return position;
		}
		
		/// <summary> Get total number of terms in this field.</summary>
		/// <returns> the length
		/// </returns>
		public int GetLength()
		{
			return length;
		}
		
		/// <summary> Get the number of terms with <code>positionIncrement == 0</code>.</summary>
		/// <returns> the numOverlap
		/// </returns>
		public int GetNumOverlap()
		{
			return numOverlap;
		}
		
		/// <summary> Get end offset of the last processed term.</summary>
		/// <returns> the offset
		/// </returns>
		public int GetOffset()
		{
			return offset;
		}
		
		/// <summary> Get boost value. This is the cumulative product of
		/// document boost and field boost for all field instances
		/// sharing the same field name.
		/// </summary>
		/// <returns> the boost
		/// </returns>
		public float GetBoost()
		{
			return boost;
		}
		
		public AttributeSource GetAttributeSource()
		{
			return attributeSource;
		}
	}
}
