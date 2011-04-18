// CS3005: Identifier `CLSClass.value' differing only in case is not CLS-compliant
// Line: 15
// Compiler options: -warnaserror

[assembly:System.CLSCompliant(true)]

public class X {
        public bool Value;
}

public class Y: X {
        private readonly bool vAalue;
}
    
public class CLSClass: Y {
        protected internal bool value;
}
