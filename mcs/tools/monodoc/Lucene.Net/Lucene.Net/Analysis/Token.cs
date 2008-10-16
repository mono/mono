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
namespace Monodoc.Lucene.Net.Analysis
{
	/// <summary>A Token is an occurence of a term from the text of a Field.  It consists of
	/// a term's text, the start and end offset of the term in the text of the Field,
	/// and a type string.
	/// The start and end offsets permit applications to re-associate a token with
	/// its source text, e.g., to display highlighted query terms in a document
	/// browser, or to show matching text fragments in a KWIC (KeyWord In Context)
	/// display, etc.
	/// The type is an interned string, assigned by a lexical analyzer
	/// (a.k.a. tokenizer), naming the lexical or syntactic class that the token
	/// belongs to.  For example an end of sentence marker token might be implemented
	/// with type "eos".  The default token type is "word".  
	/// </summary>
	
	public sealed class Token
	{
		internal System.String termText; // the text of the term
		internal int startOffset; // start in source text
		internal int endOffset; // end in source text
		internal System.String type_Renamed_Field = "word"; // lexical type
		
		private int positionIncrement = 1;
		
		/// <summary>Constructs a Token with the given term text, and start & end offsets.
		/// The type defaults to "word." 
		/// </summary>
		public Token(System.String text, int start, int end)
		{
			termText = text;
			startOffset = start;
			endOffset = end;
		}
		
		/// <summary>Constructs a Token with the given text, start and end offsets, & type. </summary>
		public Token(System.String text, int start, int end, System.String typ)
		{
			termText = text;
			startOffset = start;
			endOffset = end;
			type_Renamed_Field = typ;
		}
		
		/// <summary>Set the position increment.  This determines the position of this token
		/// relative to the previous Token in a {@link TokenStream}, used in phrase
		/// searching.
		/// 
		/// <p>The default value is one.
		/// 
		/// <p>Some common uses for this are:<ul>
		/// 
		/// <li>Set it to zero to put multiple terms in the same position.  This is
		/// useful if, e.g., a word has multiple stems.  Searches for phrases
		/// including either stem will match.  In this case, all but the first stem's
		/// increment should be set to zero: the increment of the first instance
		/// should be one.  Repeating a token with an increment of zero can also be
		/// used to boost the scores of matches on that token.
		/// 
		/// <li>Set it to values greater than one to inhibit exact phrase matches.
		/// If, for example, one does not want phrases to match across removed stop
		/// words, then one could build a stop word filter that removes stop words and
		/// also sets the increment to the number of stop words removed before each
		/// non-stop word.  Then exact phrase queries will only match when the terms
		/// occur with no intervening stop words.
		/// 
		/// </ul>
		/// </summary>
		/// <seealso cref="Monodoc.Lucene.Net.Index.TermPositions">
		/// </seealso>
		public void  SetPositionIncrement(int positionIncrement)
		{
			if (positionIncrement < 0)
				throw new System.ArgumentException("Increment must be zero or greater: " + positionIncrement);
			this.positionIncrement = positionIncrement;
		}
		
		/// <summary>Returns the position increment of this Token.</summary>
		/// <seealso cref="#setPositionIncrement">
		/// </seealso>
		public int GetPositionIncrement()
		{
			return positionIncrement;
		}
		
		/// <summary>Returns the Token's term text. </summary>
		public System.String TermText()
		{
			return termText;
		}
		
		/// <summary>Returns this Token's starting offset, the position of the first character
		/// corresponding to this token in the source text.
		/// Note that the difference between endOffset() and startOffset() may not be
		/// equal to termText.length(), as the term text may have been altered by a
		/// stemmer or some other filter. 
		/// </summary>
		public int StartOffset()
		{
			return startOffset;
		}
		
		/// <summary>Returns this Token's ending offset, one greater than the position of the
		/// last character corresponding to this token in the source text. 
		/// </summary>
		public int EndOffset()
		{
			return endOffset;
		}
		
		/// <summary>Returns this Token's lexical type.  Defaults to "word". </summary>
		public System.String Type()
		{
			return type_Renamed_Field;
		}
	}
}