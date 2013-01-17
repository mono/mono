.assembly extern mscorlib
{
}

.assembly 'CS0108-8-lib'
{
}

.module 'CS0108-8-lib.dll'


.class public auto ansi beforefieldinit A
       extends [mscorlib]System.Object
{
  .class auto ansi nested family beforefieldinit AA
         extends [mscorlib]System.Object
  {
    .method public hidebysig newslot virtual 
            instance void  Foo() cil managed
    {
      ret
    }

    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      ldarg.0
      call       instance void [mscorlib]System.Object::.ctor()
      ret
    }

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
  .class auto ansi nested public beforefieldinit BB
         extends A/AA
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
      call       instance void A/AA::.ctor()
      ret
    }

  }

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    ldarg.0
    call       instance void A::.ctor()
    ret
  }

}
