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

// C5 example: File index: read a text file, build and print a list of
// words and the line numbers (without duplicates) on which they occur.

// Compile with 
//   csc /r:C5.dll Fileindex.cs 

using System;                           // Console
using System.IO;                        // StreamReader, TextReader
using System.Text.RegularExpressions;   // Regex
using C5;                               // IDictionary, TreeDictionary, TreeSet

namespace FileIndex
{
  class Fileindex
  {
    static void Main(String[] args)
    {
      if (args.Length != 1)
        Console.WriteLine("Usage: Fileindex <filename>\n");
      else
      {
        IDictionary<String, TreeSet<int>> index = IndexFile(args[0]);
        PrintIndex(index);
      }
    }

    static IDictionary<String, TreeSet<int>> IndexFile(String filename)
    {
      IDictionary<String, TreeSet<int>> index = new TreeDictionary<String, TreeSet<int>>();
      Regex delim = new Regex("[^a-zA-Z0-9]+");
      using (TextReader rd = new StreamReader(filename))
      {
        int lineno = 0;
        for (String line = rd.ReadLine(); line != null; line = rd.ReadLine())
        {
          String[] res = delim.Split(line);
          lineno++;
          foreach (String s in res)
            if (s != "")
            {
              if (!index.Contains(s))
                index[s] = new TreeSet<int>();
              index[s].Add(lineno);
            }
        }
      }
      return index;
    }

    static void PrintIndex(IDictionary<String, TreeSet<int>> index)
    {
      foreach (String word in index.Keys)
      {
        Console.Write("{0}: ", word);
        foreach (int ln in index[word])
          Console.Write("{0} ", ln);
        Console.WriteLine();
      }
    }
  }
}