// cs0108.cs: The new keyword is required on 'Bar.this[int, int]' because it hides inherited member
// Line: 15
// Compiler options: -warnaserror -warn:1

public class Foo
{
        public long this [int start, int count] {
                set {
                }
        }
}

public class Bar : Foo
{
        public virtual long this [int i, int length] {
                set {
                }
        }
}
