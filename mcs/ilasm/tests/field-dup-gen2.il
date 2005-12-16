//Test for fields with same name but different types

.assembly extern mscorlib
{
  .ver 1:0:5000:0
}
.assembly extern 'oneg'
{
}
.assembly 'field-dup'
{
  .ver  0:0:0:0
}
.module 'field-dup.exe'

  .class private auto ansi beforefieldinit boo`1 <T>
  	extends [mscorlib]System.Object
  {
  } 

  .class private auto ansi beforefieldinit foo`1 <T>
  	extends [mscorlib]System.Object
  {
    .field private !0 p
     
    .field  private   class foo`1<!T> p
    .field  private   class boo`1<!T> p

    .field  private   class foo`1<int32> p

    .field private class [oneg]outer/inner`1<!0> p
    .field private class [oneg]outer/inner`1<int32> p
  }

