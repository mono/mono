//Test for duplicate field names with
//extern TypeRef (array)

.assembly extern mscorlib
{
  .ver 1:0:5000:0
}
.assembly 'field-dup4'
{
  .ver  0:0:0:0
}
.module 'field-dup.exe'


  .class private auto ansi beforefieldinit test
  	extends [mscorlib]System.Object
  {
    .field private class [mscorlib]System.Int32[] i
    .field private class [mscorlib]System.Int32[] i
  }

