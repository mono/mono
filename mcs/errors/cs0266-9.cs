// CS0266: Cannot implicitly convert type `E2' to `E1'. An explicit conversion exists (are you missing a cast?)
// Line : 8

enum E1 { A }
enum E2 { A }

class X {
    const E1 e = ~E2.A;
}

