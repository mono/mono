// CS0501: `C.operator !=(C, C)' must have a body because it is not marked abstract, extern, or partial
// Line: 6

class C
{
	public static bool operator != (C l, C r);
	public static bool operator == (C l, C r);
}
