class A
{
        protected int n;
}
 
class B : A
{
}
 
class C : A
{
        static B b = new B ();
 
        static void Main ()
        {
                b.n = 1;
        }
}
