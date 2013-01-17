//
// Accessibility tests for NestedPrivate classes
//
class X {

	private class E {
	}
	
	private class D {

		private class P {
			//
			// Declares an field of a "parent" private class
			//
			E c;
			
		}
	}
}

class Y {
	private class Op {
		public D d;
	}

	private enum D {
	}
}

class R {
	public static void Main ()
	{
	}
}

