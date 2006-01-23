//Test for duplicate field names with
//nested extern TypeRef

.assembly extern mscorlib
{
  .ver 1:0:5000:0
}
.assembly extern one
{
  .ver 0:0:0:0
}
.assembly 'field-dup5'
{
  .ver  0:0:0:0
}
.module 'field-dup.exe'


  .class private auto ansi beforefieldinit test
  	extends [mscorlib]System.Object
  {
    .field  private class [one]outer/inner [][] n
    .field  private class [one]outer/inner [][] n

  }

