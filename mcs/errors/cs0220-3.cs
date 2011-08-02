// CS0220: The operation overflows at compile time in checked mode
// Line: 7

enum E1 : byte
{
  A = 2
}

enum E2 : ulong
{
  A = ulong.MaxValue - 1,
  B = E1.A * E2.A
}
