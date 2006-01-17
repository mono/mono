// Test for specifying namespace as part of the type name,
// AND also using .namespace

.assembly extern mscorlib
{
  .ver 1:0:5000:0
}
.assembly 'nested'
{
  .ver  0:0:0:0
}
.module nested.exe

.namespace ns {
  .class private auto ansi beforefieldinit A.B
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

  .class nested public auto ansi beforefieldinit C.D
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
  .class nested public auto ansi beforefieldinit further
  	extends [mscorlib]System.Object
  {
  }
  }

 }
}
  .class private auto ansi beforefieldinit test
  	extends ns.A.B/C.D
  {

    .method public hidebysig  specialname  rtspecialname 
           instance default void .ctor ()  cil managed 
    {
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void class ns.A.B/C.D::.ctor()
	IL_0006:  ret 
    }

  }

