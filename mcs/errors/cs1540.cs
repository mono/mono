class A
{
        protected int n;
}

class B : A
{
        public static void Main ()
	{
		A b = new A ();
		b.n = 1;
	}
}
