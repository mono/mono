
// Test case from bug 75270

using System;

public interface I {
  void SetObject (string foo);
}

public class A {
  public virtual void SetObject (string foo) {
    Console.WriteLine ("A.SetObject {0}", foo);
  }
}

public class B : A, I {
  //public override void SetObject (string foo) {
  //Console.WriteLine ("B.SetObject {0}", foo);
  //}
}

public class C : B {
  public static bool ok = false;
  public override void SetObject (string foo) {
    Console.WriteLine ("C.SetObject {0}", foo);
    ok = true;
  }
}


public class X {
  public static int Main (string[] args) {
    I i = new C();

    // Tests that C.SetObject is called here
    i.SetObject ("hi");
    if (!C.ok)
	return 1;
    return 0;
  }
}

