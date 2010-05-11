.assembly extern wrong
{
  .ver 0:0:0:0
}
.assembly extern mscorlib
{
}

.assembly 'test-760-lib'
{
}
.module 'test-760-lib.dll'

.class private auto ansi beforefieldinit M
       extends [wrong]X
{
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [wrong]X::.ctor()
    IL_0006:  ret
  }
}

.class public auto ansi beforefieldinit C
       extends [mscorlib]System.Object
{
  .class auto ansi nested private beforefieldinit N
         extends [wrong]X
  {
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [wrong]X::.ctor()
      IL_0006:  ret
    }
  }

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  }
}

.class public auto ansi beforefieldinit D
       extends [mscorlib]System.Object
{
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  }
}
