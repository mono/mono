// cs0655.cs: 'd' is not a valid named attribute argument because its type is not valid attribute type
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