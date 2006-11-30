// cs0023: Operator `~' cannot be applied to operand of type `ulong'
// Line : 6

enum E1 { A }
enum E2 { A }

class X {
    const E1 e = ~E2.A;
}



