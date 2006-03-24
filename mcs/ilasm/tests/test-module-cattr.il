// Test for custom attributes on this .module

.assembly extern mscorlib
{
  .ver 1:0:5000:0
}
.assembly extern System
{
  .ver 1:0:5000:0
}
.assembly 'test-module-cattr'
{
  .ver  0:0:0:0
}
.module 'test-module-cattr.exe'

.custom instance void class [System]System.ComponentModel.DescriptionAttribute::.ctor(string) =  (01 00 0A 53 6F 6D 65 4D 6F 64 75 6C 65 00 00 ) // ...SomeModule..


  .class private auto ansi beforefieldinit test
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
           default void Main ()  cil managed 
    {
	.entrypoint
	.maxstack 7
	.locals init (
		class [mscorlib]System.Reflection.Module	V_0)
	IL_0000:  ldtoken test
	IL_0005:  call class [mscorlib]System.Type class [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
	IL_000a:  callvirt instance class [mscorlib]System.Reflection.Module class [mscorlib]System.Type::get_Module()
	IL_000f:  stloc.0 
	IL_0010:  ldloc.0 
	IL_0011:  call class [mscorlib]System.Attribute[] class [mscorlib]System.Attribute::GetCustomAttributes(class [mscorlib]System.Reflection.Module)
	IL_0016:  ldlen 
	IL_0017:  conv.i4 
	IL_0018:  ldc.i4.1 
	IL_0019:  bne.un IL_002d

	IL_001e:  ldstr "OK"
	IL_0023:  call void class [mscorlib]System.Console::WriteLine(string)
	IL_0028:  br IL_0037

	IL_002d:  ldstr "Error"
	IL_0032:  call void class [mscorlib]System.Console::WriteLine(string)
	IL_0037:  ret 
    }

  }
