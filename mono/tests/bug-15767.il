.assembly extern 'bug-15767-missing'
{
  .ver 0:0:0:0
}
.assembly extern mscorlib
{
  .ver 4:0:0:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 ) // .z\V.4..
}
.assembly 'bug-15767'
{
  .custom instance void class [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::'.ctor'() =  (
		01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
		63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01       ) // ceptionThrows.

  .hash algorithm 0x00008004
  .ver  0:0:0:0
}
.module 'bug-15767.exe' // GUID = {302A3718-7391-4331-B3C9-E973D4F62FB0}


  .class private auto ansi beforefieldinit H`1<T>
  	extends [mscorlib]System.Object
  {

    // method line 1
    .method public hidebysig specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x2050
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  ret 
    } // end of method H`1::.ctor

  } // end of class H`1

  .class private auto ansi beforefieldinit C
  	extends [mscorlib]System.Object
  {
    .field  public  class ['bug-15767-missing']M missing
    .field  public  class ['bug-15767-missing']M[] missing_array
    .field  public  class H`1<class ['bug-15767-missing']M> missing_generic_param
    .field  public  int32 'field'

    // method line 2
    .method public hidebysig specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x2058
	// Code size 15 (0xf)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  ldarg.0 
	IL_0007:  ldc.i4.s 0x2a
	IL_0009:  stfld int32 C::'field'
	IL_000e:  ret 
    } // end of method C::.ctor

    // method line 3
    .method public hidebysig 
           instance default void bar (class ['bug-15767-missing']M missing)  cil managed 
    {
        // Method begins at RVA 0x2068
	// Code size 1 (0x1)
	.maxstack 8
	IL_0000:  ret 
    } // end of method C::bar

  } // end of class C

  .class private auto ansi beforefieldinit D
  	extends [mscorlib]System.Object
  {
    .field  public  class ['bug-15767-missing']M missing

    // method line 4
    .method public hidebysig specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x206a
	// Code size 14 (0xe)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  ldarg.0 
	IL_0007:  ldnull 
	IL_0008:  stfld class ['bug-15767-missing']M D::missing
	IL_000d:  ret 
    } // end of method D::.ctor

  } // end of class D

  .class private auto ansi beforefieldinit T
  	extends [mscorlib]System.Object
  {
    .field  public  class ['bug-15767-missing']M missing

    // method line 5
    .method public hidebysig specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x2079
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  ret 
    } // end of method T::.ctor

    // method line 6
    .method public static hidebysig 
           default void bar (class C c, class ['bug-15767-missing']M missing)  cil managed 
    {
        // Method begins at RVA 0x2081
	// Code size 8 (0x8)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldarg.1 
	IL_0002:  callvirt instance void class C::bar(class ['bug-15767-missing']M)
	IL_0007:  ret 
    } // end of method T::bar

    // method line 7
    .method private static hidebysig 
           default int32 Main (string[] args)  cil managed 
    {
        // Method begins at RVA 0x208c
	.entrypoint
	// Code size 50 (0x32)
	.maxstack 2
	.locals init (
		class C	V_0,
		class D	V_1,
		int32	V_2,
		class [mscorlib]System.Exception	V_3,
		class [mscorlib]System.Exception	V_4)
	IL_0000:  newobj instance void class C::'.ctor'()
	IL_0005:  stloc.0 
	.try { // 0
	  IL_0006:  newobj instance void class D::'.ctor'()
	  IL_000b:  stloc.1 
	  IL_000c:  ldc.i4.2 
	  IL_000d:  stloc.2 
	  IL_000e:  leave IL_0030

	} // end .try 0
	catch class [mscorlib]System.Exception { // 0
	  IL_0013:  stloc.3 
	  IL_0014:  leave IL_0019

	} // end handler 0
	.try { // 1
	  IL_0019:  ldloc.0 
	  IL_001a:  ldnull 
	  IL_001b:  call void class T::bar(class C, class ['bug-15767-missing']M)
	  IL_0020:  ldc.i4.1 
	  IL_0021:  stloc.2 
	  IL_0022:  leave IL_0030

	} // end .try 1
	catch class [mscorlib]System.Exception { // 1
	  IL_0027:  stloc.s 4
	  IL_0029:  leave IL_002e

	} // end handler 1
	IL_002e:  ldc.i4.0 
	IL_002f:  ret 
	IL_0030:  ldloc.2 
	IL_0031:  ret 
    } // end of method T::Main

  } // end of class T

