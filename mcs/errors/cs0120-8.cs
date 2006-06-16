// cs0120-8.cs: `Test.ArrayList': An object reference is required for the nonstatic field, method or property
// Line: 10

using System.Collections;

public class Test  {
	ArrayList ArrayList;

	public static void Main () {
		ArrayList.Capacity = 5;
	}
}
