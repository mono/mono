class C <T> where T : new ()
{
}

class D <U> : C<U> where U : struct
{
}

class X
{
	public static void Main ()
        {
        }
}
