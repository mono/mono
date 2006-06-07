//Test for internalcall method with a body

.assembly Test
{
}

.namespace MonoTest
{
	.class private auto autochar beforefieldinit ConsoleApp
		extends [mscorlib]System.Object
		{
			.method assembly hidebysig static void Test() internalcall
			{
				// Code size       74 (0x4a)
				.maxstack  2
				ret
			}
		}
}
