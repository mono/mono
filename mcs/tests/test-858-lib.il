.assembly extern mscorlib
{
}

.assembly 'test-858-lib'
{
  .hash algorithm 0x00008004
  .ver  0:0:0:0
}

.module 'test-858-lib.dll'

.class public auto ansi beforefieldinit 'Foo'
  	extends [mscorlib]System.Object
  {

    .method public hidebysig  specialname  rtspecialname 
           instance default void .ctor ()  cil managed 
    {
      ldarg.0 
      call instance void valuetype [mscorlib]'System.Object'::.ctor()
      ret 
    }

    .method public virtual  hidebysig  newslot  specialname 
           instance default string 'Monkey_Jump' ()  cil managed 
    {
      ldstr "foo"
      ret 
    }

	.property specialname rtspecialname string Message ()
	{
		.get instance default string 'Foo'::'Monkey_Jump' () 
	}
}

.class public auto ansi beforefieldinit 'Bar'
  	extends Foo
  {
    .method public hidebysig  specialname  rtspecialname 
           instance default void .ctor ()  cil managed 
    {
      ldarg.0 
      call instance void class 'Foo'::.ctor()
      ret 
    }

    .method public virtual  hidebysig  specialname 
           instance default string 'Boston' ()  cil managed 
    {
      ldstr "bar"
      ret 
    }

    .property specialname rtspecialname string Message ()
    {
		  .get instance default string 'Bar'::'Boston' () 
    }
}

.class public auto ansi beforefieldinit C
       extends [mscorlib]System.Object
{
  .method public hidebysig specialname instance int32 
          get_Value() cil managed
  {
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
    ret
  }

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    ldarg.0
    call       instance void [mscorlib]System.Object::.ctor()
    ret
  }
}



