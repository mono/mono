// CS1690: Cannot call methods, properties, or indexers on `A.point' because it is a value type member of a marshal-by-reference class
// Line: 22
// Compiler options: -warn:1 -warnaserror

using System;

public struct Point
{
        public bool this [int i] { set { } }
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
        a.point [3] = false;
   }
}
