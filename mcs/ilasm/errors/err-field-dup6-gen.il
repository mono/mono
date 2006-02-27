//Test for fields with same name but different types
//GenericTypeInst

.assembly extern mscorlib
{
  .ver 1:0:5000:0
}

.assembly 'field-dup'
{
  .ver  0:0:0:0
}
.module 'field-dup.exe'


  .class private auto ansi beforefieldinit test`1 <T>
  	extends [mscorlib]System.Object
  {
    .field  private   class foo`1<!T> p
    .field  private   class foo`1<!0> p
  }

