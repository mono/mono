// cs8200.cs: Do not allow type-parameter-constraint-clauses when
// there is no type-parameter list
//
using System.Collections;
class Dingus where T : IEnumerable {
}

class D {
	static void Main ()
	{
	}
}
