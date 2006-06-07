//Test for method with no body. ilasm emits 'ret'

.assembly Test
{
}

.namespace MonoTest
{
	.class private auto autochar beforefieldinit ConsoleApp
		extends [mscorlib]System.Object
		{
			.method assembly hidebysig static void Test() cil managed
			{
				// Code size       74 (0x4a)
				.maxstack  2
			}
		}
}
