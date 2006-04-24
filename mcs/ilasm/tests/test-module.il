//Test for module, there is no Assembly manifest in
//this source file

.assembly extern mscorlib
{
  .ver 2:0:0:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 ) // .z\V.4..
}
.module a.netmodule // GUID = {CFF03797-0272-4E67-86D4-8C7263E398A4}


  .class public auto ansi beforefieldinit A
  	extends [mscorlib]System.Object
  {

    // method line 1
    .method public static  hidebysig 
           default void foo ()  cil managed 
    {
        // Method begins at RVA 0x2050
	// Code size 2 (0x2)
	.maxstack 8
	IL_0000:  nop 
	IL_0001:  ret 
    } // end of method A::default void foo () 

    // method line 2
    .method public hidebysig  specialname  rtspecialname 
           instance default void .ctor ()  cil managed 
    {
        // Method begins at RVA 0x2053
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::.ctor()
	IL_0006:  ret 
    } // end of method A::instance default void .ctor () 

  } // end of class A

