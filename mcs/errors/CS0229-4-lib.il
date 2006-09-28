.assembly extern mscorlib {}
.assembly 'CS0229-4-lib' {}
.module 'CS0229-4-lib.dll'
.imagebase 0x00400000
.subsystem 0x00000003
.file alignment 512
.corflags 0x00000001

.class public auto ansi beforefieldinit TestLib
       extends [mscorlib]System.Object
{
  .field static public int32 FOO

  .method public hidebysig static int32
          FOO() cil managed
  {
    ldsfld      int32 TestLib::FOO
    ret
  }
}
