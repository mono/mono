// CS0101: The namespace `AA.VV' already contains a definition for `SomeEnum'
// Line: 10
using System;

namespace AA {
	namespace VV {
		public enum SomeEnum {
			Something1,
			Something2
		}

		public enum SomeEnum {
			Dog,
			Fish,
			Cat
		}
	}
}
