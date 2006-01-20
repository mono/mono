//Test for duplicate field names with
//primitive types (array)

.assembly extern mscorlib
{
  .ver 1:0:5000:0
}
.assembly 'field-dup3'
{
  .ver  0:0:0:0
}
.module 'field-dup.exe'


  .class private auto ansi beforefieldinit test
  	extends [mscorlib]System.Object
  {
    .field private int32[] i
    .field private int32[] i
  }

