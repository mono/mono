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

// C5 example: Keyword recognition 2004-12-20

// Compile with 
//   csc /r:C5.dll KeywordRecognition.cs 

using System;
using C5;
using SCG = System.Collections.Generic;

namespace KeywordRecognition {

class KeywordRecognition {
  // Array of 77 keywords:

  static readonly String[] keywordArray = 
    { "abstract", "as", "base", "bool", "break", "byte", "case", "catch",
      "char", "checked", "class", "const", "continue", "decimal", "default",
      "delegate", "do", "double", "else", "enum", "event", "explicit",
      "extern", "false", "finally", "fixed", "float", "for", "foreach",
      "goto", "if", "implicit", "in", "int", "interface", "internal", "is",
      "lock", "long", "namespace", "new", "null", "object", "operator",
      "out", "override", "params", "private", "protected", "public",
      "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof",
      "stackalloc", "static", "string", "struct", "switch", "this", "throw",
      "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe",
      "ushort", "using", "virtual", "void", "volatile", "while" };
  
  private static readonly ICollection<String> kw1;

  private static readonly ICollection<String> kw2;

  private static readonly ICollection<String> kw3;

  private static readonly SCG.IDictionary<String,bool> kw4 = 
    new SCG.Dictionary<String,bool>();


  class SC : SCG.IComparer<string>
  {
    public int Compare(string a, string b)
    {
      return StringComparer.InvariantCulture.Compare(a,b);
    }
  }

  class SH : SCG.IEqualityComparer<string>
  {
    public int GetHashCode(string item)
    {
      return item.GetHashCode();
    }

    public bool Equals(string i1, string i2)
    {
      return i1 == null ? i2 == null : i1.Equals(i2,StringComparison.InvariantCulture);
    }
  }

  static KeywordRecognition() { 
    kw1 = new HashSet<String>();
    kw1.AddAll<string>(keywordArray); 
    kw2 = new TreeSet<String>(new SC());
    kw2.AddAll<string>(keywordArray);
    kw3 = new SortedArray<String>(new SC());
    kw3.AddAll<string>(keywordArray);
    kw4 = new SCG.Dictionary<String,bool>();
    foreach (String keyword in keywordArray) 
      kw4.Add(keyword, false);
  }

  public static bool IsKeyword1(String s) {
    return kw1.Contains(s);
  }

  public static bool IsKeyword2(String s) {
    return kw2.Contains(s);
  }

  public static bool IsKeyword3(String s) {
    return kw3.Contains(s);
  }

  public static bool IsKeyword4(String s) { 
    return kw4.ContainsKey(s); 
  }

  public static bool IsKeyword5(String s) { 
    return Array.BinarySearch(keywordArray, s) >= 0; 
  }

  public static void Main(String[] args) {
    if (args.Length != 2) 
      Console.WriteLine("Usage: KeywordRecognition <iterations> <word>\n");
    else {
      int count = int.Parse(args[0]);
      String id = args[1];

      {
        Console.Write("HashSet.Contains ");
        Timer t = new Timer();
        for (int i=0; i<count; i++)
          IsKeyword1(id);
        Console.WriteLine(t.Check());      
      }

      {
        Console.Write("TreeSet.Contains ");
        Timer t = new Timer();
        for (int i=0; i<count; i++)
          IsKeyword2(id);
        Console.WriteLine(t.Check());      
      }

      {
        Console.Write("SortedArray.Contains ");
        Timer t = new Timer();
        for (int i=0; i<count; i++)
          IsKeyword3(id);
        Console.WriteLine(t.Check());      
      }

      {
        Console.Write("SCG.Dictionary.ContainsKey ");
        Timer t = new Timer();
        for (int i=0; i<count; i++)
          IsKeyword4(id);
        Console.WriteLine(t.Check());      
      }

      {
        Console.Write("Array.BinarySearch ");
        Timer t = new Timer();
        for (int i=0; i<count; i++)
          IsKeyword5(id);
        Console.WriteLine(t.Check());      
      }
    }
  }
}

// Crude timing utility ----------------------------------------
   
public class Timer {
  private DateTime start;

  public Timer() {
    start = DateTime.Now;
  }

  public double Check() {
    TimeSpan dur = DateTime.Now - start;
    return dur.TotalSeconds;
  }
}

}
