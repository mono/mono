.assembly extern mscorlib
{
}

.assembly extern System.Core
{
}

.assembly extern 'CS0012-lib-missing'
{
}

.assembly 'CS0012-23-lib'
{
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}

.module 'CS0012-23-lib.dll'


.class public abstract auto ansi sealed beforefieldinit B
       extends [mscorlib]System.Object
{
  .field  public static valuetype ['CS0012-lib-missing']Struct`1<int32>[] ArrayMember
}
