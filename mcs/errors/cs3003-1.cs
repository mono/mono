// CS3003: Type of `CLSClass.MyEvent' is not CLS-compliant
// Line: 8
// Compiler options: -warnaserror -warn:1

[assembly:System.CLSCompliant (true)]

[System.CLSCompliant (false)]
public delegate void MyDelegate ();

public class CLSClass {
        public event MyDelegate MyEvent;
}
