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
namespace Monodoc.Lucene.Net.Analysis.DE
{
	/// <summary> A stemmer for German words. The algorithm is based on the report
	/// "A Fast and Simple Stemming Algorithm for German Words" by Jörg
	/// Caumanns (joerg.caumanns@isst.fhg.de).
	/// 
	/// </summary>
	/// <author>     Gerhard Schwarz
	/// </author>
	/// <version>    $Id: GermanStemmer.java,v 1.11 2004/05/30 20:24:20 otis Exp $
	/// </version>
	public class GermanStemmer
	{
		/// <summary> Buffer for the terms while stemming them.</summary>
		private System.Text.StringBuilder sb = new System.Text.StringBuilder();
		
		/// <summary> Amount of characters that are removed with <tt>substitute()</tt> while stemming.</summary>
		private int substCount = 0;
		
		/// <summary> Stemms the given term to an unique <tt>discriminator</tt>.
		/// 
		/// </summary>
		/// <param name="term"> The term that should be stemmed.
		/// </param>
		/// <returns>      Discriminator for <tt>term</tt>
		/// </returns>
		protected internal virtual System.String Stem(System.String term)
		{
			// Use lowercase for medium stemming.
			term = term.ToLower();
			if (!IsStemmable(term))
				return term;
			// Reset the StringBuffer.
			sb.Remove(0, sb.Length - 0);
			sb.Insert(0, term);
			// Stemming starts here...
			Substitute(sb);
			Strip(sb);
			Optimize(sb);
			Resubstitute(sb);
			RemoveParticleDenotion(sb);
			return sb.ToString();
		}
		
		/// <summary> Checks if a term could be stemmed.
		/// 
		/// </summary>
		/// <returns>  true if, and only if, the given term consists in letters.
		/// </returns>
		private bool IsStemmable(System.String term)
		{
			for (int c = 0; c < term.Length; c++)
			{
				if (!System.Char.IsLetter(term[c]))
					return false;
			}
			return true;
		}
		
		/// <summary> suffix stripping (stemming) on the current term. The stripping is reduced
		/// to the seven "base" suffixes "e", "s", "n", "t", "em", "er" and * "nd",
		/// from which all regular suffixes are build of. The simplification causes
		/// some overstemming, and way more irregular stems, but still provides unique.
		/// discriminators in the most of those cases.
		/// The algorithm is context free, except of the length restrictions.
		/// </summary>
		private void  Strip(System.Text.StringBuilder buffer)
		{
			bool doMore = true;
			while (doMore && buffer.Length > 3)
			{
				if ((buffer.Length + substCount > 5) && buffer.ToString(buffer.Length - 2, buffer.Length).Equals("nd"))
				{
					buffer.Remove(buffer.Length - 2, buffer.Length - (buffer.Length - 2));
				}
				else if ((buffer.Length + substCount > 4) && buffer.ToString(buffer.Length - 2, buffer.Length).Equals("em"))
				{
					buffer.Remove(buffer.Length - 2, buffer.Length - (buffer.Length - 2));
				}
				else if ((buffer.Length + substCount > 4) && buffer.ToString(buffer.Length - 2, buffer.Length).Equals("er"))
				{
					buffer.Remove(buffer.Length - 2, buffer.Length - (buffer.Length - 2));
				}
				else if (buffer[buffer.Length - 1] == 'e')
				{
					buffer.Remove(buffer.Length - 1, 1);
				}
				else if (buffer[buffer.Length - 1] == 's')
				{
					buffer.Remove(buffer.Length - 1, 1);
				}
				else if (buffer[buffer.Length - 1] == 'n')
				{
					buffer.Remove(buffer.Length - 1, 1);
				}
				// "t" occurs only as suffix of verbs.
				else if (buffer[buffer.Length - 1] == 't')
				{
					buffer.Remove(buffer.Length - 1, 1);
				}
				else
				{
					doMore = false;
				}
			}
		}
		
		/// <summary> Does some optimizations on the term. This optimisations are
		/// contextual.
		/// </summary>
		private void  Optimize(System.Text.StringBuilder buffer)
		{
			// Additional step for female plurals of professions and inhabitants.
			if (buffer.Length > 5 && buffer.ToString(buffer.Length - 5, buffer.Length).Equals("erin*"))
			{
				buffer.Remove(buffer.Length - 1, 1);
				Strip(buffer);
			}
			// Additional step for irregular plural nouns like "Matrizen -> Matrix".
			if (buffer[buffer.Length - 1] == ('z'))
			{
				buffer[buffer.Length - 1] = 'x';
			}
		}
		
		/// <summary> Removes a particle denotion ("ge") from a term.</summary>
		private void  RemoveParticleDenotion(System.Text.StringBuilder buffer)
		{
			if (buffer.Length > 4)
			{
				for (int c = 0; c < buffer.Length - 3; c++)
				{
					if (buffer.ToString(c, c + 4).Equals("gege"))
					{
						buffer.Remove(c, c + 2 - c);
						return ;
					}
				}
			}
		}
		
		/// <summary> Do some substitutions for the term to reduce overstemming:
		/// 
		/// - Substitute Umlauts with their corresponding vowel: äöü -> aou,
		/// "ß" is substituted by "ss"
		/// - Substitute a second char of a pair of equal characters with
		/// an asterisk: ?? -> ?*
		/// - Substitute some common character combinations with a token:
		/// sch/ch/ei/ie/ig/st -> $/§/%/&/#/!
		/// </summary>
		private void  Substitute(System.Text.StringBuilder buffer)
		{
			substCount = 0;
			for (int c = 0; c < buffer.Length; c++)
			{
				// Replace the second char of a pair of the equal characters with an asterisk
				if (c > 0 && buffer[c] == buffer[c - 1])
				{
					buffer[c] = '*';
				}
				// Substitute Umlauts.
				else if (buffer[c] == 'A') //// 'Ã¤')
				{
					//'ä' ) {
					buffer[c] = 'a';
				}
				else if (buffer[c] == 'A') //// 'Ã¶')
				{
					//'ö' ) {
					buffer[c] = 'o';
				}
				else if (buffer[c] == 'A') //// 'Ã¼')
				{
					// 'ü' ) {
					buffer[c] = 'u';
				}
				// Take care that at least one character is left left side from the current one
				if (c < buffer.Length - 1)
				{
					if (buffer[c] == 'A') //// 'ÃŸ')
					{
						//'ß' ) {
						buffer[c] = 's';
						buffer.Insert(c + 1, 's');
						substCount++;
					}
					// Masking several common character combinations with an token
					else if ((c < buffer.Length - 2) && buffer[c] == 's' && buffer[c + 1] == 'c' && buffer[c + 2] == 'h')
					{
						buffer[c] = '$';
						buffer.Remove(c + 1, c + 3 - (c + 1));
						substCount = + 2;
					}
					else if (buffer[c] == 'c' && buffer[c + 1] == 'h')
					{
						buffer[c] = 'A'; //// 'Â§';
						buffer.Remove(c + 1, 1);
						substCount++;
					}
					else if (buffer[c] == 'e' && buffer[c + 1] == 'i')
					{
						buffer[c] = '%';
						buffer.Remove(c + 1, 1);
						substCount++;
					}
					else if (buffer[c] == 'i' && buffer[c + 1] == 'e')
					{
						buffer[c] = '&';
						buffer.Remove(c + 1, 1);
						substCount++;
					}
					else if (buffer[c] == 'i' && buffer[c + 1] == 'g')
					{
						buffer[c] = '#';
						buffer.Remove(c + 1, 1);
						substCount++;
					}
					else if (buffer[c] == 's' && buffer[c + 1] == 't')
					{
						buffer[c] = '!';
						buffer.Remove(c + 1, 1);
						substCount++;
					}
				}
			}
		}
		
		/// <summary> Undoes the changes made by substitute(). That are character pairs and
		/// character combinations. Umlauts will remain as their corresponding vowel,
		/// as "ß" remains as "ss".
		/// </summary>
		private void  Resubstitute(System.Text.StringBuilder buffer)
		{
			for (int c = 0; c < buffer.Length; c++)
			{
				if (buffer[c] == '*')
				{
					char x = buffer[c - 1];
					buffer[c] = x;
				}
				else if (buffer[c] == '$')
				{
					buffer[c] = 's';
					buffer.Insert(c + 1, new char[]{'c', 'h'}, 0, 2);
				}
				else if (buffer[c] == 'A') //// 'Â§')
				{
					// '§' ) {
					buffer[c] = 'c';
					buffer.Insert(c + 1, 'h');
				}
				else if (buffer[c] == '%')
				{
					buffer[c] = 'e';
					buffer.Insert(c + 1, 'i');
				}
				else if (buffer[c] == '&')
				{
					buffer[c] = 'i';
					buffer.Insert(c + 1, 'e');
				}
				else if (buffer[c] == '#')
				{
					buffer[c] = 'i';
					buffer.Insert(c + 1, 'g');
				}
				else if (buffer[c] == '!')
				{
					buffer[c] = 's';
					buffer.Insert(c + 1, 't');
				}
			}
		}
	}
}