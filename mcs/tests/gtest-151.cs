class Test<T> where T: struct{
   public Test(){
      T s = new T();
   }
}

class X
{
	public static int Main ()
	{
		new Test<bool> ();
		return 0;
	}
}

