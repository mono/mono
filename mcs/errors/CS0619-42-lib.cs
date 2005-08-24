using System;

public class Error {
	[Obsolete ("NOT", true)]
	public virtual string Filename {
		set {
		}
		get {
			return "aa";
		}
	}
}