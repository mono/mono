.assembly extern mscorlib {}
.assembly 'test-531-lib' {}
.module 'test-531-lib.dll'
.imagebase 0x00400000
.subsystem 0x00000003
.file alignment 512
.corflags 0x00000001

.class public auto ansi beforefieldinit TestLib
       extends [mscorlib]System.Object
{
  .field static privatescope int32 FOO

  .method public hidebysig specialname static int32
          get_FOO() cil managed
  {
    ldsfld      int32 TestLib::FOO
    ret
  }

  .property int32 FOO()
  {
    .get int32 TestLib::get_FOO()
  }
}
