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
	
	/// <summary> Convenience class for holding TermVector information.</summary>
	public class TermVectorEntry
	{
		private System.String field;
		private System.String term;
		private int frequency;
		private TermVectorOffsetInfo[] offsets;
		internal int[] positions;
		
		
		public TermVectorEntry()
		{
		}
		
		public TermVectorEntry(System.String field, System.String term, int frequency, TermVectorOffsetInfo[] offsets, int[] positions)
		{
			this.field = field;
			this.term = term;
			this.frequency = frequency;
			this.offsets = offsets;
			this.positions = positions;
		}
		
		
		public virtual System.String GetField()
		{
			return field;
		}
		
		public virtual int GetFrequency()
		{
			return frequency;
		}
		
		public virtual TermVectorOffsetInfo[] GetOffsets()
		{
			return offsets;
		}
		
		public virtual int[] GetPositions()
		{
			return positions;
		}
		
		public virtual System.String GetTerm()
		{
			return term;
		}
		
		//Keep package local
		internal virtual void  SetFrequency(int frequency)
		{
			this.frequency = frequency;
		}
		
		internal virtual void  SetOffsets(TermVectorOffsetInfo[] offsets)
		{
			this.offsets = offsets;
		}
		
		internal virtual void  SetPositions(int[] positions)
		{
			this.positions = positions;
		}
		
		
		public  override bool Equals(System.Object o)
		{
			if (this == o)
				return true;
			if (o == null || GetType() != o.GetType())
				return false;
			
			TermVectorEntry that = (TermVectorEntry) o;
			
			if (term != null?!term.Equals(that.term):that.term != null)
				return false;
			
			return true;
		}
		
		public override int GetHashCode()
		{
			return (term != null?term.GetHashCode():0);
		}
		
		public override System.String ToString()
		{
			return "TermVectorEntry{" + "field='" + field + '\'' + ", term='" + term + '\'' + ", frequency=" + frequency + '}';
		}
	}
}
