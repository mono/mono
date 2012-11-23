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

using NumericTokenStream = Mono.Lucene.Net.Analysis.NumericTokenStream;
using NumericField = Mono.Lucene.Net.Documents.NumericField;
using IndexReader = Mono.Lucene.Net.Index.IndexReader;
using Term = Mono.Lucene.Net.Index.Term;
using NumericUtils = Mono.Lucene.Net.Util.NumericUtils;
using StringHelper = Mono.Lucene.Net.Util.StringHelper;
using ToStringUtils = Mono.Lucene.Net.Util.ToStringUtils;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> <p/>A {@link Query} that matches numeric values within a
	/// specified range.  To use this, you must first index the
	/// numeric values using {@link NumericField} (expert: {@link
	/// NumericTokenStream}).  If your terms are instead textual,
	/// you should use {@link TermRangeQuery}.  {@link
	/// NumericRangeFilter} is the filter equivalent of this
	/// query.<p/>
	/// 
	/// <p/>You create a new NumericRangeQuery with the static
	/// factory methods, eg:
	/// 
	/// <pre>
	/// Query q = NumericRangeQuery.newFloatRange("weight",
	/// new Float(0.3f), new Float(0.10f),
	/// true, true);
	/// </pre>
	/// 
	/// matches all documents whose float valued "weight" field
	/// ranges from 0.3 to 0.10, inclusive.
	/// 
	/// <p/>The performance of NumericRangeQuery is much better
	/// than the corresponding {@link TermRangeQuery} because the
	/// number of terms that must be searched is usually far
	/// fewer, thanks to trie indexing, described below.<p/>
	/// 
	/// <p/>You can optionally specify a <a
	/// href="#precisionStepDesc"><code>precisionStep</code></a>
	/// when creating this query.  This is necessary if you've
	/// changed this configuration from its default (4) during
	/// indexing.  Lower values consume more disk space but speed
	/// up searching.  Suitable values are between <b>1</b> and
	/// <b>8</b>. A good starting point to test is <b>4</b>,
	/// which is the default value for all <code>Numeric*</code>
	/// classes.  See <a href="#precisionStepDesc">below</a> for
	/// details.
	/// 
	/// <p/>This query defaults to {@linkplain
	/// MultiTermQuery#CONSTANT_SCORE_AUTO_REWRITE_DEFAULT} for
	/// 32 bit (int/float) ranges with precisionStep &lt;8 and 64
	/// bit (long/double) ranges with precisionStep &lt;6.
	/// Otherwise it uses {@linkplain
	/// MultiTermQuery#CONSTANT_SCORE_FILTER_REWRITE} as the
	/// number of terms is likely to be high.  With precision
	/// steps of &lt;4, this query can be run with one of the
	/// BooleanQuery rewrite methods without changing
	/// BooleanQuery's default max clause count.
	/// 
	/// <p/><font color="red"><b>NOTE:</b> This API is experimental and
	/// might change in incompatible ways in the next release.</font>
	/// 
	/// <br/><h3>How it works</h3>
	/// 
	/// <p/>See the publication about <a target="_blank" href="http://www.panfmp.org">panFMP</a>,
	/// where this algorithm was described (referred to as <code>TrieRangeQuery</code>):
	/// 
	/// <blockquote><strong>Schindler, U, Diepenbroek, M</strong>, 2008.
	/// <em>Generic XML-based Framework for Metadata Portals.</em>
	/// Computers &amp; Geosciences 34 (12), 1947-1955.
	/// <a href="http://dx.doi.org/10.1016/j.cageo.2008.02.023"
	/// target="_blank">doi:10.1016/j.cageo.2008.02.023</a></blockquote>
	/// 
	/// <p/><em>A quote from this paper:</em> Because Apache Lucene is a full-text
	/// search engine and not a conventional database, it cannot handle numerical ranges
	/// (e.g., field value is inside user defined bounds, even dates are numerical values).
	/// We have developed an extension to Apache Lucene that stores
	/// the numerical values in a special string-encoded format with variable precision
	/// (all numerical values like doubles, longs, floats, and ints are converted to
	/// lexicographic sortable string representations and stored with different precisions
	/// (for a more detailed description of how the values are stored,
	/// see {@link NumericUtils}). A range is then divided recursively into multiple intervals for searching:
	/// The center of the range is searched only with the lowest possible precision in the <em>trie</em>,
	/// while the boundaries are matched more exactly. This reduces the number of terms dramatically.<p/>
	/// 
	/// <p/>For the variant that stores long values in 8 different precisions (each reduced by 8 bits) that
	/// uses a lowest precision of 1 byte, the index contains only a maximum of 256 distinct values in the
	/// lowest precision. Overall, a range could consist of a theoretical maximum of
	/// <code>7*255*2 + 255 = 3825</code> distinct terms (when there is a term for every distinct value of an
	/// 8-byte-number in the index and the range covers almost all of them; a maximum of 255 distinct values is used
	/// because it would always be possible to reduce the full 256 values to one term with degraded precision).
	/// In practice, we have seen up to 300 terms in most cases (index with 500,000 metadata records
	/// and a uniform value distribution).<p/>
	/// 
	/// <a name="precisionStepDesc"/><h3>Precision Step</h3>
	/// <p/>You can choose any <code>precisionStep</code> when encoding values.
	/// Lower step values mean more precisions and so more terms in index (and index gets larger).
	/// On the other hand, the maximum number of terms to match reduces, which optimized query speed.
	/// The formula to calculate the maximum term count is:
	/// <pre>
	/// n = [ (bitsPerValue/precisionStep - 1) * (2^precisionStep - 1 ) * 2 ] + (2^precisionStep - 1 )
	/// </pre>
	/// <p/><em>(this formula is only correct, when <code>bitsPerValue/precisionStep</code> is an integer;
	/// in other cases, the value must be rounded up and the last summand must contain the modulo of the division as
	/// precision step)</em>.
	/// For longs stored using a precision step of 4, <code>n = 15*15*2 + 15 = 465</code>, and for a precision
	/// step of 2, <code>n = 31*3*2 + 3 = 189</code>. But the faster search speed is reduced by more seeking
	/// in the term enum of the index. Because of this, the ideal <code>precisionStep</code> value can only
	/// be found out by testing. <b>Important:</b> You can index with a lower precision step value and test search speed
	/// using a multiple of the original step value.<p/>
	/// 
	/// <p/>Good values for <code>precisionStep</code> are depending on usage and data type:
	/// <ul>
	/// <li>The default for all data types is <b>4</b>, which is used, when no <code>precisionStep</code> is given.</li>
	/// <li>Ideal value in most cases for <em>64 bit</em> data types <em>(long, double)</em> is <b>6</b> or <b>8</b>.</li>
	/// <li>Ideal value in most cases for <em>32 bit</em> data types <em>(int, float)</em> is <b>4</b>.</li>
	/// <li>Steps <b>&gt;64</b> for <em>long/double</em> and <b>&gt;32</b> for <em>int/float</em> produces one token
	/// per value in the index and querying is as slow as a conventional {@link TermRangeQuery}. But it can be used
	/// to produce fields, that are solely used for sorting (in this case simply use {@link Integer#MAX_VALUE} as
	/// <code>precisionStep</code>). Using {@link NumericField NumericFields} for sorting
	/// is ideal, because building the field cache is much faster than with text-only numbers.
	/// Sorting is also possible with range query optimized fields using one of the above <code>precisionSteps</code>.</li>
	/// </ul>
	/// 
	/// <p/>Comparisons of the different types of RangeQueries on an index with about 500,000 docs showed
	/// that {@link TermRangeQuery} in boolean rewrite mode (with raised {@link BooleanQuery} clause count)
	/// took about 30-40 secs to complete, {@link TermRangeQuery} in constant score filter rewrite mode took 5 secs
	/// and executing this class took &lt;100ms to complete (on an Opteron64 machine, Java 1.5, 8 bit
	/// precision step). This query type was developed for a geographic portal, where the performance for
	/// e.g. bounding boxes or exact date/time stamps is important.<p/>
	/// 
	/// </summary>
	/// <since> 2.9
	/// 
	/// </since>
	[Serializable]
	public sealed class NumericRangeQuery:MultiTermQuery
	{
		
		private NumericRangeQuery(System.String field, int precisionStep, int valSize, System.ValueType min, System.ValueType max, bool minInclusive, bool maxInclusive)
		{
			System.Diagnostics.Debug.Assert((valSize == 32 || valSize == 64));
			if (precisionStep < 1)
				throw new System.ArgumentException("precisionStep must be >=1");
			this.field = StringHelper.Intern(field);
			this.precisionStep = precisionStep;
			this.valSize = valSize;
			this.min = min;
			this.max = max;
			this.minInclusive = minInclusive;
			this.maxInclusive = maxInclusive;
			
			// For bigger precisionSteps this query likely
			// hits too many terms, so set to CONSTANT_SCORE_FILTER right off
			// (especially as the FilteredTermEnum is costly if wasted only for AUTO tests because it
			// creates new enums from IndexReader for each sub-range)
			switch (valSize)
			{
				
				case 64: 
					SetRewriteMethod((precisionStep > 6)?CONSTANT_SCORE_FILTER_REWRITE:CONSTANT_SCORE_AUTO_REWRITE_DEFAULT);
					break;
				
				case 32: 
					SetRewriteMethod((precisionStep > 8)?CONSTANT_SCORE_FILTER_REWRITE:CONSTANT_SCORE_AUTO_REWRITE_DEFAULT);
					break;
				
				default: 
					// should never happen
					throw new System.ArgumentException("valSize must be 32 or 64");
				
			}
			
			// shortcut if upper bound == lower bound
			if (min != null && min.Equals(max))
			{
				SetRewriteMethod(CONSTANT_SCORE_BOOLEAN_QUERY_REWRITE);
			}
		}
		
		/// <summary> Factory that creates a <code>NumericRangeQuery</code>, that queries a <code>long</code>
		/// range using the given <a href="#precisionStepDesc"><code>precisionStep</code></a>.
        /// You can have half-open ranges (which are in fact &lt;/&#8804; or &gt;/&#8805; queries)
		/// by setting the min or max value to <code>null</code>. By setting inclusive to false, it will
		/// match all documents excluding the bounds, with inclusive on, the boundaries are hits, too.
		/// </summary>
		public static NumericRangeQuery NewLongRange(System.String field, int precisionStep, System.ValueType min, System.ValueType max, bool minInclusive, bool maxInclusive)
		{
			return new NumericRangeQuery(field, precisionStep, 64, min, max, minInclusive, maxInclusive);
		}
		
		/// <summary> Factory that creates a <code>NumericRangeQuery</code>, that queries a <code>long</code>
		/// range using the default <code>precisionStep</code> {@link NumericUtils#PRECISION_STEP_DEFAULT} (4).
        /// You can have half-open ranges (which are in fact &lt;/&#8804; or &gt;/&#8805; queries)
		/// by setting the min or max value to <code>null</code>. By setting inclusive to false, it will
		/// match all documents excluding the bounds, with inclusive on, the boundaries are hits, too.
		/// </summary>
		public static NumericRangeQuery NewLongRange(System.String field, System.ValueType min, System.ValueType max, bool minInclusive, bool maxInclusive)
		{
			return new NumericRangeQuery(field, NumericUtils.PRECISION_STEP_DEFAULT, 64, min, max, minInclusive, maxInclusive);
		}
		
		/// <summary> Factory that creates a <code>NumericRangeQuery</code>, that queries a <code>int</code>
		/// range using the given <a href="#precisionStepDesc"><code>precisionStep</code></a>.
        /// You can have half-open ranges (which are in fact &lt;/&#8804; or &gt;/&#8805; queries)
		/// by setting the min or max value to <code>null</code>. By setting inclusive to false, it will
		/// match all documents excluding the bounds, with inclusive on, the boundaries are hits, too.
		/// </summary>
		public static NumericRangeQuery NewIntRange(System.String field, int precisionStep, System.ValueType min, System.ValueType max, bool minInclusive, bool maxInclusive)
		{
			return new NumericRangeQuery(field, precisionStep, 32, min, max, minInclusive, maxInclusive);
		}
		
		/// <summary> Factory that creates a <code>NumericRangeQuery</code>, that queries a <code>int</code>
		/// range using the default <code>precisionStep</code> {@link NumericUtils#PRECISION_STEP_DEFAULT} (4).
        /// You can have half-open ranges (which are in fact &lt;/&#8804; or &gt;/&#8805; queries)
		/// by setting the min or max value to <code>null</code>. By setting inclusive to false, it will
		/// match all documents excluding the bounds, with inclusive on, the boundaries are hits, too.
		/// </summary>
		public static NumericRangeQuery NewIntRange(System.String field, System.ValueType min, System.ValueType max, bool minInclusive, bool maxInclusive)
		{
			return new NumericRangeQuery(field, NumericUtils.PRECISION_STEP_DEFAULT, 32, min, max, minInclusive, maxInclusive);
		}
		
		/// <summary> Factory that creates a <code>NumericRangeQuery</code>, that queries a <code>double</code>
		/// range using the given <a href="#precisionStepDesc"><code>precisionStep</code></a>.
        /// You can have half-open ranges (which are in fact &lt;/&#8804; or &gt;/&#8805; queries)
		/// by setting the min or max value to <code>null</code>. By setting inclusive to false, it will
		/// match all documents excluding the bounds, with inclusive on, the boundaries are hits, too.
		/// </summary>
		public static NumericRangeQuery NewDoubleRange(System.String field, int precisionStep, System.Double min, System.Double max, bool minInclusive, bool maxInclusive)
		{
			return new NumericRangeQuery(field, precisionStep, 64, min, max, minInclusive, maxInclusive);
		}
		
		/// <summary> Factory that creates a <code>NumericRangeQuery</code>, that queries a <code>double</code>
		/// range using the default <code>precisionStep</code> {@link NumericUtils#PRECISION_STEP_DEFAULT} (4).
        /// You can have half-open ranges (which are in fact &lt;/&#8804; or &gt;/&#8805; queries)
		/// by setting the min or max value to <code>null</code>. By setting inclusive to false, it will
		/// match all documents excluding the bounds, with inclusive on, the boundaries are hits, too.
		/// </summary>
		public static NumericRangeQuery NewDoubleRange(System.String field, System.Double min, System.Double max, bool minInclusive, bool maxInclusive)
		{
			return new NumericRangeQuery(field, NumericUtils.PRECISION_STEP_DEFAULT, 64, min, max, minInclusive, maxInclusive);
		}
		
		/// <summary> Factory that creates a <code>NumericRangeQuery</code>, that queries a <code>float</code>
		/// range using the given <a href="#precisionStepDesc"><code>precisionStep</code></a>.
        /// You can have half-open ranges (which are in fact &lt;/&#8804; or &gt;/&#8805; queries)
		/// by setting the min or max value to <code>null</code>. By setting inclusive to false, it will
		/// match all documents excluding the bounds, with inclusive on, the boundaries are hits, too.
		/// </summary>
		public static NumericRangeQuery NewFloatRange(System.String field, int precisionStep, System.Single min, System.Single max, bool minInclusive, bool maxInclusive)
		{
			return new NumericRangeQuery(field, precisionStep, 32, min, max, minInclusive, maxInclusive);
		}
		
		/// <summary> Factory that creates a <code>NumericRangeQuery</code>, that queries a <code>float</code>
		/// range using the default <code>precisionStep</code> {@link NumericUtils#PRECISION_STEP_DEFAULT} (4).
        /// You can have half-open ranges (which are in fact &lt;/&#8804; or &gt;/&#8805; queries)
		/// by setting the min or max value to <code>null</code>. By setting inclusive to false, it will
		/// match all documents excluding the bounds, with inclusive on, the boundaries are hits, too.
		/// </summary>
		public static NumericRangeQuery NewFloatRange(System.String field, System.Single min, System.Single max, bool minInclusive, bool maxInclusive)
		{
			return new NumericRangeQuery(field, NumericUtils.PRECISION_STEP_DEFAULT, 32, min, max, minInclusive, maxInclusive);
		}
		
		//@Override
		public /*protected internal*/ override FilteredTermEnum GetEnum(IndexReader reader)
		{
			return new NumericRangeTermEnum(this, reader);
		}
		
		/// <summary>Returns the field name for this query </summary>
		public System.String GetField()
		{
			return field;
		}
		
		/// <summary>Returns <code>true</code> if the lower endpoint is inclusive </summary>
		public bool IncludesMin()
		{
			return minInclusive;
		}
		
		/// <summary>Returns <code>true</code> if the upper endpoint is inclusive </summary>
		public bool IncludesMax()
		{
			return maxInclusive;
		}
		
		/// <summary>Returns the lower value of this range query </summary>
		public System.ValueType GetMin()
		{
			return min;
		}
		
		/// <summary>Returns the upper value of this range query </summary>
		public System.ValueType GetMax()
		{
			return max;
		}
		
		//@Override
		public override System.String ToString(System.String field)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			if (!this.field.Equals(field))
				sb.Append(this.field).Append(':');
			return sb.Append(minInclusive?'[':'{').Append((min == null)?"*":min.ToString()).Append(" TO ").Append((max == null)?"*":max.ToString()).Append(maxInclusive?']':'}').Append(ToStringUtils.Boost(GetBoost())).ToString();
		}
		
		//@Override
		public  override bool Equals(System.Object o)
		{
			if (o == this)
				return true;
			if (!base.Equals(o))
				return false;
			if (o is NumericRangeQuery)
			{
				NumericRangeQuery q = (NumericRangeQuery) o;
				return ((System.Object) field == (System.Object) q.field && (q.min == null?min == null:q.min.Equals(min)) && (q.max == null?max == null:q.max.Equals(max)) && minInclusive == q.minInclusive && maxInclusive == q.maxInclusive && precisionStep == q.precisionStep);
			}
			return false;
		}
		
		//@Override
		public override int GetHashCode()
		{
			int hash = base.GetHashCode();
			hash += (field.GetHashCode() ^ 0x4565fd66 + precisionStep ^ 0x64365465);
			if (min != null)
				hash += (min.GetHashCode() ^ 0x14fa55fb);
			if (max != null)
				hash += (max.GetHashCode() ^ 0x733fa5fe);
			return hash + (minInclusive.GetHashCode() ^ 0x14fa55fb) + (maxInclusive.GetHashCode() ^ 0x733fa5fe);
		}

         // field must be interned after reading from stream
        //private void ReadObject(java.io.ObjectInputStream in) 
        //{
        //    in.defaultReadObject();
        //    field = StringHelper.intern(field);
        //}


        [System.Runtime.Serialization.OnDeserialized]
        internal void OnDeserialized(System.Runtime.Serialization.StreamingContext context)
        {
            field = StringHelper.Intern(field);
        }
		
		// members (package private, to be also fast accessible by NumericRangeTermEnum)
		internal System.String field;
		internal int precisionStep;
		internal int valSize;
		internal System.ValueType min;
		internal System.ValueType max;
		internal bool minInclusive;
		internal bool maxInclusive;
		
		/// <summary> Subclass of FilteredTermEnum for enumerating all terms that match the
		/// sub-ranges for trie range queries.
		/// <p/>
		/// WARNING: This term enumeration is not guaranteed to be always ordered by
		/// {@link Term#compareTo}.
		/// The ordering depends on how {@link NumericUtils#splitLongRange} and
		/// {@link NumericUtils#splitIntRange} generates the sub-ranges. For
		/// {@link MultiTermQuery} ordering is not relevant.
		/// </summary>
		private sealed class NumericRangeTermEnum:FilteredTermEnum
		{
			private class AnonymousClassLongRangeBuilder:NumericUtils.LongRangeBuilder
			{
				public AnonymousClassLongRangeBuilder(NumericRangeTermEnum enclosingInstance)
				{
					InitBlock(enclosingInstance);
				}
				private void  InitBlock(NumericRangeTermEnum enclosingInstance)
				{
					this.enclosingInstance = enclosingInstance;
				}
				private NumericRangeTermEnum enclosingInstance;
				public NumericRangeTermEnum Enclosing_Instance
				{
					get
					{
						return enclosingInstance;
					}
					
				}
				//@Override
				public override void  AddRange(System.String minPrefixCoded, System.String maxPrefixCoded)
				{
					Enclosing_Instance.rangeBounds.Add(minPrefixCoded);
					Enclosing_Instance.rangeBounds.Add(maxPrefixCoded);
				}
			}
			private class AnonymousClassIntRangeBuilder:NumericUtils.IntRangeBuilder
			{
				public AnonymousClassIntRangeBuilder(NumericRangeTermEnum enclosingInstance)
				{
					InitBlock(enclosingInstance);
				}
				private void  InitBlock(NumericRangeTermEnum enclosingInstance)
				{
					this.enclosingInstance = enclosingInstance;
				}
				private NumericRangeTermEnum enclosingInstance;
				public NumericRangeTermEnum Enclosing_Instance
				{
					get
					{
						return enclosingInstance;
					}
					
				}
				//@Override
				public override void  AddRange(System.String minPrefixCoded, System.String maxPrefixCoded)
				{
					Enclosing_Instance.rangeBounds.Add(minPrefixCoded);
					Enclosing_Instance.rangeBounds.Add(maxPrefixCoded);
				}
			}
			private void  InitBlock(NumericRangeQuery enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private NumericRangeQuery enclosingInstance;
			public NumericRangeQuery Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			
			private IndexReader reader;
			private System.Collections.ArrayList rangeBounds = new System.Collections.ArrayList();
			private System.String currentUpperBound = null;
			
			internal NumericRangeTermEnum(NumericRangeQuery enclosingInstance, IndexReader reader)
			{
				InitBlock(enclosingInstance);
				this.reader = reader;
				
				switch (Enclosing_Instance.valSize)
				{
					
					case 64:  {
							// lower
							long minBound = System.Int64.MinValue;
							if (Enclosing_Instance.min is System.Int64)
							{
								minBound = System.Convert.ToInt64(Enclosing_Instance.min);
							}
							else if (Enclosing_Instance.min is System.Double)
							{
								minBound = NumericUtils.DoubleToSortableLong(System.Convert.ToDouble(Enclosing_Instance.min));
							}
							if (!Enclosing_Instance.minInclusive && Enclosing_Instance.min != null)
							{
								if (minBound == System.Int64.MaxValue)
									break;
								minBound++;
							}
							
							// upper
							long maxBound = System.Int64.MaxValue;
							if (Enclosing_Instance.max is System.Int64)
							{
								maxBound = System.Convert.ToInt64(Enclosing_Instance.max);
							}
							else if (Enclosing_Instance.max is System.Double)
							{
								maxBound = NumericUtils.DoubleToSortableLong(System.Convert.ToDouble(Enclosing_Instance.max));
							}
							if (!Enclosing_Instance.maxInclusive && Enclosing_Instance.max != null)
							{
								if (maxBound == System.Int64.MinValue)
									break;
								maxBound--;
							}
							
							NumericUtils.SplitLongRange(new AnonymousClassLongRangeBuilder(this), Enclosing_Instance.precisionStep, minBound, maxBound);
							break;
						}
					
					
					case 32:  {
							// lower
							int minBound = System.Int32.MinValue;
							if (Enclosing_Instance.min is System.Int32)
							{
								minBound = System.Convert.ToInt32(Enclosing_Instance.min);
							}
							else if (Enclosing_Instance.min is System.Single)
							{
								minBound = NumericUtils.FloatToSortableInt(System.Convert.ToSingle(Enclosing_Instance.min));
							}
							if (!Enclosing_Instance.minInclusive && Enclosing_Instance.min != null)
							{
								if (minBound == System.Int32.MaxValue)
									break;
								minBound++;
							}
							
							// upper
							int maxBound = System.Int32.MaxValue;
							if (Enclosing_Instance.max is System.Int32)
							{
								maxBound = System.Convert.ToInt32(Enclosing_Instance.max);
							}
							else if (Enclosing_Instance.max is System.Single)
							{
								maxBound = NumericUtils.FloatToSortableInt(System.Convert.ToSingle(Enclosing_Instance.max));
							}
							if (!Enclosing_Instance.maxInclusive && Enclosing_Instance.max != null)
							{
								if (maxBound == System.Int32.MinValue)
									break;
								maxBound--;
							}
							
							NumericUtils.SplitIntRange(new AnonymousClassIntRangeBuilder(this), Enclosing_Instance.precisionStep, minBound, maxBound);
							break;
						}
					
					
					default: 
						// should never happen
						throw new System.ArgumentException("valSize must be 32 or 64");
					
				}
				
				// seek to first term
				Next();
			}
			
			//@Override
			public override float Difference()
			{
				return 1.0f;
			}
			
			/// <summary>this is a dummy, it is not used by this class. </summary>
			//@Override
			public override bool EndEnum()
			{
				System.Diagnostics.Debug.Assert(false); // should never be called
				return (currentTerm != null);
			}
			
			/// <summary> Compares if current upper bound is reached,
			/// this also updates the term count for statistics.
			/// In contrast to {@link FilteredTermEnum}, a return value
			/// of <code>false</code> ends iterating the current enum
			/// and forwards to the next sub-range.
			/// </summary>
			//@Override
			public /*protected internal*/ override bool TermCompare(Term term)
			{
				return ((System.Object) term.Field() == (System.Object) Enclosing_Instance.field && String.CompareOrdinal(term.Text(), currentUpperBound) <= 0);
			}
			
			/// <summary>Increments the enumeration to the next element.  True if one exists. </summary>
			//@Override
			public override bool Next()
			{
				// if a current term exists, the actual enum is initialized:
				// try change to next term, if no such term exists, fall-through
				if (currentTerm != null)
				{
					System.Diagnostics.Debug.Assert(actualEnum != null);
					if (actualEnum.Next())
					{
						currentTerm = actualEnum.Term();
						if (TermCompare(currentTerm))
							return true;
					}
				}
				// if all above fails, we go forward to the next enum,
				// if one is available
				currentTerm = null;
				if (rangeBounds.Count < 2)
					return false;
				// close the current enum and read next bounds
				if (actualEnum != null)
				{
					actualEnum.Close();
					actualEnum = null;
				}
				System.Object tempObject;
				tempObject = rangeBounds[0];
				rangeBounds.RemoveAt(0);
				System.String lowerBound = (System.String) tempObject;
				System.Object tempObject2;
				tempObject2 = rangeBounds[0];
				rangeBounds.RemoveAt(0);
				this.currentUpperBound = ((System.String) tempObject2);
				// this call recursively uses next(), if no valid term in
				// next enum found.
				// if this behavior is changed/modified in the superclass,
				// this enum will not work anymore!
				SetEnum(reader.Terms(new Term(Enclosing_Instance.field, lowerBound)));
				return (currentTerm != null);
			}
			
			/// <summary>Closes the enumeration to further activity, freeing resources.  </summary>
			//@Override
			public override void  Close()
			{
				rangeBounds.Clear();
				currentUpperBound = null;
				base.Close();
			}
		}
	}
}
