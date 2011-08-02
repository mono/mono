// CS0176: Static member `MyClass.Start(string)' cannot be accessed with an instance reference, qualify it with a type name instead
// Line: 10
using System;

class TestIt 
{
        public static void Main() 
        {
                MyClass p = new MyClass();
                p.Start ("hi");
        }
}

class MyClass
{
        public static void Start (string info) 
        {
        }
}
