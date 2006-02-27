// Test for field initialization, for the syntax 
//   .. = int8 (value)

.assembly extern mscorlib
{
  .ver 2:0:0:0
}
.assembly 'test-field-init'
{
  .custom instance void [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = ( 01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
                                                                                                             63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01 )       // ceptionThrows.
  .ver 0:0:0:0
}
.module 'test-field-init.dll'

.class private auto ansi beforefieldinit test
       extends [mscorlib]System.Object
{
  .field public static literal int8 tf = int8(-3)
  .field public static literal uint8 tf = int8(3)
  
  .field public static literal int16 tf = int16(-3)
  .field public static literal uint16 tf = uint16(3)
  
  .field public static literal int32 tf = int32(-3)
  .field public static literal uint32 tf = uint32(3)

  .field public static literal int64 tf = int64(12345)
  .field public static literal uint64 tf = uint64(0xFFFFFFFFFFFFFFFF)

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  }

}
