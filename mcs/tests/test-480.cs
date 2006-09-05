//
// This is a build test: checks that the compiler does not loop
// forever endlessly with anonymous methods
//
using System;
using System.Collections;
using System.Text;

  class Space
  { public int Value = -1;

    public delegate void DoCopy();

    public DoCopy CopyIt;

    public void Leak(bool useArray, int max)
    { DoCopy one;

      { int answer = 0;
        int[] work;
        
        CopyIt = delegate { Value = answer; };
        one = delegate 
              { work = new int[max];
                foreach(int x in work) answer += x;
              };
      }

      one();
    }
  }

  class Program
  { 
    static void Main(string[] args)
    {
    }
  }
