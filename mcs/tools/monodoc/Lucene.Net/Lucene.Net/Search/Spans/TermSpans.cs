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

using Term = Mono.Lucene.Net.Index.Term;
using TermPositions = Mono.Lucene.Net.Index.TermPositions;

namespace Mono.Lucene.Net.Search.Spans
{
	
	/// <summary> Expert:
	/// Public for extension only
	/// </summary>
	public class TermSpans:Spans
	{
		protected internal TermPositions positions;
		protected internal Term term;
		protected internal int doc;
		protected internal int freq;
		protected internal int count;
		protected internal int position;
		
		
		public TermSpans(TermPositions positions, Term term)
		{
			
			this.positions = positions;
			this.term = term;
			doc = - 1;
		}
		
		public override bool Next()
		{
			if (count == freq)
			{
				if (!positions.Next())
				{
					doc = System.Int32.MaxValue;
					return false;
				}
				doc = positions.Doc();
				freq = positions.Freq();
				count = 0;
			}
			position = positions.NextPosition();
			count++;
			return true;
		}
		
		public override bool SkipTo(int target)
		{
			if (!positions.SkipTo(target))
			{
				doc = System.Int32.MaxValue;
				return false;
			}
			
			doc = positions.Doc();
			freq = positions.Freq();
			count = 0;
			
			position = positions.NextPosition();
			count++;
			
			return true;
		}
		
		public override int Doc()
		{
			return doc;
		}
		
		public override int Start()
		{
			return position;
		}
		
		public override int End()
		{
			return position + 1;
		}
		
		// TODO: Remove warning after API has been finalized
		public override System.Collections.Generic.ICollection<byte[]> GetPayload()
		{
			byte[] bytes = new byte[positions.GetPayloadLength()];
			bytes = positions.GetPayload(bytes, 0);
            System.Collections.Generic.List<byte[]> val = new System.Collections.Generic.List<byte[]>();
            val.Add(bytes);
            return val;
		}
		
		// TODO: Remove warning after API has been finalized
		public override bool IsPayloadAvailable()
		{
			return positions.IsPayloadAvailable();
		}
		
		public override System.String ToString()
		{
			return "spans(" + term.ToString() + ")@" + (doc == - 1?"START":((doc == System.Int32.MaxValue)?"END":doc + "-" + position));
		}
		
		
		public virtual TermPositions GetPositions()
		{
			return positions;
		}
	}
}
