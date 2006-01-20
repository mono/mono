//Test for dotted-name for a generic type

.assembly extern mscorlib
{
  .ver 2:0:0:0
}
.assembly 'dottedname'
{
}

.module dottedname.dll

  .class private auto ansi beforefieldinit ns.gen`1<T>
  	extends [mscorlib]System.Object
  {

  }

  .class private auto ansi beforefieldinit test
  	extends [mscorlib]System.Object
  {
    .field  private   class ns.gen`1<int32> g

  }

