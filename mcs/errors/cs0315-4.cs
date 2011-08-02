// CS0315: The type `short' cannot be used as type parameter `T' in the generic type or method `A<T>'. There is no boxing conversion from `short' to `A<short>.N1<short>'
// Line: 4

public class A<T> where T : A<short>.N1<T>
{
    public class N1<U>
    {
    }
}