.assembly extern mscorlib
{
}

.assembly extern 'missing-lib'
{
}

.assembly 'test-792-lib'
{
}

.module 'test-792-lib.dll'

.class public auto ansi beforefieldinit A
       extends [mscorlib]System.Attribute
{
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class [mscorlib]System.Type 'type') cil managed
  {
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Attribute::.ctor()
    IL_0006:  ret
  }
}

.class public auto ansi beforefieldinit X extends [mscorlib]System.Object
{
  .custom instance void A::.ctor(class [mscorlib]System.Type) = ( 01 00 44 58 58 2C 20 62 75 67 2D 31 2D 6C 69 62   // ..DXX, missing-lib
                                                                  2C 20 56 65 72 73 69 6F 6E 3D 30 2E 30 2E 30 2E   // , Version=0.0.0.
                                                                  30 2C 20 43 75 6C 74 75 72 65 3D 6E 65 75 74 72   // 0, Culture=neutr
                                                                  61 6C 2C 20 50 75 62 6C 69 63 4B 65 79 54 6F 6B   // al, PublicKeyTok
                                                                  65 6E 3D 6E 75 6C 6C 00 00 )                      // en=null..
  .custom instance void [mscorlib]System.ObsoleteAttribute::.ctor() = ( 01 00 00 00 ) 
  .field private class [mscorlib]System.Collections.Generic.List`1<valuetype ['missing-lib']XX> field2
  .field private class ['missing-lib']XX field3
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  }
}

.class public auto ansi beforefieldinit MultipleSameNames extends [mscorlib]System.Object
{
  .field private class [mscorlib]System.Int32 AA
  .field public class [mscorlib]System.String AA
  
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  }  
}

.class public auto ansi beforefieldinit Another extends ['missing-lib']XXBase
{
  .class auto ansi nested public beforefieldinit Nested extends [mscorlib]System.Object
  {
    .method public hidebysig specialname rtspecialname instance void  .ctor() cil managed
    {
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
      IL_0006:  ret
    }
  }

  .method public hidebysig specialname rtspecialname instance void  .ctor() cil managed
  {
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  }
}

.class public auto ansi beforefieldinit G`1<T> extends [mscorlib]System.Object
{
  .field public class ['missing-lib']Missing foo
  
  .method public hidebysig specialname rtspecialname instance void  .ctor() cil managed
  {
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  }
}


