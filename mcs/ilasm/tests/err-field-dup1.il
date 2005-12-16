//Test for duplicate field names with
//Primitive types

.assembly extern mscorlib
{
}
.assembly 'field-dup-1'
{
  .ver  0:0:0:0
}

  .class private auto ansi beforefieldinit test
  	extends [mscorlib]System.Object
  {
	.field private static int32 i
	.field private static int32 i

  } // end of class test

