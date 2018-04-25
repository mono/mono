.assembly bz16628.typeforwarder2 {}
.assembly extern mscorlib {}
.assembly extern bz16628.typeforwardee2 {}

.class extern public forwarder NSwForwardee2.Foo_SPECIAL{
	.assembly extern bz16628.typeforwardee2
}

.class public auto ansi beforefieldinit NSwForwardee2.Bar_SPECIAL
       extends [mscorlib]System.Object
{
  .field public static int32 A
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  } // end of method Foo::.ctor

  .method private hidebysig specialname rtspecialname static 
          void  .cctor() cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldc.i4.s   320
    IL_0002:  stsfld     int32 NSwForwardee2.Bar_SPECIAL::A
    IL_0007:  ret
  } // end of method Foo::.cctor
} 

