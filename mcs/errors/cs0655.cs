// CS0655: `d' is not a valid named attribute argument because it is not a valid attribute parameter type
// Line: 11

using System;

class TestAttribute : Attribute
{
   public decimal d;
}

[Test (d = 44444)]
class C
{
}