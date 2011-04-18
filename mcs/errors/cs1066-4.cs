// CS1066: The default value specified for optional parameter `i' will never be used
// Line: 7
// Compiler options: -warnaserror

class C
{
	public static implicit operator C (int i = 8)
	{
		return null;
	}
}
