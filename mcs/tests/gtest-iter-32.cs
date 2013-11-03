using System.Collections.Generic;

class TestGoto
{
  static int x = 2;

  public static void Main(string[] args)
    {
      foreach (bool b in test())
	;
      if (x != 0)
	throw new System.Exception ();
    }

  static IEnumerable<bool> setX()
  {
    x = 1;
    try {
      yield return true;
    } finally {
      x = 0;
    }
  }

  static IEnumerable<bool> test()
  {
    foreach (bool b in setX()) {
      yield return true;
      // Change "goto label" to "break" to show the correct result.
      goto label;
    }
  label:
    yield break;
  }
}
