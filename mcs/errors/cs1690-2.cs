// cs1690.cs: Cannot call methods, properties, or indexers on 'A.point' because it is a value type member of a marshal-by-reference class
// Line: 21

using System;

public struct Point
{
        public bool Error { get { return true; } }
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
        bool b = a.point.Error;
   }
}
