// CS0186: Use of null is not valid in this context
// Line: 8

using System.Collections;

class ClassMain {    
	public static void Main() {
		foreach (System.Type type in (IEnumerable)null) {
		}                    
	}
}

