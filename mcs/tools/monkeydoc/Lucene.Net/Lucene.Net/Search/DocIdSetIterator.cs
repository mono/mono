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
	
	/// <summary> This abstract class defines methods to iterate over a set of non-decreasing
	/// doc ids. Note that this class assumes it iterates on doc Ids, and therefore
	/// {@link #NO_MORE_DOCS} is set to {@value #NO_MORE_DOCS} in order to be used as
	/// a sentinel object. Implementations of this class are expected to consider
	/// {@link Integer#MAX_VALUE} as an invalid value.
	/// </summary>
	public abstract class DocIdSetIterator
	{
		
		// TODO (3.0): review the javadocs and remove any references to '3.0'.
		private int doc = - 1;
		
		/// <summary> When returned by {@link #NextDoc()}, {@link #Advance(int)} and
		/// {@link #Doc()} it means there are no more docs in the iterator.
		/// </summary>
		public static readonly int NO_MORE_DOCS = System.Int32.MaxValue;
		
		/// <summary> Unsupported anymore. Call {@link #DocID()} instead. This method throws
		/// {@link UnsupportedOperationException} if called.
		/// 
		/// </summary>
		/// <deprecated> use {@link #DocID()} instead.
		/// </deprecated>
        [Obsolete("use DocID() instead.")]
		public virtual int Doc()
		{
			throw new System.NotSupportedException("Call docID() instead.");
		}
		
		/// <summary> Returns the following:
		/// <ul>
		/// <li>-1 or {@link #NO_MORE_DOCS} if {@link #NextDoc()} or
		/// {@link #Advance(int)} were not called yet.</li>
		/// <li>{@link #NO_MORE_DOCS} if the iterator has exhausted.</li>
		/// <li>Otherwise it should return the doc ID it is currently on.</li>
		/// </ul>
		/// <p/>
		/// <b>NOTE:</b> in 3.0, this method will become abstract.
		/// 
		/// </summary>
		/// <since> 2.9
		/// </since>
		public virtual int DocID()
		{
			return doc;
		}
		
		/// <summary> Unsupported anymore. Call {@link #NextDoc()} instead. This method throws
		/// {@link UnsupportedOperationException} if called.
		/// 
		/// </summary>
		/// <deprecated> use {@link #NextDoc()} instead. This will be removed in 3.0
		/// </deprecated>
        [Obsolete("use NextDoc() instead. This will be removed in 3.0")]
		public virtual bool Next()
		{
			throw new System.NotSupportedException("Call nextDoc() instead.");
		}
		
		/// <summary> Unsupported anymore. Call {@link #Advance(int)} instead. This method throws
		/// {@link UnsupportedOperationException} if called.
		/// 
		/// </summary>
		/// <deprecated> use {@link #Advance(int)} instead. This will be removed in 3.0
		/// </deprecated>
        [Obsolete("use Advance(int) instead. This will be removed in 3.0")]
		public virtual bool SkipTo(int target)
		{
			throw new System.NotSupportedException("Call advance() instead.");
		}
		
		/// <summary> Advances to the next document in the set and returns the doc it is
		/// currently on, or {@link #NO_MORE_DOCS} if there are no more docs in the
		/// set.<br/>
		/// 
		/// <b>NOTE:</b> in 3.0 this method will become abstract, following the removal
		/// of {@link #Next()}. For backward compatibility it is implemented as:
		/// 
		/// <pre>
		/// public int nextDoc() throws IOException {
		/// return next() ? doc() : NO_MORE_DOCS;
		/// }
		/// </pre>
		/// 
		/// <b>NOTE:</b> after the iterator has exhausted you should not call this
		/// method, as it may result in unpredicted behavior.
		/// 
		/// </summary>
		/// <since> 2.9
		/// </since>
		public virtual int NextDoc()
		{
			return doc = Next()?Doc():NO_MORE_DOCS;
		}
		
		/// <summary> Advances to the first beyond the current whose document number is greater
		/// than or equal to <i>target</i>. Returns the current document number or
		/// {@link #NO_MORE_DOCS} if there are no more docs in the set.
		/// <p/>
		/// Behaves as if written:
		/// 
		/// <pre>
		/// int advance(int target) {
		/// int doc;
		/// while ((doc = nextDoc()) &lt; target) {
		/// }
		/// return doc;
		/// }
		/// </pre>
		/// 
		/// Some implementations are considerably more efficient than that.
		/// <p/>
		/// <b>NOTE:</b> certain implemenations may return a different value (each
		/// time) if called several times in a row with the same target.
		/// <p/>
		/// <b>NOTE:</b> this method may be called with {@value #NO_MORE_DOCS} for
		/// efficiency by some Scorers. If your implementation cannot efficiently
		/// determine that it should exhaust, it is recommended that you check for that
		/// value in each call to this method.
		/// <p/>
		/// <b>NOTE:</b> after the iterator has exhausted you should not call this
		/// method, as it may result in unpredicted behavior.
		/// <p/>
		/// <b>NOTE:</b> in 3.0 this method will become abstract, following the removal
		/// of {@link #SkipTo(int)}.
		/// 
		/// </summary>
		/// <since> 2.9
		/// </since>
		public virtual int Advance(int target)
		{
			if (target == NO_MORE_DOCS)
			{
				return doc = NO_MORE_DOCS;
			}
			return doc = SkipTo(target)?Doc():NO_MORE_DOCS;
		}
	}
}
