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
    .field  private   !0 p
    .field  private   !T p

  }

