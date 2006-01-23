//Error: Invalid type parameter 'A' (!A)

.assembly extern mscorlib
{
  .ver 2:0:0:0
}
.assembly 'gen-2'
{
  .ver  0:0:0:0
}
.module 'gen-2.dll'

  .class private auto ansi beforefieldinit g`1<T>
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

    .method public static  hidebysig 
           default void foo<M> ()  cil managed 
    {
	.maxstack 8
	IL_0000:  ret 
    }

    .method private hidebysig 
           instance default void abc <A> ()  cil managed 
    {
	.maxstack 8
	IL_0000:  call void class g`1<int32>::foo<!A> ()
	IL_0005:  ret 
    }

  }

