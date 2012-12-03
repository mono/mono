// test for bug #56774

class T {
	public static int Main () {
		return X (1);
	}
  
	static int X (byte x) {
		return 0;
	}
	static int X (short x) {
		return 1;
	}
}
