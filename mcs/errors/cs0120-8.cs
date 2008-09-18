// CS0120: An object reference is required to access non-static member `Test.ArrayList'
// Line: 10

using System.Collections;

public class Test  {
	ArrayList ArrayList;

	public static void Main () {
		ArrayList.Capacity = 5;
	}
}
