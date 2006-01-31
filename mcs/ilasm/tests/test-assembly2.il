// Referencing an undeclared external assembly

.assembly extern mscorlib
{
}
.assembly 'test-fixedarray'
{
}

  .class public sequential ansi beforefieldinit Dummy
  	extends [mscorlib]System.Object
  {
    .pack 0
    .size 1024
    .field  public class [not_referenced]Test t
  } 
