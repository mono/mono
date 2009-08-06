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

// C5 example: various tests 2005 and later

// Compile with 
//   csc /r:C5.dll Try.cs 

using System;
using System.Text;
using C5;
using SCG = System.Collections.Generic;

namespace Try
{
  class MyTest
  {
    public static void Main(String[] args)
    {
      SCG.IEqualityComparer<int> natural = EquatableEqualityComparer<int>.Default;
      SCG.IEqualityComparer<int> c5comp = EqualityComparer<int>.Default;
      int count = int.Parse(args[0]);
      {
        bool res = false;
        Timer t = new Timer();
        for (int i = count; i > 0; i--)
        {
          res = natural.Equals(4, 5);
        }
        Console.WriteLine("Time = {0} ns/comparison\n", t.Check() * 1E9 / count);
      }
      {
        bool res = false;
        Timer t = new Timer();
        for (int i = count; i > 0; i--)
        {
          res = c5comp.Equals(4, 5);
        }
        Console.WriteLine("Time = {0} ns/comparison\n", t.Check() * 1E9 / count);
      }
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
