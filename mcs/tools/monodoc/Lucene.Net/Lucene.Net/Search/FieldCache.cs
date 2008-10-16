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
namespace Monodoc.Lucene.Net.Search
{
	
	/// <summary> Expert: Maintains caches of term values.
	/// 
	/// <p>Created: May 19, 2004 11:13:14 AM
	/// 
	/// </summary>
	/// <author>   Tim Jones (Nacimiento Software)
	/// </author>
	/// <since>   lucene 1.4
	/// </since>
	/// <version>  $Id: FieldCache.java,v 1.1 2004/05/19 23:05:27 tjones Exp $
	/// </version>
	/// <summary>Expert: Stores term text values and document ordering data. </summary>
	public class StringIndex
	{
		
		/// <summary>All the term values, in natural order. </summary>
		public System.String[] lookup;
		
		/// <summary>For each document, an index into the lookup array. </summary>
		public int[] order;
		
		/// <summary>Creates one of these objects </summary>
		public StringIndex(int[] values, System.String[] lookup)
		{
			this.order = values;
			this.lookup = lookup;
		}
	}
	public struct FieldCache_Fields
    {
		/// <summary>Indicator for StringIndex values in the cache. </summary>
		// NOTE: the value assigned to this constant must not be
		// the same as any of those in SortField!!
		public readonly static int STRING_INDEX = - 1;
		/// <summary>Expert: The cache used internally by sorting and range query classes. </summary>
		public readonly static FieldCache DEFAULT;
		static FieldCache_Fields()
		{
			DEFAULT = new FieldCacheImpl();
		}
	}
	public interface FieldCache
	{
		
		
		/// <summary>Checks the internal cache for an appropriate entry, and if none is
		/// found, reads the terms in <code>Field</code> as integers and returns an array
		/// of size <code>reader.maxDoc()</code> of the value each document
		/// has in the given Field.
		/// </summary>
		/// <param name="reader"> Used to get Field values.
		/// </param>
		/// <param name="Field">  Which Field contains the integers.
		/// </param>
		/// <returns> The values in the given Field for each document.
		/// </returns>
		/// <throws>  IOException  If any error occurs. </throws>
		int[] GetInts(Monodoc.Lucene.Net.Index.IndexReader reader, System.String field);
		
		/// <summary>Checks the internal cache for an appropriate entry, and if
		/// none is found, reads the terms in <code>Field</code> as floats and returns an array
		/// of size <code>reader.maxDoc()</code> of the value each document
		/// has in the given Field.
		/// </summary>
		/// <param name="reader"> Used to get Field values.
		/// </param>
		/// <param name="Field">  Which Field contains the floats.
		/// </param>
		/// <returns> The values in the given Field for each document.
		/// </returns>
		/// <throws>  IOException  If any error occurs. </throws>
		float[] GetFloats(Monodoc.Lucene.Net.Index.IndexReader reader, System.String field);
		
		/// <summary>Checks the internal cache for an appropriate entry, and if none
		/// is found, reads the term values in <code>Field</code> and returns an array
		/// of size <code>reader.maxDoc()</code> containing the value each document
		/// has in the given Field.
		/// </summary>
		/// <param name="reader"> Used to get Field values.
		/// </param>
		/// <param name="Field">  Which Field contains the strings.
		/// </param>
		/// <returns> The values in the given Field for each document.
		/// </returns>
		/// <throws>  IOException  If any error occurs. </throws>
		System.String[] GetStrings(Monodoc.Lucene.Net.Index.IndexReader reader, System.String field);
		
		/// <summary>Checks the internal cache for an appropriate entry, and if none
		/// is found reads the term values in <code>Field</code> and returns
		/// an array of them in natural order, along with an array telling
		/// which element in the term array each document uses.
		/// </summary>
		/// <param name="reader"> Used to get Field values.
		/// </param>
		/// <param name="Field">  Which Field contains the strings.
		/// </param>
		/// <returns> Array of terms and index into the array for each document.
		/// </returns>
		/// <throws>  IOException  If any error occurs. </throws>
		StringIndex GetStringIndex(Monodoc.Lucene.Net.Index.IndexReader reader, System.String field);
		
		/// <summary>Checks the internal cache for an appropriate entry, and if
		/// none is found reads <code>Field</code> to see if it contains integers, floats
		/// or strings, and then calls one of the other methods in this class to get the
		/// values.  For string values, a StringIndex is returned.  After
		/// calling this method, there is an entry in the cache for both
		/// type <code>AUTO</code> and the actual found type.
		/// </summary>
		/// <param name="reader"> Used to get Field values.
		/// </param>
		/// <param name="Field">  Which Field contains the values.
		/// </param>
		/// <returns> int[], float[] or StringIndex.
		/// </returns>
		/// <throws>  IOException  If any error occurs. </throws>
		System.Object GetAuto(Monodoc.Lucene.Net.Index.IndexReader reader, System.String field);
		
		/// <summary>Checks the internal cache for an appropriate entry, and if none
		/// is found reads the terms out of <code>Field</code> and calls the given SortComparator
		/// to get the sort values.  A hit in the cache will happen if <code>reader</code>,
		/// <code>Field</code>, and <code>comparator</code> are the same (using <code>equals()</code>)
		/// as a previous call to this method.
		/// </summary>
		/// <param name="reader"> Used to get Field values.
		/// </param>
		/// <param name="Field">  Which Field contains the values.
		/// </param>
		/// <param name="comparator">Used to convert terms into something to sort by.
		/// </param>
		/// <returns> Array of sort objects, one for each document.
		/// </returns>
		/// <throws>  IOException  If any error occurs. </throws>
		System.IComparable[] GetCustom(Monodoc.Lucene.Net.Index.IndexReader reader, System.String field, SortComparator comparator);
	}
}