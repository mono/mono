interface IMember {
  int GetId ();
}

interface IMethod : IMember { }

class C1 : IMethod
{
  public int GetId () { return 42; }
}

class X {
    static void foo<a> (a e )
      where a : IMember
    {
      e.GetId ();
    }

  public static void Main ()
  {
    foo<IMethod> (new C1 ());
  }
}
