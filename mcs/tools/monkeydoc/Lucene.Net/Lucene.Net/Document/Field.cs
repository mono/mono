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
using IndexWriter = Mono.Lucene.Net.Index.IndexWriter;
using Parameter = Mono.Lucene.Net.Util.Parameter;
using StringHelper = Mono.Lucene.Net.Util.StringHelper;

namespace Mono.Lucene.Net.Documents
{
	
	/// <summary>A field is a section of a Document.  Each field has two parts, a name and a
	/// value.  Values may be free text, provided as a String or as a Reader, or they
	/// may be atomic keywords, which are not further processed.  Such keywords may
	/// be used to represent dates, urls, etc.  Fields are optionally stored in the
	/// index, so that they may be returned with hits on the document.
	/// </summary>
	
	[Serializable]
	public sealed class Field:AbstractField, Fieldable
	{
		
		/// <summary>Specifies whether and how a field should be stored. </summary>
		[Serializable]
		public sealed class Store:Parameter
		{
			
			internal Store(System.String name):base(name)
			{
			}
			
			/// <summary>Store the original field value in the index in a compressed form. This is
			/// useful for long documents and for binary valued fields.
			/// </summary>
			/// <deprecated> Please use {@link CompressionTools} instead.
			/// For string fields that were previously indexed and stored using compression,
			/// the new way to achieve this is: First add the field indexed-only (no store)
			/// and additionally using the same field name as a binary, stored field
			/// with {@link CompressionTools#compressString}.
			/// </deprecated>
			public static readonly Store COMPRESS = new Store("COMPRESS");
			
			/// <summary>Store the original field value in the index. This is useful for short texts
			/// like a document's title which should be displayed with the results. The
			/// value is stored in its original form, i.e. no analyzer is used before it is
			/// stored.
			/// </summary>
			public static readonly Store YES = new Store("YES");
			
			/// <summary>Do not store the field value in the index. </summary>
			public static readonly Store NO = new Store("NO");
		}
		
		/// <summary>Specifies whether and how a field should be indexed. </summary>
		[Serializable]
		public sealed class Index:Parameter
		{
			
			internal Index(System.String name):base(name)
			{
			}
			
			/// <summary>Do not index the field value. This field can thus not be searched,
			/// but one can still access its contents provided it is
			/// {@link Field.Store stored}. 
			/// </summary>
			public static readonly Index NO = new Index("NO");
			
			/// <summary>Index the tokens produced by running the field's
			/// value through an Analyzer.  This is useful for
			/// common text. 
			/// </summary>
			public static readonly Index ANALYZED = new Index("ANALYZED");
			
			/// <deprecated> this has been renamed to {@link #ANALYZED} 
			/// </deprecated>
            [Obsolete("this has been renamed to ANALYZED")]
			public static readonly Index TOKENIZED;
			
			/// <summary>Index the field's value without using an Analyzer, so it can be searched.
			/// As no analyzer is used the value will be stored as a single term. This is
			/// useful for unique Ids like product numbers.
			/// </summary>
			public static readonly Index NOT_ANALYZED = new Index("NOT_ANALYZED");
			
			/// <deprecated> This has been renamed to {@link #NOT_ANALYZED} 
			/// </deprecated>
            [Obsolete("This has been renamed to NOT_ANALYZED")]
			public static readonly Index UN_TOKENIZED;
			
			/// <summary>Expert: Index the field's value without an Analyzer,
			/// and also disable the storing of norms.  Note that you
			/// can also separately enable/disable norms by calling
			/// {@link Field#setOmitNorms}.  No norms means that
			/// index-time field and document boosting and field
			/// length normalization are disabled.  The benefit is
			/// less memory usage as norms take up one byte of RAM
			/// per indexed field for every document in the index,
			/// during searching.  Note that once you index a given
			/// field <i>with</i> norms enabled, disabling norms will
			/// have no effect.  In other words, for this to have the
			/// above described effect on a field, all instances of
			/// that field must be indexed with NOT_ANALYZED_NO_NORMS
			/// from the beginning. 
			/// </summary>
			public static readonly Index NOT_ANALYZED_NO_NORMS = new Index("NOT_ANALYZED_NO_NORMS");
			
			/// <deprecated> This has been renamed to
			/// {@link #NOT_ANALYZED_NO_NORMS} 
			/// </deprecated>
            [Obsolete("This has been renamed to NOT_ANALYZED_NO_NORMS")]
			public static readonly Index NO_NORMS;
			
			/// <summary>Expert: Index the tokens produced by running the
			/// field's value through an Analyzer, and also
			/// separately disable the storing of norms.  See
			/// {@link #NOT_ANALYZED_NO_NORMS} for what norms are
			/// and why you may want to disable them. 
			/// </summary>
			public static readonly Index ANALYZED_NO_NORMS = new Index("ANALYZED_NO_NORMS");
			static Index()
			{
				TOKENIZED = ANALYZED;
				UN_TOKENIZED = NOT_ANALYZED;
				NO_NORMS = NOT_ANALYZED_NO_NORMS;
			}
		}
		
		/// <summary>Specifies whether and how a field should have term vectors. </summary>
		[Serializable]
		public sealed class TermVector:Parameter
		{
			
			internal TermVector(System.String name):base(name)
			{
			}
			
			/// <summary>Do not store term vectors. </summary>
			public static readonly TermVector NO = new TermVector("NO");
			
			/// <summary>Store the term vectors of each document. A term vector is a list
			/// of the document's terms and their number of occurrences in that document. 
			/// </summary>
			public static readonly TermVector YES = new TermVector("YES");
			
			/// <summary> Store the term vector + token position information
			/// 
			/// </summary>
			/// <seealso cref="YES">
			/// </seealso>
			public static readonly TermVector WITH_POSITIONS = new TermVector("WITH_POSITIONS");
			
			/// <summary> Store the term vector + Token offset information
			/// 
			/// </summary>
			/// <seealso cref="YES">
			/// </seealso>
			public static readonly TermVector WITH_OFFSETS = new TermVector("WITH_OFFSETS");
			
			/// <summary> Store the term vector + Token position and offset information
			/// 
			/// </summary>
			/// <seealso cref="YES">
			/// </seealso>
			/// <seealso cref="WITH_POSITIONS">
			/// </seealso>
			/// <seealso cref="WITH_OFFSETS">
			/// </seealso>
			public static readonly TermVector WITH_POSITIONS_OFFSETS = new TermVector("WITH_POSITIONS_OFFSETS");
		}
		
		
		/// <summary>The value of the field as a String, or null.  If null, the Reader value or
		/// binary value is used.  Exactly one of stringValue(),
		/// readerValue(), and getBinaryValue() must be set. 
		/// </summary>
		public override System.String StringValue()
		{
			return fieldsData is System.String?(System.String) fieldsData:null;
		}
		
		/// <summary>The value of the field as a Reader, or null.  If null, the String value or
		/// binary value is used.  Exactly one of stringValue(),
		/// readerValue(), and getBinaryValue() must be set. 
		/// </summary>
		public override System.IO.TextReader ReaderValue()
		{
			return fieldsData is System.IO.TextReader?(System.IO.TextReader) fieldsData:null;
		}
		
		/// <summary>The value of the field in Binary, or null.  If null, the Reader value,
		/// or String value is used. Exactly one of stringValue(),
		/// readerValue(), and getBinaryValue() must be set.
		/// </summary>
		/// <deprecated> This method must allocate a new byte[] if
		/// the {@link AbstractField#GetBinaryOffset()} is non-zero
		/// or {@link AbstractField#GetBinaryLength()} is not the
		/// full length of the byte[]. Please use {@link
		/// AbstractField#GetBinaryValue()} instead, which simply
		/// returns the byte[].
		/// </deprecated>
        [Obsolete("This method must allocate a new byte[] if the AbstractField.GetBinaryOffset() is non-zero or AbstractField.GetBinaryLength() is not the full length of the byte[]. Please use AbstractField.GetBinaryValue() instead, which simply returns the byte[].")]
		public override byte[] BinaryValue()
		{
			if (!isBinary)
				return null;
			byte[] data = (byte[]) fieldsData;
			if (binaryOffset == 0 && data.Length == binaryLength)
				return data; //Optimization
			
			byte[] ret = new byte[binaryLength];
			Array.Copy(data, binaryOffset, ret, 0, binaryLength);
			return ret;
		}
		
		/// <summary>The TokesStream for this field to be used when indexing, or null.  If null, the Reader value
		/// or String value is analyzed to produce the indexed tokens. 
		/// </summary>
		public override TokenStream TokenStreamValue()
		{
			return tokenStream;
		}
		
		
		/// <summary><p/>Expert: change the value of this field.  This can
		/// be used during indexing to re-use a single Field
		/// instance to improve indexing speed by avoiding GC cost
		/// of new'ing and reclaiming Field instances.  Typically
		/// a single {@link Document} instance is re-used as
		/// well.  This helps most on small documents.<p/>
		/// 
		/// <p/>Each Field instance should only be used once
		/// within a single {@link Document} instance.  See <a
		/// href="http://wiki.apache.org/lucene-java/ImproveIndexingSpeed">ImproveIndexingSpeed</a>
		/// for details.<p/> 
		/// </summary>
		public void  SetValue(System.String value_Renamed)
		{
			if (isBinary)
			{
				throw new System.ArgumentException("cannot set a String value on a binary field");
			}
			fieldsData = value_Renamed;
		}
		
		/// <summary>Expert: change the value of this field.  See <a href="#setValue(java.lang.String)">setValue(String)</a>. </summary>
		public void  SetValue(System.IO.TextReader value_Renamed)
		{
			if (isBinary)
			{
				throw new System.ArgumentException("cannot set a Reader value on a binary field");
			}
			if (isStored)
			{
				throw new System.ArgumentException("cannot set a Reader value on a stored field");
			}
			fieldsData = value_Renamed;
		}
		
		/// <summary>Expert: change the value of this field.  See <a href="#setValue(java.lang.String)">setValue(String)</a>. </summary>
		public void  SetValue(byte[] value_Renamed)
		{
			if (!isBinary)
			{
				throw new System.ArgumentException("cannot set a byte[] value on a non-binary field");
			}
			fieldsData = value_Renamed;
			binaryLength = value_Renamed.Length;
			binaryOffset = 0;
		}
		
		/// <summary>Expert: change the value of this field.  See <a href="#setValue(java.lang.String)">setValue(String)</a>. </summary>
		public void  SetValue(byte[] value_Renamed, int offset, int length)
		{
			if (!isBinary)
			{
				throw new System.ArgumentException("cannot set a byte[] value on a non-binary field");
			}
			fieldsData = value_Renamed;
			binaryLength = length;
			binaryOffset = offset;
		}
		
		
		/// <summary>Expert: change the value of this field.  See <a href="#setValue(java.lang.String)">setValue(String)</a>.</summary>
		/// <deprecated> use {@link #setTokenStream} 
		/// </deprecated>
        [Obsolete("use SetTokenStream ")]
		public void  SetValue(TokenStream value_Renamed)
		{
			if (isBinary)
			{
				throw new System.ArgumentException("cannot set a TokenStream value on a binary field");
			}
			if (isStored)
			{
				throw new System.ArgumentException("cannot set a TokenStream value on a stored field");
			}
			fieldsData = null;
			tokenStream = value_Renamed;
		}
		
		/// <summary>Expert: sets the token stream to be used for indexing and causes isIndexed() and isTokenized() to return true.
		/// May be combined with stored values from stringValue() or binaryValue() 
		/// </summary>
		public void  SetTokenStream(TokenStream tokenStream)
		{
			this.isIndexed = true;
			this.isTokenized = true;
			this.tokenStream = tokenStream;
		}
		
		/// <summary> Create a field by specifying its name, value and how it will
		/// be saved in the index. Term vectors will not be stored in the index.
		/// 
		/// </summary>
		/// <param name="name">The name of the field
		/// </param>
		/// <param name="value">The string to process
		/// </param>
		/// <param name="store">Whether <code>value</code> should be stored in the index
		/// </param>
		/// <param name="index">Whether the field should be indexed, and if so, if it should
		/// be tokenized before indexing 
		/// </param>
		/// <throws>  NullPointerException if name or value is <code>null</code> </throws>
		/// <throws>  IllegalArgumentException if the field is neither stored nor indexed  </throws>
		public Field(System.String name, System.String value_Renamed, Store store, Index index):this(name, value_Renamed, store, index, TermVector.NO)
		{
		}
		
		/// <summary> Create a field by specifying its name, value and how it will
		/// be saved in the index.
		/// 
		/// </summary>
		/// <param name="name">The name of the field
		/// </param>
		/// <param name="value">The string to process
		/// </param>
		/// <param name="store">Whether <code>value</code> should be stored in the index
		/// </param>
		/// <param name="index">Whether the field should be indexed, and if so, if it should
		/// be tokenized before indexing 
		/// </param>
		/// <param name="termVector">Whether term vector should be stored
		/// </param>
		/// <throws>  NullPointerException if name or value is <code>null</code> </throws>
		/// <throws>  IllegalArgumentException in any of the following situations: </throws>
		/// <summary> <ul> 
		/// <li>the field is neither stored nor indexed</li> 
		/// <li>the field is not indexed but termVector is <code>TermVector.YES</code></li>
		/// </ul> 
		/// </summary>
		public Field(System.String name, System.String value_Renamed, Store store, Index index, TermVector termVector):this(name, true, value_Renamed, store, index, termVector)
		{
		}
		
		/// <summary> Create a field by specifying its name, value and how it will
		/// be saved in the index.
		/// 
		/// </summary>
		/// <param name="name">The name of the field
		/// </param>
		/// <param name="internName">Whether to .intern() name or not
		/// </param>
		/// <param name="value">The string to process
		/// </param>
		/// <param name="store">Whether <code>value</code> should be stored in the index
		/// </param>
		/// <param name="index">Whether the field should be indexed, and if so, if it should
		/// be tokenized before indexing 
		/// </param>
		/// <param name="termVector">Whether term vector should be stored
		/// </param>
		/// <throws>  NullPointerException if name or value is <code>null</code> </throws>
		/// <throws>  IllegalArgumentException in any of the following situations: </throws>
		/// <summary> <ul> 
		/// <li>the field is neither stored nor indexed</li> 
		/// <li>the field is not indexed but termVector is <code>TermVector.YES</code></li>
		/// </ul> 
		/// </summary>
		public Field(System.String name, bool internName, System.String value_Renamed, Store store, Index index, TermVector termVector)
		{
			if (name == null)
				throw new System.NullReferenceException("name cannot be null");
			if (value_Renamed == null)
				throw new System.NullReferenceException("value cannot be null");
			if (name.Length == 0 && value_Renamed.Length == 0)
				throw new System.ArgumentException("name and value cannot both be empty");
			if (index == Index.NO && store == Store.NO)
				throw new System.ArgumentException("it doesn't make sense to have a field that " + "is neither indexed nor stored");
			if (index == Index.NO && termVector != TermVector.NO)
				throw new System.ArgumentException("cannot store term vector information " + "for a field that is not indexed");
			
			if (internName)
			// field names are optionally interned
				name = StringHelper.Intern(name);
			
			this.name = name;
			
			this.fieldsData = value_Renamed;
			
			if (store == Store.YES)
			{
				this.isStored = true;
				this.isCompressed = false;
			}
			else if (store == Store.COMPRESS)
			{
				this.isStored = true;
				this.isCompressed = true;
			}
			else if (store == Store.NO)
			{
				this.isStored = false;
				this.isCompressed = false;
			}
			else
			{
				throw new System.ArgumentException("unknown store parameter " + store);
			}
			
			if (index == Index.NO)
			{
				this.isIndexed = false;
				this.isTokenized = false;
				this.omitTermFreqAndPositions = false;
				this.omitNorms = true;
			}
			else if (index == Index.ANALYZED)
			{
				this.isIndexed = true;
				this.isTokenized = true;
			}
			else if (index == Index.NOT_ANALYZED)
			{
				this.isIndexed = true;
				this.isTokenized = false;
			}
			else if (index == Index.NOT_ANALYZED_NO_NORMS)
			{
				this.isIndexed = true;
				this.isTokenized = false;
				this.omitNorms = true;
			}
			else if (index == Index.ANALYZED_NO_NORMS)
			{
				this.isIndexed = true;
				this.isTokenized = true;
				this.omitNorms = true;
			}
			else
			{
				throw new System.ArgumentException("unknown index parameter " + index);
			}
			
			this.isBinary = false;
			
			SetStoreTermVector(termVector);
		}
		
		/// <summary> Create a tokenized and indexed field that is not stored. Term vectors will
		/// not be stored.  The Reader is read only when the Document is added to the index,
		/// i.e. you may not close the Reader until {@link IndexWriter#AddDocument(Document)}
		/// has been called.
		/// 
		/// </summary>
		/// <param name="name">The name of the field
		/// </param>
		/// <param name="reader">The reader with the content
		/// </param>
		/// <throws>  NullPointerException if name or reader is <code>null</code> </throws>
		public Field(System.String name, System.IO.TextReader reader):this(name, reader, TermVector.NO)
		{
		}
		
		/// <summary> Create a tokenized and indexed field that is not stored, optionally with 
		/// storing term vectors.  The Reader is read only when the Document is added to the index,
		/// i.e. you may not close the Reader until {@link IndexWriter#AddDocument(Document)}
		/// has been called.
		/// 
		/// </summary>
		/// <param name="name">The name of the field
		/// </param>
		/// <param name="reader">The reader with the content
		/// </param>
		/// <param name="termVector">Whether term vector should be stored
		/// </param>
		/// <throws>  NullPointerException if name or reader is <code>null</code> </throws>
		public Field(System.String name, System.IO.TextReader reader, TermVector termVector)
		{
			if (name == null)
				throw new System.NullReferenceException("name cannot be null");
			if (reader == null)
				throw new System.NullReferenceException("reader cannot be null");
			
			this.name = StringHelper.Intern(name); // field names are interned
			this.fieldsData = reader;
			
			this.isStored = false;
			this.isCompressed = false;
			
			this.isIndexed = true;
			this.isTokenized = true;
			
			this.isBinary = false;
			
			SetStoreTermVector(termVector);
		}
		
		/// <summary> Create a tokenized and indexed field that is not stored. Term vectors will
		/// not be stored. This is useful for pre-analyzed fields.
		/// The TokenStream is read only when the Document is added to the index,
		/// i.e. you may not close the TokenStream until {@link IndexWriter#AddDocument(Document)}
		/// has been called.
		/// 
		/// </summary>
		/// <param name="name">The name of the field
		/// </param>
		/// <param name="tokenStream">The TokenStream with the content
		/// </param>
		/// <throws>  NullPointerException if name or tokenStream is <code>null</code> </throws>
		public Field(System.String name, TokenStream tokenStream):this(name, tokenStream, TermVector.NO)
		{
		}
		
		/// <summary> Create a tokenized and indexed field that is not stored, optionally with 
		/// storing term vectors.  This is useful for pre-analyzed fields.
		/// The TokenStream is read only when the Document is added to the index,
		/// i.e. you may not close the TokenStream until {@link IndexWriter#AddDocument(Document)}
		/// has been called.
		/// 
		/// </summary>
		/// <param name="name">The name of the field
		/// </param>
		/// <param name="tokenStream">The TokenStream with the content
		/// </param>
		/// <param name="termVector">Whether term vector should be stored
		/// </param>
		/// <throws>  NullPointerException if name or tokenStream is <code>null</code> </throws>
		public Field(System.String name, TokenStream tokenStream, TermVector termVector)
		{
			if (name == null)
				throw new System.NullReferenceException("name cannot be null");
			if (tokenStream == null)
				throw new System.NullReferenceException("tokenStream cannot be null");
			
			this.name = StringHelper.Intern(name); // field names are interned
			this.fieldsData = null;
			this.tokenStream = tokenStream;
			
			this.isStored = false;
			this.isCompressed = false;
			
			this.isIndexed = true;
			this.isTokenized = true;
			
			this.isBinary = false;
			
			SetStoreTermVector(termVector);
		}
		
		
		/// <summary> Create a stored field with binary value. Optionally the value may be compressed.
		/// 
		/// </summary>
		/// <param name="name">The name of the field
		/// </param>
		/// <param name="value">The binary value
		/// </param>
		/// <param name="store">How <code>value</code> should be stored (compressed or not)
		/// </param>
		/// <throws>  IllegalArgumentException if store is <code>Store.NO</code>  </throws>
		public Field(System.String name, byte[] value_Renamed, Store store):this(name, value_Renamed, 0, value_Renamed.Length, store)
		{
		}
		
		/// <summary> Create a stored field with binary value. Optionally the value may be compressed.
		/// 
		/// </summary>
		/// <param name="name">The name of the field
		/// </param>
		/// <param name="value">The binary value
		/// </param>
		/// <param name="offset">Starting offset in value where this Field's bytes are
		/// </param>
		/// <param name="length">Number of bytes to use for this Field, starting at offset
		/// </param>
		/// <param name="store">How <code>value</code> should be stored (compressed or not)
		/// </param>
		/// <throws>  IllegalArgumentException if store is <code>Store.NO</code>  </throws>
		public Field(System.String name, byte[] value_Renamed, int offset, int length, Store store)
		{
			
			if (name == null)
				throw new System.ArgumentException("name cannot be null");
			if (value_Renamed == null)
				throw new System.ArgumentException("value cannot be null");
			
			this.name = StringHelper.Intern(name); // field names are interned
			fieldsData = value_Renamed;
			
			if (store == Store.YES)
			{
				isStored = true;
				isCompressed = false;
			}
			else if (store == Store.COMPRESS)
			{
				isStored = true;
				isCompressed = true;
			}
			else if (store == Store.NO)
				throw new System.ArgumentException("binary values can't be unstored");
			else
			{
				throw new System.ArgumentException("unknown store parameter " + store);
			}
			
			isIndexed = false;
			isTokenized = false;
			omitTermFreqAndPositions = false;
			omitNorms = true;
			
			isBinary = true;
			binaryLength = length;
			binaryOffset = offset;
			
			SetStoreTermVector(TermVector.NO);
		}
	}
}
