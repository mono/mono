using System;

public class TestAttribute : Attribute {
    Type type;

    public TestAttribute(Type type)
    {
        this.type = type;
    }

    public Type Type
    {
        get { return type; }
    }
}

[TestAttribute(typeof(void))]
public class Test {
    public static void Main()
    {
        object[] attrs =
            typeof(Test).GetCustomAttributes(typeof(TestAttribute), false);
        foreach (TestAttribute attr in attrs) {
            Console.WriteLine("TestAttribute({0})", attr.Type);
        }
    }
}
