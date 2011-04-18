// CS3005: Identifier `CLSClass.this[int].set' differing only in case is not CLS-compliant
// Line: 8
// Compiler options: -warnaserror

[assembly:System.CLSCompliant(true)]

public class CLSClass {
        protected int SET_item;
        public int this[int index] { set {} }        
}
