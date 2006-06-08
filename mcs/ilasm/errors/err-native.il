//Test for unmanaged method. Should not compile

.assembly Test
{
}

.namespace MonoTest
{
	.class private auto autochar beforefieldinit ConsoleApp
		extends [mscorlib]System.Object
		{
			.method assembly hidebysig static void Test() unmanaged
			{
				// Code size       74 (0x4a)
				.maxstack  2
			}
		}
}
