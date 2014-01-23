using System;
class Foo {
  static int calls;
  static bool False { get { ++calls; return true; } }
  static void ping () { ++calls; }
  static int test_while (int n)
  {
    int i = 0;
    calls = 0;
    while (!(False & false)) {
      if (calls != ++i)
	throw new Exception ();
      if (calls == n)
	return 0;
    }
  }
  static int test_do_while (int n)
  {
    int i = 0;
    calls = 0;
    do {
      if (calls != i++)
	throw new Exception ();
      if (calls == n)
	return 0;
    } while (!(False & false));
  }
  static int test_for (int n)
  {
    int i = 2;
    calls = 0;
    for (bool dummy = False; !(False & false); ++i) {
      if (calls != i)
	throw new Exception ();
      if (calls == n)
	return 0;
    }
  }
  static void test_for_empty ()
  {
    calls = 0;
    for (ping (); False & false; )
      throw new Exception ();
    if (calls != 2)
      throw new Exception ();
  }

  public static void Main ()
  {
    test_while (100);
    test_do_while (100);
    test_for (100);
    test_for_empty ();
  }
}
