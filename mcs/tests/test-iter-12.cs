class X {
	System.Collections.IEnumerable a ()
	{
		lock (this){
			yield return "a";
			yield return "b";
		}
	}

	public static void Main () {}
}
