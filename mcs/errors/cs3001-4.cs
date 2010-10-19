// CS3001: Argument type `IError' is not CLS-compliant
// Line: 13
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant(true)]

[CLSCompliant(false)]
public interface IError{
}

public interface I {
        void Error(out IError arg);
}

public class c {
        public void Error (out IError arg) { arg = null; }
}
