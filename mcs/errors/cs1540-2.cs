// CS1540: Cannot access protected member `A.X()' via a qualifier of type `B'. The qualifier must be of type `C' or derived from it
// Line: 21

class A
{
        protected virtual void X ()
        {
        }
}
 
class B : A
{
}
 
class C : A
{
        static B b = new B ();
 
        static void M ()
        {
                b.X ();
        }
}