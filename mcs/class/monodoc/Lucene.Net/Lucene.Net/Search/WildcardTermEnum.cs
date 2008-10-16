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
namespace Monodoc.Lucene.Net.Search
{
	
	/// <summary> Subclass of FilteredTermEnum for enumerating all terms that match the
	/// specified wildcard filter term.
	/// <p>
	/// Term enumerations are always ordered by Term.compareTo().  Each term in
	/// the enumeration is greater than all that precede it.
	/// 
	/// </summary>
	/// <version>  $Id: WildcardTermEnum.java,v 1.8 2004/05/11 17:23:21 otis Exp $
	/// </version>
	public class WildcardTermEnum:FilteredTermEnum
	{
		internal Term searchTerm;
		internal System.String field = "";
		internal System.String text = "";
		internal System.String pre = "";
		internal int preLen = 0;
		internal bool fieldMatch = false;
		internal bool endEnum = false;
		
		/// <summary> Creates a new <code>WildcardTermEnum</code>.  Passing in a
		/// {@link Monodoc.Lucene.Net.Index.Term Term} that does not contain a
		/// <code>WILDCARD_CHAR</code> will cause an exception to be thrown.
		/// </summary>
		public WildcardTermEnum(Monodoc.Lucene.Net.Index.IndexReader reader, Term term):base()
		{
			searchTerm = term;
			field = searchTerm.Field();
			text = searchTerm.Text();
			
			int sidx = text.IndexOf((System.Char) WILDCARD_STRING);
			int cidx = text.IndexOf((System.Char) WILDCARD_CHAR);
			int idx = sidx;
			if (idx == - 1)
			{
				idx = cidx;
			}
			else if (cidx >= 0)
			{
				idx = System.Math.Min(idx, cidx);
			}
			
			pre = searchTerm.Text().Substring(0, (idx) - (0));
			preLen = pre.Length;
			text = text.Substring(preLen);
			SetEnum(reader.Terms(new Term(searchTerm.Field(), pre)));
		}
		
		protected internal override bool TermCompare(Term term)
		{
			if ((System.Object) field == (System.Object) term.Field())
			{
				System.String searchText = term.Text();
				if (searchText.StartsWith(pre))
				{
					return WildcardEquals(text, 0, searchText, preLen);
				}
			}
			endEnum = true;
			return false;
		}
		
		public override float Difference()
		{
			return 1.0f;
		}
		
		public override bool EndEnum()
		{
			return endEnum;
		}
		
		/// <summary>*****************************************
		/// String equality with support for wildcards
		/// ******************************************
		/// </summary>
		
		public const char WILDCARD_STRING = '*';
		public const char WILDCARD_CHAR = '?';
		
		/// <summary> Determines if a word matches a wildcard pattern.
		/// <small>Work released by Granta Design Ltd after originally being done on
		/// company time.</small>
		/// </summary>
		public static bool WildcardEquals(System.String pattern, int patternIdx, System.String string_Renamed, int stringIdx)
		{
			for (int p = patternIdx; ; ++p)
			{
				for (int s = stringIdx; ; ++p, ++s)
				{
					// End of string yet?
					bool sEnd = (s >= string_Renamed.Length);
					// End of pattern yet?
					bool pEnd = (p >= pattern.Length);
					
					// If we're looking at the end of the string...
					if (sEnd)
					{
						// Assume the only thing left on the pattern is/are wildcards
						bool justWildcardsLeft = true;
						
						// Current wildcard position
						int wildcardSearchPos = p;
						// While we haven't found the end of the pattern,
						// and haven't encountered any non-wildcard characters
						while (wildcardSearchPos < pattern.Length && justWildcardsLeft)
						{
							// Check the character at the current position
							char wildchar = pattern[wildcardSearchPos];
							// If it's not a wildcard character, then there is more
							// pattern information after this/these wildcards.
							
							if (wildchar != WILDCARD_CHAR && wildchar != WILDCARD_STRING)
							{
								justWildcardsLeft = false;
							}
							else
							{
								// Look at the next character
								wildcardSearchPos++;
							}
						}
						
						// This was a prefix wildcard search, and we've matched, so
						// return true.
						if (justWildcardsLeft)
						{
							return true;
						}
					}
					
					// If we've gone past the end of the string, or the pattern,
					// return false.
					if (sEnd || pEnd)
					{
						break;
					}
					
					// Match a single character, so continue.
					if (pattern[p] == WILDCARD_CHAR)
					{
						continue;
					}
					
					//
					if (pattern[p] == WILDCARD_STRING)
					{
						// Look at the character beyond the '*'.
						++p;
						// Examine the string, starting at the last character.
						for (int i = string_Renamed.Length; i >= s; --i)
						{
							if (WildcardEquals(pattern, p, string_Renamed, i))
							{
								return true;
							}
						}
						break;
					}
					if (pattern[p] != string_Renamed[s])
					{
						break;
					}
				}
				return false;
			}
		}
		
		public override void  Close()
		{
			base.Close();
			searchTerm = null;
			field = null;
			text = null;
		}
	}
}