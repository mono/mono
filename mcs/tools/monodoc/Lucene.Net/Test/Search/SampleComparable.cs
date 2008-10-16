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
using IndexReader = Lucene.Net.Index.IndexReader;
using Term = Lucene.Net.Index.Term;
using TermDocs = Lucene.Net.Index.TermDocs;
using TermEnum = Lucene.Net.Index.TermEnum;
namespace Lucene.Net.Search
{
	
	/// <summary> An example Comparable for use with the custom sort tests.
	/// It implements a comparable for "id" sort of values which
	/// consist of an alphanumeric part and a numeric part, such as:
	/// <p/>
	/// <P>ABC-123, A-1, A-7, A-100, B-99999
	/// <p/>
	/// <p>Such values cannot be sorted as strings, since A-100 needs
	/// to come after A-7.
	/// <p/>
	/// <p>It could be argued that the "ids" should be rewritten as
	/// A-0001, A-0100, etc. so they will sort as strings.  That is
	/// a valid alternate way to solve it - but
	/// this is only supposed to be a simple test case.
	/// <p/>
	/// <p>Created: Apr 21, 2004 5:34:47 PM
	/// 
	/// </summary>
	/// <author>  Tim Jones
	/// </author>
	/// <version>  $Id: SampleComparable.java,v 1.3 2004/05/19 23:05:27 tjones Exp $
	/// </version>
	/// <since> 1.4
	/// </since>
	[Serializable]
	public class SampleComparable : System.IComparable
	{
		[Serializable]
		private class AnonymousClassSortComparatorSource : SortComparatorSource
		{
			private class AnonymousClassScoreDocComparator : ScoreDocComparator
			{
				public AnonymousClassScoreDocComparator(Lucene.Net.Index.IndexReader reader, Lucene.Net.Index.TermEnum enumerator, System.String field, AnonymousClassSortComparatorSource enclosingInstance)
				{
					InitBlock(reader, enumerator, field, enclosingInstance);
				}
				private void  InitBlock(Lucene.Net.Index.IndexReader reader, Lucene.Net.Index.TermEnum enumerator, System.String field, AnonymousClassSortComparatorSource enclosingInstance)
				{
					this.reader = reader;
					this.enumerator = enumerator;
					this.field = field;
					this.enclosingInstance = enclosingInstance;
					cachedValues = Enclosing_Instance.FillCache(reader, enumerator, field);
				}
				private Lucene.Net.Index.IndexReader reader;
				private Lucene.Net.Index.TermEnum enumerator;
				private System.String field;
				private AnonymousClassSortComparatorSource enclosingInstance;
				public AnonymousClassSortComparatorSource Enclosing_Instance
				{
					get
					{
						return enclosingInstance;
					}
					
				}
				protected internal System.IComparable[] cachedValues;
				
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
			public virtual ScoreDocComparator NewComparator(IndexReader reader, System.String fieldname)
			{
				System.String field = String.Intern(fieldname);
				TermEnum enumerator = reader.Terms(new Term(fieldname, ""));
				try
				{
					return new AnonymousClassScoreDocComparator(reader, enumerator, field, this);
				}
				finally
				{
					enumerator.Close();
				}
			}
			
			/// <summary> Returns an array of objects which represent that natural order
			/// of the term values in the given Field.
			/// 
			/// </summary>
			/// <param name="reader">    Terms are in this index.
			/// </param>
			/// <param name="enumerator">Use this to get the term values and TermDocs.
			/// </param>
			/// <param name="fieldname"> Comparables should be for this Field.
			/// </param>
			/// <returns> Array of objects representing natural order of terms in Field.
			/// </returns>
			/// <throws>  IOException If an error occurs reading the index. </throws>
			protected internal virtual System.IComparable[] FillCache(IndexReader reader, TermEnum enumerator, System.String fieldname)
			{
				System.String field = String.Intern(fieldname);
				System.IComparable[] retArray = new System.IComparable[reader.MaxDoc()];
				if (retArray.Length > 0)
				{
					TermDocs termDocs = reader.TermDocs();
					try
					{
						if (enumerator.Term() == null)
						{
							throw new System.SystemException("no terms in Field " + field);
						}
						do 
						{
							Term term = enumerator.Term();
							if ((System.Object) term.Field() != (System.Object) field)
								break;
							System.IComparable termval = GetComparable(term.Text());
							termDocs.Seek(enumerator);
							while (termDocs.Next())
							{
								retArray[termDocs.Doc()] = termval;
							}
						}
						while (enumerator.Next());
					}
					finally
					{
						termDocs.Close();
					}
				}
				return retArray;
			}
			
			internal virtual System.IComparable GetComparable(System.String termtext)
			{
				return new SampleComparable(termtext);
			}
		}
		[Serializable]
		private class AnonymousClassSortComparator : SortComparator
		{
			public /*protected internal*/ override System.IComparable GetComparable(System.String termtext)
			{
				return new SampleComparable(termtext);
			}
		}
		public static SortComparatorSource ComparatorSource
		{
			get
			{
				return new AnonymousClassSortComparatorSource();
			}
			
		}
		public static SortComparator Comparator
		{
			get
			{
				return new AnonymousClassSortComparator();
			}
			
		}
		
		internal System.String string_part;
		internal System.Int32 int_part;
		
		public SampleComparable(System.String s)
		{
			int i = s.IndexOf("-");
			string_part = s.Substring(0, (i) - (0));
			int_part = System.Int32.Parse(s.Substring(i + 1));
		}
		
		public virtual int CompareTo(System.Object o)
		{
			SampleComparable otherid = (SampleComparable) o;
			int i = String.CompareOrdinal(string_part, otherid.string_part);
			if (i == 0)
			{
				return int_part.CompareTo(otherid.int_part);
			}
			return i;
		}
	}
}