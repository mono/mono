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

using IndexReader = Mono.Lucene.Net.Index.IndexReader;
using Term = Mono.Lucene.Net.Index.Term;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> Subclass of FilteredTermEnum for enumerating all terms that match the
	/// specified prefix filter term.
	/// <p/>
	/// Term enumerations are always ordered by Term.compareTo().  Each term in
	/// the enumeration is greater than all that precede it.
	/// 
	/// </summary>
	public class PrefixTermEnum:FilteredTermEnum
	{
		
		private Term prefix;
		private bool endEnum = false;
		
		public PrefixTermEnum(IndexReader reader, Term prefix)
		{
			this.prefix = prefix;
			
			SetEnum(reader.Terms(new Term(prefix.Field(), prefix.Text())));
		}
		
		public override float Difference()
		{
			return 1.0f;
		}
		
		public override bool EndEnum()
		{
			return endEnum;
		}
		
		protected internal virtual Term GetPrefixTerm()
		{
			return prefix;
		}
		
		public /*protected internal*/ override bool TermCompare(Term term)
		{
			if ((System.Object) term.Field() == (System.Object) prefix.Field() && term.Text().StartsWith(prefix.Text()))
			{
				return true;
			}
			endEnum = true;
			return false;
		}
	}
}
