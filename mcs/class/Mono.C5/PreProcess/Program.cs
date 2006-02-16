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

using System;
using System.Text;
using System.IO;

namespace PreProcess
{
  class Program
  {
    static void preprocess(string dir, string filein, string fileout, string symbol, string classin, string classout)
    {
      fileout = Path.Combine(dir, fileout);
      string[] contents = File.ReadAllLines(Path.Combine(dir, filein));
      string symboldef = "#define " + symbol + "not";
      bool equal = File.Exists(fileout);
      TextReader oldversion = equal ? new StreamReader(fileout) : null;
      for (int lineno = 0; lineno < contents.Length; lineno++)
      {
        if (contents[lineno].StartsWith(symboldef))
          contents[lineno] = "#define " + symbol;
        else
          contents[lineno] = contents[lineno].Replace(classin, classout);
        if (equal)
          equal = contents[lineno] == oldversion.ReadLine();
      }
      if (equal && oldversion.ReadLine() == null)
      {
        Console.Error.WriteLine("File {0} is up-to-date", fileout);
        return;
      }
      File.WriteAllLines(fileout + "-new", contents);
      if (oldversion != null)
      {
        oldversion.Close();
        File.Replace(fileout + "-new", fileout, fileout + ".bak");
        Console.Error.WriteLine("Updated {0}", fileout);
      }
      else
      {
        File.Move(fileout + "-new", fileout);
        Console.Error.WriteLine("Created {0}", fileout);
      }
    }
    static void Main(string[] args)
    {
      System.Environment.CurrentDirectory = @"..\..\..\C5";
      preprocess("trees", "RedBlackTreeSet.cs", "RedBlackTreeBag.cs", "BAG", "TreeSet", "TreeBag");
      preprocess("arrays", "ArrayList.cs", "HashedArrayList.cs", "HASHINDEX", "ArrayList", "HashedArrayList");
      preprocess("linkedlists", "LinkedList.cs", "HashedLinkedList.cs", "HASHINDEX", "LinkedList", "HashedLinkedList");
    }
  }
}
