// cs0619-41.cs: `A.Filename' is obsolete: `Obsolete'
// Line: 8

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