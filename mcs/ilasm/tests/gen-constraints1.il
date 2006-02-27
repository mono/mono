// Test for constraints on generic parameters

.assembly extern mscorlib
{
  .ver 2:0:0:0
}
.assembly 'const'
{
  .custom instance void class [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() =  (
		01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   
		63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01       ) 

  .ver  0:0:0:0
}
.module const.dll

  .class interface private auto ansi abstract ITest1
  {
  } 

  .class interface private auto ansi abstract ITest
  {
  } 

  .class private auto ansi beforefieldinit bb
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

  } 

  /* Constraints : .ctor, base class, interfaces */
  .class private auto ansi beforefieldinit gen`1 <.ctor (class bb, class [mscorlib]System.Collections.Generic.ICollection`1<!T>, class [mscorlib]System.Collections.ICollection, class ITest1) T>
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
    
    /* Constraints on a method's generic params */
    .method public hidebysig 
           instance default void foo<.ctor (class [mscorlib]System.Collections.Generic.ICollection`1<!!0>) M> ()  cil managed 
    {
	.maxstack 8
	IL_0000:  ret 
    }

  } 

  /* Constraint : class */
  .class private auto ansi beforefieldinit foo`1<class M>
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

  } 

  /* Constraint : valuetype */
  .class private auto ansi beforefieldinit bar`1<valuetype M>
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

  } 

  /* Constraint : generic constraint */
  .class private auto ansi beforefieldinit abc`1 <(class foo`1<class bb>) M>
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

  } 

