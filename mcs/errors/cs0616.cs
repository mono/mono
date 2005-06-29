// cs0616.cs: `FakeAttribute': is not an attribute class
// Line: 8

class FakeAttribute {
}

class T {
	[Fake]
	static int Main() {
		return 0;
	}
}
