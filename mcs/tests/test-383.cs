using System;

public class X
{
	public readonly int Data;

        public X testme (out int x)
	{
                x = 1;
		return this;
        }

        public X ()
	{
                int x, y;

                y = this.testme (out x).Data;
                Console.WriteLine("X is {0}", x);
        }

        public static void Main ()
	{
                X x = new X ();
        }
}
