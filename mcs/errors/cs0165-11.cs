// CS0165: Use of unassigned local variable `fb'
// Line: 12
using System.Collections;

public class EntryPoint {
  public static void Main() {
    ArrayList fields = new ArrayList();

    Field fb;
    while (fields.Count > 0) {
      fb = (Field) fields[0];
    }

    if (fb.Name != "b") {
      System.Console.WriteLine ("shouldn't compile here.");
    }
  }

  public class Field
  {
    public string Name;
  }
}

