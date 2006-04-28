class Base
{
    public int Property { get { return 42; } }
    static void Main () {}
}

class Derived : Base
{
    public int get_Property() { return 42; }
}