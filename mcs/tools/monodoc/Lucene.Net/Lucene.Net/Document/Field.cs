/*
 * Copyright 2004 The Apache Software Foundation
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
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
using Monodoc.Lucene.Net.Index;
using Hits = Monodoc.Lucene.Net.Search.Hits;
using Similarity = Monodoc.Lucene.Net.Search.Similarity;
namespace Monodoc.Lucene.Net.Documents
{
	
	/// <summary>A Field is a section of a Document.  Each Field has two parts, a name and a
	/// value.  Values may be free text, provided as a String or as a Reader, or they
	/// may be atomic keywords, which are not further processed.  Such keywords may
	/// be used to represent dates, urls, etc.  Fields are optionally stored in the
	/// index, so that they may be returned with hits on the document.
	/// </summary>
	
	[Serializable]
	public sealed class Field
	{
		private System.String name = "body";
		private System.String stringValue = null;
		private bool storeTermVector = false;
		private System.IO.TextReader readerValue = null;
		private bool isStored = false;
		private bool isIndexed = true;
		private bool isTokenized = true;
		
		private float boost = 1.0f;
		
		/// <summary>Sets the boost factor hits on this Field.  This value will be
		/// multiplied into the score of all hits on this this Field of this
		/// document.
		/// 
		/// <p>The boost is multiplied by {@link Document#GetBoost()} of the document
		/// containing this Field.  If a document has multiple fields with the same
		/// name, all such values are multiplied together.  This product is then
		/// multipled by the value {@link Similarity#LengthNorm(String,int)}, and
		/// rounded by {@link Similarity#EncodeNorm(float)} before it is stored in the
		/// index.  One should attempt to ensure that this product does not overflow
		/// the range of that encoding.
		/// 
		/// </summary>
		/// <seealso cref="Document#SetBoost(float)">
		/// </seealso>
		/// <seealso cref="int)">
		/// </seealso>
		/// <seealso cref="Similarity#EncodeNorm(float)">
		/// </seealso>
		public void  SetBoost(float boost)
		{
			this.boost = boost;
		}
		
		/// <summary>Returns the boost factor for hits on any Field of this document.
		/// 
		/// <p>The default value is 1.0.
		/// 
		/// <p>Note: this value is not stored directly with the document in the index.
		/// Documents returned from {@link Monodoc.Lucene.Net.Index.IndexReader#Document(int)} and {@link
		/// Hits#Doc(int)} may thus not have the same value present as when this Field
		/// was indexed.
		/// 
		/// </summary>
		/// <seealso cref="#SetBoost(float)">
		/// </seealso>
		public float GetBoost()
		{
			return boost;
		}
		
		/// <summary>Constructs a String-valued Field that is not tokenized, but is indexed
		/// and stored.  Useful for non-text fields, e.g. date or url.  
		/// </summary>
		public static Field Keyword(System.String name, System.String value_Renamed)
		{
			return new Field(name, value_Renamed, true, true, false);
		}
		
		/// <summary>Constructs a String-valued Field that is not tokenized nor indexed,
		/// but is stored in the index, for return with hits. 
		/// </summary>
		public static Field UnIndexed(System.String name, System.String value_Renamed)
		{
			return new Field(name, value_Renamed, true, false, false);
		}
		
		/// <summary>Constructs a String-valued Field that is tokenized and indexed,
		/// and is stored in the index, for return with hits.  Useful for short text
		/// fields, like "title" or "subject". Term vector will not be stored for this Field. 
		/// </summary>
		public static Field Text(System.String name, System.String value_Renamed)
		{
			return Text(name, value_Renamed, false);
		}
		
		/// <summary>Constructs a Date-valued Field that is not tokenized and is indexed,
		/// and stored in the index, for return with hits. 
		/// </summary>
		public static Field Keyword(System.String name, System.DateTime value_Renamed)
		{
			return new Field(name, DateField.DateToString(value_Renamed), true, true, false);
		}
		
		/// <summary>Constructs a String-valued Field that is tokenized and indexed,
		/// and is stored in the index, for return with hits.  Useful for short text
		/// fields, like "title" or "subject". 
		/// </summary>
		public static Field Text(System.String name, System.String value_Renamed, bool storeTermVector)
		{
			return new Field(name, value_Renamed, true, true, true, storeTermVector);
		}
		
		/// <summary>Constructs a String-valued Field that is tokenized and indexed,
		/// but that is not stored in the index.  Term vector will not be stored for this Field. 
		/// </summary>
		public static Field UnStored(System.String name, System.String value_Renamed)
		{
			return UnStored(name, value_Renamed, false);
		}
		
		/// <summary>Constructs a String-valued Field that is tokenized and indexed,
		/// but that is not stored in the index. 
		/// </summary>
		public static Field UnStored(System.String name, System.String value_Renamed, bool storeTermVector)
		{
			return new Field(name, value_Renamed, false, true, true, storeTermVector);
		}
		
		/// <summary>Constructs a Reader-valued Field that is tokenized and indexed, but is
		/// not stored in the index verbatim.  Useful for longer text fields, like
		/// "body". Term vector will not be stored for this Field. 
		/// </summary>
		public static Field Text(System.String name, System.IO.TextReader value_Renamed)
		{
			return Text(name, value_Renamed, false);
		}
		
		/// <summary>Constructs a Reader-valued Field that is tokenized and indexed, but is
		/// not stored in the index verbatim.  Useful for longer text fields, like
		/// "body". 
		/// </summary>
		public static Field Text(System.String name, System.IO.TextReader value_Renamed, bool storeTermVector)
		{
			Field f = new Field(name, value_Renamed);
			f.storeTermVector = storeTermVector;
			return f;
		}
		
		/// <summary>The name of the Field (e.g., "date", "subject", "title", or "body")
		/// as an interned string. 
		/// </summary>
		public System.String Name()
		{
			return name;
		}
		
		/// <summary>The value of the Field as a String, or null.  If null, the Reader value
		/// is used.  Exactly one of stringValue() and readerValue() must be set. 
		/// </summary>
		public System.String StringValue()
		{
			return stringValue;
		}
		/// <summary>The value of the Field as a Reader, or null.  If null, the String value
		/// is used.  Exactly one of stringValue() and readerValue() must be set. 
		/// </summary>
		public System.IO.TextReader ReaderValue()
		{
			return readerValue;
		}
		
		
		/// <summary>Create a Field by specifying all parameters except for <code>storeTermVector</code>,
		/// which is set to <code>false</code>.
		/// </summary>
		public Field(System.String name, System.String string_Renamed, bool store, bool index, bool token):this(name, string_Renamed, store, index, token, false)
		{
		}
		
		/// <summary> </summary>
		/// <param name="name">The name of the Field
		/// </param>
		/// <param name="string">The string to process
		/// </param>
		/// <param name="store">true if the Field should store the string
		/// </param>
		/// <param name="index">true if the Field should be indexed
		/// </param>
		/// <param name="token">true if the Field should be tokenized
		/// </param>
		/// <param name="storeTermVector">true if we should store the Term Vector info
		/// </param>
		public Field(System.String name, System.String string_Renamed, bool store, bool index, bool token, bool storeTermVector)
		{
			if (name == null)
				throw new System.ArgumentException("name cannot be null");
			if (string_Renamed == null)
				throw new System.ArgumentException("value cannot be null");
			if (!index && storeTermVector)
				throw new System.ArgumentException("cannot store a term vector for fields that are not indexed.");
			
			this.name = String.Intern(name); // Field names are interned
			this.stringValue = string_Renamed;
			this.isStored = store;
			this.isIndexed = index;
			this.isTokenized = token;
			this.storeTermVector = storeTermVector;
		}
		
		internal Field(System.String name, System.IO.TextReader reader)
		{
			if (name == null)
				throw new System.ArgumentException("name cannot be null");
			if (reader == null)
				throw new System.ArgumentException("value cannot be null");
			
			this.name = String.Intern(name); // Field names are interned
			this.readerValue = reader;
		}
		
		/// <summary>True iff the value of the Field is to be stored in the index for return
		/// with search hits.  It is an error for this to be true if a Field is
		/// Reader-valued. 
		/// </summary>
		public bool IsStored()
		{
			return isStored;
		}
		
		/// <summary>True iff the value of the Field is to be indexed, so that it may be
		/// searched on. 
		/// </summary>
		public bool IsIndexed()
		{
			return isIndexed;
		}
		
		/// <summary>True iff the value of the Field should be tokenized as text prior to
		/// indexing.  Un-tokenized fields are indexed as a single word and may not be
		/// Reader-valued. 
		/// </summary>
		public bool IsTokenized()
		{
			return isTokenized;
		}
		
		/// <summary>True iff the term or terms used to index this Field are stored as a term
		/// vector, available from {@link Monodoc.Lucene.Net.Index.IndexReader#GetTermFreqVector(int,String)}.
		/// These methods do not provide access to the original content of the Field,
		/// only to terms used to index it. If the original content must be
		/// preserved, use the <code>stored</code> attribute instead.
		/// 
		/// </summary>
		/// <seealso cref="String)">
		/// </seealso>
		public bool IsTermVectorStored()
		{
			return storeTermVector;
		}
		
		/// <summary>Prints a Field for human consumption. </summary>
		public override System.String ToString()
		{
			if (isStored && isIndexed && !isTokenized)
				return "Keyword<" + name + ":" + stringValue + ">";
			else if (isStored && !isIndexed && !isTokenized)
				return "Unindexed<" + name + ":" + stringValue + ">";
			else if (isStored && isIndexed && isTokenized && stringValue != null)
				return "Text<" + name + ":" + stringValue + ">";
			else if (!isStored && isIndexed && isTokenized && readerValue != null)
			{
				return "Text<" + name + ":" + readerValue + ">";
			}
			else if (!isStored && isIndexed && isTokenized)
			{
				return "UnStored<" + name + ">";
			}
			else
			{
				return base.ToString();
			}
		}
	}
}