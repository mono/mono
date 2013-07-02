namespace Test {
    class Cache<T> where T : class {
    }

    class Base {
    }

    class MyType<T> where T : Base {
        Cache<T> _cache;   // CS0452
    }

    class Foo { public static void Main () { object foo = new MyType<Base> (); } }
}
