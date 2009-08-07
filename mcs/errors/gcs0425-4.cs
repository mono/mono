// CS0425: The constraints for type parameter `T' of method `Test.Baz.Method<T,V>()' must match the constraints for type parameter `T' of interface method `Test.IBar.Method<T,V>()'. Consider using an explicit interface implementation instead
// Line: 18
namespace Test
{
    using System;

    public interface IFoo
    {
    }

    public interface IBar
    {
        void Method<T, V>() where T : IFoo where V : T;
    }

    public class Baz : IBar
    {
        public void Method<T, V>() where T : IBar where V : T
        {
        }
    }
}
