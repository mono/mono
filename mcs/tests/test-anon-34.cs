//
// This test is from Nigel Perry, Bugzilla #77060
//
// The issue here was that in the past we used to emit the
// Scope initialization on first use, which is wrong as illustrated
// in this test (the shared scope is not initialized for differnt
// code paths).
//
// This test is a running test, ensuring that it runs
//
#region Using directives

using System;
using System.Collections;
using System.Text;
using System.Timers;

#endregion

namespace Delegates
{ class Space
  { public int Value;

    public delegate void DoCopy();

    public DoCopy CopyIt;

    public void Leak(bool useArray, int max)
    { DoCopy one = null;

      if(useArray)
      { 
        int[] work;
        
        one = delegate 
              { work = new int[max];
              };
      }
      else
      { 
        one = delegate { int xans = (max + 1) * max / 2; };
      }

       Console.WriteLine("Here goes...");
	one();
	Console.WriteLine("... made it");
    }
  }

  class Program
  { static void SpaceLeak()
    { Space s = new Space();

      Console.WriteLine(s.Value);

      s.Leak(false, 1);

      Console.WriteLine(s.Value);
    }

    public static void Main(string[] args)
    { SpaceLeak();
    }
  }
}
