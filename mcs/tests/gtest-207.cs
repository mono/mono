// Note that this test actually checks if we compiled mscorlib.dll properly.

class M {
  static void p (string x) {
    System.Console.WriteLine (x);
  }

  public static void Main () {
    string[] arr = new string[] { "a", "b", "c" };
    System.Array.ForEach (arr, p);
  }
}
