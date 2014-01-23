using System;
using System.Threading;

enum A {
  Hello,
  Bye
}

class X {

	public static int Main () {
		try {
			switch (0) {
			default:
			  throw new Exception("FOO");
			  break;
			}
		} catch (Exception) {
			return 0;
		}
		
		return 1;
	}
}

