/*
 Copyright (c) 2003-2006 Niels Kokholm and Peter Sestoft
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
*/

// C5 example: anagrams 2004-08-08, 2004-11-16

// Compile with 
//   csc /r:C5.dll AnagramTreeBag.cs 

using System;
using System.IO;                        // StreamReader, TextReader
using System.Text;			// Encoding
using System.Text.RegularExpressions;   // Regex
using C5;
using SCG = System.Collections.Generic;

namespace AnagramTreeBag
{
  class MyTest
  {
    public static void Main(String[] args)
    {
      Console.OutputEncoding = Encoding.GetEncoding("iso-8859-1");
      SCG.IEnumerable<String> ss;
      if (args.Length == 2)
        ss = ReadFileWords(args[0], int.Parse(args[1]));
      else
        ss = args;
      // foreach (String s in FirstAnagramOnly(ss)) 
      //   Console.WriteLine(s);
      //   Console.WriteLine("===");
      Timer t = new Timer();
      SCG.IEnumerable<SCG.IEnumerable<String>> classes = AnagramClasses(ss);
      int count = 0;
      foreach (SCG.IEnumerable<String> anagramClass in classes)
      {
        count++;
	// foreach (String s in anagramClass)
	//   Console.Write(s + " ");
	// Console.WriteLine();
      }
      Console.WriteLine("{0} non-trivial anagram classes", count);
      Console.WriteLine(t.Check());
    }

    // Read words at most n words from a file

    public static SCG.IEnumerable<String> ReadFileWords(String filename, int n)
    {
      Regex delim = new Regex("[^a-zæøåA-ZÆØÅ0-9-]+");
      Encoding enc = Encoding.GetEncoding("iso-8859-1");
      using (TextReader rd = new StreamReader(filename, enc))
      {
        for (String line = rd.ReadLine(); line != null; line = rd.ReadLine())
        {
          foreach (String s in delim.Split(line))
            if (s != "")
              yield return s.ToLower();
          if (--n == 0)
            yield break;
        }
      }
    }

    // From an anagram point of view, a word is just a bag of
    // characters.  So an anagram class is represented as TreeBag<char>
    // which permits fast equality comparison -- we shall use them as
    // elements of hash sets or keys in hash maps.

    public static TreeBag<char> AnagramClass(String s)
    {
      TreeBag<char> anagram = new TreeBag<char>(Comparer<char>.Default, EqualityComparer<char>.Default);
      foreach (char c in s)
        anagram.Add(c);
      return anagram;
    }

    // Given a sequence of strings, return only the first member of each
    // anagram class.

    public static SCG.IEnumerable<String> FirstAnagramOnly(SCG.IEnumerable<String> ss)
    {
      HashSet<TreeBag<char>> anagrams = new HashSet<TreeBag<char>>();
      foreach (String s in ss)
      {
        TreeBag<char> anagram = AnagramClass(s);
        if (!anagrams.Contains(anagram))
        {
          anagrams.Add(anagram);
          yield return s;
        }
      }
    }

    // Given a sequence of strings, return all non-trivial anagram
    // classes.  

    // Using TreeBag<char> and an unsequenced equalityComparer, this performs as
    // follows on 1600 MHz Mobile P4 and .Net 2.0 beta 1 (wall-clock
    // time; number of distinct words):

    //  50 000 words  2 822 classes   2.4 sec
    // 100 000 words  5 593 classes   4.6 sec
    // 200 000 words 11 705 classes   9.6 sec
    // 300 000 words 20 396 classes  88.2 sec (includes swapping)
    // 347 165 words 24 428 classes 121.3 sec (includes swapping)

    // The maximal memory consumption is around 180 MB.

    public static SCG.IEnumerable<SCG.IEnumerable<String>>
      AnagramClasses(SCG.IEnumerable<String> ss)
    {
      IDictionary<TreeBag<char>, TreeSet<String>> classes;
      classes = new HashDictionary<TreeBag<char>, TreeSet<String>>();
      foreach (String s in ss)
      {
        TreeBag<char> anagram = AnagramClass(s);
        TreeSet<String> anagramClass;
        if (!classes.Find(anagram, out anagramClass))
          classes[anagram] = anagramClass = new TreeSet<String>();
        anagramClass.Add(s);
      }
      foreach (TreeSet<String> anagramClass in classes.Values)
        if (anagramClass.Count > 1)
          yield return anagramClass;
    }
  }

// Crude timing utility ----------------------------------------

  public class Timer
  {
    private DateTime start;

    public Timer()
    {
      start = DateTime.Now;
    }

    public double Check()
    {
      TimeSpan dur = DateTime.Now - start;
      return dur.TotalSeconds;
    }
  }
}
