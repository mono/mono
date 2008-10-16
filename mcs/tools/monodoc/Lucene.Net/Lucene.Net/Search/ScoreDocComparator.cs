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
namespace Monodoc.Lucene.Net.Search
{
	
	
	/// <summary> Expert: Compares two ScoreDoc objects for sorting.
	/// 
	/// <p>Created: Feb 3, 2004 9:00:16 AM 
	/// 
	/// </summary>
	/// <author>   Tim Jones (Nacimiento Software)
	/// </author>
	/// <since>   lucene 1.4
	/// </since>
	/// <version>  $Id: ScoreDocComparator.java,v 1.5 2004/05/19 23:05:27 tjones Exp $
	/// </version>
	public struct ScoreDocComparator_Fields
    {
		/// <summary>Special comparator for sorting hits according to computed relevance (document score). </summary>
		public readonly static ScoreDocComparator RELEVANCE;
		/// <summary>Special comparator for sorting hits according to index order (document number). </summary>
		public readonly static ScoreDocComparator INDEXORDER;
		static ScoreDocComparator_Fields()
		{
			RELEVANCE = new AnonymousClassScoreDocComparator();
			INDEXORDER = new AnonymousClassScoreDocComparator1();
		}
	}
	public class AnonymousClassScoreDocComparator : ScoreDocComparator
	{
		public virtual int Compare(ScoreDoc i, ScoreDoc j)
		{
			if (i.score > j.score)
				return - 1;
			if (i.score < j.score)
				return 1;
			return 0;
		}
		public virtual System.IComparable SortValue(ScoreDoc i)
		{
			return (float) i.score;
		}
		public virtual int SortType()
		{
			return SortField.SCORE;
		}
	}
	public class AnonymousClassScoreDocComparator1 : ScoreDocComparator
	{
		public virtual int Compare(ScoreDoc i, ScoreDoc j)
		{
			if (i.doc < j.doc)
				return - 1;
			if (i.doc > j.doc)
				return 1;
			return 0;
		}
		public virtual System.IComparable SortValue(ScoreDoc i)
		{
			return (System.Int32) i.doc;
		}
		public virtual int SortType()
		{
			return SortField.DOC;
		}
	}
	public interface ScoreDocComparator
	{
		
		
		/// <summary> Compares two ScoreDoc objects and returns a result indicating their
		/// sort order.
		/// </summary>
		/// <param name="i">First ScoreDoc
		/// </param>
		/// <param name="j">Second ScoreDoc
		/// </param>
		/// <returns> <code>-1</code> if <code>i</code> should come before <code>j</code><br><code>1</code> if <code>i</code> should come after <code>j</code><br><code>0</code> if they are equal
		/// </returns>
		/// <seealso cref="java.util.Comparator">
		/// </seealso>
		int Compare(ScoreDoc i, ScoreDoc j);
		
		
		/// <summary> Returns the value used to sort the given document.  The
		/// object returned must implement the java.io.Serializable
		/// interface.  This is used by multisearchers to determine how to collate results from their searchers.
		/// </summary>
		/// <seealso cref="FieldDoc">
		/// </seealso>
		/// <param name="i">Document
		/// </param>
		/// <returns> Serializable object
		/// </returns>
		System.IComparable SortValue(ScoreDoc i);
		
		
		/// <summary> Returns the type of sort.  Should return <code>SortField.SCORE</code>, <code>SortField.DOC</code>, <code>SortField.STRING</code>, <code>SortField.INTEGER</code>, 
		/// <code>SortField.FLOAT</code> or <code>SortField.CUSTOM</code>.  It is not valid to return <code>SortField.AUTO</code>.
		/// This is used by multisearchers to determine how to collate results from their searchers.
		/// </summary>
		/// <returns> One of the constants in SortField.
		/// </returns>
		/// <seealso cref="SortField">
		/// </seealso>
		int SortType();
	}
}