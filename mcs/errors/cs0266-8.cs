// CS0266: Cannot implicitly convert type `void*' to `int*'. An explicit conversion exists (are you missing a cast?)
// Line: 7
// Compiler options: -unsafe

unsafe class MainClass {
	static void *pv = null;
	static int *pi = pv;
        public static void Main () { }
}
