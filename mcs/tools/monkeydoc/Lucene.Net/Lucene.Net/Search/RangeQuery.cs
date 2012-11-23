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
using Term = Mono.Lucene.Net.Index.Term;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> A Query that matches documents within an exclusive range of terms.
	/// 
	/// <p/>This query matches the documents looking for terms that fall into the
	/// supplied range according to {@link Term#CompareTo(Term)}. It is not intended
	/// for numerical ranges, use {@link NumericRangeQuery} instead.
	/// 
	/// <p/>This query uses {@linkplain
	/// MultiTermQuery#SCORING_BOOLEAN_QUERY_REWRITE}.  If you
	/// want to change this, use the new {@link TermRangeQuery}
	/// instead.
	/// 
	/// </summary>
	/// <deprecated> Use {@link TermRangeQuery} for term ranges or
	/// {@link NumericRangeQuery} for numeric ranges instead.
	/// This class will be removed in Lucene 3.0.
	/// </deprecated>
    [Obsolete("Use TermRangeQuery for term ranges or NumericRangeQuery for numeric ranges instead. This class will be removed in Lucene 3.0")]
	[Serializable]
	public class RangeQuery:Query
	{
		private TermRangeQuery delegate_Renamed;
		
		/// <summary>Constructs a query selecting all terms greater than
		/// <code>lowerTerm</code> but less than <code>upperTerm</code>.
		/// There must be at least one term and either term may be null,
		/// in which case there is no bound on that side, but if there are
		/// two terms, both terms <b>must</b> be for the same field.
		/// 
		/// </summary>
		/// <param name="lowerTerm">The Term at the lower end of the range
		/// </param>
		/// <param name="upperTerm">The Term at the upper end of the range
		/// </param>
		/// <param name="inclusive">If true, both <code>lowerTerm</code> and
		/// <code>upperTerm</code> will themselves be included in the range.
		/// </param>
		public RangeQuery(Term lowerTerm, Term upperTerm, bool inclusive):this(lowerTerm, upperTerm, inclusive, null)
		{
		}
		
		/// <summary>Constructs a query selecting all terms greater than
		/// <code>lowerTerm</code> but less than <code>upperTerm</code>.
		/// There must be at least one term and either term may be null,
		/// in which case there is no bound on that side, but if there are
		/// two terms, both terms <b>must</b> be for the same field.
		/// <p/>
		/// If <code>collator</code> is not null, it will be used to decide whether
		/// index terms are within the given range, rather than using the Unicode code
		/// point order in which index terms are stored.
		/// <p/>
		/// <strong>WARNING:</strong> Using this constructor and supplying a non-null
		/// value in the <code>collator</code> parameter will cause every single 
		/// index Term in the Field referenced by lowerTerm and/or upperTerm to be
		/// examined.  Depending on the number of index Terms in this Field, the 
		/// operation could be very slow.
		/// 
		/// </summary>
		/// <param name="lowerTerm">The Term at the lower end of the range
		/// </param>
		/// <param name="upperTerm">The Term at the upper end of the range
		/// </param>
		/// <param name="inclusive">If true, both <code>lowerTerm</code> and
		/// <code>upperTerm</code> will themselves be included in the range.
		/// </param>
		/// <param name="collator">The collator to use to collate index Terms, to determine
		/// their membership in the range bounded by <code>lowerTerm</code> and
		/// <code>upperTerm</code>.
		/// </param>
		public RangeQuery(Term lowerTerm, Term upperTerm, bool inclusive, System.Globalization.CompareInfo collator)
		{
			if (lowerTerm == null && upperTerm == null)
				throw new System.ArgumentException("At least one term must be non-null");
			if (lowerTerm != null && upperTerm != null && (System.Object) lowerTerm.Field() != (System.Object) upperTerm.Field())
				throw new System.ArgumentException("Both terms must have the same field");
			
			delegate_Renamed = new TermRangeQuery((lowerTerm == null)?upperTerm.Field():lowerTerm.Field(), (lowerTerm == null)?null:lowerTerm.Text(), (upperTerm == null)?null:upperTerm.Text(), inclusive, inclusive, collator);
			delegate_Renamed.SetRewriteMethod(TermRangeQuery.SCORING_BOOLEAN_QUERY_REWRITE);
		}
		
		public override void  SetBoost(float b)
		{
			base.SetBoost(b);
			delegate_Renamed.SetBoost(b);
		}
		
		public override Query Rewrite(IndexReader reader)
		{
			return delegate_Renamed.Rewrite(reader);
		}
		
		/// <summary>Returns the field name for this query </summary>
		public virtual System.String GetField()
		{
			return delegate_Renamed.GetField();
		}
		
		/// <summary>Returns the lower term of this range query. </summary>
		public virtual Term GetLowerTerm()
		{
			System.String term = delegate_Renamed.GetLowerTerm();
			return (term == null)?null:new Term(GetField(), term);
		}
		
		/// <summary>Returns the upper term of this range query. </summary>
		public virtual Term GetUpperTerm()
		{
			System.String term = delegate_Renamed.GetUpperTerm();
			return (term == null)?null:new Term(GetField(), term);
		}
		
		/// <summary>Returns <code>true</code> if the range query is inclusive </summary>
		public virtual bool IsInclusive()
		{
			return delegate_Renamed.IncludesLower() && delegate_Renamed.IncludesUpper();
		}
		
		/// <summary>Returns the collator used to determine range inclusion, if any. </summary>
		public virtual System.Globalization.CompareInfo GetCollator()
		{
			return delegate_Renamed.GetCollator();
		}
		
		/// <summary>Prints a user-readable version of this query. </summary>
		public override System.String ToString(System.String field)
		{
			return delegate_Renamed.ToString(field);
		}
		
		/// <summary>Returns true iff <code>o</code> is equal to this. </summary>
		public  override bool Equals(System.Object o)
		{
			if (this == o)
				return true;
			if (!(o is RangeQuery))
				return false;
			
			RangeQuery other = (RangeQuery) o;
			return this.delegate_Renamed.Equals(other.delegate_Renamed);
		}
		
		/// <summary>Returns a hash code value for this object.</summary>
		public override int GetHashCode()
		{
			return delegate_Renamed.GetHashCode();
		}
	}
}
