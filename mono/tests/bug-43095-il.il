
// Metadata version: v4.0.30319
.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 4:0:0:0
}
.assembly 'bug-43095-il'
{
  .custom instance void [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = ( 01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
                                                                                                             63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01 )       // ceptionThrows.
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}
.module 'bug-43095.exe'
// MVID: {BBBBFE99-8258-45F1-89C3-C9A3AF34CC9B}
.imagebase 0x00400000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000001    //  ILONLY


// =============== CLASS MEMBERS DECLARATION ===================

.class public auto ansi beforefieldinit Bug43095
       extends [mscorlib]System.Object
{
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  } // end of method Bug43095::.ctor

  .method public hidebysig static void  Main(string[] args) cil managed
  {
    .entrypoint
    // Code size       11 (0xb)
    .maxstack  8
    IL_0000:  ldstr      "bla"
    IL_0005:  call       void Bug43095::ThrowArgumentNullException(string)
    IL_000a:  ret
  } // end of method Bug43095::Main

  .method public hidebysig static void  ThrowArgumentNullException(string a) cil managed noinlining
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  nop
    IL_0002:  nop
    IL_0003:  nop
    IL_0004:  nop
    IL_0005:  ldarg.0
    IL_0006:  newobj     instance void [mscorlib]System.ArgumentNullException::.ctor(string)
    IL_000b:  throw
  } // end of method Bug43095::ThrowArgumentNullException

} // end of class Bug43095


// =============================================================

// *********** DISASSEMBLY COMPLETE ***********************
