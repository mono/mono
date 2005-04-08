// cs0655.cs: 'd' is not a valid named attribute argument because its type is not valid attribute type
// Line: 11

using System;

class TestAttribute : Attribute
{
   public int[][] a;
}

[Test (a = null)]
class C
{
}