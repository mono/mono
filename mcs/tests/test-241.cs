//
// This test is for bug 57303
//
// Access via a base-instance to a protected method is allowed if we are a nested class
//
using System;

public class Foo {
  protected virtual int SomeProperty {
    get { return 10; }
  }

  private class FooPrivate : Foo {
    Foo _realFoo;

    internal FooPrivate(Foo f) {
      _realFoo = f;
    }

    protected override int SomeProperty {
      get { return
      this._realFoo.SomeProperty;
      }
    }
  }

  public static void Main () { }
}
