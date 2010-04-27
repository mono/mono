// CS0675: The operator `|' used on the sign-extended type `int'. Consider casting to a smaller unsigned type first
// Line: 11
// Compiler options: -warnaserror -warn:3

public class C
{
	uint extra_flags;
		
	internal bool BestFitMapping {
		set {
			extra_flags = (uint) ((extra_flags & ~0x30) | (value ? 0x10 : 0x20));
		}
	}
}

