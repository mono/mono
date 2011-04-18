// CS0110: The evaluation of the constant value for `E.a' involves a circular definition
// Line: 6

enum E
{
	a = b,
	b = c,
	c = a
}
