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
namespace Monodoc.Lucene.Net.Index
{
	
	/// <summary>Provides access to stored term vector of 
	/// a document Field.
	/// </summary>
	public interface TermFreqVector
	{
		/// <summary> </summary>
		/// <returns> The Field this vector is associated with.
		/// 
		/// </returns>
		System.String GetField();
		
		/// <returns> The number of terms in the term vector.
		/// </returns>
		int Size();
		
		/// <returns> An Array of term texts in ascending order.
		/// </returns>
		System.String[] GetTerms();
		
		
		/// <summary>Array of term frequencies. Locations of the array correspond one to one
		/// to the terms in the array obtained from <code>getTerms</code>
		/// method. Each location in the array contains the number of times this
		/// term occurs in the document or the document Field.
		/// </summary>
		int[] GetTermFrequencies();
		
		
		/// <summary>Return an index in the term numbers array returned from
		/// <code>getTerms</code> at which the term with the specified
		/// <code>term</code> appears. If this term does not appear in the array,
		/// return -1.
		/// </summary>
		int IndexOf(System.String term);
		
		
		/// <summary>Just like <code>indexOf(int)</code> but searches for a number of terms
		/// at the same time. Returns an array that has the same size as the number
		/// of terms searched for, each slot containing the result of searching for
		/// that term number.
		/// 
		/// </summary>
		/// <param name="terms">array containing terms to look for
		/// </param>
		/// <param name="start">index in the array where the list of terms starts
		/// </param>
		/// <param name="len">the number of terms in the list
		/// </param>
		int[] IndexesOf(System.String[] terms, int start, int len);
	}
}