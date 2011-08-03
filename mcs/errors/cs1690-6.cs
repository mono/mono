// CS1690: Cannot call methods, properties, or indexers on `A.point' because it is a value type member of a marshal-by-reference class
// Line: 27
// Compiler options: -warn:1 -warnaserror

using System;

public struct Coord 
{
        public int val; 
}

public struct Point {
	public Coord x;
	public Coord y;
}

public class A : MarshalByRefObject
{
   public Point point = new Point ();
}

public class Test
{
   public static void Main ()
   {
        A a = new A ();
        Console.WriteLine (a.point.x.val);
   }
}
