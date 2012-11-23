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

using TokenStream = Mono.Lucene.Net.Analysis.TokenStream;
using FieldInvertState = Mono.Lucene.Net.Index.FieldInvertState;

namespace Mono.Lucene.Net.Documents
{
	
	/// <summary> Synonymous with {@link Field}.
	/// 
	/// <p/><bold>WARNING</bold>: This interface may change within minor versions, despite Lucene's backward compatibility requirements.
	/// This means new methods may be added from version to version.  This change only affects the Fieldable API; other backwards
	/// compatibility promises remain intact. For example, Lucene can still
	/// read and write indices created within the same major version.
	/// <p/>
	/// 
	/// 
	/// </summary>
	public interface Fieldable
	{
		/// <summary>Sets the boost factor hits on this field.  This value will be
		/// multiplied into the score of all hits on this this field of this
		/// document.
		/// 
		/// <p/>The boost is multiplied by {@link Mono.Lucene.Net.Documents.Document#GetBoost()} of the document
		/// containing this field.  If a document has multiple fields with the same
		/// name, all such values are multiplied together.  This product is then
		/// used to compute the norm factor for the field.  By
		/// default, in the {@link
		/// Mono.Lucene.Net.Search.Similarity#ComputeNorm(String,
		/// FieldInvertState)} method, the boost value is multiplied
		/// by the {@link
		/// Mono.Lucene.Net.Search.Similarity#LengthNorm(String,
		/// int)} and then rounded by {@link Mono.Lucene.Net.Search.Similarity#EncodeNorm(float)} before it is stored in the
		/// index.  One should attempt to ensure that this product does not overflow
		/// the range of that encoding.
		/// 
		/// </summary>
		/// <seealso cref="Mono.Lucene.Net.Documents.Document.SetBoost(float)">
		/// </seealso>
		/// <seealso cref="Mono.Lucene.Net.Search.Similarity.ComputeNorm(String, FieldInvertState)">
		/// </seealso>
		/// <seealso cref="Mono.Lucene.Net.Search.Similarity.EncodeNorm(float)">
		/// </seealso>
		void  SetBoost(float boost);
		
		/// <summary>Returns the boost factor for hits for this field.
		/// 
		/// <p/>The default value is 1.0.
		/// 
		/// <p/>Note: this value is not stored directly with the document in the index.
		/// Documents returned from {@link Mono.Lucene.Net.Index.IndexReader#Document(int)} and
		/// {@link Mono.Lucene.Net.Search.Hits#Doc(int)} may thus not have the same value present as when
		/// this field was indexed.
		/// 
		/// </summary>
		/// <seealso cref="SetBoost(float)">
		/// </seealso>
		float GetBoost();
		
		/// <summary>Returns the name of the field as an interned string.
		/// For example "date", "title", "body", ...
		/// </summary>
		System.String Name();
		
		/// <summary>The value of the field as a String, or null.
		/// <p/>
		/// For indexing, if isStored()==true, the stringValue() will be used as the stored field value
		/// unless isBinary()==true, in which case binaryValue() will be used.
		/// 
		/// If isIndexed()==true and isTokenized()==false, this String value will be indexed as a single token.
		/// If isIndexed()==true and isTokenized()==true, then tokenStreamValue() will be used to generate indexed tokens if not null,
		/// else readerValue() will be used to generate indexed tokens if not null, else stringValue() will be used to generate tokens.
		/// </summary>
		System.String StringValue();
		
		/// <summary>The value of the field as a Reader, which can be used at index time to generate indexed tokens.</summary>
		/// <seealso cref="StringValue()">
		/// </seealso>
		System.IO.TextReader ReaderValue();
		
		/// <summary>The value of the field in Binary, or null.</summary>
		/// <seealso cref="StringValue()">
		/// </seealso>
		byte[] BinaryValue();
		
		/// <summary>The TokenStream for this field to be used when indexing, or null.</summary>
		/// <seealso cref="StringValue()">
		/// </seealso>
		TokenStream TokenStreamValue();
		
		/// <summary>True if the value of the field is to be stored in the index for return
		/// with search hits. 
		/// </summary>
		bool IsStored();
		
		/// <summary>True if the value of the field is to be indexed, so that it may be
		/// searched on. 
		/// </summary>
		bool IsIndexed();
		
		/// <summary>True if the value of the field should be tokenized as text prior to
		/// indexing.  Un-tokenized fields are indexed as a single word and may not be
		/// Reader-valued. 
		/// </summary>
		bool IsTokenized();
		
		/// <summary>True if the value of the field is stored and compressed within the index </summary>
		bool IsCompressed();
		
		/// <summary>True if the term or terms used to index this field are stored as a term
		/// vector, available from {@link Mono.Lucene.Net.Index.IndexReader#GetTermFreqVector(int,String)}.
		/// These methods do not provide access to the original content of the field,
		/// only to terms used to index it. If the original content must be
		/// preserved, use the <code>stored</code> attribute instead.
		/// 
		/// </summary>
		/// <seealso cref="Mono.Lucene.Net.Index.IndexReader.GetTermFreqVector(int, String)">
		/// </seealso>
		bool IsTermVectorStored();
		
		/// <summary> True if terms are stored as term vector together with their offsets 
		/// (start and end positon in source text).
		/// </summary>
		bool IsStoreOffsetWithTermVector();
		
		/// <summary> True if terms are stored as term vector together with their token positions.</summary>
		bool IsStorePositionWithTermVector();
		
		/// <summary>True if the value of the field is stored as binary </summary>
		bool IsBinary();
		
		/// <summary>True if norms are omitted for this indexed field </summary>
		bool GetOmitNorms();
		
		/// <summary>Expert:
		/// 
		/// If set, omit normalization factors associated with this indexed field.
		/// This effectively disables indexing boosts and length normalization for this field.
		/// </summary>
		void  SetOmitNorms(bool omitNorms);
		
		/// <deprecated> Renamed to {@link AbstractField#setOmitTermFreqAndPositions} 
		/// </deprecated>
        [Obsolete("Renamed to AbstractField.SetOmitTermFreqAndPositions")]
		void  SetOmitTf(bool omitTf);
		
		/// <deprecated> Renamed to {@link AbstractField#getOmitTermFreqAndPositions} 
		/// </deprecated>
        [Obsolete("Renamed to AbstractField.GetOmitTermFreqAndPositions")]
		bool GetOmitTf();
		
		/// <summary> Indicates whether a Field is Lazy or not.  The semantics of Lazy loading are such that if a Field is lazily loaded, retrieving
		/// it's values via {@link #StringValue()} or {@link #BinaryValue()} is only valid as long as the {@link Mono.Lucene.Net.Index.IndexReader} that
		/// retrieved the {@link Document} is still open.
		/// 
		/// </summary>
		/// <returns> true if this field can be loaded lazily
		/// </returns>
		bool IsLazy();
		
		/// <summary> Returns offset into byte[] segment that is used as value, if Field is not binary
		/// returned value is undefined
		/// </summary>
		/// <returns> index of the first character in byte[] segment that represents this Field value
		/// </returns>
		int GetBinaryOffset();
		
		/// <summary> Returns length of byte[] segment that is used as value, if Field is not binary
		/// returned value is undefined
		/// </summary>
		/// <returns> length of byte[] segment that represents this Field value
		/// </returns>
		int GetBinaryLength();
		
		/// <summary> Return the raw byte[] for the binary field.  Note that
		/// you must also call {@link #getBinaryLength} and {@link
		/// #getBinaryOffset} to know which range of bytes in this
		/// returned array belong to the field.
		/// </summary>
		/// <returns> reference to the Field value as byte[].
		/// </returns>
		byte[] GetBinaryValue();
		
		/// <summary> Return the raw byte[] for the binary field.  Note that
		/// you must also call {@link #getBinaryLength} and {@link
		/// #getBinaryOffset} to know which range of bytes in this
		/// returned array belong to the field.<p/>
		/// About reuse: if you pass in the result byte[] and it is
		/// used, likely the underlying implementation will hold
		/// onto this byte[] and return it in future calls to
		/// {@link #BinaryValue()} or {@link #GetBinaryValue()}.
		/// So if you subsequently re-use the same byte[] elsewhere
		/// it will alter this Fieldable's value.
		/// </summary>
		/// <param name="result"> User defined buffer that will be used if
		/// possible.  If this is null or not large enough, a new
		/// buffer is allocated
		/// </param>
		/// <returns> reference to the Field value as byte[].
		/// </returns>
		byte[] GetBinaryValue(byte[] result);
	}
}
