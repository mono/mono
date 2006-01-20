// a Type or method with a custom attribute SuppressUnmanagedCodeSecurityAttribute
// automatically gets a HasSecurity attribute set.

.assembly extern mscorlib
{
}
.assembly 'sec'
{
}
.module sec.dll


  .class private auto ansi beforefieldinit test
  	extends [mscorlib]System.Object
  {
    .custom instance void class [mscorlib]System.Security.SuppressUnmanagedCodeSecurityAttribute::.ctor() =  (01 00 00 00 )

    .method public hidebysig  specialname  rtspecialname 
           instance default void .ctor ()  cil managed 
    {
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::.ctor()
	IL_0006:  ret 
    }

    .method private hidebysig 
           instance default void abc ()  cil managed 
    {
        .custom instance void class [mscorlib]System.Security.SuppressUnmanagedCodeSecurityAttribute::.ctor() =  (01 00 00 00 )

	.maxstack 8
	IL_0000:  ret 
    }

  }
