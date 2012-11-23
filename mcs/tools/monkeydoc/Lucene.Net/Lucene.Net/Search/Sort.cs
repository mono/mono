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
	
	
	/// <summary> Encapsulates sort criteria for returned hits.
	/// 
	/// <p/>The fields used to determine sort order must be carefully chosen.
	/// Documents must contain a single term in such a field,
	/// and the value of the term should indicate the document's relative position in
	/// a given sort order.  The field must be indexed, but should not be tokenized,
	/// and does not need to be stored (unless you happen to want it back with the
	/// rest of your document data).  In other words:
	/// 
	/// <p/><code>document.add (new Field ("byNumber", Integer.toString(x), Field.Store.NO, Field.Index.NOT_ANALYZED));</code><p/>
	/// 
	/// 
	/// <p/><h3>Valid Types of Values</h3>
	/// 
	/// <p/>There are four possible kinds of term values which may be put into
	/// sorting fields: Integers, Longs, Floats, or Strings.  Unless
	/// {@link SortField SortField} objects are specified, the type of value
	/// in the field is determined by parsing the first term in the field.
	/// 
	/// <p/>Integer term values should contain only digits and an optional
	/// preceding negative sign.  Values must be base 10 and in the range
	/// <code>Integer.MIN_VALUE</code> and <code>Integer.MAX_VALUE</code> inclusive.
	/// Documents which should appear first in the sort
	/// should have low value integers, later documents high values
	/// (i.e. the documents should be numbered <code>1..n</code> where
	/// <code>1</code> is the first and <code>n</code> the last).
	/// 
	/// <p/>Long term values should contain only digits and an optional
	/// preceding negative sign.  Values must be base 10 and in the range
	/// <code>Long.MIN_VALUE</code> and <code>Long.MAX_VALUE</code> inclusive.
	/// Documents which should appear first in the sort
	/// should have low value integers, later documents high values.
	/// 
	/// <p/>Float term values should conform to values accepted by
	/// {@link Float Float.valueOf(String)} (except that <code>NaN</code>
	/// and <code>Infinity</code> are not supported).
	/// Documents which should appear first in the sort
	/// should have low values, later documents high values.
	/// 
	/// <p/>String term values can contain any valid String, but should
	/// not be tokenized.  The values are sorted according to their
	/// {@link Comparable natural order}.  Note that using this type
	/// of term value has higher memory requirements than the other
	/// two types.
	/// 
	/// <p/><h3>Object Reuse</h3>
	/// 
	/// <p/>One of these objects can be
	/// used multiple times and the sort order changed between usages.
	/// 
	/// <p/>This class is thread safe.
	/// 
	/// <p/><h3>Memory Usage</h3>
	/// 
	/// <p/>Sorting uses of caches of term values maintained by the
	/// internal HitQueue(s).  The cache is static and contains an integer
	/// or float array of length <code>IndexReader.maxDoc()</code> for each field
	/// name for which a sort is performed.  In other words, the size of the
	/// cache in bytes is:
	/// 
	/// <p/><code>4 * IndexReader.maxDoc() * (# of different fields actually used to sort)</code>
	/// 
	/// <p/>For String fields, the cache is larger: in addition to the
	/// above array, the value of every term in the field is kept in memory.
	/// If there are many unique terms in the field, this could
	/// be quite large.
	/// 
	/// <p/>Note that the size of the cache is not affected by how many
	/// fields are in the index and <i>might</i> be used to sort - only by
	/// the ones actually used to sort a result set.
	/// 
	/// <p/>Created: Feb 12, 2004 10:53:57 AM
	/// 
	/// </summary>
	/// <since>   lucene 1.4
	/// </since>
	/// <version>  $Id: Sort.java 795179 2009-07-17 18:23:30Z mikemccand $
	/// </version>
	[Serializable]
	public class Sort
	{
		
		/// <summary> Represents sorting by computed relevance. Using this sort criteria returns
		/// the same results as calling
		/// {@link Searcher#Search(Query) Searcher#search()}without a sort criteria,
		/// only with slightly more overhead.
		/// </summary>
		public static readonly Sort RELEVANCE = new Sort();
		
		/// <summary>Represents sorting by index order. </summary>
		public static readonly Sort INDEXORDER;
		
		// internal representation of the sort criteria
		internal SortField[] fields;
		
		/// <summary> Sorts by computed relevance. This is the same sort criteria as calling
		/// {@link Searcher#Search(Query) Searcher#search()}without a sort criteria,
		/// only with slightly more overhead.
		/// </summary>
		public Sort():this(SortField.FIELD_SCORE)
		{
		}
		
		/// <summary> Sorts by the terms in <code>field</code> then by index order (document
		/// number). The type of value in <code>field</code> is determined
		/// automatically.
		/// 
		/// </summary>
		/// <seealso cref="SortField.AUTO">
		/// </seealso>
		/// <deprecated> Please specify the type explicitly by
		/// first creating a {@link SortField} and then use {@link
		/// #Sort(SortField)}
		/// </deprecated>
        [Obsolete("Please specify the type explicitly by first creating a SortField and then use Sort(SortField)")]
		public Sort(System.String field)
		{
			SetSort(field, false);
		}
		
		/// <summary> Sorts possibly in reverse by the terms in <code>field</code> then by
		/// index order (document number). The type of value in <code>field</code> is
		/// determined automatically.
		/// 
		/// </summary>
		/// <seealso cref="SortField.AUTO">
		/// </seealso>
		/// <deprecated> Please specify the type explicitly by
		/// first creating a {@link SortField} and then use {@link
		/// #Sort(SortField)}
		/// </deprecated>
        [Obsolete("Please specify the type explicitly by first creating a SortField and then use Sort(SortField)")]
		public Sort(System.String field, bool reverse)
		{
			SetSort(field, reverse);
		}
		
		/// <summary> Sorts in succession by the terms in each field. The type of value in
		/// <code>field</code> is determined automatically.
		/// 
		/// </summary>
		/// <seealso cref="SortField.AUTO">
		/// </seealso>
		/// <deprecated> Please specify the type explicitly by
		/// first creating {@link SortField}s and then use {@link
		/// #Sort(SortField[])}
		/// </deprecated>
        [Obsolete("Please specify the type explicitly by first creating SortFields and then use Sort(SortField[])")]
		public Sort(System.String[] fields)
		{
			SetSort(fields);
		}
		
		/// <summary>Sorts by the criteria in the given SortField. </summary>
		public Sort(SortField field)
		{
			SetSort(field);
		}
		
		/// <summary>Sorts in succession by the criteria in each SortField. </summary>
		public Sort(SortField[] fields)
		{
			SetSort(fields);
		}
		
		/// <summary> Sets the sort to the terms in <code>field</code> then by index order
		/// (document number).
		/// </summary>
		/// <deprecated> Please specify the type explicitly by
		/// first creating a {@link SortField} and then use {@link
		/// #SetSort(SortField)}
		/// </deprecated>
        [Obsolete("Please specify the type explicitly by first creating a SortField and then use SetSort(SortField)")]
		public void  SetSort(System.String field)
		{
			SetSort(field, false);
		}
		
		/// <summary> Sets the sort to the terms in <code>field</code> possibly in reverse,
		/// then by index order (document number).
		/// </summary>
		/// <deprecated> Please specify the type explicitly by
		/// first creating a {@link SortField} and then use {@link
		/// #SetSort(SortField)}
		/// </deprecated>
        [Obsolete("Please specify the type explicitly by first creating a SortField and then use SetSort(SortField)")]
		public virtual void  SetSort(System.String field, bool reverse)
		{
			fields = new SortField[]{new SortField(field, SortField.AUTO, reverse)};
		}
		
		/// <summary>Sets the sort to the terms in each field in succession.</summary>
		/// <deprecated> Please specify the type explicitly by
		/// first creating {@link SortField}s and then use {@link
		/// #SetSort(SortField[])} 
		/// </deprecated>
        [Obsolete("Please specify the type explicitly by first creating a SortFields and then use SetSort(SortField[])")]
		public virtual void  SetSort(System.String[] fieldnames)
		{
			int n = fieldnames.Length;
			SortField[] nfields = new SortField[n];
			for (int i = 0; i < n; ++i)
			{
				nfields[i] = new SortField(fieldnames[i], SortField.AUTO);
			}
			fields = nfields;
		}
		
		/// <summary>Sets the sort to the given criteria. </summary>
		public virtual void  SetSort(SortField field)
		{
			this.fields = new SortField[]{field};
		}
		
		/// <summary>Sets the sort to the given criteria in succession. </summary>
		public virtual void  SetSort(SortField[] fields)
		{
			this.fields = fields;
		}
		
		/// <summary> Representation of the sort criteria.</summary>
		/// <returns> Array of SortField objects used in this sort criteria
		/// </returns>
		public virtual SortField[] GetSort()
		{
			return fields;
		}
		
		public override System.String ToString()
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			
			for (int i = 0; i < fields.Length; i++)
			{
				buffer.Append(fields[i].ToString());
				if ((i + 1) < fields.Length)
					buffer.Append(',');
			}
			
			return buffer.ToString();
		}
		
		/// <summary>Returns true if <code>o</code> is equal to this. </summary>
		public  override bool Equals(System.Object o)
		{
			if (this == o)
				return true;
			if (!(o is Sort))
				return false;
			Sort other = (Sort) o;

            bool result = false;
            if ((this.fields == null) && (other.fields == null))
                result = true;
            else if ((this.fields != null) && (other.fields != null))
            {
                if (this.fields.Length == other.fields.Length)
                {
                    int length = this.fields.Length;
                    result = true;
                    for (int i = 0; i < length; i++)
                    {
                        if (!(this.fields[i].Equals(other.fields[i])))
                        {
                            result = false;
                            break;
                        }
                    }
                }
            }
            return result;
		}
		
		/// <summary>Returns a hash code value for this object. </summary>
		public override int GetHashCode()
		{
			// TODO in Java 1.5: switch to Arrays.hashCode().  The 
			// Java 1.4 workaround below calculates the same hashCode
			// as Java 1.5's new Arrays.hashCode()
			return 0x45aaf665 + SupportClass.EquatableList<SortField>.GetHashCode(fields);
		}
		static Sort()
		{
			INDEXORDER = new Sort(SortField.FIELD_DOC);
		}
	}
}
