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
using DateField = Monodoc.Lucene.Net.Documents.DateField;
using Monodoc.Lucene.Net.Index;
using Term = Monodoc.Lucene.Net.Index.Term;
using TermDocs = Monodoc.Lucene.Net.Index.TermDocs;
using TermEnum = Monodoc.Lucene.Net.Index.TermEnum;
namespace Monodoc.Lucene.Net.Search
{
	
	/// <summary> A Filter that restricts search results to a range of time.
	/// 
	/// <p>For this to work, documents must have been indexed with a
	/// {@link DateField}.
	/// </summary>
	[Serializable]
	public class DateFilter:Filter
	{
		private void  InitBlock()
		{
			start = DateField.MIN_DATE_STRING();
			end = DateField.MAX_DATE_STRING();
		}
		internal System.String field;
		
		internal System.String start;
		internal System.String end;
		
		private DateFilter(System.String f)
		{
			InitBlock();
			field = f;
		}
		
		/// <summary> Constructs a filter for Field <code>f</code> matching dates
		/// between <code>from</code> and <code>to</code> inclusively.
		/// </summary>
		public DateFilter(System.String f, System.DateTime from, System.DateTime to)
		{
			InitBlock();
			field = f;
			start = DateField.DateToString(from);
			end = DateField.DateToString(to);
		}
		
		/// <summary> Constructs a filter for Field <code>f</code> matching times
		/// between <code>from</code> and <code>to</code> inclusively.
		/// </summary>
		public DateFilter(System.String f, long from, long to)
		{
			InitBlock();
			field = f;
			start = DateField.TimeToString(from);
			end = DateField.TimeToString(to);
		}
		
		/// <summary> Constructs a filter for Field <code>f</code> matching
		/// dates on or before before <code>date</code>.
		/// </summary>
		public static DateFilter Before(System.String field, System.DateTime date)
		{
			DateFilter result = new DateFilter(field);
			result.end = DateField.DateToString(date);
			return result;
		}
		
		/// <summary> Constructs a filter for Field <code>f</code> matching times
		/// on or before <code>time</code>.
		/// </summary>
		public static DateFilter Before(System.String field, long time)
		{
			DateFilter result = new DateFilter(field);
			result.end = DateField.TimeToString(time);
			return result;
		}
		
		/// <summary> Constructs a filter for Field <code>f</code> matching
		/// dates on or after <code>date</code>.
		/// </summary>
		public static DateFilter After(System.String field, System.DateTime date)
		{
			DateFilter result = new DateFilter(field);
			result.start = DateField.DateToString(date);
			return result;
		}
		
		/// <summary> Constructs a filter for Field <code>f</code> matching
		/// times on or after <code>time</code>.
		/// </summary>
		public static DateFilter After(System.String field, long time)
		{
			DateFilter result = new DateFilter(field);
			result.start = DateField.TimeToString(time);
			return result;
		}
		
		/// <summary> Returns a BitSet with true for documents which should be
		/// permitted in search results, and false for those that should
		/// not.
		/// </summary>
		public override System.Collections.BitArray Bits(Monodoc.Lucene.Net.Index.IndexReader reader)
		{
			System.Collections.BitArray bits = new System.Collections.BitArray((reader.MaxDoc() % 64 == 0?reader.MaxDoc() / 64:reader.MaxDoc() / 64 + 1) * 64);
			TermEnum enumerator = reader.Terms(new Term(field, start));
			TermDocs termDocs = reader.TermDocs();
			if (enumerator.Term() == null)
			{
				return bits;
			}
			
			try
			{
				Term stop = new Term(field, end);
				while (enumerator.Term().CompareTo(stop) <= 0)
				{
					termDocs.Seek(enumerator.Term());
					while (termDocs.Next())
					{
						bits.Set(termDocs.Doc(), true);
					}
					if (!enumerator.Next())
					{
						break;
					}
				}
			}
			finally
			{
				enumerator.Close();
				termDocs.Close();
			}
			return bits;
		}
		
		public override System.String ToString()
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			buffer.Append(field);
			buffer.Append(":");
			buffer.Append(DateField.StringToDate(start).ToString("r"));
			buffer.Append("-");
			buffer.Append(DateField.StringToDate(end).ToString("r"));
			return buffer.ToString();
		}
	}
}