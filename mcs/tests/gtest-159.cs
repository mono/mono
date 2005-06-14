using System;
using System.Collections.Generic;

public class App {
  public static void Main() {
    Dictionary<string, int> values = new Dictionary<string, int>();
    values["one"] = 1; values["two"] = 2;

    foreach (string key in values.Keys) {
      System.Console.WriteLine("key: {0}", key);
    }
  }
}
