using System.Collections.Generic;
using System.Collections.ObjectModel;

public class MyCollection<T> : Collection<T>
{
    public void Foo()
    {
        T t = Items[0];
    }
}

public class C
{
    public static void Main () {}
}