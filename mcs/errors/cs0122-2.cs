// CS0122: `A.prop' is inaccessible due to its protection level
// Line: 19
// Compiler options: -t:library

class A
{
        int i;

        int prop
        {
                set { i = value; }
        }
}

class B : A
{
        void M ()
        {
                prop = 2;
        }
}
