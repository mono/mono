//Test for fields with same name but different types

.assembly extern mscorlib
{
  .ver 1:0:5000:0
}
.assembly extern one
{
  .ver 0:0:0:0
}
.assembly extern two
{
  .ver 0:0:0:0
}
.assembly 'field-dup'
{
  .ver  0:0:0:0
}
.module 'field-dup.exe'


  .class private auto ansi beforefieldinit test
  	extends [mscorlib]System.Object
  {
    .field private int32 i
    .field private int32[] i
    .field private class [mscorlib]System.Int32 i
    .field private class [mscorlib]System.Int32[] i
    
    .field  private class [one]outer n
    .field  private class [one]outer[] n
    .field  private class [one]outer[][] n
    .field  private class [two]outer[][] n

    .field  private class [one]outer/inner i
    .field  private class [two]outer/inner i

    .field  private class [one]outer/inner [] i
    .field  private class [two]outer/inner [] i

  }

