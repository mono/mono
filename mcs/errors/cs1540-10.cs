// CS1540: Cannot access protected member `Test.A.Property' via a qualifier of type `Test.A'. The qualifier must be of type `Test.B.C' or derived from it
// Line: 19

namespace Test
{
    public class A
    {
        protected int Property {
            get { return 0; }
        }
    }
 
    public class B : A
    {
        private sealed class C
        {
            public C (A a)
            {
                int test = a.Property;
                test++;
            }
        }
    } 
}