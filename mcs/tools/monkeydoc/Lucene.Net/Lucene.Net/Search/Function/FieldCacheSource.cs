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

using IndexReader = Mono.Lucene.Net.Index.IndexReader;
using FieldCache = Mono.Lucene.Net.Search.FieldCache;

namespace Mono.Lucene.Net.Search.Function
{
	
	/// <summary> Expert: A base class for ValueSource implementations that retrieve values for
	/// a single field from the {@link Mono.Lucene.Net.Search.FieldCache FieldCache}.
	/// <p/>
	/// Fields used herein nust be indexed (doesn't matter if these fields are stored or not).
	/// <p/> 
	/// It is assumed that each such indexed field is untokenized, or at least has a single token in a document.
	/// For documents with multiple tokens of the same field, behavior is undefined (It is likely that current 
	/// code would use the value of one of these tokens, but this is not guaranteed).
	/// <p/>
	/// Document with no tokens in this field are assigned the <code>Zero</code> value.    
	/// 
	/// <p/><font color="#FF0000">
	/// WARNING: The status of the <b>Search.Function</b> package is experimental. 
	/// The APIs introduced here might change in the future and will not be 
	/// supported anymore in such a case.</font>
	/// 
	/// <p/><b>NOTE</b>: with the switch in 2.9 to segment-based
	/// searching, if {@link #getValues} is invoked with a
	/// composite (multi-segment) reader, this can easily cause
	/// double RAM usage for the values in the FieldCache.  It's
	/// best to switch your application to pass only atomic
	/// (single segment) readers to this API.  Alternatively, for
	/// a short-term fix, you could wrap your ValueSource using
	/// {@link MultiValueSource}, which costs more CPU per lookup
	/// but will not consume double the FieldCache RAM.<p/>
	/// </summary>
	[Serializable]
	public abstract class FieldCacheSource:ValueSource
	{
		private System.String field;
		
		/// <summary> Create a cached field source for the input field.  </summary>
		public FieldCacheSource(System.String field)
		{
			this.field = field;
		}
		
		/* (non-Javadoc) @see Mono.Lucene.Net.Search.Function.ValueSource#getValues(Mono.Lucene.Net.Index.IndexReader) */
		public override DocValues GetValues(IndexReader reader)
		{
			return GetCachedFieldValues(Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT, field, reader);
		}
		
		/* (non-Javadoc) @see Mono.Lucene.Net.Search.Function.ValueSource#description() */
		public override System.String Description()
		{
			return field;
		}
		
		/// <summary> Return cached DocValues for input field and reader.</summary>
		/// <param name="cache">FieldCache so that values of a field are loaded once per reader (RAM allowing)
		/// </param>
		/// <param name="field">Field for which values are required.
		/// </param>
		/// <seealso cref="ValueSource">
		/// </seealso>
		public abstract DocValues GetCachedFieldValues(FieldCache cache, System.String field, IndexReader reader);
		
		/*(non-Javadoc) @see java.lang.Object#equals(java.lang.Object) */
		public  override bool Equals(System.Object o)
		{
			if (!(o is FieldCacheSource))
			{
				return false;
			}
			FieldCacheSource other = (FieldCacheSource) o;
			return this.field.Equals(other.field) && CachedFieldSourceEquals(other);
		}
		
		/*(non-Javadoc) @see java.lang.Object#hashCode() */
		public override int GetHashCode()
		{
			return field.GetHashCode() + CachedFieldSourceHashCode();
		}
		
		/// <summary> Check if equals to another {@link FieldCacheSource}, already knowing that cache and field are equal.  </summary>
		/// <seealso cref="Object.equals(java.lang.Object)">
		/// </seealso>
		public abstract bool CachedFieldSourceEquals(FieldCacheSource other);
		
		/// <summary> Return a hash code of a {@link FieldCacheSource}, without the hash-codes of the field 
		/// and the cache (those are taken care of elsewhere).  
		/// </summary>
		/// <seealso cref="Object.hashCode()">
		/// </seealso>
		public abstract int CachedFieldSourceHashCode();
	}
}
