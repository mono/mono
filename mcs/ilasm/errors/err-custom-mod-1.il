// Custom mods are part of method signature
// .get of property Prop doesn't have a complete
// signature as its missing the custom mods defined in
// get method

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
    .method public static  hidebysig  specialname
           default int32 modopt ([mscorlib]System.String)
           modreq ([mscorlib]System.ValueType) get_Prop ()  cil managed
    {
        .maxstack 8
        IL_0000:  ldc.i4.s 0x0b
        IL_0002:  ret
    }

        .property int32 Prop ()
        {
		.get default int32 TestMods::get_Prop () 
        }

  }
