//nested classes 'nested_private' and 'nested_public' don't have
//nested visibility set. ilasm should it accordingly.

.assembly extern mscorlib
{
  .ver 1:0:5000:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 ) // .z\V.4..
}
.assembly 'nested'
{
  .hash algorithm 0x00008004
  .ver  0:0:0:0
}
.module nested.dll // GUID = {3C1BE155-805F-4170-ACEB-D97A86B467A2}


  .class private auto ansi beforefieldinit outer
  	extends [mscorlib]System.Object
  {

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
    } // end of method outer::instance default void .ctor () 

  .class auto ansi beforefieldinit nested_private
  	extends [mscorlib]System.Object
  {

    // method line 2
    .method public hidebysig  specialname  rtspecialname 
           instance default void .ctor ()  cil managed 
    {
        // Method begins at RVA 0x20f4
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::.ctor()
	IL_0006:  ret 
    } // end of method nested_private::instance default void .ctor () 

  } // end of class nested_private

  .class public auto ansi beforefieldinit nested_public
  	extends [mscorlib]System.Object
  {

    // method line 3
    .method public hidebysig  specialname  rtspecialname 
           instance default void .ctor ()  cil managed 
    {
        // Method begins at RVA 0x20fc
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::.ctor()
	IL_0006:  ret 
    } // end of method nested_public::instance default void .ctor () 

  } // end of class nested_public

  } // end of class outer

