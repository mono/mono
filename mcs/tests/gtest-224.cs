class Base
{
        public virtual void Foo<T> () {}
}

class Derived : Base
{
        public override void Foo <T> () {}
}

class Driver
{
        public static void Main ()
        {
                new Derived ().Foo<int> ();
        }
}

