using System;
using N1;
using N2;

namespace N1.Derived {
	class Dummy {}
}

namespace N2.Derived {
	class Dummy {}
}

public class DerivedAttribute : Attribute {
}

[Derived ()]
class T {
	static void Main () {}
}
