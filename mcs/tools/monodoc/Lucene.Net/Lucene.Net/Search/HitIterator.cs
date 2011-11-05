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

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> An iterator over {@link Hits} that provides lazy fetching of each document.
	/// {@link Hits#Iterator()} returns an instance of this class.  Calls to {@link #next()}
	/// return a {@link Hit} instance.
	/// 
	/// </summary>
	/// <deprecated> Use {@link TopScoreDocCollector} and {@link TopDocs} instead. Hits will be removed in Lucene 3.0.
	/// </deprecated>
    [Obsolete("Use TopScoreDocCollector and TopDocs instead. Hits will be removed in Lucene 3.0.")]
	public class HitIterator : System.Collections.IEnumerator
	{
		/// <summary> Returns a {@link Hit} instance representing the next hit in {@link Hits}.
		/// 
		/// </summary>
		/// <returns> Next {@link Hit}.
		/// </returns>
		public virtual System.Object Current
		{
			get
			{
				if (hitNumber == hits.Length())
					throw new System.ArgumentOutOfRangeException();
				
				System.Object next = new Hit(hits, hitNumber);
				hitNumber++;
				return next;
			}
			
		}
		private Hits hits;
		private int hitNumber = 0;
		
		/// <summary> Constructed from {@link Hits#Iterator()}.</summary>
		internal HitIterator(Hits hits)
		{
			this.hits = hits;
		}
		
		/// <returns> true if current hit is less than the total number of {@link Hits}.
		/// </returns>
		public virtual bool MoveNext()
		{
			return hitNumber < hits.Length();
		}
		
		/// <summary> Unsupported operation.
		/// 
		/// </summary>
		/// <throws>  UnsupportedOperationException </throws>
		public virtual void  Remove()
		{
			throw new System.NotSupportedException();
		}
		
		/// <summary> Returns the total number of hits.</summary>
		public virtual int Length()
		{
			return hits.Length();
		}
		
		virtual public void  Reset()
		{
            System.Diagnostics.Debug.Fail("Port issue:", "Lets see if we need this HitIterator.Reset()"); // {{Aroush-2.9}}
		}
	}
}
