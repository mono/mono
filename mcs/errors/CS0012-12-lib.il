.assembly extern mscorlib
{
}

.assembly extern 'CS0012-lib-missing'
{
}

.assembly 'CS0012-12-lib'
{
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}

.module 'CS0012-12-lib.dll'

.class public auto ansi beforefieldinit Lib1.Class1`1<T>
       extends class ['CS0012-lib-missing']Lib2.Class1`1<class ['CS0012-lib-missing']Lib2.Class2>
{
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    IL_0000:  ldarg.0
    IL_0001:  call       instance void class ['CS0012-lib-missing']Lib2.Class1`1<class ['CS0012-lib-missing']Lib2.Class2>::.ctor()
    IL_0006:  ret
  }

}
