using System;
[assembly: CLSCompliantAttribute (true)]

public class CLSClass_A {
	[CLSCompliantAttribute(true)]
	virtual public bool Universal {
            get {
		return false;
            }
        }
}