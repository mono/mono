// Test for custom modifiers (modreq, modopt)
// They become part of method signature
// test case from bug #80261

.assembly extern mscorlib
{
  .ver 1:0:5000:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 ) // .z\V.4..
}
.assembly 'property'
{
  .hash algorithm 0x00008004
  .ver  0:0:0:0
}
.module property.dll // GUID = {82691A71-F9F3-480E-96B0-09358B1EDA12}

  .class public auto ansi beforefieldinit TestMods
        extends [mscorlib]System.Object
  {
    .field static int32 modopt ([mscorlib] System.Int32) foo

    // method line 1
    .method public hidebysig  specialname  rtspecialname
           instance default void .ctor ()  cil managed
    {
        // Method begins at RVA 0x20ec
        // Code size 7 (0x7)
        .maxstack 8
        IL_0000:  ldarg.0
        IL_0001:  call instance void object::.ctor()
        IL_0006:  ret
    } // end of method TestMods::.ctor

    // method line 2
    .method public static  hidebysig  specialname
           default int32 modopt ([mscorlib]System.String)
           modreq ([mscorlib]System.ValueType) get_Prop ()  cil managed
    {
        // Method begins at RVA 0x20f4
        // Code size 3 (0x3)
        .maxstack 8
        IL_0000:  ldc.i4.s 0x0b
        IL_0002:  ret
    } // end of method TestMods::get_Prop

    // method line 3
    .method public static  hidebysig
           default int64 modopt ([mscorlib]System.GC)
           modreq ([mscorlib]System.Exception) 'method' (float64 d)  cil
managed
    {
        // Method begins at RVA 0x20f8
        // Code size 6 (0x6)
        .maxstack 8
        IL_0000:  ldarg.0
        IL_0001:  conv.i4
        IL_0002:  ldc.i4.s 0x0f
        IL_0004:  add
        IL_0005:  ret
    } // end of method TestMods::method

        .method private static  hidebysig default void Main ()  cil managed
        {
                .entrypoint
                ret
        }

        .property int32 Prop ()
        {
		.get default int32  
			modopt ([mscorlib]System.String)  
			modreq ([mscorlib]System.ValueType) 
			TestMods::get_Prop () 
        }
  } // end of class TestMods


