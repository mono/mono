//
// This test exposed a bug that Dan found:
//
// The InnerBase used to be the `builder' that was passed to InnerBase,
// so even if InnerBase was a toplevel, it would be defined in the context
// of being nested.  Buggy.
//
class Outer {
	class Inner : InnerBase {
	}
}

abstract class InnerBase {
}

class MainClass {
	public static int Main () {
		return 0;
	}
}
