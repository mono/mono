using System;

class Repro
{
  private int[] stack = new int[1];
  private int cc;
  public int fc;
  private int sp;

  public static int Main()
  {
    Repro r = new Repro();
    r.foo();
    Console.WriteLine(r.stack[0]);
    return r.stack[0] == 42 ? 0 : 1;
  }

  public void foo()
  {
    fc = cc = bar();
    fc = stack[sp++] = cc;
  }

  private int bar()
  {
    return 42;
  }
}

