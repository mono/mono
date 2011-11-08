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

namespace Mono.Lucene.Net.Index
{
	
	/// <summary>Abstract class for enumerating terms.
	/// <p/>Term enumerations are always ordered by Term.compareTo().  Each term in
	/// the enumeration is greater than all that precede it.  
	/// </summary>
	
	public abstract class TermEnum
	{
		/// <summary>Increments the enumeration to the next element.  True if one exists.</summary>
		public abstract bool Next();
		
		/// <summary>Returns the current Term in the enumeration.</summary>
		public abstract Term Term();
		
		/// <summary>Returns the docFreq of the current Term in the enumeration.</summary>
		public abstract int DocFreq();
		
		/// <summary>Closes the enumeration to further activity, freeing resources. </summary>
		public abstract void  Close();
		
		/// <summary>Skips terms to the first beyond the current whose value is
		/// greater or equal to <i>target</i>. <p/>Returns true iff there is such
		/// an entry.  <p/>Behaves as if written: <pre>
		/// public boolean skipTo(Term target) {
		/// do {
		/// if (!next())
		/// return false;
		/// } while (target > term());
		/// return true;
		/// }
		/// </pre>
		/// Some implementations *could* be considerably more efficient than a linear scan.
		/// Check the implementation to be sure.
		/// </summary>
		/// <deprecated> This method is not performant and will be removed in Lucene 3.0.
		/// Use {@link IndexReader#Terms(Term)} to create a new TermEnum positioned at a
		/// given term.
		/// </deprecated>
        [Obsolete("This method is not performant and will be removed in Lucene 3.0.Use IndexReader.Terms(Term) to create a new TermEnum positioned at a given term.")]
		public virtual bool SkipTo(Term target)
		{
			do 
			{
				if (!Next())
					return false;
			}
			while (target.CompareTo(Term()) > 0);
			return true;
		}
	}
}
