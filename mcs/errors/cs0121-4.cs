// CS0121: The call is ambiguous between the following methods or properties: `X.Add(float, float, float)' and `X.Add(params decimal[])'
// Line: 7

class X {
	static void Add (float f1, float f2, float f3) {}
	static void Add (params decimal [] ds) {}
	public static void Main () { Add (1, 2, 3); }
}
