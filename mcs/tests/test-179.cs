using System;

class A {
        public object this[double x] {
                get { return 3*x; }
        }
}

class B : A {
        public new object this[double x] {
                get { return x + 100; }
        }
}

class C : B{
        public object this[string s] {
                get { return "hey:" + s; }
        }
        public object this[int x] {
                get { return x * 2; }
        }
}

struct EntryPoint {

        public static int Main (string[] args) {
                C test = new C();

		if (((double)test [333.333]) != 433.333)
			return 1;
		
		if (((string)test ["a string"]) != "hey:a string")
			return 2;

		if (((int)test [111]) != 222)
			return 3;

		System.Console.WriteLine ("Passes");
		return 0;
        }

}
