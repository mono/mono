// This code must be compilable without any warning
// Compiler options: -warnaserror -warn:4

using System;

public class A {
	[Obsolete()]
	public virtual string Warning {
		get { return ""; }
	}
}

public class B : A {
	[Obsolete()]
	public override string Warning {
		get { return ""; }
	}
        
        public static void Main ()
        {
        }
}

