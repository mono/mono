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
using Term = Monodoc.Lucene.Net.Index.Term;
using TermEnum = Monodoc.Lucene.Net.Index.TermEnum;
namespace Monodoc.Lucene.Net.Search
{
	
	/// <summary>Abstract class for enumerating a subset of all terms. 
	/// <p>Term enumerations are always ordered by Term.compareTo().  Each term in
	/// the enumeration is greater than all that precede it.  
	/// </summary>
	public abstract class FilteredTermEnum:TermEnum
	{
		private Term currentTerm = null;
		private TermEnum actualEnum = null;
		
		public FilteredTermEnum()
		{
		}
		
		/// <summary>Equality compare on the term </summary>
		protected internal abstract bool TermCompare(Term term);
		
		/// <summary>Equality measure on the term </summary>
		public abstract float Difference();
		
		/// <summary>Indiciates the end of the enumeration has been reached </summary>
		public abstract bool EndEnum();
		
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
		/// Initially invalid, valid after next() called for the first time. 
		/// </summary>
		public override int DocFreq()
		{
			if (actualEnum == null)
				return - 1;
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
		/// Initially invalid, valid after next() called for the first time. 
		/// </summary>
		public override Term Term()
		{
			return currentTerm;
		}
		
		/// <summary>Closes the enumeration to further activity, freeing resources.  </summary>
		public override void  Close()
		{
			actualEnum.Close();
			currentTerm = null;
			actualEnum = null;
		}
	}
}