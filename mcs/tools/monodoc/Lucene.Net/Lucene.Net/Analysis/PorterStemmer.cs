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

/*

Porter stemmer in Java. The original paper is in

Porter, 1980, An algorithm for suffix stripping, Program, Vol. 14,
no. 3, pp 130-137,

See also http://www.tartarus.org/~martin/PorterStemmer/index.html

Bug 1 (reported by Gonzalo Parra 16/10/99) fixed as marked below.
Tthe words 'aed', 'eed', 'oed' leave k at 'a' for step 3, and b[k-1]
is then out outside the bounds of b.

Similarly,

Bug 2 (reported by Steve Dyrdahl 22/2/00) fixed as marked below.
'ion' by itself leaves j = -1 in the test for 'ion' in step 5, and
b[j] is then outside the bounds of b.

Release 3.

[ This version is derived from Release 3, modified by Brian Goetz to
optimize for fewer object creations.  ]
*/
using System;
namespace Monodoc.Lucene.Net.Analysis
{
	
	/// <summary> 
	/// Stemmer, implementing the Porter Stemming Algorithm
	/// 
	/// The Stemmer class transforms a word into its root form.  The input
	/// word can be provided a character at time (by calling add()), or at once
	/// by calling one of the various stem(something) methods.
	/// </summary>
	
	class PorterStemmer
	{
		private char[] b;
		private int i, j, k, k0;
		private bool dirty = false;
		private const int INC = 50; /* unit of size whereby b is increased */
		private const int EXTRA = 1;
		
		public PorterStemmer()
		{
			b = new char[INC];
			i = 0;
		}
		
		/// <summary> reset() resets the stemmer so it can stem another word.  If you invoke
		/// the stemmer by calling add(char) and then stem(), you must call reset()
		/// before starting another word.
		/// </summary>
		public virtual void  Reset()
		{
			i = 0; dirty = false;
		}
		
		/// <summary> Add a character to the word being stemmed.  When you are finished
		/// adding characters, you can call stem(void) to process the word.
		/// </summary>
		public virtual void  Add(char ch)
		{
			if (b.Length <= i + EXTRA)
			{
				char[] new_b = new char[b.Length + INC];
				for (int c = 0; c < b.Length; c++)
					new_b[c] = b[c];
				b = new_b;
			}
			b[i++] = ch;
		}
		
		/// <summary> After a word has been stemmed, it can be retrieved by toString(),
		/// or a reference to the internal buffer can be retrieved by getResultBuffer
		/// and getResultLength (which is generally more efficient.)
		/// </summary>
		public override System.String ToString()
		{
			return new System.String(b, 0, i);
		}
		
		/// <summary> Returns the length of the word resulting from the stemming process.</summary>
		public virtual int GetResultLength()
		{
			return i;
		}
		
		/// <summary> Returns a reference to a character buffer containing the results of
		/// the stemming process.  You also need to consult getResultLength()
		/// to determine the length of the result.
		/// </summary>
		public virtual char[] GetResultBuffer()
		{
			return b;
		}
		
		/* cons(i) is true <=> b[i] is a consonant. */
		
		private bool Cons(int i)
		{
			switch (b[i])
			{
				
				case 'a': 
				case 'e': 
				case 'i': 
				case 'o': 
				case 'u': 
					return false;
				
				case 'y': 
					return (i == k0)?true:!Cons(i - 1);
				
				default: 
					return true;
				
			}
		}
		
		/* m() measures the number of consonant sequences between k0 and j. if c is
		a consonant sequence and v a vowel sequence, and <..> indicates arbitrary
		presence,
		
		<c><v>       gives 0
		<c>vc<v>     gives 1
		<c>vcvc<v>   gives 2
		<c>vcvcvc<v> gives 3
		....
		*/
		
		private int M()
		{
			int n = 0;
			int i = k0;
			while (true)
			{
				if (i > j)
					return n;
				if (!Cons(i))
					break;
				i++;
			}
			i++;
			while (true)
			{
				while (true)
				{
					if (i > j)
						return n;
					if (Cons(i))
						break;
					i++;
				}
				i++;
				n++;
				while (true)
				{
					if (i > j)
						return n;
					if (!Cons(i))
						break;
					i++;
				}
				i++;
			}
		}
		
		/* vowelinstem() is true <=> k0,...j contains a vowel */
		
		private bool vowelinstem()
		{
			int i;
			for (i = k0; i <= j; i++)
				if (!Cons(i))
					return true;
			return false;
		}
		
		/* doublec(j) is true <=> j,(j-1) contain a double consonant. */
		
		private bool Doublec(int j)
		{
			if (j < k0 + 1)
				return false;
			if (b[j] != b[j - 1])
				return false;
			return Cons(j);
		}
		
		/* cvc(i) is true <=> i-2,i-1,i has the form consonant - vowel - consonant
		and also if the second c is not w,x or y. this is used when trying to
		restore an e at the end of a short word. e.g.
		
		cav(e), lov(e), hop(e), crim(e), but
		snow, box, tray.
		
		*/
		
		private bool Cvc(int i)
		{
			if (i < k0 + 2 || !Cons(i) || Cons(i - 1) || !Cons(i - 2))
				return false;
			else
			{
				int ch = b[i];
				if (ch == 'w' || ch == 'x' || ch == 'y')
					return false;
			}
			return true;
		}
		
		private bool Ends(System.String s)
		{
			int l = s.Length;
			int o = k - l + 1;
			if (o < k0)
				return false;
			for (int i = 0; i < l; i++)
				if (b[o + i] != s[i])
					return false;
			j = k - l;
			return true;
		}
		
		/* setto(s) sets (j+1),...k to the characters in the string s, readjusting
		k. */
		
		internal virtual void  Setto(System.String s)
		{
			int l = s.Length;
			int o = j + 1;
			for (int i = 0; i < l; i++)
				b[o + i] = s[i];
			k = j + l;
			dirty = true;
		}
		
		/* r(s) is used further down. */
		
		internal virtual void  R(System.String s)
		{
			if (M() > 0)
				Setto(s);
		}
		
		/* step1() gets rid of plurals and -ed or -ing. e.g.
		
		caresses  ->  caress
		ponies    ->  poni
		ties      ->  ti
		caress    ->  caress
		cats      ->  cat
		
		feed      ->  feed
		agreed    ->  agree
		disabled  ->  disable
		
		matting   ->  mat
		mating    ->  mate
		meeting   ->  meet
		milling   ->  mill
		messing   ->  mess
		
		meetings  ->  meet
		
		*/
		
		private void  step1()
		{
			if (b[k] == 's')
			{
				if (Ends("sses"))
					k -= 2;
				else if (Ends("ies"))
					Setto("i");
				else if (b[k - 1] != 's')
					k--;
			}
			if (Ends("eed"))
			{
				if (M() > 0)
					k--;
			}
			else if ((Ends("ed") || Ends("ing")) && vowelinstem())
			{
				k = j;
				if (Ends("at"))
					Setto("ate");
				else if (Ends("bl"))
					Setto("ble");
				else if (Ends("iz"))
					Setto("ize");
				else if (Doublec(k))
				{
					int ch = b[k--];
					if (ch == 'l' || ch == 's' || ch == 'z')
						k++;
				}
				else if (M() == 1 && Cvc(k))
					Setto("e");
			}
		}
		
		/* step2() turns terminal y to i when there is another vowel in the stem. */
		
		private void  step2()
		{
			if (Ends("y") && vowelinstem())
			{
				b[k] = 'i';
				dirty = true;
			}
		}
		
		/* step3() maps double suffices to single ones. so -ization ( = -ize plus
		-ation) maps to -ize etc. note that the string before the suffix must give
		m() > 0. */
		
		private void  step3()
		{
			if (k == k0)
				return ; /* For Bug 1 */
			switch (b[k - 1])
			{
				
				case 'a': 
					if (Ends("ational"))
					{
						R("ate"); break;
					}
					if (Ends("tional"))
					{
						R("tion"); break;
					}
					break;
				
				case 'c': 
					if (Ends("enci"))
					{
						R("ence"); break;
					}
					if (Ends("anci"))
					{
						R("ance"); break;
					}
					break;
				
				case 'e': 
					if (Ends("izer"))
					{
						R("ize"); break;
					}
					break;
				
				case 'l': 
					if (Ends("bli"))
					{
						R("ble"); break;
					}
					if (Ends("alli"))
					{
						R("al"); break;
					}
					if (Ends("entli"))
					{
						R("ent"); break;
					}
					if (Ends("eli"))
					{
						R("e"); break;
					}
					if (Ends("ousli"))
					{
						R("ous"); break;
					}
					break;
				
				case 'o': 
					if (Ends("ization"))
					{
						R("ize"); break;
					}
					if (Ends("ation"))
					{
						R("ate"); break;
					}
					if (Ends("ator"))
					{
						R("ate"); break;
					}
					break;
				
				case 's': 
					if (Ends("alism"))
					{
						R("al"); break;
					}
					if (Ends("iveness"))
					{
						R("ive"); break;
					}
					if (Ends("fulness"))
					{
						R("ful"); break;
					}
					if (Ends("ousness"))
					{
						R("ous"); break;
					}
					break;
				
				case 't': 
					if (Ends("aliti"))
					{
						R("al"); break;
					}
					if (Ends("iviti"))
					{
						R("ive"); break;
					}
					if (Ends("biliti"))
					{
						R("ble"); break;
					}
					break;
				
				case 'g': 
					if (Ends("logi"))
					{
						R("log"); break;
					}
					break;
				}
		}
		
		/* step4() deals with -ic-, -full, -ness etc. similar strategy to step3. */
		
		private void  step4()
		{
			switch (b[k])
			{
				
				case 'e': 
					if (Ends("icate"))
					{
						R("ic"); break;
					}
					if (Ends("ative"))
					{
						R(""); break;
					}
					if (Ends("alize"))
					{
						R("al"); break;
					}
					break;
				
				case 'i': 
					if (Ends("iciti"))
					{
						R("ic"); break;
					}
					break;
				
				case 'l': 
					if (Ends("ical"))
					{
						R("ic"); break;
					}
					if (Ends("ful"))
					{
						R(""); break;
					}
					break;
				
				case 's': 
					if (Ends("ness"))
					{
						R(""); break;
					}
					break;
				}
		}
		
		/* step5() takes off -ant, -ence etc., in context <c>vcvc<v>. */
		
		private void  step5()
		{
			if (k == k0)
				return ; /* for Bug 1 */
			switch (b[k - 1])
			{
				
				case 'a': 
					if (Ends("al"))
						break;
					return ;
				
				case 'c': 
					if (Ends("ance"))
						break;
					if (Ends("ence"))
						break;
					return ;
				
				case 'e': 
					if (Ends("er"))
						break; return ;
				
				case 'i': 
					if (Ends("ic"))
						break; return ;
				
				case 'l': 
					if (Ends("able"))
						break;
					if (Ends("ible"))
						break; return ;
				
				case 'n': 
					if (Ends("ant"))
						break;
					if (Ends("ement"))
						break;
					if (Ends("ment"))
						break;
					/* element etc. not stripped before the m */
					if (Ends("ent"))
						break;
					return ;
				
				case 'o': 
					if (Ends("ion") && j >= 0 && (b[j] == 's' || b[j] == 't'))
						break;
					/* j >= 0 fixes Bug 2 */
					if (Ends("ou"))
						break;
					return ;
					/* takes care of -ous */
				
				case 's': 
					if (Ends("ism"))
						break;
					return ;
				
				case 't': 
					if (Ends("ate"))
						break;
					if (Ends("iti"))
						break;
					return ;
				
				case 'u': 
					if (Ends("ous"))
						break;
					return ;
				
				case 'v': 
					if (Ends("ive"))
						break;
					return ;
				
				case 'z': 
					if (Ends("ize"))
						break;
					return ;
				
				default: 
					return ;
				
			}
			if (M() > 1)
				k = j;
		}
		
		/* step6() removes a final -e if m() > 1. */
		
		private void  step6()
		{
			j = k;
			if (b[k] == 'e')
			{
				int a = M();
				if (a > 1 || a == 1 && !Cvc(k - 1))
					k--;
			}
			if (b[k] == 'l' && Doublec(k) && M() > 1)
				k--;
		}
		
		
		/// <summary> Stem a word provided as a String.  Returns the result as a String.</summary>
		public virtual System.String Stem(System.String s)
		{
			if (Stem(s.ToCharArray(), s.Length))
			{
				return ToString();
			}
			else
				return s;
		}
		
		/// <summary>Stem a word contained in a char[].  Returns true if the stemming process
		/// resulted in a word different from the input.  You can retrieve the
		/// result with getResultLength()/getResultBuffer() or toString().
		/// </summary>
		public virtual bool Stem(char[] word)
		{
			return Stem(word, word.Length);
		}
		
		/// <summary>Stem a word contained in a portion of a char[] array.  Returns
		/// true if the stemming process resulted in a word different from
		/// the input.  You can retrieve the result with
		/// getResultLength()/getResultBuffer() or toString().
		/// </summary>
		public virtual bool Stem(char[] wordBuffer, int offset, int wordLen)
		{
			Reset();
			if (b.Length < wordLen)
			{
				char[] new_b = new char[wordLen + EXTRA];
				b = new_b;
			}
			for (int j = 0; j < wordLen; j++)
				b[j] = wordBuffer[offset + j];
			i = wordLen;
			return Stem(0);
		}
		
		/// <summary>Stem a word contained in a leading portion of a char[] array.
		/// Returns true if the stemming process resulted in a word different
		/// from the input.  You can retrieve the result with
		/// getResultLength()/getResultBuffer() or toString().
		/// </summary>
		public virtual bool Stem(char[] word, int wordLen)
		{
			return Stem(word, 0, wordLen);
		}
		
		/// <summary>Stem the word placed into the Stemmer buffer through calls to add().
		/// Returns true if the stemming process resulted in a word different
		/// from the input.  You can retrieve the result with
		/// getResultLength()/getResultBuffer() or toString().
		/// </summary>
		public virtual bool Stem()
		{
			return Stem(0);
		}
		
		public virtual bool Stem(int i0)
		{
			k = i - 1;
			k0 = i0;
			if (k > k0 + 1)
			{
				step1(); step2(); step3(); step4(); step5(); step6();
			}
			// Also, a word is considered dirty if we lopped off letters
			// Thanks to Ifigenia Vairelles for pointing this out.
			if (i != k + 1)
				dirty = true;
			i = k + 1;
			return dirty;
		}
		
		/// <summary>Test program for demonstrating the Stemmer.  It reads a file and
		/// stems each word, writing the result to standard out.
		/// Usage: Stemmer file-name
		/// </summary>
		[STAThread]
		public static void  Main(System.String[] args)
		{
			PorterStemmer s = new PorterStemmer();
			
			for (int i = 0; i < args.Length; i++)
			{
				try
				{
					System.IO.BinaryReader in_Renamed = new System.IO.BinaryReader(System.IO.File.Open(args[i], System.IO.FileMode.Open, System.IO.FileAccess.Read));
					byte[] buffer = new byte[1024];
					int bufferLen, offset, ch;
					
					bufferLen = in_Renamed.Read(buffer, 0, buffer.Length);
					offset = 0;
					s.Reset();
					
					while (true)
					{
						if (offset < bufferLen)
							ch = buffer[offset++];
						else
						{
							bufferLen = in_Renamed.Read(buffer, 0, buffer.Length);
							offset = 0;
							if (bufferLen <= 0)
								ch = - 1;
							else
								ch = buffer[offset++];
						}
						
						if (System.Char.IsLetter((char) ch))
						{
							s.Add(System.Char.ToLower((char) ch));
						}
						else
						{
							s.Stem();
							System.Console.Out.Write(s.ToString());
							s.Reset();
							if (ch < 0)
								break;
							else
							{
								System.Console.Out.Write((char) ch);
							}
						}
					}
					
					in_Renamed.Close();
				}
				catch (System.IO.IOException )
				{
					System.Console.Out.WriteLine("error reading " + args[i]);
				}
			}
		}
	}
}