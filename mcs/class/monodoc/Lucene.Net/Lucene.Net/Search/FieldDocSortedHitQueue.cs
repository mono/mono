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
using PriorityQueue = Monodoc.Lucene.Net.Util.PriorityQueue;
namespace Monodoc.Lucene.Net.Search
{
	
	/// <summary> Expert: Collects sorted results from Searchable's and collates them.
	/// The elements put into this queue must be of type FieldDoc.
	/// 
	/// <p>Created: Feb 11, 2004 2:04:21 PM
	/// 
	/// </summary>
	/// <author>   Tim Jones (Nacimiento Software)
	/// </author>
	/// <since>   lucene 1.4
	/// </since>
	/// <version>  $Id: FieldDocSortedHitQueue.java,v 1.5 2004/05/24 22:51:42 tjones Exp $
	/// </version>
	class FieldDocSortedHitQueue : PriorityQueue
	{
		
		// this cannot contain AUTO fields - any AUTO fields should
		// have been resolved by the time this class is used.
		internal volatile SortField[] fields;
		
		// used in the case where the fields are sorted by locale
		// based strings
		internal volatile System.Globalization.CompareInfo[] collators;
		
		
		/// <summary> Creates a hit queue sorted by the given list of fields.</summary>
		/// <param name="fields">Field names, in priority order (highest priority first).
		/// </param>
		/// <param name="size"> The number of hits to retain.  Must be greater than zero.
		/// </param>
		/// <throws>  IOException </throws>
		internal FieldDocSortedHitQueue(SortField[] fields, int size)
		{
			this.fields = fields;
			this.collators = HasCollators(fields);
			Initialize(size);
		}
		
		
		/// <summary> Allows redefinition of sort fields if they are <code>null</code>.
		/// This is to handle the case using ParallelMultiSearcher where the
		/// original list contains AUTO and we don't know the actual sort
		/// type until the values come back.  The fields can only be set once.
		/// This method is thread safe.
		/// </summary>
		/// <param name="">fields
		/// </param>
		internal virtual void  SetFields(SortField[] fields)
		{
			lock (this)
			{
				if (this.fields == null)
				{
					this.fields = fields;
					this.collators = HasCollators(fields);
				}
			}
		}
		
		
		/// <summary>Returns the fields being used to sort. </summary>
		internal virtual SortField[] GetFields()
		{
			return fields;
		}
		
		
		/// <summary>Returns an array of collators, possibly <code>null</code>.  The collators
		/// correspond to any SortFields which were given a specific locale.
		/// </summary>
		/// <param name="fields">Array of sort fields.
		/// </param>
		/// <returns> Array, possibly <code>null</code>.
		/// </returns>
		private System.Globalization.CompareInfo[] HasCollators(SortField[] fields)
		{
			if (fields == null)
				return null;
			System.Globalization.CompareInfo[] ret = new System.Globalization.CompareInfo[fields.Length];
			for (int i = 0; i < fields.Length; ++i)
			{
				System.Globalization.CultureInfo locale = fields[i].GetLocale();
				if (locale != null)
					ret[i] = locale.CompareInfo;
			}
			return ret;
		}
		
		
		/// <summary> Returns whether <code>a</code> is less relevant than <code>b</code>.</summary>
		/// <param name="a">ScoreDoc
		/// </param>
		/// <param name="b">ScoreDoc
		/// </param>
		/// <returns> <code>true</code> if document <code>a</code> should be sorted after document <code>b</code>.
		/// </returns>
		public override bool LessThan(System.Object a, System.Object b)
		{
			FieldDoc docA = (FieldDoc) a;
			FieldDoc docB = (FieldDoc) b;
			int n = fields.Length;
			int c = 0;
			for (int i = 0; i < n && c == 0; ++i)
			{
				int type = fields[i].GetType();
				if (fields[i].GetReverse())
				{
					switch (type)
					{
						
						case SortField.SCORE: 
							float r1 = (float) ((System.Single) docA.fields[i]);
							float r2 = (float) ((System.Single) docB.fields[i]);
							if (r1 < r2)
								c = - 1;
							if (r1 > r2)
								c = 1;
							break;
						
						case SortField.DOC: 
						case SortField.INT: 
							int i1 = ((System.Int32) docA.fields[i]);
							int i2 = ((System.Int32) docB.fields[i]);
							if (i1 > i2)
								c = - 1;
							if (i1 < i2)
								c = 1;
							break;
						
						case SortField.STRING: 
							System.String s1 = (System.String) docA.fields[i];
							System.String s2 = (System.String) docB.fields[i];
							if (s2 == null)
								c = - 1;
							// could be null if there are
							else if (s1 == null)
								c = 1;
							// no terms in the given Field
							else if (fields[i].GetLocale() == null)
							{
								c = String.CompareOrdinal(s2, s1);
							}
							else
							{
								c = collators[i].Compare(s2.ToString(), s1.ToString());
							}
							break;
						
						case SortField.FLOAT: 
							float f1 = (float) ((System.Single) docA.fields[i]);
							float f2 = (float) ((System.Single) docB.fields[i]);
							if (f1 > f2)
								c = - 1;
							if (f1 < f2)
								c = 1;
							break;
						
						case SortField.CUSTOM: 
							c = docB.fields[i].CompareTo(docA.fields[i]);
							break;
						
						case SortField.AUTO: 
							// we cannot handle this - even if we determine the type of object (Float or
							// Integer), we don't necessarily know how to compare them (both SCORE and
							// FLOAT both contain floats, but are sorted opposite of each other). Before
							// we get here, each AUTO should have been replaced with its actual value.
							throw new System.SystemException("FieldDocSortedHitQueue cannot use an AUTO SortField");
						
						default: 
							throw new System.SystemException("invalid SortField type: " + type);
						
					}
				}
				else
				{
					switch (type)
					{
						
						case SortField.SCORE: 
							float r1 = (float) ((System.Single) docA.fields[i]);
							float r2 = (float) ((System.Single) docB.fields[i]);
							if (r1 > r2)
								c = - 1;
							if (r1 < r2)
								c = 1;
							break;
						
						case SortField.DOC: 
						case SortField.INT: 
							int i1 = ((System.Int32) docA.fields[i]);
							int i2 = ((System.Int32) docB.fields[i]);
							if (i1 < i2)
								c = - 1;
							if (i1 > i2)
								c = 1;
							break;
						
						case SortField.STRING: 
							System.String s1 = (System.String) docA.fields[i];
							System.String s2 = (System.String) docB.fields[i];
							// null values need to be sorted first, because of how FieldCache.getStringIndex()
							// works - in that routine, any documents without a value in the given Field are
							// put first.
							if (s1 == null)
								c = - 1;
							// could be null if there are
							else if (s2 == null)
								c = 1;
							// no terms in the given Field
							else if (fields[i].GetLocale() == null)
							{
								c = String.CompareOrdinal(s1, s2);
							}
							else
							{
								c = collators[i].Compare(s1.ToString(), s2.ToString());
							}
							break;
						
						case SortField.FLOAT: 
							float f1 = (float) ((System.Single) docA.fields[i]);
							float f2 = (float) ((System.Single) docB.fields[i]);
							if (f1 < f2)
								c = - 1;
							if (f1 > f2)
								c = 1;
							break;
						
						case SortField.CUSTOM: 
							c = docA.fields[i].CompareTo(docB.fields[i]);
							break;
						
						case SortField.AUTO: 
							// we cannot handle this - even if we determine the type of object (Float or
							// Integer), we don't necessarily know how to compare them (both SCORE and
							// FLOAT both contain floats, but are sorted opposite of each other). Before
							// we get here, each AUTO should have been replaced with its actual value.
							throw new System.SystemException("FieldDocSortedHitQueue cannot use an AUTO SortField");
						
						default: 
							throw new System.SystemException("invalid SortField type: " + type);
						
					}
				}
			}
			return c > 0;
		}
	}
}