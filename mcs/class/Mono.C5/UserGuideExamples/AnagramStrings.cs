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

// C5 example: anagrams represented as sorted strings 2004-08-26

// To represent an anagram class, use a string containing the sorted
// characters of a word.  

// This is faster than a TreeBag<char> because the words and hence
// bags are small.  Takes 15 CPU seconds and 138 MB RAM to find the
// 26,058 anagram classes among 347,000 distinct words.

// Compile with 
//   csc /r:C5.dll Anagrams.cs 

using System;
using System.IO;                        // StreamReader, TextReader
using System.Text;			// Encoding
using System.Text.RegularExpressions;   // Regex
using C5;
using SCG = System.Collections.Generic;

namespace AnagramStrings
{
  class MyTest
  {
    public static void Main(String[] args)
    {
      Console.OutputEncoding = Encoding.GetEncoding("iso-8859-1");
      SCG.IEnumerable<String> ss;
      if (args.Length == 1)
        ss = ReadFileWords(args[0]);
      else
        ss = args;

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
      Console.WriteLine("{0} anagram classes", count);
      Console.WriteLine(t.Check());
    }

    // Read words from a file

    public static SCG.IEnumerable<String> ReadFileWords(String filename)
    {
      Regex delim = new Regex("[^a-zæøåA-ZÆØÅ0-9-]+");
      using (TextReader rd = new StreamReader(filename, Encoding.GetEncoding("iso-8859-1")))
      {
        for (String line = rd.ReadLine(); line != null; line = rd.ReadLine())
          foreach (String s in delim.Split(line))
            if (s != "")
              yield return s.ToLower();
      }
    }

    // From an anagram point of view, a word is just a bag of characters.

    public static CharBag AnagramClass(String s)
    {
      return new CharBag(s);
    }

    // Given a sequence of strings, return all non-trivial anagram classes   

    public static SCG.IEnumerable<SCG.IEnumerable<String>> AnagramClasses(SCG.IEnumerable<String> ss)
    {
      IDictionary<CharBag, HashSet<String>> classes
        = new TreeDictionary<CharBag, HashSet<String>>();
      foreach (String s in ss)
      {
        CharBag anagram = AnagramClass(s);
        HashSet<String> anagramClass;
        if (!classes.Find(anagram, out anagramClass))
          classes[anagram] = anagramClass = new HashSet<String>();
        anagramClass.Add(s);
      }
      foreach (HashSet<String> anagramClass in classes.Values)
        if (anagramClass.Count > 1
      ) // && anagramClass.Exists(delegate(String s) { return !s.EndsWith("s"); }))
          yield return anagramClass;
    }
  }

// A bag of characters is represented as a sorted string of the
// characters, with multiplicity.  Since natural language words are
// short, the bags are small, so this is vastly better than
// representing character bags using HashBag<char> or TreeBag<char>

  class CharBag : IComparable<CharBag>
  {
    private readonly String contents; // The bag's characters, sorted, with multiplicity

    public CharBag(String s)
    {
      char[] chars = s.ToCharArray();
      Array.Sort(chars);
      this.contents = new String(chars);
    }

    public override int GetHashCode()
    {
      return contents.GetHashCode();
    }

    public bool Equals(CharBag that)
    {
      return this.contents.Equals(that.contents);
    }

    public int CompareTo(CharBag that)
    {
      return this.contents.CompareTo(that.contents);
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