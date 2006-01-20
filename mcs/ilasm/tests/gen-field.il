//Test generic field ref: should compile fine, but should give
//PEVerify error:
//	f`1[T]::foo][offset 0x00000014] Field is not visible.

.assembly extern mscorlib
{
  .ver 2:0:0:0
}
.assembly 'f2'
{
  .ver  0:0:0:0
}
.module f2.dll


  .class private auto ansi beforefieldinit 'f`1'<T>
  	extends [mscorlib]System.Object
  {
    .field  public static   !0 _t
    .field  public static   !0[] _tarr
    .field  public static   bool b

    .method public hidebysig  specialname  rtspecialname 
           instance default void .ctor ()  cil managed 
    {
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::.ctor()
	IL_0006:  ret 
    }

    .method public static  hidebysig 
           default void foo ()  cil managed 
    {
	.maxstack 2
	.locals init (
		!T	V_0)
	IL_0000:  ldloca.s 0
	IL_0002:  initobj !0
	IL_0008:  ldloc.0 
	IL_0009:  stsfld  !0 class 'f`1'<!0>::_t
	IL_000e:  ldc.i4.5 
	IL_000f:  newarr !0
	IL_0014:  stsfld  !!0[] class 'f`1'<!0>::_tarr
	IL_0019:  ldc.i4.1 
	IL_001a:  stsfld  bool class 'f`1'<!0>::b
	IL_001f:  ret 
    }

  }

  .class private auto ansi beforefieldinit g
  	extends [mscorlib]System.Object
  {

    .method public hidebysig  specialname  rtspecialname 
           instance default void .ctor ()  cil managed 
    {
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::.ctor()
	IL_0006:  ret 
    }

    .method private hidebysig 
           instance default void foo2 ()  cil managed 
    {
	.maxstack 8
	IL_0000:  ldc.i4.4 
	IL_0001:  stsfld  !0 class 'f`1'<int32>::_t
	IL_0006:  ldc.i4.5 
	IL_0007:  newarr [mscorlib]System.String
	IL_000c:  stsfld  !0[] class 'f`1'<string>::_tarr
	IL_0011:  ldc.i4.1 
	IL_0012:  stsfld  bool class 'f`1'<string>::b
	IL_0017:  ret 
    }

  }
