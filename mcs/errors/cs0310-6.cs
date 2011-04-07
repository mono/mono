// CS0310: The type `Class1' must have a public parameterless constructor in order to use it as parameter `T' in the generic type or method `Class3<T>'
// Line: 18

public class Class1
{
	public Class1 (int i) { }
}

public class Class2<T>
{
}

public class Class3<T> where T : new ()
{
}


class A : Class2<Class3<Class1>>
{
}

