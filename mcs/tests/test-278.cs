using System;

struct Rect {
        int x;

        public int X { get { return x; } set { x = value; } }
}

class X {
        static int Main ()
        {
                Rect rect = new Rect ();
                rect.X += 20;
                Console.WriteLine ("Should be 20: " + rect.X);
                return rect.X == 20 ? 0 : 1;
        }
}
