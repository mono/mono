.assembly extern mscorlib
{
}

.assembly extern 'CS0012-lib-missing'
{
}

.assembly 'CS0012-18-lib'
{
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}

.module 'CS0012-18-lib.dll'


.class public auto ansi beforefieldinit B
       extends [mscorlib]System.Object
{
  .field public static literal valuetype ['CS0012-lib-missing']E e = int32(0x00000001)

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    ret
  }

}

