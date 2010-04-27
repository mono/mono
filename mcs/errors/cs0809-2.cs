// CS0809: Obsolete member `A.Filename' overrides non-obsolete member `Error.Filename'
// Line: 8
// Compiler options: -warnaserror

class A: Error {
	[System.ObsoleteAttribute ("Obsolete", true)]	
	public override string Filename {
		set {
		}
	}
	
	public static void Main () {}
}

public class Error {
	public virtual string Filename {
		set {
		}
		get {
			return "aa";
		}
	}
}

class B {
	void TT () {
		new A ().Filename = "Filename";
	}
}
