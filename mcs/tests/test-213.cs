using System;

class MyTest {
	public static void Main(String[] args) {
		S s1 = new S(11);
		I s2 = s1;                          // Implicit boxing S-->I
		S s3 = (S)s2;                       // Explicit unboxing I-->S
		s3.Print();                         // Should print 11, does not
	}
}

interface I {
	void Print();
}

struct S : I {
	public int i;
	public S(int i) { 
		this.i = i;
	}
	public void Print() {
		Console.WriteLine(i);
	}
}
