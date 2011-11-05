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

using NumericField = Mono.Lucene.Net.Documents.NumericField;
using IndexReader = Mono.Lucene.Net.Index.IndexReader;
using Term = Mono.Lucene.Net.Index.Term;
using TermEnum = Mono.Lucene.Net.Index.TermEnum;
using StringHelper = Mono.Lucene.Net.Util.StringHelper;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> Stores information about how to sort documents by terms in an individual
	/// field.  Fields must be indexed in order to sort by them.
	/// 
	/// <p/>Created: Feb 11, 2004 1:25:29 PM
	/// 
	/// </summary>
	/// <since>   lucene 1.4
	/// </since>
	/// <version>  $Id: SortField.java 801344 2009-08-05 18:05:06Z yonik $
	/// </version>
	/// <seealso cref="Sort">
	/// </seealso>
	[Serializable]
	public class SortField
	{
		
		/// <summary>Sort by document score (relevancy).  Sort values are Float and higher
		/// values are at the front. 
		/// </summary>
		public const int SCORE = 0;
		
		/// <summary>Sort by document number (index order).  Sort values are Integer and lower
		/// values are at the front. 
		/// </summary>
		public const int DOC = 1;
		
		/// <summary>Guess type of sort based on field contents.  A regular expression is used
		/// to look at the first term indexed for the field and determine if it
		/// represents an integer number, a floating point number, or just arbitrary
		/// string characters.
		/// </summary>
		/// <deprecated> Please specify the exact type, instead.
		/// Especially, guessing does <b>not</b> work with the new
		/// {@link NumericField} type.
		/// </deprecated>
        [Obsolete("Please specify the exact type, instead. Especially, guessing does not work with the new NumericField type.")]
		public const int AUTO = 2;
		
		/// <summary>Sort using term values as Strings.  Sort values are String and lower
		/// values are at the front. 
		/// </summary>
		public const int STRING = 3;
		
		/// <summary>Sort using term values as encoded Integers.  Sort values are Integer and
		/// lower values are at the front. 
		/// </summary>
		public const int INT = 4;
		
		/// <summary>Sort using term values as encoded Floats.  Sort values are Float and
		/// lower values are at the front. 
		/// </summary>
		public const int FLOAT = 5;
		
		/// <summary>Sort using term values as encoded Longs.  Sort values are Long and
		/// lower values are at the front. 
		/// </summary>
		public const int LONG = 6;
		
		/// <summary>Sort using term values as encoded Doubles.  Sort values are Double and
		/// lower values are at the front. 
		/// </summary>
		public const int DOUBLE = 7;
		
		/// <summary>Sort using term values as encoded Shorts.  Sort values are Short and
		/// lower values are at the front. 
		/// </summary>
		public const int SHORT = 8;
		
		/// <summary>Sort using a custom Comparator.  Sort values are any Comparable and
		/// sorting is done according to natural order. 
		/// </summary>
		public const int CUSTOM = 9;
		
		/// <summary>Sort using term values as encoded Bytes.  Sort values are Byte and
		/// lower values are at the front. 
		/// </summary>
		public const int BYTE = 10;
		
		/// <summary>Sort using term values as Strings, but comparing by
		/// value (using String.compareTo) for all comparisons.
		/// This is typically slower than {@link #STRING}, which
		/// uses ordinals to do the sorting. 
		/// </summary>
		public const int STRING_VAL = 11;
		
		// IMPLEMENTATION NOTE: the FieldCache.STRING_INDEX is in the same "namespace"
		// as the above static int values.  Any new values must not have the same value
		// as FieldCache.STRING_INDEX.
		
		/// <summary>Represents sorting by document score (relevancy). </summary>
		public static readonly SortField FIELD_SCORE = new SortField(null, SCORE);
		
		/// <summary>Represents sorting by document number (index order). </summary>
		public static readonly SortField FIELD_DOC = new SortField(null, DOC);
		
		private System.String field;
		private int type = AUTO; // defaults to determining type dynamically
		private System.Globalization.CultureInfo locale; // defaults to "natural order" (no Locale)
		internal bool reverse = false; // defaults to natural order
		private SortComparatorSource factory;
		private Mono.Lucene.Net.Search.Parser parser;
		
		// Used for CUSTOM sort
		private FieldComparatorSource comparatorSource;
		
		private bool useLegacy = false; // remove in Lucene 3.0
		
		/// <summary>Creates a sort by terms in the given field where the type of term value
		/// is determined dynamically ({@link #AUTO AUTO}).
		/// </summary>
		/// <param name="field">Name of field to sort by, cannot be
		/// <code>null</code>.
		/// </param>
		/// <deprecated> Please specify the exact type instead.
		/// </deprecated>
        [Obsolete("Please specify the exact type instead.")]
		public SortField(System.String field)
		{
			InitFieldType(field, AUTO);
		}
		
		/// <summary>Creates a sort, possibly in reverse, by terms in the given field where
		/// the type of term value is determined dynamically ({@link #AUTO AUTO}).
		/// </summary>
		/// <param name="field">Name of field to sort by, cannot be <code>null</code>.
		/// </param>
		/// <param name="reverse">True if natural order should be reversed.
		/// </param>
		/// <deprecated> Please specify the exact type instead.
		/// </deprecated>
        [Obsolete("Please specify the exact type instead.")]
		public SortField(System.String field, bool reverse)
		{
			InitFieldType(field, AUTO);
			this.reverse = reverse;
		}
		
		/// <summary>Creates a sort by terms in the given field with the type of term
		/// values explicitly given.
		/// </summary>
		/// <param name="field"> Name of field to sort by.  Can be <code>null</code> if
		/// <code>type</code> is SCORE or DOC.
		/// </param>
		/// <param name="type">  Type of values in the terms.
		/// </param>
		public SortField(System.String field, int type)
		{
			InitFieldType(field, type);
		}
		
		/// <summary>Creates a sort, possibly in reverse, by terms in the given field with the
		/// type of term values explicitly given.
		/// </summary>
		/// <param name="field"> Name of field to sort by.  Can be <code>null</code> if
		/// <code>type</code> is SCORE or DOC.
		/// </param>
		/// <param name="type">  Type of values in the terms.
		/// </param>
		/// <param name="reverse">True if natural order should be reversed.
		/// </param>
		public SortField(System.String field, int type, bool reverse)
		{
			InitFieldType(field, type);
			this.reverse = reverse;
		}
		
		/// <summary>Creates a sort by terms in the given field, parsed
		/// to numeric values using a custom {@link FieldCache.Parser}.
		/// </summary>
		/// <param name="field"> Name of field to sort by.  Must not be null.
		/// </param>
		/// <param name="parser">Instance of a {@link FieldCache.Parser},
		/// which must subclass one of the existing numeric
		/// parsers from {@link FieldCache}. Sort type is inferred
		/// by testing which numeric parser the parser subclasses.
		/// </param>
		/// <throws>  IllegalArgumentException if the parser fails to </throws>
		/// <summary>  subclass an existing numeric parser, or field is null
		/// </summary>
		public SortField(System.String field, Mono.Lucene.Net.Search.Parser parser):this(field, parser, false)
		{
		}
		
		/// <summary>Creates a sort, possibly in reverse, by terms in the given field, parsed
		/// to numeric values using a custom {@link FieldCache.Parser}.
		/// </summary>
		/// <param name="field"> Name of field to sort by.  Must not be null.
		/// </param>
		/// <param name="parser">Instance of a {@link FieldCache.Parser},
		/// which must subclass one of the existing numeric
		/// parsers from {@link FieldCache}. Sort type is inferred
		/// by testing which numeric parser the parser subclasses.
		/// </param>
		/// <param name="reverse">True if natural order should be reversed.
		/// </param>
		/// <throws>  IllegalArgumentException if the parser fails to </throws>
		/// <summary>  subclass an existing numeric parser, or field is null
		/// </summary>
		public SortField(System.String field, Mono.Lucene.Net.Search.Parser parser, bool reverse)
		{
			if (parser is Mono.Lucene.Net.Search.IntParser)
				InitFieldType(field, INT);
			else if (parser is Mono.Lucene.Net.Search.FloatParser)
				InitFieldType(field, FLOAT);
			else if (parser is Mono.Lucene.Net.Search.ShortParser)
				InitFieldType(field, SHORT);
			else if (parser is Mono.Lucene.Net.Search.ByteParser)
				InitFieldType(field, BYTE);
			else if (parser is Mono.Lucene.Net.Search.LongParser)
				InitFieldType(field, LONG);
			else if (parser is Mono.Lucene.Net.Search.DoubleParser)
				InitFieldType(field, DOUBLE);
			else
			{
				throw new System.ArgumentException("Parser instance does not subclass existing numeric parser from FieldCache (got " + parser + ")");
			}
			
			this.reverse = reverse;
			this.parser = parser;
		}
		
		/// <summary>Creates a sort by terms in the given field sorted
		/// according to the given locale.
		/// </summary>
		/// <param name="field"> Name of field to sort by, cannot be <code>null</code>.
		/// </param>
		/// <param name="locale">Locale of values in the field.
		/// </param>
		public SortField(System.String field, System.Globalization.CultureInfo locale)
		{
			InitFieldType(field, STRING);
			this.locale = locale;
		}
		
		/// <summary>Creates a sort, possibly in reverse, by terms in the given field sorted
		/// according to the given locale.
		/// </summary>
		/// <param name="field"> Name of field to sort by, cannot be <code>null</code>.
		/// </param>
		/// <param name="locale">Locale of values in the field.
		/// </param>
		public SortField(System.String field, System.Globalization.CultureInfo locale, bool reverse)
		{
			InitFieldType(field, STRING);
			this.locale = locale;
			this.reverse = reverse;
		}
		
		/// <summary>Creates a sort with a custom comparison function.</summary>
		/// <param name="field">Name of field to sort by; cannot be <code>null</code>.
		/// </param>
		/// <param name="comparator">Returns a comparator for sorting hits.
		/// </param>
		/// <deprecated> use SortField (String field, FieldComparatorSource comparator)
		/// </deprecated>
        [Obsolete("use SortField (String field, FieldComparatorSource comparator)")]
		public SortField(System.String field, SortComparatorSource comparator)
		{
			InitFieldType(field, CUSTOM);
			SetUseLegacySearch(true);
			this.factory = comparator;
		}
		
		/// <summary>Creates a sort with a custom comparison function.</summary>
		/// <param name="field">Name of field to sort by; cannot be <code>null</code>.
		/// </param>
		/// <param name="comparator">Returns a comparator for sorting hits.
		/// </param>
		public SortField(System.String field, FieldComparatorSource comparator)
		{
			InitFieldType(field, CUSTOM);
			this.comparatorSource = comparator;
		}
		
		/// <summary>Creates a sort, possibly in reverse, with a custom comparison function.</summary>
		/// <param name="field">Name of field to sort by; cannot be <code>null</code>.
		/// </param>
		/// <param name="comparator">Returns a comparator for sorting hits.
		/// </param>
		/// <param name="reverse">True if natural order should be reversed.
		/// </param>
		/// <deprecated> use SortField (String field, FieldComparatorSource comparator, boolean reverse)
		/// </deprecated>
        [Obsolete("use SortField(String field, FieldComparatorSource comparator, boolean reverse)")]
		public SortField(System.String field, SortComparatorSource comparator, bool reverse)
		{
			InitFieldType(field, CUSTOM);
			SetUseLegacySearch(true);
			this.reverse = reverse;
			this.factory = comparator;
		}
		
		/// <summary>Creates a sort, possibly in reverse, with a custom comparison function.</summary>
		/// <param name="field">Name of field to sort by; cannot be <code>null</code>.
		/// </param>
		/// <param name="comparator">Returns a comparator for sorting hits.
		/// </param>
		/// <param name="reverse">True if natural order should be reversed.
		/// </param>
		public SortField(System.String field, FieldComparatorSource comparator, bool reverse)
		{
			InitFieldType(field, CUSTOM);
			this.reverse = reverse;
			this.comparatorSource = comparator;
		}
		
		// Sets field & type, and ensures field is not NULL unless
		// type is SCORE or DOC
		private void  InitFieldType(System.String field, int type)
		{
			this.type = type;
			if (field == null)
			{
				if (type != SCORE && type != DOC)
					throw new System.ArgumentException("field can only be null when type is SCORE or DOC");
			}
			else
			{
				this.field = StringHelper.Intern(field);
			}
		}
		
		/// <summary>Returns the name of the field.  Could return <code>null</code>
		/// if the sort is by SCORE or DOC.
		/// </summary>
		/// <returns> Name of field, possibly <code>null</code>.
		/// </returns>
		public virtual System.String GetField()
		{
			return field;
		}
		
		/// <summary>Returns the type of contents in the field.</summary>
		/// <returns> One of the constants SCORE, DOC, AUTO, STRING, INT or FLOAT.
		/// </returns>
		public new virtual int GetType()
		{
			return type;
		}
		
		/// <summary>Returns the Locale by which term values are interpreted.
		/// May return <code>null</code> if no Locale was specified.
		/// </summary>
		/// <returns> Locale, or <code>null</code>.
		/// </returns>
		public virtual System.Globalization.CultureInfo GetLocale()
		{
			return locale;
		}
		
		/// <summary>Returns the instance of a {@link FieldCache} parser that fits to the given sort type.
		/// May return <code>null</code> if no parser was specified. Sorting is using the default parser then.
		/// </summary>
		/// <returns> An instance of a {@link FieldCache} parser, or <code>null</code>.
		/// </returns>
		public virtual Mono.Lucene.Net.Search.Parser GetParser()
		{
			return parser;
		}
		
		/// <summary>Returns whether the sort should be reversed.</summary>
		/// <returns>  True if natural order should be reversed.
		/// </returns>
		public virtual bool GetReverse()
		{
			return reverse;
		}
		
		/// <deprecated> use {@link #GetComparatorSource()}
		/// </deprecated>
        [Obsolete("use GetComparatorSource()")]
		public virtual SortComparatorSource GetFactory()
		{
			return factory;
		}
		
		public virtual FieldComparatorSource GetComparatorSource()
		{
			return comparatorSource;
		}
		
		/// <summary> Use legacy IndexSearch implementation: search with a DirectoryReader rather
		/// than passing a single hit collector to multiple SegmentReaders.
		/// 
		/// </summary>
		/// <param name="legacy">true for legacy behavior
		/// </param>
		/// <deprecated> will be removed in Lucene 3.0.
		/// </deprecated>
        [Obsolete("will be removed in Lucene 3.0.")]
		public virtual void  SetUseLegacySearch(bool legacy)
		{
			this.useLegacy = legacy;
		}
		
		/// <returns> if true, IndexSearch will use legacy sorting search implementation.
		/// eg. multiple Priority Queues.
		/// </returns>
		/// <deprecated> will be removed in Lucene 3.0.
		/// </deprecated>
        [Obsolete("will be removed in Lucene 3.0.")]
		public virtual bool GetUseLegacySearch()
		{
			return this.useLegacy;
		}
		
		public override System.String ToString()
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			switch (type)
			{
				
				case SCORE: 
					buffer.Append("<score>");
					break;
				
				
				case DOC: 
					buffer.Append("<doc>");
					break;
				
				
				case AUTO: 
					buffer.Append("<auto: \"").Append(field).Append("\">");
					break;
				
				
				case STRING: 
					buffer.Append("<string: \"").Append(field).Append("\">");
					break;
				
				
				case STRING_VAL: 
					buffer.Append("<string_val: \"").Append(field).Append("\">");
					break;
				
				
				case BYTE: 
					buffer.Append("<byte: \"").Append(field).Append("\">");
					break;
				
				
				case SHORT: 
					buffer.Append("<short: \"").Append(field).Append("\">");
					break;
				
				
				case INT: 
					buffer.Append("<int: \"").Append(field).Append("\">");
					break;
				
				
				case LONG: 
					buffer.Append("<long: \"").Append(field).Append("\">");
					break;
				
				
				case FLOAT: 
					buffer.Append("<float: \"").Append(field).Append("\">");
					break;
				
				
				case DOUBLE: 
					buffer.Append("<double: \"").Append(field).Append("\">");
					break;
				
				
				case CUSTOM: 
					buffer.Append("<custom:\"").Append(field).Append("\": ").Append(factory).Append('>');
					break;
				
				
				default: 
					buffer.Append("<???: \"").Append(field).Append("\">");
					break;
				
			}
			
			if (locale != null)
				buffer.Append('(').Append(locale).Append(')');
			if (parser != null)
				buffer.Append('(').Append(parser).Append(')');
			if (reverse)
				buffer.Append('!');
			
			return buffer.ToString();
		}
		
		/// <summary>Returns true if <code>o</code> is equal to this.  If a
		/// {@link SortComparatorSource} (deprecated) or {@link
		/// FieldCache.Parser} was provided, it must properly
		/// implement equals (unless a singleton is always used). 
		/// </summary>
		public  override bool Equals(System.Object o)
		{
			if (this == o)
				return true;
			if (!(o is SortField))
				return false;
			SortField other = (SortField) o;
			return ((System.Object) other.field == (System.Object) this.field && other.type == this.type && other.reverse == this.reverse && (other.locale == null?this.locale == null:other.locale.Equals(this.locale)) && (other.factory == null?this.factory == null:other.factory.Equals(this.factory)) && (other.comparatorSource == null?this.comparatorSource == null:other.comparatorSource.Equals(this.comparatorSource)) && (other.parser == null?this.parser == null:other.parser.Equals(this.parser)));
		}
		
		/// <summary>Returns true if <code>o</code> is equal to this.  If a
		/// {@link SortComparatorSource} (deprecated) or {@link
		/// FieldCache.Parser} was provided, it must properly
		/// implement hashCode (unless a singleton is always
		/// used). 
		/// </summary>
		public override int GetHashCode()
		{
			int hash = type ^ 0x346565dd + (reverse ? Boolean.TrueString.GetHashCode() : Boolean.FalseString.GetHashCode()) ^ unchecked((int) 0xaf5998bb);
			if (field != null)
				hash += (field.GetHashCode() ^ unchecked((int) 0xff5685dd));
			if (locale != null)
			{
				hash += (locale.GetHashCode() ^ 0x08150815);
			}
			if (factory != null)
				hash += (factory.GetHashCode() ^ 0x34987555);
			if (comparatorSource != null)
				hash += comparatorSource.GetHashCode();
			if (parser != null)
				hash += (parser.GetHashCode() ^ 0x3aaf56ff);
			return hash;
		}
		
        
       //// field must be interned after reading from stream
       // private void readObject(java.io.ObjectInputStream in) throws java.io.IOException, ClassNotFoundException {
       //  in.defaultReadObject();
       //  if (field != null)
       //    field = StringHelper.intern(field);
       // }

        [System.Runtime.Serialization.OnDeserialized]
        internal void OnDeserialized(System.Runtime.Serialization.StreamingContext context)
        {
            field = StringHelper.Intern(field);
        }
		
		/// <summary>Returns the {@link FieldComparator} to use for
		/// sorting.
		/// 
		/// <b>NOTE:</b> This API is experimental and might change in
		/// incompatible ways in the next release.
		/// 
		/// </summary>
		/// <param name="numHits">number of top hits the queue will store
		/// </param>
		/// <param name="sortPos">position of this SortField within {@link
		/// Sort}.  The comparator is primary if sortPos==0,
		/// secondary if sortPos==1, etc.  Some comparators can
		/// optimize themselves when they are the primary sort.
		/// </param>
		/// <returns> {@link FieldComparator} to use when sorting
		/// </returns>
		public virtual FieldComparator GetComparator(int numHits, int sortPos)
		{
			
			if (locale != null)
			{
				// TODO: it'd be nice to allow FieldCache.getStringIndex
				// to optionally accept a Locale so sorting could then use
				// the faster StringComparator impls
				return new FieldComparator.StringComparatorLocale(numHits, field, locale);
			}
			
			switch (type)
			{
				
				case SortField.SCORE: 
					return new FieldComparator.RelevanceComparator(numHits);
				
				
				case SortField.DOC: 
					return new FieldComparator.DocComparator(numHits);
				
				
				case SortField.INT: 
					return new FieldComparator.IntComparator(numHits, field, parser);
				
				
				case SortField.FLOAT: 
					return new FieldComparator.FloatComparator(numHits, field, parser);
				
				
				case SortField.LONG: 
					return new FieldComparator.LongComparator(numHits, field, parser);
				
				
				case SortField.DOUBLE: 
					return new FieldComparator.DoubleComparator(numHits, field, parser);
				
				
				case SortField.BYTE: 
					return new FieldComparator.ByteComparator(numHits, field, parser);
				
				
				case SortField.SHORT: 
					return new FieldComparator.ShortComparator(numHits, field, parser);
				
				
				case SortField.CUSTOM: 
					System.Diagnostics.Debug.Assert(factory == null && comparatorSource != null);
					return comparatorSource.NewComparator(field, numHits, sortPos, reverse);
				
				
				case SortField.STRING: 
					return new FieldComparator.StringOrdValComparator(numHits, field, sortPos, reverse);
				
				
				case SortField.STRING_VAL: 
					return new FieldComparator.StringValComparator(numHits, field);
				
				
				default: 
					throw new System.SystemException("Illegal sort type: " + type);
				
			}
		}
		
		/// <summary> Attempts to detect the given field type for an IndexReader.</summary>
		/// <deprecated>
		/// </deprecated>
        [Obsolete]
		internal static int DetectFieldType(IndexReader reader, System.String fieldKey)
		{
			System.String field = StringHelper.Intern(fieldKey);
			TermEnum enumerator = reader.Terms(new Term(field));
			try
			{
				Term term = enumerator.Term();
				if (term == null)
				{
					throw new System.SystemException("no terms in field " + field + " - cannot determine sort type");
				}
				int ret = 0;
				if ((System.Object) term.Field() == (System.Object) field)
				{
					System.String termtext = term.Text().Trim();
                    
                    int tmpI32; long tmpI64; float tmpF;
                    if      (System.Int32.TryParse(termtext, out tmpI32))       ret = SortField.INT;
                    else if (System.Int64.TryParse(termtext, out tmpI64))       ret = SortField.LONG;
                    else if (SupportClass.Single.TryParse(termtext, out tmpF))  ret = SortField.FLOAT;
                    else ret = SortField.STRING;
				}
				else
				{
					throw new System.SystemException("field \"" + field + "\" does not appear to be indexed");
				}
				return ret;
			}
			finally
			{
				enumerator.Close();
			}
		}
	}
}
