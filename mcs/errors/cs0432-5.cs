// CS0432: Alias `BB' not found
// Line: 13

namespace A
{
	using BB = System.Collections.Generic;
}

namespace A.B
{
	class X
	{
		BB::List<int> p;
	}
}
