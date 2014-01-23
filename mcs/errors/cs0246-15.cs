// CS0246: The type or namespace name `InvalidTypeBlah' could not be found. Are you missing an assembly reference?
// Line: 17

//
// This test is here to test that the compiler does not crash after it
// reports the error due to an invalid type being referenced in the 
// delegate
//
using System;

public class AnonDelegateTest 
{
        public delegate void TestDelegate(AnonDelegateTest something, string b);

        public static void Main()
        {
                AnonDelegateTest test = new AnonDelegateTest();
                
                // Incorrect; mcs throws unhandled exception here
                test.Call(delegate(InvalidTypeBlah something, string b) {
                        Console.WriteLine(b);
                });
        }

        public void Call(TestDelegate d)
        {
                d(this, "Hello");
        }
}


