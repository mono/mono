// CS0266: Cannot implicitly convert type `long' to `uint'. An explicit conversion exists (are you missing a cast?)
// Line: 7

namespace MWFTestApplication {
	class MainWindow {
		public enum Testme : uint {
			value   = (1L << 1)
		}
	}
}
