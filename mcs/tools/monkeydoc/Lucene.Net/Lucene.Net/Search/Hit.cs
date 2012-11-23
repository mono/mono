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

using Document = Mono.Lucene.Net.Documents.Document;
using CorruptIndexException = Mono.Lucene.Net.Index.CorruptIndexException;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> Wrapper used by {@link HitIterator} to provide a lazily loaded hit
	/// from {@link Hits}.
	/// 
	/// </summary>
	/// <deprecated> Use {@link TopScoreDocCollector} and {@link TopDocs} instead. Hits will be removed in Lucene 3.0.
	/// </deprecated>
    [Obsolete("Use TopScoreDocCollector and TopDocs instead. Hits will be removed in Lucene 3.0.")]
	[Serializable]
	public class Hit
	{
		
		private Document doc = null;
		
		private bool resolved = false;
		
		private Hits hits = null;
		private int hitNumber;
		
		/// <summary> Constructed from {@link HitIterator}</summary>
		/// <param name="hits">Hits returned from a search
		/// </param>
		/// <param name="hitNumber">Hit index in Hits
		/// </param>
		internal Hit(Hits hits, int hitNumber)
		{
			this.hits = hits;
			this.hitNumber = hitNumber;
		}
		
		/// <summary> Returns document for this hit.
		/// 
		/// </summary>
		/// <seealso cref="Hits.Doc(int)">
		/// </seealso>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual Document GetDocument()
		{
			if (!resolved)
				FetchTheHit();
			return doc;
		}
		
		/// <summary> Returns score for this hit.
		/// 
		/// </summary>
		/// <seealso cref="Hits.Score(int)">
		/// </seealso>
		public virtual float GetScore()
		{
			return hits.Score(hitNumber);
		}
		
		/// <summary> Returns id for this hit.
		/// 
		/// </summary>
		/// <seealso cref="Hits.Id(int)">
		/// </seealso>
		public virtual int GetId()
		{
			return hits.Id(hitNumber);
		}
		
		private void  FetchTheHit()
		{
			doc = hits.Doc(hitNumber);
			resolved = true;
		}
		
		// provide some of the Document style interface (the simple stuff)
		
		/// <summary> Returns the boost factor for this hit on any field of the underlying document.
		/// 
		/// </summary>
		/// <seealso cref="Document.GetBoost()">
		/// </seealso>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual float GetBoost()
		{
			return GetDocument().GetBoost();
		}
		
		/// <summary> Returns the string value of the field with the given name if any exist in
		/// this document, or null.  If multiple fields exist with this name, this
		/// method returns the first value added. If only binary fields with this name
		/// exist, returns null.
		/// 
		/// </summary>
		/// <seealso cref="Document.Get(String)">
		/// </seealso>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual System.String Get(System.String name)
		{
			return GetDocument().Get(name);
		}
		
		/// <summary> Prints the parameters to be used to discover the promised result.</summary>
		public override System.String ToString()
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			buffer.Append("Hit<");
			buffer.Append(hits.ToString());
			buffer.Append(" [");
			buffer.Append(hitNumber);
			buffer.Append("] ");
			if (resolved)
			{
				buffer.Append("resolved");
			}
			else
			{
				buffer.Append("unresolved");
			}
			buffer.Append(">");
			return buffer.ToString();
		}
	}
}
