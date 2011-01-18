// CS0012: The type `Lib2.Class1`1<Lib2.Class2>' is defined in an assembly that is not referenced. Consider adding a reference to assembly `CS0012-lib-missing, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
// Line: 5
// Compiler options: -r:CS0012-12-lib.dll

class Program : Lib1.Class1<int>
{
}
