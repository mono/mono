
class A {
	double d1,d2;
	public double this[double x] {
		set {
			d1 = x;
			d2 = value;
		}
		get {
			if (d1 == x) {
				return d2;
			}
			return 0.0;
		}
	}
}

class B : A {
	double d1,d2;
	public new double this[double x] {
		set {
			d1 = x;
			d2 = value;
		}
		get {
			if (d1 == x) {
				return d2;
			}
			return 0.0;
		}
	}
}

class C : B{
	string s1,s2;
	int i1,i2;
	public string this[string x] {
		set {
			s1 = x;
			s2 = value;
		}
		get {
			if (s1 == x) {
				return s2;
			}
			return "";
		}
	}
	public int this[int x] {
		set {
			i1 = x;
			i2 = value;
		}
		get {
			if (i1 == x) {
				return i2;
			}
			return 0;
		}
	}
}

struct EntryPoint {

	public static int Main (string[] args) {
		C test = new C();

		test[333.333] = 444.444;
		if (test[333.333] != 444.444)
			return 1;

		test["a string"] = "another string";
		if (test["a string"] != "another string")
			return 2;

		test[111] = 222;
		if (test[111] != 222)
			return 3;

		System.Console.WriteLine ("Passes");
		return 0;
	}

}
