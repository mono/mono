// cs0626.cs: Method, operator, or accessor 'ExternClass.implicit operator ExternClass(byte)' is marked external and has no attributes on it. Consider adding a DllImport attribute to specify the external implementation
// Line: 6
// Compiler options: -warnaserror -warn:1

class ExternClass {
        static public extern implicit operator ExternClass(byte value);
}
