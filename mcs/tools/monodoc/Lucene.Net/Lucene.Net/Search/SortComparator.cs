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

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> Abstract base class for sorting hits returned by a Query.
	/// 
	/// <p/>
	/// This class should only be used if the other SortField types (SCORE, DOC,
	/// STRING, INT, FLOAT) do not provide an adequate sorting. It maintains an
	/// internal cache of values which could be quite large. The cache is an array of
	/// Comparable, one for each document in the index. There is a distinct
	/// Comparable for each unique term in the field - if some documents have the
	/// same term in the field, the cache array will have entries which reference the
	/// same Comparable.
	/// 
	/// This class will be used as part of a key to a FieldCache value. You must
	/// implement hashCode and equals to avoid an explosion in RAM usage if you use
	/// instances that are not the same instance. If you are searching using the
	/// Remote contrib, the same instance of this class on the client will be a new
	/// instance on every call to the server, so hashCode/equals is very important in
	/// that situation.
	/// 
	/// <p/>
	/// Created: Apr 21, 2004 5:08:38 PM
	/// 
	/// 
	/// </summary>
	/// <version>  $Id: SortComparator.java 800119 2009-08-02 17:59:21Z markrmiller $
	/// </version>
	/// <since> 1.4
	/// </since>
	/// <deprecated> Please use {@link FieldComparatorSource} instead.
	/// </deprecated>
    [Obsolete("Please use FieldComparatorSource instead.")]
	[Serializable]
	public abstract class SortComparator : SortComparatorSource
	{
		private class AnonymousClassScoreDocComparator : ScoreDocComparator
		{
			public AnonymousClassScoreDocComparator(System.IComparable[] cachedValues, SortComparator enclosingInstance)
			{
				InitBlock(cachedValues, enclosingInstance);
			}
			private void  InitBlock(System.IComparable[] cachedValues, SortComparator enclosingInstance)
			{
				this.cachedValues = cachedValues;
				this.enclosingInstance = enclosingInstance;
			}
			private System.IComparable[] cachedValues;
			private SortComparator enclosingInstance;
			public SortComparator Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			
			public virtual int Compare(ScoreDoc i, ScoreDoc j)
			{
				return cachedValues[i.doc].CompareTo(cachedValues[j.doc]);
			}
			
			public virtual System.IComparable SortValue(ScoreDoc i)
			{
				return cachedValues[i.doc];
			}
			
			public virtual int SortType()
			{
				return SortField.CUSTOM;
			}
		}
		
		// inherit javadocs
		public virtual ScoreDocComparator NewComparator(IndexReader reader, System.String fieldname)
		{
			System.String field = String.Intern(fieldname);
			System.IComparable[] cachedValues = Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.GetCustom(reader, field, this);
			
			return new AnonymousClassScoreDocComparator(cachedValues, this);
		}
		
		/// <summary> Returns an object which, when sorted according to natural order,
		/// will order the Term values in the correct order.
		/// <p/>For example, if the Terms contained integer values, this method
		/// would return <code>new Integer(termtext)</code>.  Note that this
		/// might not always be the most efficient implementation - for this
		/// particular example, a better implementation might be to make a
		/// ScoreDocLookupComparator that uses an internal lookup table of int.
		/// </summary>
		/// <param name="termtext">The textual value of the term.
		/// </param>
		/// <returns> An object representing <code>termtext</code> that sorts according to the natural order of <code>termtext</code>.
		/// </returns>
		/// <seealso cref="Comparable">
		/// </seealso>
		/// <seealso cref="ScoreDocComparator">
		/// </seealso>
		public /*protected internal*/ abstract System.IComparable GetComparable(System.String termtext);
	}
}
