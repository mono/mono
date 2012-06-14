.assembly extern mscorlib
{
}

.assembly 'test-846-lib'
{
  .hash algorithm 0x00008004
}

.module 'test-846-lib.dll'

.class private auto ansi beforefieldinit A
       extends [mscorlib]System.Object
{
  .method public hidebysig newslot virtual 
          instance void  Foo() cil managed
  {
    newobj     instance void [mscorlib]System.ApplicationException::.ctor()
    throw
  }

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    ldarg.0
    call       instance void [mscorlib]System.Object::.ctor()
    ret
  }

}

.class public auto ansi beforefieldinit B
       extends A
{
  .method public hidebysig virtual instance void 
          Foo() cil managed
  {
    ret
  }

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    ldarg.0
    call       instance void A::.ctor()
    ret
  }
}
