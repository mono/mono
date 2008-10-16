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
using Monodoc.Lucene.Net.Analysis;
namespace Monodoc.Lucene.Net.Analysis.Standard
{
	
	/// <summary>Normalizes tokens extracted with {@link StandardTokenizer}. </summary>
	
	public sealed class StandardFilter : TokenFilter
	{
		
		
		/// <summary>Construct filtering <i>in</i>. </summary>
		public StandardFilter(TokenStream in_Renamed) : base(in_Renamed)
		{
		}
		
		private static readonly System.String APOSTROPHE_TYPE = Monodoc.Lucene.Net.Analysis.Standard.StandardTokenizerConstants.tokenImage[Monodoc.Lucene.Net.Analysis.Standard.StandardTokenizerConstants.APOSTROPHE];
		private static readonly System.String ACRONYM_TYPE = Monodoc.Lucene.Net.Analysis.Standard.StandardTokenizerConstants.tokenImage[Monodoc.Lucene.Net.Analysis.Standard.StandardTokenizerConstants.ACRONYM];
		
		/// <summary>Returns the next token in the stream, or null at EOS.
		/// <p>Removes <tt>'s</tt> from the end of words.
		/// <p>Removes dots from acronyms.
		/// </summary>
		public override Monodoc.Lucene.Net.Analysis.Token Next()
		{
			Monodoc.Lucene.Net.Analysis.Token t = input.Next();
			
			if (t == null)
				return null;
			
			System.String text = t.TermText();
			System.String type = t.Type();
			
			if ((System.Object) type == (System.Object) APOSTROPHE_TYPE && (text.EndsWith("'s") || text.EndsWith("'S")))
			{
				return new Monodoc.Lucene.Net.Analysis.Token(text.Substring(0, (text.Length - 2) - (0)), t.StartOffset(), t.EndOffset(), type);
			}
			else if ((System.Object) type == (System.Object) ACRONYM_TYPE)
			{
				// remove dots
				System.Text.StringBuilder trimmed = new System.Text.StringBuilder();
				for (int i = 0; i < text.Length; i++)
				{
					char c = text[i];
					if (c != '.')
						trimmed.Append(c);
				}
				return new Monodoc.Lucene.Net.Analysis.Token(trimmed.ToString(), t.StartOffset(), t.EndOffset(), type);
			}
			else
			{
				return t;
			}
		}
	}
}