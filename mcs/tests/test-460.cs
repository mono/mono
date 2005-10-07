// Compiler options: -unsafe

unsafe class MainClass {
	static void *pv = null;
	static int *pi = (int *) pv;
        public static void Main () { }
}

