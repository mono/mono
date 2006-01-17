// Test for : inheriting generic base class and implementing generic interfaces

.assembly extern mscorlib
{
  .ver 2:0:0:0
}
.assembly 'gen-interf-1'
{
  .custom instance void class [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() =  (
		01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
		63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01       ) // ceptionThrows.

  .ver  0:0:0:0
}
.module interf.exe

  .class interface private auto ansi abstract IGen`1<T>
  {
  }

  .class interface private auto ansi abstract IFen`1<T>
  {
  }

  .class interface private auto ansi abstract ISimple
  {
  }

  .class private auto ansi beforefieldinit cbase`1<Z>
  	extends [mscorlib]System.Object
  {
    .method public hidebysig  specialname  rtspecialname 
           instance default void .ctor ()  cil managed 
    {
	.locals init (class cbase`1<int32>[] c)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::.ctor()
	IL_0006:  ret 
    }
    
   }

  /* Inherits a generic base class and implements generic interfaces */
  .class private auto ansi beforefieldinit gen_another`2<A,B>
  	extends class cbase`1<!1>
  	implements class IGen`1<!0>, class IFen`1<!1>, ISimple  {

   }

  /* Inherits a generic base class and implements generic interfaces,
     but uses !A (type param name) instead of index */
  .class private auto ansi beforefieldinit badname`2<A,B>
  	extends class cbase`1<!B>
  	implements class IGen`1<!A>, class IFen`1<!B>, ISimple  {

   }
