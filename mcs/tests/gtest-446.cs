using System;

namespace BugTest
{
	class Bug<T> where T : new ()
	{
		public void CreateObject (out T param)
		{
			param = new T ();
		}
	}

	static class Program
	{
		public static int Main ()
		{
			Bug<object> bug = new Bug<object> ();
			object test;
			bug.CreateObject (out test);
			return 0;
		}
	}
}
