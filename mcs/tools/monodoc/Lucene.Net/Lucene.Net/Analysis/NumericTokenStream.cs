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

using AttributeSource = Mono.Lucene.Net.Util.AttributeSource;
using NumericUtils = Mono.Lucene.Net.Util.NumericUtils;
using NumericField = Mono.Lucene.Net.Documents.NumericField;
// for javadocs
using NumericRangeQuery = Mono.Lucene.Net.Search.NumericRangeQuery;
using NumericRangeFilter = Mono.Lucene.Net.Search.NumericRangeFilter;
using SortField = Mono.Lucene.Net.Search.SortField;
using FieldCache = Mono.Lucene.Net.Search.FieldCache;
// javadocs
using TermAttribute = Mono.Lucene.Net.Analysis.Tokenattributes.TermAttribute;
using TypeAttribute = Mono.Lucene.Net.Analysis.Tokenattributes.TypeAttribute;
using PositionIncrementAttribute = Mono.Lucene.Net.Analysis.Tokenattributes.PositionIncrementAttribute;

namespace Mono.Lucene.Net.Analysis
{
	
	/// <summary> <b>Expert:</b> This class provides a {@link TokenStream}
	/// for indexing numeric values that can be used by {@link
	/// NumericRangeQuery} or {@link NumericRangeFilter}.
	/// 
	/// <p/>Note that for simple usage, {@link NumericField} is
	/// recommended.  {@link NumericField} disables norms and
	/// term freqs, as they are not usually needed during
	/// searching.  If you need to change these settings, you
	/// should use this class.
	/// 
	/// <p/>See {@link NumericField} for capabilities of fields
	/// indexed numerically.<p/>
	/// 
	/// <p/>Here's an example usage, for an <code>int</code> field:
	/// 
	/// <pre>
	///  Field field = new Field(name, new NumericTokenStream(precisionStep).setIntValue(value));
	///  field.setOmitNorms(true);
	///  field.setOmitTermFreqAndPositions(true);
	///  document.add(field);
	/// </pre>
	/// 
	/// <p/>For optimal performance, re-use the TokenStream and Field instance
	/// for more than one document:
	/// 
	/// <pre>
	///  NumericTokenStream stream = new NumericTokenStream(precisionStep);
	///  Field field = new Field(name, stream);
	///  field.setOmitNorms(true);
	///  field.setOmitTermFreqAndPositions(true);
	///  Document document = new Document();
	///  document.add(field);
	/// 
	///  for(all documents) {
	///    stream.setIntValue(value)
	///    writer.addDocument(document);
	///  }
	/// </pre>
	/// 
	/// <p/>This stream is not intended to be used in analyzers;
	/// it's more for iterating the different precisions during
	/// indexing a specific numeric value.<p/>
	/// 
	/// <p/><b>NOTE</b>: as token streams are only consumed once
	/// the document is added to the index, if you index more
	/// than one numeric field, use a separate <code>NumericTokenStream</code>
	/// instance for each.<p/>
	/// 
	/// <p/>See {@link NumericRangeQuery} for more details on the
	/// <a
	/// href="../search/NumericRangeQuery.html#precisionStepDesc"><code>precisionStep</code></a>
	/// parameter as well as how numeric fields work under the hood.<p/>
	/// 
	/// <p/><font color="red"><b>NOTE:</b> This API is experimental and
	/// might change in incompatible ways in the next release.</font>
	/// 
	/// </summary>
	/// <since> 2.9
	/// </since>
	public sealed class NumericTokenStream:TokenStream
	{
		private void  InitBlock()
		{
			termAtt = (TermAttribute) AddAttribute(typeof(TermAttribute));
			typeAtt = (TypeAttribute) AddAttribute(typeof(TypeAttribute));
			posIncrAtt = (PositionIncrementAttribute) AddAttribute(typeof(PositionIncrementAttribute));
		}
		
		/// <summary>The full precision token gets this token type assigned. </summary>
		public const System.String TOKEN_TYPE_FULL_PREC = "fullPrecNumeric";
		
		/// <summary>The lower precision tokens gets this token type assigned. </summary>
		public const System.String TOKEN_TYPE_LOWER_PREC = "lowerPrecNumeric";
		
		/// <summary> Creates a token stream for numeric values using the default <code>precisionStep</code>
		/// {@link NumericUtils#PRECISION_STEP_DEFAULT} (4). The stream is not yet initialized,
		/// before using set a value using the various set<em>???</em>Value() methods.
		/// </summary>
		public NumericTokenStream():this(NumericUtils.PRECISION_STEP_DEFAULT)
		{
		}
		
		/// <summary> Creates a token stream for numeric values with the specified
		/// <code>precisionStep</code>. The stream is not yet initialized,
		/// before using set a value using the various set<em>???</em>Value() methods.
		/// </summary>
		public NumericTokenStream(int precisionStep):base()
		{
			InitBlock();
			this.precisionStep = precisionStep;
			if (precisionStep < 1)
				throw new System.ArgumentException("precisionStep must be >=1");
		}
		
		/// <summary> Expert: Creates a token stream for numeric values with the specified
		/// <code>precisionStep</code> using the given {@link AttributeSource}.
		/// The stream is not yet initialized,
		/// before using set a value using the various set<em>???</em>Value() methods.
		/// </summary>
		public NumericTokenStream(AttributeSource source, int precisionStep):base(source)
		{
			InitBlock();
			this.precisionStep = precisionStep;
			if (precisionStep < 1)
				throw new System.ArgumentException("precisionStep must be >=1");
		}
		
		/// <summary> Expert: Creates a token stream for numeric values with the specified
		/// <code>precisionStep</code> using the given
		/// {@link org.apache.lucene.util.AttributeSource.AttributeFactory}.
		/// The stream is not yet initialized,
		/// before using set a value using the various set<em>???</em>Value() methods.
		/// </summary>
		public NumericTokenStream(AttributeFactory factory, int precisionStep):base(factory)
		{
			InitBlock();
			this.precisionStep = precisionStep;
			if (precisionStep < 1)
				throw new System.ArgumentException("precisionStep must be >=1");
		}
		
		/// <summary> Initializes the token stream with the supplied <code>long</code> value.</summary>
		/// <param name="value">the value, for which this TokenStream should enumerate tokens.
		/// </param>
		/// <returns> this instance, because of this you can use it the following way:
		/// <code>new Field(name, new NumericTokenStream(precisionStep).SetLongValue(value))</code>
		/// </returns>
		public NumericTokenStream SetLongValue(long value_Renamed)
		{
			this.value_Renamed = value_Renamed;
			valSize = 64;
			shift = 0;
			return this;
		}
		
		/// <summary> Initializes the token stream with the supplied <code>int</code> value.</summary>
		/// <param name="value">the value, for which this TokenStream should enumerate tokens.
		/// </param>
		/// <returns> this instance, because of this you can use it the following way:
		/// <code>new Field(name, new NumericTokenStream(precisionStep).SetIntValue(value))</code>
		/// </returns>
		public NumericTokenStream SetIntValue(int value_Renamed)
		{
			this.value_Renamed = (long) value_Renamed;
			valSize = 32;
			shift = 0;
			return this;
		}
		
		/// <summary> Initializes the token stream with the supplied <code>double</code> value.</summary>
		/// <param name="value">the value, for which this TokenStream should enumerate tokens.
		/// </param>
		/// <returns> this instance, because of this you can use it the following way:
		/// <code>new Field(name, new NumericTokenStream(precisionStep).SetDoubleValue(value))</code>
		/// </returns>
		public NumericTokenStream SetDoubleValue(double value_Renamed)
		{
			this.value_Renamed = NumericUtils.DoubleToSortableLong(value_Renamed);
			valSize = 64;
			shift = 0;
			return this;
		}
		
		/// <summary> Initializes the token stream with the supplied <code>float</code> value.</summary>
		/// <param name="value">the value, for which this TokenStream should enumerate tokens.
		/// </param>
		/// <returns> this instance, because of this you can use it the following way:
		/// <code>new Field(name, new NumericTokenStream(precisionStep).SetFloatValue(value))</code>
		/// </returns>
		public NumericTokenStream SetFloatValue(float value_Renamed)
		{
			this.value_Renamed = (long) NumericUtils.FloatToSortableInt(value_Renamed);
			valSize = 32;
			shift = 0;
			return this;
		}
		
		// @Override
		public override void  Reset()
		{
			if (valSize == 0)
				throw new System.SystemException("call set???Value() before usage");
			shift = 0;
		}
		
		// @Override
		public override bool IncrementToken()
		{
			if (valSize == 0)
				throw new System.SystemException("call set???Value() before usage");
			if (shift >= valSize)
				return false;
			
			ClearAttributes();
			char[] buffer;
			switch (valSize)
			{
				
				case 64: 
					buffer = termAtt.ResizeTermBuffer(NumericUtils.BUF_SIZE_LONG);
					termAtt.SetTermLength(NumericUtils.LongToPrefixCoded(value_Renamed, shift, buffer));
					break;
				
				
				case 32: 
					buffer = termAtt.ResizeTermBuffer(NumericUtils.BUF_SIZE_INT);
					termAtt.SetTermLength(NumericUtils.IntToPrefixCoded((int) value_Renamed, shift, buffer));
					break;
				
				
				default: 
					// should not happen
					throw new System.ArgumentException("valSize must be 32 or 64");
				
			}
			
			typeAtt.SetType((shift == 0)?TOKEN_TYPE_FULL_PREC:TOKEN_TYPE_LOWER_PREC);
			posIncrAtt.SetPositionIncrement((shift == 0)?1:0);
			shift += precisionStep;
			return true;
		}
		
		// @Override
		public override System.String ToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder("(numeric,valSize=").Append(valSize);
			sb.Append(",precisionStep=").Append(precisionStep).Append(')');
			return sb.ToString();
		}
		
		// members
		private TermAttribute termAtt;
		private TypeAttribute typeAtt;
		private PositionIncrementAttribute posIncrAtt;
		
		private int shift = 0, valSize = 0; // valSize==0 means not initialized
		private int precisionStep;
		
		private long value_Renamed = 0L;
	}
}
