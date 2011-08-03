// CS0108: `Bar.this[int, int]' hides inherited member `Foo.this[int, int]'. Use the new keyword if hiding was intended
// Line: 15
// Compiler options: -warnaserror -warn:2

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
