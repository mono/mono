// CS1540: Cannot access protected member `A.n()' via a qualifier of type `A'. The qualifier must be of type `B' or derived from it
// Line: 14

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



