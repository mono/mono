// CS0626: `ExternClass.implicit operator ExternClass(byte)' is marked as an external but has no DllImport attribute. Consider adding a DllImport attribute to specify the external implementation
// Line: 6
// Compiler options: -warnaserror -warn:1

class ExternClass {
        static public extern implicit operator ExternClass(byte value);
}
