// CS0165: Use of unassigned local variable `fb'
// Line: 15
using System.Collections;

public class EntryPoint {
  public static void Main() {
    ArrayList fields = new ArrayList();

    Field fb;
    for (int i = 0; i < fields.Count; i++) {
      if (((Field) fields[i]).Name == "abc") {
        fb = (Field) fields[i];
	break;
      }
    }

    if (fb.Name != "b") {
    }
  }

  public class Field
  {
    public string Name;
  }
}

