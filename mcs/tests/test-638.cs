using System;
using System.Threading;

class Fail {
  public static void Main () {
    string a = "";
    a += 0 + "A" + 1 + "B" + 2;
    EventHandler t = delegate {
      if (a != "0A1B2")
	throw new Exception ();
    };
    t (null, null);
  }
}
