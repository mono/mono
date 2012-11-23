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
using NumericUtils = Mono.Lucene.Net.Util.NumericUtils;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> A {@link Filter} that only accepts numeric values within
	/// a specified range. To use this, you must first index the
	/// numeric values using {@link NumericField} (expert: {@link
	/// NumericTokenStream}).
	/// 
	/// <p/>You create a new NumericRangeFilter with the static
	/// factory methods, eg:
	/// 
	/// <pre>
	/// Filter f = NumericRangeFilter.newFloatRange("weight",
	/// new Float(0.3f), new Float(0.10f),
	/// true, true);
	/// </pre>
	/// 
	/// accepts all documents whose float valued "weight" field
	/// ranges from 0.3 to 0.10, inclusive.
	/// See {@link NumericRangeQuery} for details on how Lucene
	/// indexes and searches numeric valued fields.
	/// 
	/// <p/><font color="red"><b>NOTE:</b> This API is experimental and
	/// might change in incompatible ways in the next
	/// release.</font>
	/// 
	/// </summary>
	/// <since> 2.9
	/// 
	/// </since>
	[Serializable]
	public sealed class NumericRangeFilter:MultiTermQueryWrapperFilter
	{
		
		private NumericRangeFilter(NumericRangeQuery query):base(query)
		{
		}
		
		/// <summary> Factory that creates a <code>NumericRangeFilter</code>, that filters a <code>long</code>
		/// range using the given <a href="NumericRangeQuery.html#precisionStepDesc"><code>precisionStep</code></a>.
        /// You can have half-open ranges (which are in fact &lt;/&#8804; or &gt;/&#8805; queries)
		/// by setting the min or max value to <code>null</code>. By setting inclusive to false, it will
		/// match all documents excluding the bounds, with inclusive on, the boundaries are hits, too.
		/// </summary>
		public static NumericRangeFilter NewLongRange(System.String field, int precisionStep, System.ValueType min, System.ValueType max, bool minInclusive, bool maxInclusive)
		{
			return new NumericRangeFilter(NumericRangeQuery.NewLongRange(field, precisionStep, min, max, minInclusive, maxInclusive));
		}
		
		/// <summary> Factory that creates a <code>NumericRangeFilter</code>, that queries a <code>long</code>
		/// range using the default <code>precisionStep</code> {@link NumericUtils#PRECISION_STEP_DEFAULT} (4).
        /// You can have half-open ranges (which are in fact &lt;/&#8804; or &gt;/&#8805; queries)
		/// by setting the min or max value to <code>null</code>. By setting inclusive to false, it will
		/// match all documents excluding the bounds, with inclusive on, the boundaries are hits, too.
		/// </summary>
		public static NumericRangeFilter NewLongRange(System.String field, System.ValueType min, System.ValueType max, bool minInclusive, bool maxInclusive)
		{
			return new NumericRangeFilter(NumericRangeQuery.NewLongRange(field, min, max, minInclusive, maxInclusive));
		}
		
		/// <summary> Factory that creates a <code>NumericRangeFilter</code>, that filters a <code>int</code>
		/// range using the given <a href="NumericRangeQuery.html#precisionStepDesc"><code>precisionStep</code></a>.
        /// You can have half-open ranges (which are in fact &lt;/&#8804; or &gt;/&#8805; queries)
		/// by setting the min or max value to <code>null</code>. By setting inclusive to false, it will
		/// match all documents excluding the bounds, with inclusive on, the boundaries are hits, too.
		/// </summary>
		public static NumericRangeFilter NewIntRange(System.String field, int precisionStep, System.ValueType min, System.ValueType max, bool minInclusive, bool maxInclusive)
		{
			return new NumericRangeFilter(NumericRangeQuery.NewIntRange(field, precisionStep, min, max, minInclusive, maxInclusive));
		}
		
		/// <summary> Factory that creates a <code>NumericRangeFilter</code>, that queries a <code>int</code>
		/// range using the default <code>precisionStep</code> {@link NumericUtils#PRECISION_STEP_DEFAULT} (4).
        /// You can have half-open ranges (which are in fact &lt;/&#8804; or &gt;/&#8805; queries)
		/// by setting the min or max value to <code>null</code>. By setting inclusive to false, it will
		/// match all documents excluding the bounds, with inclusive on, the boundaries are hits, too.
		/// </summary>
		public static NumericRangeFilter NewIntRange(System.String field, System.ValueType min, System.ValueType max, bool minInclusive, bool maxInclusive)
		{
			return new NumericRangeFilter(NumericRangeQuery.NewIntRange(field, min, max, minInclusive, maxInclusive));
		}
		
		/// <summary> Factory that creates a <code>NumericRangeFilter</code>, that filters a <code>double</code>
		/// range using the given <a href="NumericRangeQuery.html#precisionStepDesc"><code>precisionStep</code></a>.
        /// You can have half-open ranges (which are in fact &lt;/&#8804; or &gt;/&#8805; queries)
		/// by setting the min or max value to <code>null</code>. By setting inclusive to false, it will
		/// match all documents excluding the bounds, with inclusive on, the boundaries are hits, too.
		/// </summary>
		public static NumericRangeFilter NewDoubleRange(System.String field, int precisionStep, System.Double min, System.Double max, bool minInclusive, bool maxInclusive)
		{
			return new NumericRangeFilter(NumericRangeQuery.NewDoubleRange(field, precisionStep, min, max, minInclusive, maxInclusive));
		}
		
		/// <summary> Factory that creates a <code>NumericRangeFilter</code>, that queries a <code>double</code>
		/// range using the default <code>precisionStep</code> {@link NumericUtils#PRECISION_STEP_DEFAULT} (4).
        /// You can have half-open ranges (which are in fact &lt;/&#8804; or &gt;/&#8805; queries)
		/// by setting the min or max value to <code>null</code>. By setting inclusive to false, it will
		/// match all documents excluding the bounds, with inclusive on, the boundaries are hits, too.
		/// </summary>
		public static NumericRangeFilter NewDoubleRange(System.String field, System.Double min, System.Double max, bool minInclusive, bool maxInclusive)
		{
			return new NumericRangeFilter(NumericRangeQuery.NewDoubleRange(field, min, max, minInclusive, maxInclusive));
		}
		
		/// <summary> Factory that creates a <code>NumericRangeFilter</code>, that filters a <code>float</code>
		/// range using the given <a href="NumericRangeQuery.html#precisionStepDesc"><code>precisionStep</code></a>.
        /// You can have half-open ranges (which are in fact &lt;/&#8804; or &gt;/&#8805; queries)
		/// by setting the min or max value to <code>null</code>. By setting inclusive to false, it will
		/// match all documents excluding the bounds, with inclusive on, the boundaries are hits, too.
		/// </summary>
		public static NumericRangeFilter NewFloatRange(System.String field, int precisionStep, System.Single min, System.Single max, bool minInclusive, bool maxInclusive)
		{
			return new NumericRangeFilter(NumericRangeQuery.NewFloatRange(field, precisionStep, min, max, minInclusive, maxInclusive));
		}
		
		/// <summary> Factory that creates a <code>NumericRangeFilter</code>, that queries a <code>float</code>
		/// range using the default <code>precisionStep</code> {@link NumericUtils#PRECISION_STEP_DEFAULT} (4).
        /// You can have half-open ranges (which are in fact &lt;/&#8804; or &gt;/&#8805; queries)
		/// by setting the min or max value to <code>null</code>. By setting inclusive to false, it will
		/// match all documents excluding the bounds, with inclusive on, the boundaries are hits, too.
		/// </summary>
		public static NumericRangeFilter NewFloatRange(System.String field, System.Single min, System.Single max, bool minInclusive, bool maxInclusive)
		{
			return new NumericRangeFilter(NumericRangeQuery.NewFloatRange(field, min, max, minInclusive, maxInclusive));
		}
		
		/// <summary>Returns the field name for this filter </summary>
		public System.String GetField()
		{
			return ((NumericRangeQuery) query).GetField();
		}
		
		/// <summary>Returns <code>true</code> if the lower endpoint is inclusive </summary>
		public bool IncludesMin()
		{
			return ((NumericRangeQuery) query).IncludesMin();
		}
		
		/// <summary>Returns <code>true</code> if the upper endpoint is inclusive </summary>
		public bool IncludesMax()
		{
			return ((NumericRangeQuery) query).IncludesMax();
		}
		
		/// <summary>Returns the lower value of this range filter </summary>
		public System.ValueType GetMin()
		{
			return ((NumericRangeQuery) query).GetMin();
		}
		
		/// <summary>Returns the upper value of this range filter </summary>
		public System.ValueType GetMax()
		{
			return ((NumericRangeQuery) query).GetMax();
		}
	}
}
