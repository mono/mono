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
//   csc /r:C5.dll Anagrams.cs 

using System;
using System.IO;                        // StreamReader, TextReader
using System.Text;			// Encoding
using System.Text.RegularExpressions;   // Regex
using C5;
using SCG = System.Collections.Generic;

namespace Anagrams
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
      SCG.IEqualityComparer<TreeBag<char>> tbh
        = UnsequencedCollectionEqualityComparer<TreeBag<char>, char>.Default;
      HashSet<TreeBag<char>> anagrams = new HashSet<TreeBag<char>>(tbh);
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
    // classes.  Should use a *sequenced* equalityComparer on a TreeBag<char>,
    // obviously: after all, characters can be sorted by ASCII code.  On
    // 347 000 distinct Danish words this takes 70 cpu seconds, 180 MB
    // memory, and 263 wall-clock seconds (due to swapping).

    // Using a TreeBag<char> and a sequenced equalityComparer takes 82 cpu seconds
    // and 180 MB RAM to find the 26,058 anagram classes among 347,000
    // distinct words.

    // Using an unsequenced equalityComparer on TreeBag<char> or HashBag<char>
    // makes it criminally slow: at least 1200 cpu seconds.  This must
    // be because many bags get the same hash code, so that there are
    // many collisions.  But exactly how the unsequenced equalityComparer works is
    // not clear ... or is it because unsequenced equality is slow?

    public static SCG.IEnumerable<SCG.IEnumerable<String>> AnagramClasses(SCG.IEnumerable<String> ss)
    {
      bool unseq = true;
      IDictionary<TreeBag<char>, TreeSet<String>> classes;
      if (unseq)
      {
        SCG.IEqualityComparer<TreeBag<char>> unsequencedTreeBagEqualityComparer
    = UnsequencedCollectionEqualityComparer<TreeBag<char>, char>.Default;
        classes = new HashDictionary<TreeBag<char>, TreeSet<String>>(unsequencedTreeBagEqualityComparer);
      }
      else
      {
        SCG.IEqualityComparer<TreeBag<char>> sequencedTreeBagEqualityComparer
    = SequencedCollectionEqualityComparer<TreeBag<char>, char>.Default;
        classes = new HashDictionary<TreeBag<char>, TreeSet<String>>(sequencedTreeBagEqualityComparer);
      }
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