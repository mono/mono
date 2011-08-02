.assembly 'test-483-lib' {}
.assembly extern mscorlib {}

.class public auto ansi beforefieldinit Foo {

    .method famorassem virtual  newslot 
           instance default object clone ()  cil managed
	{
		.maxstack 8
		ldarg.0
		ret
	}
}

.class public auto ansi Bar extends Foo
{
	.method public hidebysig specialname rtspecialname 
		instance void  .ctor() cil managed
	{
		ldarg.0
		call       instance void Foo::.ctor()
		ret
	}

	.method public virtual instance default object clone ()  cil managed
	{
		.maxstack 8
		ldarg.0
		ret
	}
}
