// CS0219: The variable `o' is assigned but its value is never used
// Line: 10
// Compiler options: -warn:3 -warnaserror

public class MyClass2
{
	static public bool b;
        static public void Main ()
        {
                object o;
                switch (b) {
		case true:
			o = "yo";
			break;
                }
        }
}
