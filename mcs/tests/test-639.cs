class Foo {
  bool got;
  string s {
    get { got = true; return ""; }
    set { if (!got || value != "A1B2") throw new System.Exception (); }
  }

  public static void Main ()
  {
    (new Foo ()).s += "A" + 1 + "B" + 2;
  }
}
