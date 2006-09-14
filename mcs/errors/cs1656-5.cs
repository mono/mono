// CS1656: Cannot assign to `Method_1' because it is a `method group'
// Line: 14

public class Test
{
	void Method_1 ()
	{
	}
	
        public static void Main ()
        {
		Test t = new Test ();
		
                t.Method_1 += delegate {  };
        }
}
