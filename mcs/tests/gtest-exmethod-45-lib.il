.assembly extern mscorlib
{
}

.assembly 'gtest-exmethod-45-lib'
{
  .hash algorithm 0x00008004
}

.module 'gtest-exmethod-45-lib.dll'

.class public abstract auto F
{
  .custom instance void class [mscorlib]System.Runtime.CompilerServices.ExtensionAttribute::'.ctor'() =  (01 00 00 00 )  

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    ldarg.0
    call       instance void [mscorlib]System.Object::.ctor()
    ret
  }

  .method public hidebysig static void TestExt(string s) cil managed
  {
    .custom instance void class [mscorlib]System.Runtime.CompilerServices.ExtensionAttribute::'.ctor'() =  (01 00 00 00 )  
    
    ret
  }
}
