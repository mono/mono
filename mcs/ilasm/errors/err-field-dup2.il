//Test for duplicate field names with
//extern TypeRef types

.assembly extern mscorlib
{
}
.assembly 'field-dup-2'
{
  .ver  0:0:0:0
}

  .class private auto ansi beforefieldinit test
  	extends [mscorlib]System.Object
  {
	.field private static class [mscorlib]System.Int32 i
	.field private static class [mscorlib]System.Int32 i

  } // end of class test
