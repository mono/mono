// cs3001-4.cs: Argument type 'out IError' is not CLS-compliant
// Line: 12

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