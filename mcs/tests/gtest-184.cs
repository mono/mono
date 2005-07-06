class X {
      static void Main ()
      {
	string [] s = { "a", "b", "a" };
	System.Array.FindAll (s, delegate (string str) { return str == "a"; });
      }
}

