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
namespace Monodoc.Lucene.Net.Analysis.RU
{
	/// <summary> Russian stemming algorithm implementation (see http://snowball.sourceforge.net for detailed description).
	/// 
	/// </summary>
	/// <author>   Boris Okner, b.okner@rogers.com
	/// </author>
	/// <version>  $Id: RussianStemmer.java,v 1.5 2004/03/29 22:48:01 cutting Exp $
	/// </version>
	public class RussianStemmer
	{
		private char[] charset;
		
		// positions of RV, R1 and R2 respectively
		private int RV, R1, R2;
		
		// letters
		private static char A = (char) (0);
		private static char B = (char) (1);
		private static char V = (char) (2);
		private static char G = (char) (3);
		private static char D = (char) (4);
		private static char E = (char) (5);
		private static char ZH = (char) (6);
		private static char Z = (char) (7);
		private static char I = (char) (8);
		private static char I_ = (char) (9);
		private static char K = (char) (10);
		private static char L = (char) (11);
		private static char M = (char) (12);
		private static char N = (char) (13);
		private static char O = (char) (14);
		private static char P = (char) (15);
		private static char R = (char) (16);
		private static char S = (char) (17);
		private static char T = (char) (18);
		private static char U = (char) (19);
		private static char F = (char) (20);
		private static char X = (char) (21);
		private static char TS = (char) (22);
		private static char CH = (char) (23);
		private static char SH = (char) (24);
		private static char SHCH = (char) (25);
		private static char HARD = (char) (26);
		private static char Y = (char) (27);
		private static char SOFT = (char) (28);
		private static char AE = (char) (29);
		private static char IU = (char) (30);
		private static char IA = (char) (31);
		
		// stem definitions
		private static char[] vowels = new char[]{A, E, I, O, U, Y, AE, IU, IA};
		
		private static char[][] perfectiveGerundEndings1 = new char[][]{new char[]{V}, new char[]{V, SH, I}, new char[]{V, SH, I, S, SOFT}};
		
		private static char[][] perfectiveGerund1Predessors = new char[][]{new char[]{A}, new char[]{IA}};
		
		private static char[][] perfectiveGerundEndings2 = new char[][]{new char[]{I, V}, new char[]{Y, V}, new char[]{I, V, SH, I}, new char[]{Y, V, SH, I}, new char[]{I, V, SH, I, S, SOFT}, new char[]{Y, V, SH, I, S, SOFT}};
		
		private static char[][] adjectiveEndings = new char[][]{new char[]{E, E}, new char[]{I, E}, new char[]{Y, E}, new char[]{O, E}, new char[]{E, I_}, new char[]{I, I_}, new char[]{Y, I_}, new char[]{O, I_}, new char[]{E, M}, new char[]{I, M}, new char[]{Y, M}, new char[]{O, M}, new char[]{I, X}, new char[]{Y, X}, new char[]{U, IU}, new char[]{IU, IU}, new char[]{A, IA}, new char[]{IA, IA}, new char[]{O, IU}, new char[]{E, IU}, new char[]{I, M, I}, new char[]{Y, M, I}, new char[]{E, G, O}, new char[]{O, G, O}, new char[]{E, M, U}, new char[]{O, M, U}};
		
		private static char[][] participleEndings1 = new char[][]{new char[]{SHCH}, new char[]{E, M}, new char[]{N, N}, new char[]{V, SH}, new char[]{IU, SHCH}};
		
		private static char[][] participleEndings2 = new char[][]{new char[]{I, V, SH}, new char[]{Y, V, SH}, new char[]{U, IU, SHCH}};
		
		private static char[][] participle1Predessors = new char[][]{new char[]{A}, new char[]{IA}};
		
		private static char[][] reflexiveEndings = new char[][]{new char[]{S, IA}, new char[]{S, SOFT}};
		
		private static char[][] verbEndings1 = new char[][]{new char[]{I_}, new char[]{L}, new char[]{N}, new char[]{L, O}, new char[]{N, O}, new char[]{E, T}, new char[]{IU, T}, new char[]{L, A}, new char[]{N, A}, new char[]{L, I}, new char[]{E, M}, new char[]{N, Y}, new char[]{E, T, E}, new char[]{I_, T, E}, new char[]{T, SOFT}, new char[]{E, SH, SOFT}, new char[]{N, N, O}};
		
		private static char[][] verbEndings2 = new char[][]{new char[]{IU}, new char[]{U, IU}, new char[]{E, N}, new char[]{E, I_}, new char[]{IA, T}, new char[]{U, I_}, new char[]{I, L}, new char[]{Y, L}, new char[]{I, M}, new char[]{Y, M}, new char[]{I, T}, new char[]{Y, T}, new char[]{I, L, A}, new char[]{Y, L, A}, new char[]{E, N, A}, new char[]{I, T, E}, new char[]{I, L, I}, new char[]{Y, L, I}, new char[]{I, L, O}, new char[]{Y, L, O}, new char[]{E, N, O}, new char[]{U, E, T}, new char[]{U, IU, T}, new char[]{E, N, Y}, new char[]{I, T, SOFT}, new char[]{Y, T, SOFT}, new char[]{I, SH, SOFT}, new char[]{E, I_, T, E}, new char[]{U, I_, T, E}};
		
		private static char[][] verb1Predessors = new char[][]{new char[]{A}, new char[]{IA}};
		
		private static char[][] nounEndings = new char[][]{new char[]{A}, new char[]{U}, new char[]{I_}, new char[]{O}, new char[]{U}, new char[]{E}, new char[]{Y}, new char[]{I}, new char[]{SOFT}, new char[]{IA}, new char[]{E, V}, new char[]{O, V}, new char[]{I, E}, new char[]{SOFT, E}, new char[]{IA, X}, new char[]{I, IU}, new char[]{E, I}, new char[]{I, I}, new char[]{E, I_}, new char[]{O, I_}, new char[]{E, M}, new char[]{A, M}, new char[]{O, M}, new char[]{A, X}, new char[]{SOFT, IU}, new char[]{I, IA}, new char[]{SOFT, IA}, new char[]{I, I_}, new char[]{IA, M}, new char[]{IA, M, I}, new char[]{A, M, I}, new char[]{I, E, I_}, new char[]{I, IA, M}, new char[]{I, E, M}, new char[]{I, IA, X}, new char[]{I, IA, M, I}};
		
		private static char[][] superlativeEndings = new char[][]{new char[]{E, I_, SH}, new char[]{E, I_, SH, E}};
		
		private static char[][] derivationalEndings = new char[][]{new char[]{O, S, T}, new char[]{O, S, T, SOFT}};
		
		/// <summary> RussianStemmer constructor comment.</summary>
		public RussianStemmer():base()
		{
		}
		
		/// <summary> RussianStemmer constructor comment.</summary>
		public RussianStemmer(char[] charset):base()
		{
			this.charset = charset;
		}
		
		/// <summary> Adjectival ending is an adjective ending,
		/// optionally preceded by participle ending.
		/// Creation date: (17/03/2002 12:14:58 AM)
		/// </summary>
		/// <param name="stemmingZone">java.lang.StringBuffer
		/// </param>
		private bool Adjectival(System.Text.StringBuilder stemmingZone)
		{
			// look for adjective ending in a stemming zone
			if (!FindAndRemoveEnding(stemmingZone, adjectiveEndings))
				return false;
			// if adjective ending was found, try for participle ending
			bool r = FindAndRemoveEnding(stemmingZone, participleEndings1, participle1Predessors) || FindAndRemoveEnding(stemmingZone, participleEndings2);
			return true;
		}
		
		/// <summary> Derivational endings
		/// Creation date: (17/03/2002 12:14:58 AM)
		/// </summary>
		/// <param name="stemmingZone">java.lang.StringBuffer
		/// </param>
		private bool Derivational(System.Text.StringBuilder stemmingZone)
		{
			int endingLength = FindEnding(stemmingZone, derivationalEndings);
			if (endingLength == 0)
			// no derivational ending found
				return false;
			else
			{
				// Ensure that the ending locates in R2
				if (R2 - RV <= stemmingZone.Length - endingLength)
				{
					stemmingZone.Length -= endingLength;
					return true;
				}
				else
				{
					return false;
				}
			}
		}
		
		/// <summary> Finds ending among given ending class and returns the length of ending found(0, if not found).
		/// Creation date: (17/03/2002 8:18:34 PM)
		/// </summary>
		private int FindEnding(System.Text.StringBuilder stemmingZone, int startIndex, char[][] theEndingClass)
		{
			bool match = false;
			for (int i = theEndingClass.Length - 1; i >= 0; i--)
			{
				char[] theEnding = theEndingClass[i];
				// check if the ending is bigger than stemming zone
				if (startIndex < theEnding.Length - 1)
				{
					match = false;
					continue;
				}
				match = true;
				int stemmingIndex = startIndex;
				for (int j = theEnding.Length - 1; j >= 0; j--)
				{
					if (stemmingZone[stemmingIndex--] != charset[theEnding[j]])
					{
						match = false;
						break;
					}
				}
				// check if ending was found
				if (match)
				{
					return theEndingClass[i].Length; // cut ending
				}
			}
			return 0;
		}
		
		private int FindEnding(System.Text.StringBuilder stemmingZone, char[][] theEndingClass)
		{
			return FindEnding(stemmingZone, stemmingZone.Length - 1, theEndingClass);
		}
		
		/// <summary> Finds the ending among the given class of endings and removes it from stemming zone.
		/// Creation date: (17/03/2002 8:18:34 PM)
		/// </summary>
		private bool FindAndRemoveEnding(System.Text.StringBuilder stemmingZone, char[][] theEndingClass)
		{
			int endingLength = FindEnding(stemmingZone, theEndingClass);
			if (endingLength == 0)
			// not found
				return false;
			else
			{
				stemmingZone.Length -= endingLength;
				// cut the ending found
				return true;
			}
		}
		
		/// <summary> Finds the ending among the given class of endings, then checks if this ending was
		/// preceded by any of given predessors, and if so, removes it from stemming zone.
		/// Creation date: (17/03/2002 8:18:34 PM)
		/// </summary>
		private bool FindAndRemoveEnding(System.Text.StringBuilder stemmingZone, char[][] theEndingClass, char[][] thePredessors)
		{
			int endingLength = FindEnding(stemmingZone, theEndingClass);
			if (endingLength == 0)
			// not found
				return false;
			else
			{
				int predessorLength = FindEnding(stemmingZone, stemmingZone.Length - endingLength - 1, thePredessors);
				if (predessorLength == 0)
					return false;
				else
				{
					stemmingZone.Length -= endingLength;
					// cut the ending found
					return true;
				}
			}
		}
		
		/// <summary> Marks positions of RV, R1 and R2 in a given word.
		/// Creation date: (16/03/2002 3:40:11 PM)
		/// </summary>
		private void  MarkPositions(System.String word)
		{
			RV = 0;
			R1 = 0;
			R2 = 0;
			int i = 0;
			// find RV
			while (word.Length > i && !IsVowel(word[i]))
			{
				i++;
			}
			if (word.Length - 1 < ++i)
				return ; // RV zone is empty
			RV = i;
			// find R1
			while (word.Length > i && IsVowel(word[i]))
			{
				i++;
			}
			if (word.Length - 1 < ++i)
				return ; // R1 zone is empty
			R1 = i;
			// find R2
			while (word.Length > i && !IsVowel(word[i]))
			{
				i++;
			}
			if (word.Length - 1 < ++i)
				return ; // R2 zone is empty
			while (word.Length > i && IsVowel(word[i]))
			{
				i++;
			}
			if (word.Length - 1 < ++i)
				return ; // R2 zone is empty
			R2 = i;
		}
		
		/// <summary> Checks if character is a vowel..
		/// Creation date: (16/03/2002 10:47:03 PM)
		/// </summary>
		/// <returns> boolean
		/// </returns>
		/// <param name="letter">char
		/// </param>
		private bool IsVowel(char letter)
		{
			for (int i = 0; i < vowels.Length; i++)
			{
				if (letter == charset[vowels[i]])
					return true;
			}
			return false;
		}
		
		/// <summary> Noun endings.
		/// Creation date: (17/03/2002 12:14:58 AM)
		/// </summary>
		/// <param name="stemmingZone">java.lang.StringBuffer
		/// </param>
		private bool Noun(System.Text.StringBuilder stemmingZone)
		{
			return FindAndRemoveEnding(stemmingZone, nounEndings);
		}
		
		/// <summary> Perfective gerund endings.
		/// Creation date: (17/03/2002 12:14:58 AM)
		/// </summary>
		/// <param name="stemmingZone">java.lang.StringBuffer
		/// </param>
		private bool PerfectiveGerund(System.Text.StringBuilder stemmingZone)
		{
			return FindAndRemoveEnding(stemmingZone, perfectiveGerundEndings1, perfectiveGerund1Predessors) || FindAndRemoveEnding(stemmingZone, perfectiveGerundEndings2);
		}
		
		/// <summary> Reflexive endings.
		/// Creation date: (17/03/2002 12:14:58 AM)
		/// </summary>
		/// <param name="stemmingZone">java.lang.StringBuffer
		/// </param>
		private bool Reflexive(System.Text.StringBuilder stemmingZone)
		{
			return FindAndRemoveEnding(stemmingZone, reflexiveEndings);
		}
		
		/// <summary> Insert the method's description here.
		/// Creation date: (17/03/2002 12:14:58 AM)
		/// </summary>
		/// <param name="stemmingZone">java.lang.StringBuffer
		/// </param>
		private bool RemoveI(System.Text.StringBuilder stemmingZone)
		{
			if (stemmingZone.Length > 0 && stemmingZone[stemmingZone.Length - 1] == charset[I])
			{
				stemmingZone.Length -= 1;
				return true;
			}
			else
			{
				return false;
			}
		}
		
		/// <summary> Insert the method's description here.
		/// Creation date: (17/03/2002 12:14:58 AM)
		/// </summary>
		/// <param name="stemmingZone">java.lang.StringBuffer
		/// </param>
		private bool RemoveSoft(System.Text.StringBuilder stemmingZone)
		{
			if (stemmingZone.Length > 0 && stemmingZone[stemmingZone.Length - 1] == charset[SOFT])
			{
				stemmingZone.Length -= 1;
				return true;
			}
			else
			{
				return false;
			}
		}
		
		/// <summary> Insert the method's description here.
		/// Creation date: (16/03/2002 10:58:42 PM)
		/// </summary>
		/// <param name="newCharset">char[]
		/// </param>
		public virtual void  SetCharset(char[] newCharset)
		{
			charset = newCharset;
		}
		
		/// <summary> Set ending definition as in Russian stemming algorithm.
		/// Creation date: (16/03/2002 11:16:36 PM)
		/// </summary>
		private void  SetEndings()
		{
			vowels = new char[]{A, E, I, O, U, Y, AE, IU, IA};
			
			perfectiveGerundEndings1 = new char[][]{new char[]{V}, new char[]{V, SH, I}, new char[]{V, SH, I, S, SOFT}};
			
			perfectiveGerund1Predessors = new char[][]{new char[]{A}, new char[]{IA}};
			
			perfectiveGerundEndings2 = new char[][]{new char[]{I, V}, new char[]{Y, V}, new char[]{I, V, SH, I}, new char[]{Y, V, SH, I}, new char[]{I, V, SH, I, S, SOFT}, new char[]{Y, V, SH, I, S, SOFT}};
			
			adjectiveEndings = new char[][]{new char[]{E, E}, new char[]{I, E}, new char[]{Y, E}, new char[]{O, E}, new char[]{E, I_}, new char[]{I, I_}, new char[]{Y, I_}, new char[]{O, I_}, new char[]{E, M}, new char[]{I, M}, new char[]{Y, M}, new char[]{O, M}, new char[]{I, X}, new char[]{Y, X}, new char[]{U, IU}, new char[]{IU, IU}, new char[]{A, IA}, new char[]{IA, IA}, new char[]{O, IU}, new char[]{E, IU}, new char[]{I, M, I}, new char[]{Y, M, I}, new char[]{E, G, O}, new char[]{O, G, O}, new char[]{E, M, U}, new char[]{O, M, U}};
			
			participleEndings1 = new char[][]{new char[]{SHCH}, new char[]{E, M}, new char[]{N, N}, new char[]{V, SH}, new char[]{IU, SHCH}};
			
			participleEndings2 = new char[][]{new char[]{I, V, SH}, new char[]{Y, V, SH}, new char[]{U, IU, SHCH}};
			
			participle1Predessors = new char[][]{new char[]{A}, new char[]{IA}};
			
			reflexiveEndings = new char[][]{new char[]{S, IA}, new char[]{S, SOFT}};
			
			verbEndings1 = new char[][]{new char[]{I_}, new char[]{L}, new char[]{N}, new char[]{L, O}, new char[]{N, O}, new char[]{E, T}, new char[]{IU, T}, new char[]{L, A}, new char[]{N, A}, new char[]{L, I}, new char[]{E, M}, new char[]{N, Y}, new char[]{E, T, E}, new char[]{I_, T, E}, new char[]{T, SOFT}, new char[]{E, SH, SOFT}, new char[]{N, N, O}};
			
			verbEndings2 = new char[][]{new char[]{IU}, new char[]{U, IU}, new char[]{E, N}, new char[]{E, I_}, new char[]{IA, T}, new char[]{U, I_}, new char[]{I, L}, new char[]{Y, L}, new char[]{I, M}, new char[]{Y, M}, new char[]{I, T}, new char[]{Y, T}, new char[]{I, L, A}, new char[]{Y, L, A}, new char[]{E, N, A}, new char[]{I, T, E}, new char[]{I, L, I}, new char[]{Y, L, I}, new char[]{I, L, O}, new char[]{Y, L, O}, new char[]{E, N, O}, new char[]{U, E, T}, new char[]{U, IU, T}, new char[]{E, N, Y}, new char[]{I, T, SOFT}, new char[]{Y, T, SOFT}, new char[]{I, SH, SOFT}, new char[]{E, I_, T, E}, new char[]{U, I_, T, E}};
			
			verb1Predessors = new char[][]{new char[]{A}, new char[]{IA}};
			
			nounEndings = new char[][]{new char[]{A}, new char[]{IU}, new char[]{I_}, new char[]{O}, new char[]{U}, new char[]{E}, new char[]{Y}, new char[]{I}, new char[]{SOFT}, new char[]{IA}, new char[]{E, V}, new char[]{O, V}, new char[]{I, E}, new char[]{SOFT, E}, new char[]{IA, X}, new char[]{I, IU}, new char[]{E, I}, new char[]{I, I}, new char[]{E, I_}, new char[]{O, I_}, new char[]{E, M}, new char[]{A, M}, new char[]{O, M}, new char[]{A, X}, new char[]{SOFT, IU}, new char[]{I, IA}, new char[]{SOFT, IA}, new char[]{I, I_}, new char[]{IA, M}, new char[]{IA, M, I}, new char[]{A, M, I}, new char[]{I, E, I_}, new char[]{I, IA, M}, new char[]{I, E, M}, new char[]{I, IA, X}, new char[]{I, IA, M, I}};
			
			superlativeEndings = new char[][]{new char[]{E, I_, SH}, new char[]{E, I_, SH, E}};
			
			derivationalEndings = new char[][]{new char[]{O, S, T}, new char[]{O, S, T, SOFT}};
		}
		
		/// <summary> Finds the stem for given Russian word.
		/// Creation date: (16/03/2002 3:36:48 PM)
		/// </summary>
		/// <returns> java.lang.String
		/// </returns>
		/// <param name="input">java.lang.String
		/// </param>
		public virtual System.String Stem(System.String input)
		{
			MarkPositions(input);
			if (RV == 0)
				return input; //RV wasn't detected, nothing to stem
			System.Text.StringBuilder stemmingZone = new System.Text.StringBuilder(input.Substring(RV));
			// stemming goes on in RV
			// Step 1
			
			if (!PerfectiveGerund(stemmingZone))
			{
				Reflexive(stemmingZone);
				bool r = Adjectival(stemmingZone) || Verb(stemmingZone) || Noun(stemmingZone);
			}
			// Step 2
			RemoveI(stemmingZone);
			// Step 3
			Derivational(stemmingZone);
			// Step 4
			Superlative(stemmingZone);
			UndoubleN(stemmingZone);
			RemoveSoft(stemmingZone);
			// return result
			return input.Substring(0, (RV) - (0)) + stemmingZone.ToString();
		}
		
		/// <summary> Superlative endings.
		/// Creation date: (17/03/2002 12:14:58 AM)
		/// </summary>
		/// <param name="stemmingZone">java.lang.StringBuffer
		/// </param>
		private bool Superlative(System.Text.StringBuilder stemmingZone)
		{
			return FindAndRemoveEnding(stemmingZone, superlativeEndings);
		}
		
		/// <summary> Undoubles N.
		/// Creation date: (17/03/2002 12:14:58 AM)
		/// </summary>
		/// <param name="stemmingZone">java.lang.StringBuffer
		/// </param>
		private bool UndoubleN(System.Text.StringBuilder stemmingZone)
		{
			char[][] doubleN = new char[][]{new char[]{N, N}};
			if (FindEnding(stemmingZone, doubleN) != 0)
			{
				stemmingZone.Length -= 1;
				return true;
			}
			else
			{
				return false;
			}
		}
		
		/// <summary> Verb endings.
		/// Creation date: (17/03/2002 12:14:58 AM)
		/// </summary>
		/// <param name="stemmingZone">java.lang.StringBuffer
		/// </param>
		private bool Verb(System.Text.StringBuilder stemmingZone)
		{
			return FindAndRemoveEnding(stemmingZone, verbEndings1, verb1Predessors) || FindAndRemoveEnding(stemmingZone, verbEndings2);
		}
		
		/// <summary> Static method for stemming with different charsets</summary>
		public static System.String Stem(System.String theWord, char[] charset)
		{
			RussianStemmer stemmer = new RussianStemmer();
			stemmer.SetCharset(charset);
			return stemmer.Stem(theWord);
		}
	}
}