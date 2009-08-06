// Experiment: implicit conversion of indexer to function
// sestoft@dina.kvl.dk * 2005-11-08

using System;
using C5;

class MyFunTest {
  public static void Main(String[] args) {
    FooBar fb = new FooBar();
    IList<int> list = new LinkedList<int>();
    list.AddAll(new int[] { 2, 3, 5, 7, 11 });
    list.Map<double>(fb).Apply(Console.WriteLine);
    list.Apply(fb);
  }
}

class FooBar {
  public double this[int x] { 
    get { 
      Console.WriteLine(x); 
      return x + 1.5; 
    } 
  }

  public Fun<int,double> Fun {
    get { 
      return delegate(int x) { return this[x]; };
    }
  }

  public Act<int> Act {
    get { 
      return delegate(int x) { double junk = this[x]; };
    }
  }
  
  public static implicit operator Fun<int,double>(FooBar fb) {
    return delegate(int x) { return fb[x]; };
  }  

  public static implicit operator Act<int>(FooBar fb) {
    return delegate(int x) { double junk = fb[x]; };
  }  
}


