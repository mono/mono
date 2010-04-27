// cs0122-19.cs: `A.IFileWatcher' is inaccessible due to its protection level
// Line: 9
// Compiler options: -r:CS0122-19-lib.dll

namespace A {
	class C {
		public static void Main ()
		{
			IFileWatcher i;
		}
	}
}