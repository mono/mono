// cs0121.cs: Ambiguous call when selecting function due to implicit casts
// Line: 7

class X {
	static void Add (float f1, float f2, float f3) {}
	static void Add (params decimal [] ds) {}
	public static void Main () { Add (1, 2, 3); }
}
