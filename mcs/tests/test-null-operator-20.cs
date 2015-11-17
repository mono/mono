class M
{
	public static void Main ()
	{
		string s = null;
		s?.CompareTo ("xx").CompareTo(s?.EndsWith ("x")).GetHashCode ();

		string s1 = "abcd";
		string s2 = null;

		var idx = s1.Substring(1)[s2?.GetHashCode () ?? 0].GetHashCode ();
	}
}
