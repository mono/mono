// CS0683: `C.I.set_Foo(int)' explicit method implementation cannot implement `I.Foo.set' because it is an accessor
// Line: 11

interface I
{
   int Foo { set; }
}

class C: I
{
   void I.set_Foo (int v) { }
}
