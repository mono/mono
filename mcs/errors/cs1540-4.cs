class A
{
        protected void n () { }
}

class B : A
{
        public static void Main ()
	{
		A b = new A ();
		b.n ();
	}
}



