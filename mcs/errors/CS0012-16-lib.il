.assembly extern mscorlib
{
}

.assembly extern 'CS0012-lib-missing'
{
}

.assembly 'CS0012-16-lib'
{
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}

.module 'CS0012-16-lib.dll'

.class public auto ansi beforefieldinit B
       extends [mscorlib]System.Object
{
  .method public hidebysig static void  Foo(object o) cil managed
  {
    ret
  }

  .method public hidebysig static void  Foo(class ['CS0012-lib-missing']AA t) cil managed
  {
    ret
  }

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    ret
  }

}
