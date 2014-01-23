using System;

class ShortCircuitFold {
  static int calls;
  static bool False { get { ++calls; return false; } }
  static bool True { get { ++calls; return true; } }
  static void a (bool e, bool v) { if (e != v) throw new Exception ("unexpected value"); }
  static void c (int e) { if (e != calls) throw new Exception ("call count mismatch: expected " + e + " but got " + calls); }
  static bool f () { throw new Exception ("not short circuited out"); }
  public static void Main ()
  {
    // short circuit out f ()
    a (false, false && f ());
    a (true,  true || f ());

    // short circuit out side effects
    a (false, false && False); c (0);
    a (true,  true || True);   c (0);

    // ensure all side effects occur
    a (false, true && False);  c (1);
    a (true,  false || True);  c (2);

    a (false, false & False);  c (3);
    a (true,  true | True);    c (4);

    a (false, true & False);   c (5);
    a (true,  false | True);   c (6);
  }
}
