// cs0683.cs: `C.I.set_Foo(int)' explicit method implementation cannot implement `I.Foo' because it is an accessor
// Line: 11

interface I
{
   int Foo { set; }
}

class C: I
{
   void I.set_Foo (int v) { }
}
