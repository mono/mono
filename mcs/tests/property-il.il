.assembly extern mscorlib
{
  .ver 1:0:5000:0
}
.assembly 'property-il'
{
  .hash algorithm 0x00008004
  .ver  0:0:0:0
}
.module 'property-il.dll' // GUID = {CACC88BA-6ED4-45E0-8E59-C3ABEBA9753A}


  .class public auto ansi beforefieldinit 'Foo'
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
	IL_0001:  call instance void valuetype [mscorlib]'System.Object'::.ctor()
	IL_0006:  ret 
    } // end of method Foo::instance default void .ctor () 

    // method line 2
    .method public virtual  hidebysig  newslot  specialname 
           instance default string 'Monkey_Jump' ()  cil managed 
    {
        // Method begins at RVA 0x20f4
	// Code size 6 (0x6)
	.maxstack 8
	IL_0000:  ldstr "foo"
	IL_0005:  ret 
    } // end of method Foo::instance default string 'Monkey_Jump' () 

	.property specialname rtspecialname string Message ()
	{
		.get instance default string 'Foo'::'Monkey_Jump' () 
	}
  } // end of type Foo

  .class public auto ansi beforefieldinit 'Bar'
  	extends Foo
  {

    // method line 3
    .method public hidebysig  specialname  rtspecialname 
           instance default void .ctor ()  cil managed 
    {
        // Method begins at RVA 0x20fb
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void class 'Foo'::.ctor()
	IL_0006:  ret 
    } // end of method Bar::instance default void .ctor () 

    // method line 4
    .method public virtual  hidebysig  specialname 
           instance default string 'Boston' ()  cil managed 
    {
        // Method begins at RVA 0x2103
	// Code size 6 (0x6)
	.maxstack 8
	IL_0000:  ldstr "bar"
	IL_0005:  ret 
    } // end of method Bar::instance default string 'Boston' () 

	.property specialname rtspecialname string Message ()
	{
		.get instance default string 'Bar'::'Boston' () 
	}
  } // end of type Bar
  
.class public auto ansi beforefieldinit C
       extends [mscorlib]System.Object
{
  .method public hidebysig specialname instance int32 
          get_Value() cil managed
  {
    .maxstack  1
    .locals init (int32 V_0)
    IL_0000:  ldc.i4.3
    IL_0001:  stloc.0
    IL_0002:  br.s       IL_0004

    IL_0004:  ldloc.0
    IL_0005:  ret
  }

  .method public hidebysig specialname instance void 
          add_Arg(bool arg) cil managed
  {
    .maxstack  0
    IL_0000:  ret
  }

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    .maxstack  1
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  }

}
  
