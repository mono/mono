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
    .field  public  marshal (fixed array [16]) unsigned int8[] a

  } 
