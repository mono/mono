// Test for : inheriting generic (external) base class and implementing (external) generic interfaces

.assembly extern mscorlib
{
  .ver 2:0:0:0
}
.assembly extern one
{
}
.assembly 'gen-interf-2'
{
  .custom instance void class [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() =  (
		01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
		63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01       ) // ceptionThrows.

  .ver  0:0:0:0
}
.module interf.exe


  .class interface private auto ansi abstract ISimple
  {

  }

  .class private auto ansi beforefieldinit gen_another`2<A,B>
  	extends class [one]gen`1<!0>
  	implements class [one]If1`1<!0>, class [one]If2`1<!1>, ISimple, [one]If3  {

  }

  .class private auto ansi beforefieldinit badname`2<A,B>
  	extends class [one]gen`1<!A>
  	implements class [one]If1`1<!A>, class [one]If2`1<!B>, ISimple, [one]If3  {

  }
