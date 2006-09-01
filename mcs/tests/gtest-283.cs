public interface IFoo
{
        void Foo<T>(ref T? v) where T:struct;
        void Foo<T>(ref T v) where T:new();
}

public struct Point
{
        int x, y;
        public Point(int x, int y) { this.x = x; this.y = y; }
}

struct TestPoint
{
        public static void Serialize(IFoo h)
        {
		Point  point1 = new Point (0, 1);
                Point? point2 = new Point (1, 2);
		h.Foo (ref point1);
                h.Foo (ref point2);
        }
        public static void Main(){}
}
