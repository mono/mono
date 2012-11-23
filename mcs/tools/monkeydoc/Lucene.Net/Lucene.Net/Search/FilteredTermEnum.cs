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

using Term = Mono.Lucene.Net.Index.Term;
using TermEnum = Mono.Lucene.Net.Index.TermEnum;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary>Abstract class for enumerating a subset of all terms. 
	/// <p/>Term enumerations are always ordered by Term.compareTo().  Each term in
	/// the enumeration is greater than all that precede it.  
	/// </summary>
	public abstract class FilteredTermEnum:TermEnum
	{
		/// <summary>the current term </summary>
		protected internal Term currentTerm = null;
		
		/// <summary>the delegate enum - to set this member use {@link #setEnum} </summary>
		protected internal TermEnum actualEnum = null;
		
		public FilteredTermEnum()
		{
		}
		
		/// <summary>Equality compare on the term </summary>
		public /*protected internal*/ abstract bool TermCompare(Term term);
		
		/// <summary>Equality measure on the term </summary>
		public abstract float Difference();
		
		/// <summary>Indicates the end of the enumeration has been reached </summary>
		public abstract bool EndEnum();
		
		/// <summary> use this method to set the actual TermEnum (e.g. in ctor),
		/// it will be automatically positioned on the first matching term.
		/// </summary>
		protected internal virtual void  SetEnum(TermEnum actualEnum)
		{
			this.actualEnum = actualEnum;
			// Find the first term that matches
			Term term = actualEnum.Term();
			if (term != null && TermCompare(term))
				currentTerm = term;
			else
				Next();
		}
		
		/// <summary> Returns the docFreq of the current Term in the enumeration.
		/// Returns -1 if no Term matches or all terms have been enumerated.
		/// </summary>
		public override int DocFreq()
		{
			if (currentTerm == null)
				return - 1;
			System.Diagnostics.Debug.Assert(actualEnum != null);
			return actualEnum.DocFreq();
		}
		
		/// <summary>Increments the enumeration to the next element.  True if one exists. </summary>
		public override bool Next()
		{
			if (actualEnum == null)
				return false; // the actual enumerator is not initialized!
			currentTerm = null;
			while (currentTerm == null)
			{
				if (EndEnum())
					return false;
				if (actualEnum.Next())
				{
					Term term = actualEnum.Term();
					if (TermCompare(term))
					{
						currentTerm = term;
						return true;
					}
				}
				else
					return false;
			}
			currentTerm = null;
			return false;
		}
		
		/// <summary>Returns the current Term in the enumeration.
		/// Returns null if no Term matches or all terms have been enumerated. 
		/// </summary>
		public override Term Term()
		{
			return currentTerm;
		}
		
		/// <summary>Closes the enumeration to further activity, freeing resources.  </summary>
		public override void  Close()
		{
			if (actualEnum != null)
				actualEnum.Close();
			currentTerm = null;
			actualEnum = null;
		}
	}
}
