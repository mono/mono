// Testcase for bug #59980

namespace Test {
	public abstract class C0 {
		public abstract int foo { get; }
	}
	public abstract class C1 : C0 {
	}
	public class C2 : C1 {
		public override int foo { get { return 0; } }
	}
}
