using System;
using System.Threading;

enum A {
  Hello,
  Bye
}

class X {

	static void Main () {
		switch (0) {
		default:
		  throw new Exception("FOO");
		  break;
		}
	}
}

