.assembly extern mscorlib
{
}

.assembly 'test-912-lib'
{
}

.module 'test-912-lib.dll'

.namespace TypeLib
{
  .class public auto ansi abstract Foo extends [mscorlib]System.Object
  {
    .field public static literal  unsigned int16 CONSTANT = int16(0x0000ffff)
  }
}